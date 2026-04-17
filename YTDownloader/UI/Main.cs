using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using YTDownloader.Controller;
using YTDownloader.Model;
using YTDownloader.Service;
using YTDownloader.UI.CustomUI;

namespace YTDownloader
{
    public partial class Main : Form
    {
        private ILogger logger = Program.Startup.Container.Resolve<ILogger<Main>>();
        private MainInitializationService initializationService = Program.Startup.Container.Resolve<MainInitializationService>();
        private Dictionary<string, List<KeyValuePair<string, string>>> options;
        private IConfiguration config;
        private string DownloadFolder;
        private string ytDlpPath;
        private string ffmpegPath;

        /// <summary>以 Task ID 為 Key，記錄每筆下載任務的控制器。</summary>
        private readonly Dictionary<int, DownloadTaskController> _downloadControllers = new();

        /// <summary>自動遞增的任務 ID，確保行刪除後 Key 不衝突。</summary>
        private int _nextTaskId = 0;

        /// <summary>跨批次共用的並發信號量，最多同時執行 3 個下載。</summary>
        private readonly SemaphoreSlim _downloadSemaphore = new(3, 3);

        /// <summary>共用的下載服務實例（延遲建立，確保 ytDlpPath / ffmpegPath 已讀取完畢）。</summary>
        private YtDlpDownloadService? _downloadService;

        private PlaylistHandler playlistHandlerForm;

        public Main()
        {
            InitializeComponent();
            logger.LogInformation("Main form initialized.");
            Init();
            InitUI();
        }

        #region Init

        private void Init()
        {
            InitConfig();
            InitOptions();
        }

        private void InitUI()
        {
            InitDataGridView();
        }

        private void InitDataGridView()
        {
            dGV_DownloadList.Columns.Clear();

            // # 序號
            dGV_DownloadList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colIndex",
                HeaderText = "#",
                Width = 40,
                ReadOnly = true,
                Resizable = DataGridViewTriState.False
            });

