using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YTDownloader.Model;
using YTDownloader.Service;

namespace YTDownloader
{
    public partial class PlaylistHandler : Form
    {
        private ILogger logger = Program.Startup.Container.Resolve<ILogger<PlaylistHandler>>();
        private PlaylistHandlerInitializationService initializationService = Program.Startup.Container.Resolve<PlaylistHandlerInitializationService>();
        private IConfiguration config;
        private string ytDlpPath;
        private string ffmpegPath;
        private string playlistUrl;
        private Main mainForm;
        private List<PlaylistVideoItem> playlistVideos = new();


        public PlaylistHandler()
        {
            InitializeComponent();
            logger.LogInformation("PlaylistHandler form initialized.");

        }

        public PlaylistHandler(string playlistUrl, Main mainForm) : this()
        {
            this.playlistUrl = playlistUrl;
            this.mainForm = mainForm;
            Init();
            InitUI();
        }

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
        }

        /// <summary>
        /// 非同步取得播放清單資訊，並填入 DataGridView。
        /// 避免在 UI Thread 上使用 .Result/.GetAwaiter().GetResult()，防止 async deadlock。
        /// </summary>
        /// <returns>(IsSuccess, Message)</returns>
        public async Task<(bool IsSuccess, string Message)> GetPlaylistInfoAsync()
        {
            try
            {
                var YTDownloadService = new YtDlpDownloadService(ytDlpPath, ffmpegPath);
                var playlist = await YTDownloadService.GetPlaylistVideosAsync(playlistUrl);
                dGV_PlayList.Rows.Clear();
                if (playlist.Videos != null && playlist.Videos.Count > 0)
                {
                    foreach (var video in playlist.Videos)
                    {
                        dGV_PlayList.Rows.Add(
                            video.IsSelected,       // CheckBox 欄
                            video.Index,            // #
                            video.Id,                   //Id（隱藏欄位）
                            video.DisplayTitle,     // 標題（自動 fallback 到 ID）
                            video.Uploader,         // 頻道
                            video.DurationString,    // 時長（mm:ss / hh:mm:ss）
                            video.WebpageUrl        // 連結（隱藏欄位）
                        );
                    }
                }
                return (playlist.IsSuccess, playlist.Message);
            }
            catch (Exception ex)
            {
                return (false, $"獲取播放列表資訊時發生錯誤: {ex.Message}");
            }
        }

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
            
        }
    }
}
