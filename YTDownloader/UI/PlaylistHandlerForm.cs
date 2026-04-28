using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YTDownloader.Init;
using YTDownloader.Model;
using YTDownloader.Service;

namespace YTDownloader
{
    public partial class PlaylistHandlerForm : Form
    {
        private ILogger logger = Program.Startup.Container.Resolve<ILogger<PlaylistHandlerForm>>();
        private PlaylistHandlerInitializationService initializationService = Program.Startup.Container.Resolve<PlaylistHandlerInitializationService>();
        private IConfiguration config;
        private string ytDlpPath;
        private string ffmpegPath;
        private string downloadDir;
        private string playlistUrl;
        private MainForm mainForm;
        private MediaType mediaType;
        private string mediaTypeDisplay;
        private List<PlaylistVideoItem> playlistVideos = new();

        public PlaylistHandlerForm()
        {
            InitializeComponent();
            logger.LogInformation("PlaylistHandlerForm form initialized.");

        }

        public PlaylistHandlerForm(string playlistUrl, MainForm mainForm, MediaType mediaType, string mediaTypeDisplay) : this()
        {
            this.playlistUrl = playlistUrl;
            this.mainForm = mainForm;
            this.mediaType = mediaType;
            this.mediaTypeDisplay = mediaTypeDisplay;
            Init();
            InitUI();
        }

        #region Init
        private void Init()
        {
            InitConfig();
        }

        private void InitUI()
        {
            InitDataGridView();
        }

        private void InitDataGridView()
        {
            dGV_PlayList.Columns.Clear();
            dGV_PlayList.AllowUserToAddRows = false;   // 禁止自動產生空白新增列
            dGV_PlayList.AllowUserToDeleteRows = false;   // 禁止使用者刪除列

            // 勾選欄
            dGV_PlayList.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "colSelected",
                HeaderText = "選取",
                Width = 50,
                ReadOnly = false,
                Resizable = DataGridViewTriState.False
            });

