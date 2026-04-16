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
        }

        private void Init()
        {
            InitConfig();
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
        /// 再想一下要怎麼設計?
        /// </summary>
        /// <param name="returnMsg"></param>
        /// <returns></returns>
        public bool GetPlaylistInfo(out string returnMsg)
        {
            var result = false;
            returnMsg = string.Empty;
            try
            {
                var YTDownloadService = new YtDlpDownloadService(ytDlpPath, ffmpegPath);
                var playlist = YTDownloadService.GetPlaylistVideosAsync(playlistUrl).Result;
                result = playlist.IsSuccess;
                returnMsg = playlist.Message;
                dGV_PlayList.Rows.Clear();
                if(playlist.Videos != null && playlist.Videos.Count > 0)
                {
                    foreach (var video in playlist.Videos)
                    {
                        dGV_PlayList.Rows.Add(
                            video.IsSelected,       // CheckBox 欄
                            video.Index,            // #
                            video.DisplayTitle,     // 標題（自動 fallback 到 ID）
                            video.Uploader,         // 頻道
                            video.DurationString    // 時長（mm:ss / hh:mm:ss）
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                   returnMsg = $"獲取播放列表資訊時發生錯誤: {ex.Message}";
            }
            return result;
        }
       
    }
}
