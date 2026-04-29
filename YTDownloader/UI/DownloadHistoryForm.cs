

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
        
        private const string OptionListDownloadStatus = "ListDownloadStatus";
        
        public DownloadHistoryForm()
        {
            _configService = new ConfigService();
            _settings = _configService.Load();
            _optionService = Program.Startup.Container.Resolve<OptionService>();
            _logger = Program.Startup.Container.Resolve<ILogger<DownloadHistoryForm>>();
            InitializeForm();
        }
        
        public DownloadHistoryForm(ConfigService configService, OptionService optionService, ILogger<DownloadHistoryForm> logger)
        {
            ///TODO:
            ///讀歷史紀錄，並顯示
            _configService = configService;
            _settings = _configService.Load();
            _optionService = optionService;
            _logger = logger;
            InitializeForm();
        }

        private void InitializeForm()
        {
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
            InitDataGridView();
        }

        private void InitDataGridView()
        {
            dGV_SearchResult.Columns.Clear();
            //勾選選項
            dGV_SearchResult.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "colSelect",
                HeaderText = "",
                Width = 30,
                ReadOnly = false,
                Resizable = DataGridViewTriState.False
            });
            // # 序號
            dGV_SearchResult.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colIndex",
                HeaderText = "#",
                Width = 40,
                ReadOnly = true,
                Resizable = DataGridViewTriState.False
            });
            // 檔名
            dGV_SearchResult.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTitle",
                HeaderText = "檔名",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });
            // 類型（音訊 / 視訊）
            dGV_SearchResult.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMediaType",
                HeaderText = "類型",
                Width = 55,
                ReadOnly = true,
                Resizable = DataGridViewTriState.False
            });
            // 狀態文字
            dGV_SearchResult.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStatus",
                HeaderText = "狀態",
                Width = 200,
                ReadOnly = true,
                Resizable = DataGridViewTriState.True,
                DefaultCellStyle = new DataGridViewCellStyle
                    { Alignment = DataGridViewContentAlignment.MiddleLeft }
            });
            // 隱藏的任務 ID（用於刪除列後仍能找到正確的 controller）
            dGV_SearchResult.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTaskId",
                HeaderText = "TaskId",
                Visible = false
            });
            dGV_SearchResult.Rows.Clear();
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
            _logger.LogInformation("Initializing options...");
            try
            {
                GUITool.BindComboBox(cBO_DownloadResult, OptionListDownloadStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize options.");
                MessageBox.Show("選項載入失敗，請確認資料庫連線。", "初始化錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
