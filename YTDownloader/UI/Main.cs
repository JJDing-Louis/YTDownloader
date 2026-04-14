using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

        public Main()
        {
            InitializeComponent();
            logger.LogInformation("Main form initialized.");
            Init();
        }

        private void Init()
        {
            InitConfig();
            InitOptions();
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

        private void btn_Download_Click(object sender, EventArgs e)
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

                var SourceType = YTDownloadService.DetectResourceAsync(URL).GetAwaiter().GetResult();
                switch (SourceType.ResourceType)
                {
                    case UrlResourceType.SingleVideo:
                       logger.LogInformation ($"檢測到單一影片：{SourceType.Title}", "資源檢測", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case UrlResourceType.Playlist:
                        logger.LogInformation($"檢測到播放清單：{SourceType.PlaylistTitle}，共 {SourceType.PlaylistCount} 部影片", "資源檢測", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}