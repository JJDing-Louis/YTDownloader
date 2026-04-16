using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
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
        private string ytDlpPath;
        private string ffmpegPath;

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
        }

        private void InitDownloadListColumns()
        {
            dGV_DownloadList.Columns.Clear();

            dGV_DownloadList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colIndex",
                HeaderText = "#",
                Width = 40,
                ReadOnly = true,
                Resizable = DataGridViewTriState.False
            });

            dGV_DownloadList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTitle",
                HeaderText = "標題",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });

            dGV_DownloadList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMediaType",
                HeaderText = "類型",
                Width = 55,
                ReadOnly = true,
                Resizable = DataGridViewTriState.False
            });

            dGV_DownloadList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colProgress",
                HeaderText = "進度",
                Width = 65,
                ReadOnly = true,
                Resizable = DataGridViewTriState.False,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dGV_DownloadList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStatus",
                HeaderText = "狀態",
                Width = 75,
                ReadOnly = true,
                Resizable = DataGridViewTriState.False,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
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
                dGV_DownloadList.Rows.Count + 1,
                title,
                mediaType,
                "0.0 %",
                "等待中"
            );
            return rowIndex;
        }

        /// <summary>
        /// 更新指定 Row 的進度與狀態欄位。
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
            row.Cells["colProgress"].Value = $"{percent:F1} %";
            row.Cells["colStatus"].Value   = status;

            // 依狀態換色
            row.DefaultCellStyle.BackColor = status switch
            {
                "完成" => Color.FromArgb(200, 240, 200),
                "失敗" => Color.FromArgb(255, 200, 200),
                "下載中" => Color.FromArgb(230, 240, 255),
                _     => dGV_DownloadList.DefaultCellStyle.BackColor
            };
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
            var ytDlpRel = config["Path:yt-dlp"] ;
            var ffmpegRel = config["Path:ffmpeg"] ;

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
                       logger.LogInformation ($"檢測到單一影片：{SourceType.Title}", "資源檢測", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}