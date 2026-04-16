using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using System.Diagnostics;
using YTDownloader.Model;
using YTDownloader.Service;

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

        /// <summary>以 Row Index 為 Key，記錄每筆下載任務的控制器。</summary>
        private readonly Dictionary<int, DownloadTaskController> _downloadControllers = new();

        #region
        private PlaylistHandler playlistHandlerForm;
        #endregion

        public Main()
        {
            InitializeComponent();
            logger.LogInformation("Main form initialized.");
            Init();
            InitUI();
        }

        private void Init()
        {
            InitConfig();
            InitOptions();
        }

        private void InitUI()
        {
            InitDownloadListColumns();
            dGV_DownloadList.CellContentClick += OnDownloadListCellContentClick;
        }

        private void InitDownloadListColumns()
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
        }

        /// <summary>
        /// 目前選取的媒體類型 Value（如 "Audio" / "Video"）
        /// </summary>
        public string SelectedMediaTypeValue =>
            cB_ListMediaType.SelectedItem is KeyValuePair<string, string> kv ? kv.Value : string.Empty;

        /// <summary>
        /// 新增一筆下載項目至清單，回傳其 Row Index，可用於後續進度更新。
        /// 執行緒安全（可從非 UI 執行緒呼叫）。
        /// </summary>
        public int AddDownloadItem(string title, string mediaType)
        {
            if (dGV_DownloadList.InvokeRequired)
                return (int)dGV_DownloadList.Invoke(new Func<int>(() => AddDownloadItem(title, mediaType)));

            int rowIndex = dGV_DownloadList.Rows.Add(
                dGV_DownloadList.Rows.Count + 1,  // colIndex
                title,                             // colTitle
                mediaType,                         // colMediaType
                0.0,                               // colProgress（double，ProgressBarCell 讀取）
                "等待中",                           // colStatus
                "暫停"                              // colAction 按鈕文字
            );
            return rowIndex;
        }

        /// <summary>
        /// 更新指定 Row 的進度條與狀態欄位，並同步更新控制器的 LastPercent。
        /// 執行緒安全（可從非 UI 執行緒呼叫）。
        /// </summary>
        public void UpdateDownloadProgress(int rowIndex, double percent, string status)
        {
            if (dGV_DownloadList.InvokeRequired)
            {
                dGV_DownloadList.Invoke(new Action(() => UpdateDownloadProgress(rowIndex, percent, status)));
                return;
            }

            if (rowIndex < 0 || rowIndex >= dGV_DownloadList.Rows.Count)
                return;

            var row = dGV_DownloadList.Rows[rowIndex];
            row.Cells["colProgress"].Value = percent;   // double → DataGridViewProgressBarCell
            row.Cells["colStatus"].Value = status;

            // 同步控制器的最後進度（供「繼續」時顯示）
            if (_downloadControllers.TryGetValue(rowIndex, out var ctrl))
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
        public DownloadTaskController RegisterDownload(int rowIndex, Func<CancellationToken, Task> restartAction)
        {
            var controller = new DownloadTaskController { RestartAction = restartAction };
            _downloadControllers[rowIndex] = controller;
            return controller;
        }

        /// <summary>
        /// 設定指定 Row 的操作按鈕文字。執行緒安全（可從非 UI 執行緒呼叫）。
        /// </summary>
        public void SetActionButton(int rowIndex, string text)
        {
            if (dGV_DownloadList.InvokeRequired)
            {
                dGV_DownloadList.Invoke(new Action(() => SetActionButton(rowIndex, text)));
                return;
            }

            if (rowIndex >= 0 && rowIndex < dGV_DownloadList.Rows.Count)
                dGV_DownloadList.Rows[rowIndex].Cells["colAction"].Value = text;
        }

        /// <summary>
        /// 處理下載清單的操作按鈕點擊（暫停 / 繼續 / 重試）。
        /// </summary>
        private void OnDownloadListCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dGV_DownloadList.Columns[e.ColumnIndex].Name != "colAction") return;

            var row = dGV_DownloadList.Rows[e.RowIndex];
            var btnText = row.Cells["colAction"].Value?.ToString() ?? "";

            if (!_downloadControllers.TryGetValue(e.RowIndex, out var controller)) return;

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
                    UpdateDownloadProgress(e.RowIndex, controller.LastPercent, "下載中");
                    var cts = controller.Cts;
                    _ = Task.Run(async () => await controller.RestartAction(cts.Token));
                    break;

                // ── 已完成 / 其他（不動作）───────────────────────
                default:
                    break;
            }
        }

        #region Init

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
        }

        #endregion Init

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
            var DownLoadForderPath = Path.Combine( Environment.CurrentDirectory,DownloadFolder);
            Process.Start("explorer.exe", DownLoadForderPath);
        }
    }
}