

using Autofac;
using JJNET.Utility.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YTDownloader.Model;
using YTDownloader.Service;
using YTDownloader.Tool;

namespace YTDownloader
{
    public partial class DownloadHistoryForm : Form
    {
        private readonly ILogger _logger;
        private readonly ConfigService _configService;
        private readonly OptionService _optionService;
        private IConfiguration config = null!;
        private ConfigModel _settings;
        
        public DownloadHistoryForm()
        {

            _configService = new ConfigService();
            _optionService = Program.Startup.Container.Resolve<OptionService>();
            _logger = Program.Startup.Container.Resolve<ILogger<DownloadHistoryForm>>();
        }
        
        public DownloadHistoryForm(ConfigService configService, OptionService optionService, ILogger<MainForm> logger)
        {
            ///TODO:
            ///讀歷史紀錄，並顯示
            _configService = configService;
            _settings = _configService.Load();
            _optionService = optionService;
            _logger = logger;
            GUITool.ApplyStartupFont(this, _settings);
            InitializeComponent();
            LockWindowSize();
            _logger.LogInformation("DownloadHistoryForm form initialized.");
            Init();
        }

        private void Init()
        {
            InitConfig();
            InitOptions();
            InitUI();
        }

        private void InitUI()
        {
            
        }

        private void InitConfig()
        {
            _logger.LogInformation("Initializing configuration...");
            config = ParameterTool.GetConfiguration();

            if (config == null)
            {
                _logger.LogError("Configuration object is null.");
                MessageBox.Show("載入設定失敗（config 為 null）。", "初始化錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        
        private void InitOptions()
        {
            
        }
        
        private void LockWindowSize()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimumSize = Size;
            MaximumSize = Size;
        }
    }
}
