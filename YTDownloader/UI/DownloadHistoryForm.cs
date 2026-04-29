

using Autofac;
using JJNET.Utility.Tools;
using Microsoft.Extensions.Logging;
using YTDownloader.Service;
using YTDownloader.Tool;

namespace YTDownloader
{
    public partial class DownloadHistoryForm : Form
    {
        private readonly ILogger<MainForm> _logger;
        private readonly ConfigService _configService;
        private readonly OptionService _optionService;
        
        public DownloadHistoryForm()
        {
            GUITool.ApplyStartupFontFromConfig(this);
            InitializeComponent();
            GUITool.ApplyFromConfig(this);
            _configService = new ConfigService();
            _optionService = Program.Startup.Container.Resolve<OptionService>();
            _logger = Program.Startup.Container.Resolve<ILogger<DownloadHistoryForm>>();
        }
        
        public DownloadHistoryForm(MainForm main) : this()
        {
            ///TODO:
            ///讀歷史紀錄，並顯示

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
    }
}