            // 標題
            dGV_DownloadList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTitle",
                HeaderText = "標題",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });

            // 類型（音訊 / 視訊）
            dGV_DownloadList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMediaType",
                HeaderText = "類型",
                Width = 55,
                ReadOnly = true,
                Resizable = DataGridViewTriState.False
            });

            // 進度條（自訂繪製）
            dGV_DownloadList.Columns.Add(new DataGridViewProgressBarColumn
            {
                Name = "colProgress",
                HeaderText = "進度",
                Width = 160,
                Resizable = DataGridViewTriState.False
            });

            // 狀態文字
            dGV_DownloadList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStatus",
                HeaderText = "狀態",
                Width = 75,
                ReadOnly = true,
                Resizable = DataGridViewTriState.False,
                DefaultCellStyle = new DataGridViewCellStyle
                { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // 操作按鈕（暫停 / 繼續 / 重試 / —）
            dGV_DownloadList.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colAction",
                HeaderText = "操作",
                Width = 60,
                Resizable = DataGridViewTriState.False,
                UseColumnTextForButtonValue = false   // 使用每格的 Value 作為按鈕文字
            });

            // 取消按鈕
            dGV_DownloadList.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colCancel",
                HeaderText = "",
                Width = 55,
                Resizable = DataGridViewTriState.False,
                UseColumnTextForButtonValue = false
            });

            // 隱藏的任務 ID（用於刪除列後仍能找到正確的 controller）
            dGV_DownloadList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTaskId",
                HeaderText = "TaskId",
                Visible = false
            });

            dGV_DownloadList.CellContentClick += OnDownloadListCellContentClick;

            dGV_DownloadList.Rows.Clear();
        }

        private void InitOptions()
        {
            logger.LogInformation("Initializing options...");

            try
            {
                options = initializationService.GetOptions();
                BindComboBox(cB_ListMediaType, options, "ListMediaType");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize options.");
                MessageBox.Show("選項載入失敗，請確認資料庫連線。", "初始化錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitConfig()
        {
            logger.LogInformation("Initializing configuration...");
            config = initializationService.GetConfig();

            if (config == null)
            {
                logger.LogError("Configuration object is null.");
                MessageBox.Show("載入設定失敗（config 為 null）。", "初始化錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 以安全方式讀取設定 (支援 appsettings.json 的 "yt-dlp"/"ffmpeg" 與原本預期的 "ytDlpPath"/"ffmpegPath")
            var ytDlpRel = config["Path:yt-dlp"];
            var ffmpegRel = config["Path:ffmpeg"];
            var downloadFolder = config["Path:DownLoadDir"];

            if (string.IsNullOrWhiteSpace(ytDlpRel))
            {
                logger.LogError("yt-dlp path is not configured (Path:yt-dlp or Path:ytDlpPath).");
                MessageBox.Show("yt-dlp 路徑未在設定中指定。", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                ytDlpPath = Path.Combine(Environment.CurrentDirectory, ytDlpRel.Trim());
                if (!File.Exists(ytDlpPath))
                {
                    logger.LogError("yt-dlp executable not found at path: {Path}", ytDlpPath);
                    MessageBox.Show($"yt-dlp 可執行檔未找到，請確認路徑：{ytDlpPath}", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (string.IsNullOrWhiteSpace(ffmpegRel))
            {
                logger.LogError("ffmpeg path is not configured (Path:ffmpeg or Path:ffmpegPath).");
                MessageBox.Show("ffmpeg 路徑未在設定中指定。", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                ffmpegPath = Path.Combine(Environment.CurrentDirectory, ffmpegRel.Trim());
                if (!File.Exists(ffmpegPath))
                {
                    logger.LogError("ffmpeg executable not found at path: {Path}", ffmpegPath);
                    MessageBox.Show($"ffmpeg 可執行檔未找到，請確認路徑：{ffmpegPath}", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (string.IsNullOrWhiteSpace(downloadFolder))
            {
                logger.LogError("downloadFolder path is not configured (Path:DownLoadDir or Path:ffmpegPath).");
                MessageBox.Show("DownLoadDir 路徑未在設定中指定。", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                DownloadFolder = Path.Combine(Environment.CurrentDirectory, downloadFolder.Trim());
                if (!Directory.Exists(DownloadFolder))
                {
                    logger.LogError("Download folder not found at path: {Path}", DownloadFolder);
                    MessageBox.Show($"下載資料夾未找到，請確認路徑：{DownloadFolder}", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BindComboBox(ComboBox comboBox, Dictionary<string, List<KeyValuePair<string, string>>> options, string key)
        {
            if (!options.TryGetValue(key, out var items) || items == null || items.Count == 0)
            {
                logger.LogWarning("Options key '{Key}' is missing or empty.", key);
                return;
            }

            comboBox.DisplayMember = "Key";
            comboBox.Items.AddRange(items.Cast<object>().ToArray());
            if (comboBox.Items.Count > 0) { comboBox.SelectedIndex = 0; }
        }

        #endregion Init

        #region UI Functions

        private async void btn_Download_Click(object sender, EventArgs e)
        {
            try
            {
                var SelectedMediaType = ((KeyValuePair<string, string>)cB_ListMediaType.SelectedItem).Value;
                var URL = tB_URL.Text;
                if (string.IsNullOrWhiteSpace(URL))
                {
                    MessageBox.Show("請輸入有效的 URL。", "輸入錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var YTDownloadService = new YtDlpDownloadService(ytDlpPath, ffmpegPath);

                var SourceType = await YTDownloadService.DetectResourceAsync(URL);
                switch (SourceType.ResourceType)
                {
                    case UrlResourceType.SingleVideo:
                        logger.LogInformation($"檢測到單一影片：{SourceType.Title}", "資源檢測", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        var resquest = new DownloadRequest
                        {
                            Title = SourceType.Title ?? "未知標題",
                            WebpageUrl = URL,
                            IsAudio = SelectedMediaType.Equals("Audio", StringComparison.OrdinalIgnoreCase),
                            DownloadDir = DownloadFolder
                        };

                        break;
                    case UrlResourceType.Playlist:
                        logger.LogInformation($"檢測到播放清單：{SourceType.PlaylistTitle}，共 {SourceType.PlaylistCount} 部影片", "資源檢測", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        playlistHandlerForm = new PlaylistHandler(URL, this);
                        var (isSuccess, msg) = await playlistHandlerForm.GetPlaylistInfoAsync();
                        if (isSuccess)
                        {
                            logger.LogInformation($"成功獲取播放清單資訊：{msg}");
                            playlistHandlerForm.Show();
                        }
                        else
                        {
                            logger.LogInformation($"獲取播放清單資訊失敗：{msg}");
                        }

                        playlistHandlerForm.Location = new Point(700, 0);
                        playlistHandlerForm.Disposed += new EventHandler(playlistHandlerForm_Disposed);

                        break;
                    default:
                        MessageBox.Show("無法識別的 URL 類型。", "資源檢測", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve selected options.");
            }
        }

        private void playlistHandlerForm_Disposed(object? sender, EventArgs e)
        {
            playlistHandlerForm = null;
        }

        private void btn_OpenDownloadForder_Click(object sender, EventArgs e)
        {
            var DownLoadForderPath = Path.Combine(Environment.CurrentDirectory, DownloadFolder);
            Process.Start("explorer.exe", DownLoadForderPath);
        }


        /// <summary>
        /// 處理下載清單的按鈕點擊（暫停 / 繼續 / 重試 / 取消）。
        /// </summary>
        private void OnDownloadListCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var colName = dGV_DownloadList.Columns[e.ColumnIndex].Name;
            if (colName != "colAction" && colName != "colCancel") return;

            var row = dGV_DownloadList.Rows[e.RowIndex];

            // 取得此列的 Task ID（TextBox 儲存格可能回傳 int 或 string）
            var taskIdCell = row.Cells["colTaskId"].Value;
            int taskId;
            if (taskIdCell is int directId)
                taskId = directId;
            else if (taskIdCell is string s && int.TryParse(s, out int parsedId))
                taskId = parsedId;
            else
                return;

            // ── 取消按鈕 ──────────────────────────────────────────
            if (colName == "colCancel")
            {
                if (_downloadControllers.TryGetValue(taskId, out var cancelCtrl))
                {
                    cancelCtrl.Cts.Cancel();
                    _downloadControllers.Remove(taskId);
                }
                dGV_DownloadList.Rows.RemoveAt(e.RowIndex);
                RenumberRows();
                return;
            }

            // ── colAction ─────────────────────────────────────────
            if (!_downloadControllers.TryGetValue(taskId, out var controller)) return;

            var btnText = row.Cells["colAction"].Value?.ToString() ?? "";
            switch (btnText)
            {
                // ── 暫停 ─────────────────────────────────────────
                case "暫停":
                    controller.Cts.Cancel();
                    controller.IsPaused = true;
                    row.Cells["colAction"].Value = "繼續";
                    row.Cells["colStatus"].Value = "已暫停";
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 220);
                    break;

                // ── 繼續 / 重試 ──────────────────────────────────
                case "繼續":
                case "重試":
                    controller.Cts = new CancellationTokenSource();
                    controller.IsPaused = false;
                    row.Cells["colAction"].Value = "暫停";
                    UpdateDownloadProgress(taskId, controller.LastPercent, "下載中");
                    var cts = controller.Cts;
                    _ = Task.Run(async () => await controller.RestartAction(cts.Token));
                    break;

                // ── 已完成 / 其他（不動作）───────────────────────
                default:
                    break;
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// 依 Task ID 找出對應的 DataGridView 列（列刪除後仍穩定）。
        /// </summary>
        private DataGridViewRow? FindRowByTaskId(int taskId)
        {
            foreach (DataGridViewRow row in dGV_DownloadList.Rows)
            {
                var cellVal = row.Cells["colTaskId"].Value;
                // DataGridViewTextBoxColumn 可能以 int 或 string 儲存
                if (cellVal is int id && id == taskId) return row;
                if (cellVal is string s && int.TryParse(s, out int sid) && sid == taskId) return row;
            }
            return null;
        }

        /// <summary>
        /// 重新編排 colIndex（刪除列後維持連續序號）。
        /// </summary>
        private void RenumberRows()
        {
            for (int i = 0; i < dGV_DownloadList.Rows.Count; i++)
                dGV_DownloadList.Rows[i].Cells["colIndex"].Value = i + 1;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 目前選取的媒體類型 Value（如 "Audio" / "Video"）
        /// </summary>
        public string SelectedMediaTypeValue =>
            cB_ListMediaType.SelectedItem is KeyValuePair<string, string> kv ? kv.Value : string.Empty;

        /// <summary>
        /// 新增一筆下載項目至清單，回傳穩定的 Task ID（不受列刪除影響）。
        /// 執行緒安全（可從非 UI 執行緒呼叫）。
        /// </summary>
        public int AddDownloadItem(string title, string mediaType)
        {
            // 如果不是在 UI 執行緒，使用 Invoke 切換到 UI 執行緒執行，確保 DataGridView 操作安全。
            if (dGV_DownloadList.InvokeRequired)
                return (int)dGV_DownloadList.Invoke(new Func<int>(() => AddDownloadItem(title, mediaType)));

            int taskId = _nextTaskId++;
            dGV_DownloadList.Rows.Add(
                dGV_DownloadList.Rows.Count + 1,  // colIndex
                title,                                                      // colTitle
                mediaType,                                          // colMediaType
                0.0,                                                       // colProgress（double，ProgressBarCell 讀取）
                "等待中",                                              // colStatus
                "暫停",                                                 // colAction 按鈕文字
                "取消",                                                 // colCancel 按鈕文字
                taskId                                                  // colTaskId（隱藏）
            );
            return taskId;
        }

        /// <summary>
        /// 更新指定任務的進度條與狀態欄位，並同步更新控制器的 LastPercent。
        /// 執行緒安全（可從非 UI 執行緒呼叫）。
        /// </summary>
        public void UpdateDownloadProgress(int taskId, double percent, string status)
        {
            // 如果不是在 UI 執行緒，使用 Invoke 切換到 UI 執行緒執行，確保 DataGridView 操作安全。
            if (dGV_DownloadList.InvokeRequired)
            {
                dGV_DownloadList.Invoke(new Action(() => UpdateDownloadProgress(taskId, percent, status)));
                return;
            }

            var row = FindRowByTaskId(taskId);
            if (row == null) return;

            row.Cells["colProgress"].Value = percent;   // double → DataGridViewProgressBarCell
            row.Cells["colStatus"].Value = status;

            // 同步控制器的最後進度（供「繼續」時顯示）
            if (_downloadControllers.TryGetValue(taskId, out var ctrl))
                ctrl.LastPercent = percent;

            // 依狀態設定列底色
            row.DefaultCellStyle.BackColor = status switch
            {
                "完成" => Color.FromArgb(200, 240, 200),   // 淡綠
                "失敗" => Color.FromArgb(255, 200, 200),   // 淡紅
                "下載中" => Color.FromArgb(230, 240, 255),   // 淡藍
                "已暫停" => Color.FromArgb(255, 248, 220),   // 淡黃
                _ => dGV_DownloadList.DefaultCellStyle.BackColor
            };
        }

        /// <summary>
        /// 向 Main 登錄一筆下載任務的控制器（含重啟 Action），回傳可操控該任務的 controller。
        /// </summary>
        public DownloadTaskController RegisterDownload(int taskId, Func<CancellationToken, Task> restartAction)
        {
            var controller = new DownloadTaskController { RestartAction = restartAction };
            _downloadControllers[taskId] = controller;
            return controller;
        }

        /// <summary>
        /// 設定指定任務的操作按鈕文字。執行緒安全（可從非 UI 執行緒呼叫）。
        /// </summary>
        public void SetActionButton(int taskId, string text)
        {
            if (dGV_DownloadList.InvokeRequired)
            {
                dGV_DownloadList.Invoke(new Action(() => SetActionButton(taskId, text)));
                return;
            }

            var row = FindRowByTaskId(taskId);
            if (row != null)
                row.Cells["colAction"].Value = text;
        }

        /// <summary>
        /// 接收來自 PlaylistHandler（或其他來源）的下載請求，
        /// 加入清單並立即以背景工作排入佇列執行。
        /// 可在視窗已關閉後安全呼叫（執行緒安全）。
        /// </summary>
        public void EnqueueDownloads(IEnumerable<DownloadRequest> requests)
        {
            _downloadService ??= new YtDlpDownloadService(ytDlpPath, ffmpegPath);

            foreach (var req in requests)
            {
                // 先在 UI 清單加入一列，取得穩定 taskId
                int taskId = AddDownloadItem(req.Title, req.MediaTypeDisplay);

                // 捕捉區域變數，避免 closure 捕捉迴圈變數
                var capturedReq = req;
                var svc = _downloadService;

                Func<CancellationToken, Task> downloadAction = async (ct) =>
                {
                    UpdateDownloadProgress(taskId, 0, "下載中");

                    DownloadResult result = capturedReq.IsAudio
                        ? await svc.DownloadAudioAsync(
                            url: capturedReq.WebpageUrl,
                            outputFolder: capturedReq.DownloadDir,
                            onProgress: pct => UpdateDownloadProgress(taskId, pct, "下載中"),
                            cancellationToken: ct)
                        : await svc.DownloadVideoAsync(
                            url: capturedReq.WebpageUrl,
                            outputFolder: capturedReq.DownloadDir,
                            onProgress: pct => UpdateDownloadProgress(taskId, pct, "下載中"),
                            cancellationToken: ct);

                    // 被取消（暫停 / 取消按鈕）→ 不覆蓋狀態
                    if (ct.IsCancellationRequested) return;

                    if (result.IsSuccess)
                    {
                        UpdateDownloadProgress(taskId, 100, "完成");
                        SetActionButton(taskId, "—");
                    }
                    else
                    {
                        UpdateDownloadProgress(taskId, 0, "失敗");
                        SetActionButton(taskId, "重試");
                        logger.LogError("下載失敗 [{Title}]：{Message}", capturedReq.Title, result.Message);
                    }
                };

                //註冊至 Main，取得控制器以供暫停/繼續使用
                var controller = RegisterDownload(taskId, downloadAction);

                // 排入背景，受全域信號量並發控制
                _ = Task.Run(async () =>
                {
                    await _downloadSemaphore.WaitAsync();
                    try { await downloadAction(controller.Cts.Token); }
                    finally { _downloadSemaphore.Release(); }
                });
            }
        }

        public string GetMediaTypeDisplay() 
        {
            var mediaTypeValue = SelectedMediaTypeValue;
            bool isAudio = mediaTypeValue.Equals("Audio", StringComparison.OrdinalIgnoreCase);
            return isAudio ? "音訊" : "視訊";
        }

        #endregion

    }
}