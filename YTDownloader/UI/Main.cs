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

        #region MediaType helpers

        /// <summary>
        /// 將設定字串（來自 ComboBox Value）轉換為 MediaType enum。
        /// 新增媒體類型時，在這裡加一個 case 即可。
        /// </summary>
        private static MediaType ParseMediaType(string value) => value switch
        {
            "Audio" => MediaType.Audio,
            "Video" => MediaType.Video,
            _       => throw new NotSupportedException($"未知的媒體類型值：{value}")
        };

        /// <summary>
        /// 取得 MediaType 對應的中文顯示名稱（用於 UI 清單的「類型」欄）。
        /// </summary>
        private static string GetMediaTypeDisplay(MediaType mediaType) => mediaType switch
        {
            MediaType.Audio => "音訊",
            MediaType.Video => "視訊",
            _               => mediaType.ToString()
        };

        #endregion

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
                Width = 200,
                ReadOnly = true,
                Resizable = DataGridViewTriState.True,
                DefaultCellStyle = new DataGridViewCellStyle
                { Alignment = DataGridViewContentAlignment.MiddleLeft }
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

                var ffmpegDir = File.Exists(ffmpegPath)
                    ? Path.GetDirectoryName(ffmpegPath)!
                    : ffmpegPath;
                var YTDownloadService = new YtDlpDownloadService(ytDlpPath, ffmpegDir);

                var SourceType = await YTDownloadService.DetectResourceAsync(URL);
                switch (SourceType.ResourceType)
                {
                    case UrlResourceType.SingleVideo:
                        logger.LogInformation($"檢測到單一影片：{SourceType.Title}", "資源檢測", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        var mediaType = ParseMediaType(SelectedMediaType);
                        var playlist = await YTDownloadService.GetPlaylistVideosAsync(URL);
                        var video =  playlist.Videos.FirstOrDefault() ?? throw new Exception("無法取得影片資訊");
                        var request = new DownloadRequest
                        {
                            Title            = video.Title ?? "未知標題",
                            WebpageUrl       = URL,
                            MediaType        = mediaType,
                            MediaTypeDisplay = GetMediaTypeDisplay(mediaType),
                            DownloadDir      = DownloadFolder
                        };
                        EnqueueDownloads(new[] { request });
                        break;
                    case UrlResourceType.Playlist:
                        logger.LogInformation($"檢測到播放清單：{SourceType.PlaylistTitle}，共 {SourceType.PlaylistCount} 部影片");
                        playlistHandlerForm = new PlaylistHandler(URL, this);
                        var (isSuccess, msg) = await playlistHandlerForm.GetPlaylistInfoAsync();
                        if (isSuccess)
                        {
                            logger.LogInformation($"成功獲取播放清單資訊：{msg}");
                            playlistHandlerForm.Location = new Point(700, 0);
                            playlistHandlerForm.Disposed += new EventHandler(playlistHandlerForm_Disposed);
                            playlistHandlerForm.Show();
                        }
                        else
                        {
                            logger.LogError($"獲取播放清單資訊失敗：{msg}");
                            playlistHandlerForm.Dispose();
                            MessageBox.Show(
                                $"無法載入播放清單，請確認連結是否正確。\n\n原因：{msg}",
                                "播放清單載入失敗",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
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
                    // 重啟前先清理：避免不完整輸出或殘留串流檔干擾 yt-dlp 判斷
                    CleanTempFilesBeforeRestart(controller);
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
        /// <summary>目前選取的媒體類型原始 Value 字串（如 "Audio" / "Video"）。</summary>
        public string SelectedMediaTypeValue =>
            cB_ListMediaType.SelectedItem is KeyValuePair<string, string> kv ? kv.Value : string.Empty;

        /// <summary>目前選取的媒體類型（enum）。</summary>
        public MediaType SelectedMediaType => ParseMediaType(SelectedMediaTypeValue);

        /// <summary>目前選取的媒體類型中文顯示名稱（用於 UI 清單）。</summary>
        public string SelectedMediaTypeDisplay => GetMediaTypeDisplay(SelectedMediaType);

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
        /// 將錯誤訊息設為狀態欄的 ToolTip，讓使用者滑鼠停留時可看到失敗原因。
        /// 執行緒安全（可從非 UI 執行緒呼叫）。
        /// </summary>
        private void SetStatusTooltip(int taskId, string message)
        {
            if (dGV_DownloadList.InvokeRequired)
            {
                dGV_DownloadList.Invoke(new Action(() => SetStatusTooltip(taskId, message)));
                return;
            }

            var row = FindRowByTaskId(taskId);
            if (row != null)
                row.Cells["colStatus"].ToolTipText = message;
        }

        /// <summary>
        /// 重啟下載前，清除可能使 yt-dlp 誤判「已完成」的殘留檔案：
        /// <list type="bullet">
        ///   <item><b>.part</b> 部分下載暫存檔 — 允許 yt-dlp 重新下載（不嘗試 resume）。</item>
        ///   <item><b>不完整的最終音訊輸出</b>（如 .mp3）— 若同一 stem 仍有原始串流檔（如 .webm），
        ///         代表 ffmpeg 轉檔未完成；刪除後 yt-dlp 重啟時可利用現有串流檔直接重新轉檔，
        ///         不需重新下載串流。</item>
        /// </list>
        /// 僅對音訊任務執行；視訊任務不清除中間串流，避免誤刪最終輸出。
        /// </summary>
        private void CleanTempFilesBeforeRestart(DownloadTaskController controller)
        {
            var req = controller.OriginalRequest;
            if (req == null
                || string.IsNullOrWhiteSpace(req.DownloadDir)
                || !Directory.Exists(req.DownloadDir))
                return;

            // 對「標題」與「檔名」均做相同的正規化後再比對，
            // 解決 yt-dlp 將受限字元替換為全形字元（如 / → ／）
            // 而本端僅替換為底線所造成的前綴不吻合問題。
            var normalizedTitle = NormalizeForFileMatch(req.Title);
            if (string.IsNullOrWhiteSpace(normalizedTitle)) return;

            // yt-dlp 的「影片串流」暫存副檔名（轉音訊前的原始檔）
            var videoStreamExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".webm", ".mkv", ".mp4", ".m4a", ".ogg", ".opus", ".flv", ".avi"
            };

            // ffmpeg 轉出的最終音訊副檔名
            var audioOutputExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".mp3", ".m4a", ".flac", ".wav", ".opus", ".aac"
            };

            int deletedCount = 0;

            foreach (var file in Directory.GetFiles(req.DownloadDir))
            {
                var fileName = Path.GetFileName(file);
                var ext      = Path.GetExtension(file);
                var stem     = Path.GetFileNameWithoutExtension(file);

                // 正規化後做前綴比對，容忍 yt-dlp 全形字元 vs. 底線的差異
                if (!NormalizeForFileMatch(fileName).StartsWith(normalizedTitle, StringComparison.OrdinalIgnoreCase))
                    continue;

                // ── 1. 刪除 .part 部分下載暫存檔 ─────────────────────────────
                if (ext.Equals(".part", StringComparison.OrdinalIgnoreCase))
                {
                    TryDeleteTempFile(file, ref deletedCount);
                    continue;
                }

                // ── 2. 音訊任務：偵測並刪除不完整的最終音訊輸出 ──────────────
                //    判定條件：最終音訊檔（.mp3 等）與原始串流檔（.webm 等）同時存在
                //    → ffmpeg 轉檔被中斷，音訊輸出不完整，需強制重新轉檔
                if (req.MediaType == MediaType.Audio && audioOutputExts.Contains(ext))
                {
                    bool videoStreamExists = videoStreamExts.Any(ve =>
                        File.Exists(Path.Combine(req.DownloadDir, stem + ve)));

                    if (videoStreamExists)
                        TryDeleteTempFile(file, ref deletedCount);
                }
            }

            if (deletedCount > 0)
                logger.LogInformation(
                    "重啟前清除 {Count} 個殘留暫存檔（標題：{Title}）",
                    deletedCount, req.Title);
            else
                logger.LogDebug(
                    "重啟前未找到需清除的暫存檔（正規化標題：{NTitle}）",
                    normalizedTitle);
        }

        private void TryDeleteTempFile(string path, ref int count)
        {
            try
            {
                File.Delete(path);
                count++;
                logger.LogInformation("已刪除暫存檔：{File}", Path.GetFileName(path));
            }
            catch (Exception ex)
            {
                logger.LogWarning("無法刪除暫存檔 [{File}]：{Msg}", Path.GetFileName(path), ex.Message);
            }
        }

        /// <summary>
        /// 將字串正規化以便與 yt-dlp 產生的檔名做前綴比對。
        /// <para>
        /// yt-dlp 在 Windows 上對受限字元的處理方式因版本而異：
        /// 可能替換為 <c>_</c>（底線）或對應的全形字元（如 <c>/</c> → <c>／</c> U+FF0F）。
        /// 此方法將 ASCII 受限字元及其全形對應字元統一對應至 <c>_</c>，
        /// 確保無論 yt-dlp 採用哪種替換策略，標題與檔名的前綴比對都能成立。
        /// </para>
        /// </summary>
        private static string NormalizeForFileMatch(string? s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return new string(s.Select(c => c switch
            {
                '/' or '／'  => '_',   // SOLIDUS           / FULLWIDTH SOLIDUS
                '\\' or '＼' => '_',   // REVERSE SOLIDUS   / FULLWIDTH REVERSE SOLIDUS
                ':' or '：'  => '_',   // COLON             / FULLWIDTH COLON
                '*' or '＊'  => '_',   // ASTERISK          / FULLWIDTH ASTERISK
                '?' or '？'  => '_',   // QUESTION MARK     / FULLWIDTH QUESTION MARK
                '"' or '＂'  => '_',   // QUOTATION MARK    / FULLWIDTH QUOTATION MARK
                '<' or '＜'  => '_',   // LESS-THAN SIGN    / FULLWIDTH LESS-THAN SIGN
                '>' or '＞'  => '_',   // GREATER-THAN SIGN / FULLWIDTH GREATER-THAN SIGN
                '|' or '｜'  => '_',   // VERTICAL LINE     / FULLWIDTH VERTICAL LINE
                _            => c
            }).ToArray());
        }

        /// <summary>
        /// 音訊下載成功完成後，清除 yt-dlp 或 ffmpeg 未自動刪除的中間影片串流暫存檔
        /// （如 .mp4、.webm、.mkv 等），保持下載目錄乾淨。
        /// <para>
        /// 此情況的發生原因：yt-dlp 以 <c>--extract-audio</c> 下載時，正常流程應在
        /// ffmpeg 轉檔後自動移除原始串流檔，但部分 yt-dlp / 包裝函式庫版本或特定格式
        /// 組合下，原始串流檔會被保留。
        /// </para>
        /// </summary>
        private void CleanVideoStreamsAfterAudioDownload(DownloadRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.DownloadDir) || !Directory.Exists(req.DownloadDir))
                return;

            var normalizedTitle = NormalizeForFileMatch(req.Title);
            if (string.IsNullOrWhiteSpace(normalizedTitle)) return;

            // 確定是影片容器格式（不會是音訊下載的最終輸出）
            var videoOnlyExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".mp4", ".webm", ".mkv", ".flv", ".avi", ".ts", ".mov"
            };

            int deletedCount = 0;
            foreach (var file in Directory.GetFiles(req.DownloadDir))
            {
                var fileName = Path.GetFileName(file);
                var ext      = Path.GetExtension(file);

                if (!NormalizeForFileMatch(fileName).StartsWith(normalizedTitle, StringComparison.OrdinalIgnoreCase)) continue;
                if (!videoOnlyExts.Contains(ext)) continue;

                TryDeleteTempFile(file, ref deletedCount);
            }

            if (deletedCount > 0)
                logger.LogInformation(
                    "音訊下載完成後清除 {Count} 個殘留串流暫存檔（標題：{Title}）",
                    deletedCount, req.Title);
        }

        /// <summary>
        /// 接收來自 PlaylistHandler（或其他來源）的下載請求，
        /// 加入清單並立即以背景工作排入佇列執行。
        /// 可在視窗已關閉後安全呼叫（執行緒安全）。
        /// </summary>
        public void EnqueueDownloads(IEnumerable<DownloadRequest> requests)
        {
            // ffmpegPath 指向可執行檔；YtDlpDownloadService 的第二參數需要「資料夾」路徑
            var ffmpegDir = File.Exists(ffmpegPath)
                ? Path.GetDirectoryName(ffmpegPath)!
                : ffmpegPath;

            _downloadService ??= new YtDlpDownloadService(ytDlpPath, ffmpegDir);

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

                    DownloadResult result = capturedReq.MediaType switch
                    {
                        MediaType.Audio => await svc.DownloadAudioAsync(
                            url:               capturedReq.WebpageUrl,
                            outputFolder:      capturedReq.DownloadDir,
                            onProgress:        pct => UpdateDownloadProgress(taskId, pct, "下載中"),
                            cancellationToken: ct),

                        MediaType.Video => await svc.DownloadVideoAsync(
                            url:               capturedReq.WebpageUrl,
                            outputFolder:      capturedReq.DownloadDir,
                            onProgress:        pct => UpdateDownloadProgress(taskId, pct, "下載中"),
                            cancellationToken: ct),

                        _ => throw new NotSupportedException(
                                 $"尚未支援的媒體類型：{capturedReq.MediaType}")
                    };

                    // 被取消（暫停 / 取消按鈕）→ 不覆蓋狀態
                    if (ct.IsCancellationRequested) return;

                    if (result.IsSuccess)
                    {
                        UpdateDownloadProgress(taskId, 100, "完成");
                        SetActionButton(taskId, "—");
                        // 音訊下載完成後，清除 yt-dlp/ffmpeg 未自動刪除的中間影片串流暫存檔
                        if (capturedReq.MediaType == MediaType.Audio)
                            CleanVideoStreamsAfterAudioDownload(capturedReq);
                    }
                    else
                    {
                        // 取第一行作為簡短狀態文字，完整訊息放 Tooltip
                        var firstLine = (result.Message ?? "未知錯誤")
                            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .FirstOrDefault() ?? "未知錯誤";

                        UpdateDownloadProgress(taskId, 0, $"失敗：{firstLine}");
                        SetActionButton(taskId, "重試");
                        SetStatusTooltip(taskId, result.Message ?? "未知錯誤");
                        logger.LogError("下載失敗 [{Title}]：{Message}", capturedReq.Title, result.Message);
                    }
                };

                //註冊至 Main，取得控制器以供暫停/繼續使用
                var controller = RegisterDownload(taskId, downloadAction);
                controller.OriginalRequest = capturedReq;   // 供重啟前清除暫存檔使用

                // 排入背景，受全域信號量並發控制
                _ = Task.Run(async () =>
                {
                    await _downloadSemaphore.WaitAsync();
                    try { await downloadAction(controller.Cts.Token); }
                    finally { _downloadSemaphore.Release(); }
                });
            }
        }

        #endregion

    }
}