            // 序號欄
            dGV_PlayList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colIndex",
                HeaderText = "#",
                Width = 45,
                ReadOnly = true
            });

            // 影片唯一識別碼
            dGV_PlayList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "Id",
                Width = 45,
                ReadOnly = true,
                Visible = false // 隱藏 ID 欄位，僅供內部使用
            });

            // 標題欄
            dGV_PlayList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTitle",
                HeaderText = "標題",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });

            // 頻道欄
            dGV_PlayList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colUploader",
                HeaderText = "頻道",
                Width = 150,
                ReadOnly = true
            });

            // 時長欄
            dGV_PlayList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDuration",
                HeaderText = "時長",
                Width = 80,
                ReadOnly = true
            });

            // 時長欄
            dGV_PlayList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colURL",
                HeaderText = "連結",
                Width = 80,
                ReadOnly = true,
                Visible = false // 隱藏 URL 欄位，僅供內部使用
            });
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

            var downloadDirRel = config["Path:DownLoadDir"];
            downloadDir = string.IsNullOrWhiteSpace(downloadDirRel)
                ? Path.Combine(Environment.CurrentDirectory, "Downloads")
                : Path.Combine(Environment.CurrentDirectory, downloadDirRel.Trim());
        }
        #endregion

        #region UI Functions
        private void cB_SelectedAll_CheckStateChanged(object sender, EventArgs e)
        {
            if (dGV_PlayList.RowCount > 0)
            {
                bool isChecked = cB_SelectedAll.Checked;
                foreach (DataGridViewRow row in dGV_PlayList.Rows)
                {
                    row.Cells["colSelected"].Value = isChecked;
                }
            }
        }

        private void btn_Download_Click(object sender, EventArgs e)
        {
            // ── 1. 收集已勾選項目的 ID ──────────────────────────────────────
            var selectedItemsId = new List<string>();
            foreach (DataGridViewRow row in dGV_PlayList.Rows)
            {
                if (row.Cells["colSelected"].Value is true)
                    selectedItemsId.Add(row.Cells["colId"].Value?.ToString() ?? string.Empty);
            }

            if (selectedItemsId.Count == 0)
            {
                MessageBox.Show("請至少選取一個項目。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ── 2. 從暫存的 playlistVideos 查回完整物件 ──────────────────────
            var selectedVideoItems = selectedItemsId
                .Select(id => playlistVideos.FirstOrDefault(v => v.Id == id))
                .Where(v => v != null)
                .Cast<PlaylistVideoItem>()
                .ToList();

            if (selectedVideoItems.Count == 0)
            {
                MessageBox.Show("找不到對應的影片資訊，請重新載入播放清單。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── 3. 組成 DownloadRequest 清單，交給 MainForm 執行 ─────────────────
            var requests = selectedVideoItems.Select(item => new DownloadRequest
            {
                Title            = item.DisplayTitle,
                WebpageUrl       = item.WebpageUrl!,
                MediaType        = mediaType,
                MediaTypeDisplay = mediaTypeDisplay,
                DownloadDir      = downloadDir
            });

            mainForm.EnqueueDownloads(requests);

            // ── 4. 關閉視窗，下載在 MainForm 背景進行 ────────────────────────────
            logger.LogInformation("已提交 {Count} 個下載任務，關閉 PlaylistHandlerForm。", selectedVideoItems.Count);
            this.Close();
        }

        private void btn_Cancel_Click(object sender, EventArgs e) => this.Close();
        #endregion

        #region  Public Methods
        /// <summary>
        /// 非同步取得播放清單資訊，並填入 DataGridView。
        /// 避免在 UI Thread 上使用 .Result/.GetAwaiter().GetResult()，防止 async deadlock。
        /// </summary>
        /// <returns>(IsSuccess, Message)</returns>
        public async Task<(bool IsSuccess, string Message)> GetPlaylistInfoAsync()
        {
            try
            {
                var ffmpegDir = File.Exists(ffmpegPath)
                    ? Path.GetDirectoryName(ffmpegPath)!
                    : ffmpegPath;
                var YTDownloadService = new YtDlpDownloadService(ytDlpPath, ffmpegDir);
                var playlist = await YTDownloadService.GetPlaylistVideosAsync(playlistUrl);
                dGV_PlayList.Rows.Clear();
                if (playlist.Videos != null && playlist.Videos.Count > 0)
                {
                    //暫存目前取得的播放清單媒體資訊
                    playlistVideos = playlist.Videos.DeepClone();

                    //UI顯示
                    foreach (var video in playlist.Videos)
                    {
                        dGV_PlayList.Rows.Add(
                            video.IsSelected,           // CheckBox 欄
                            video.Index,                    // #
                            video.Id,                        //Id（隱藏欄位）
                            video.DisplayTitle,         // 標題（自動 fallback 到 ID）
                            video.Uploader,             // 頻道
                            video.DurationString,    // 時長（mm:ss / hh:mm:ss）
                            video.WebpageUrl        // 連結（隱藏欄位）
                        );
                    }
                }
                // 若有不可播放的影片（私人 / 刪除 / 地區限制），跳出一次性提示
                if (playlist.IsSuccess && playlist.SkippedCount > 0)
                {
                    int privateCount = playlist.UnavailableEntries
                        .Count(e => e.Contains("[Private video]", StringComparison.OrdinalIgnoreCase));
                    int deletedCount = playlist.UnavailableEntries
                        .Count(e => e.Contains("[Deleted video]", StringComparison.OrdinalIgnoreCase));
                    int otherCount   = playlist.SkippedCount - privateCount - deletedCount;

                    var lines = new System.Text.StringBuilder();
                    if (privateCount > 0) lines.AppendLine($"私人影片：{privateCount} 部");
                    if (deletedCount > 0) lines.AppendLine($"刪除影片：{deletedCount} 部");
                    if (otherCount   > 0) lines.AppendLine($"其他無法存取：{otherCount} 部");

                    MessageBox.Show(
                        $"{lines}\n可下載：{playlist.TotalCount} 部",
                        "部分影片無法存取",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                return (playlist.IsSuccess, playlist.Message);
            }
            catch (Exception ex)
            {
                return (false, $"獲取播放列表資訊時發生錯誤: {ex.Message}");
            }
        }
        #endregion
    }
}
