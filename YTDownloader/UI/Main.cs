using Autofac;
using Microsoft.Extensions.Logging;
using YTDownloader.Service;

namespace YTDownloader
{
    public partial class Main : Form
    {
        private ILogger logger = Program.Startup.Container.Resolve<ILogger<Main>>();
        private MainInitializationService initializationService = Program.Startup.Container.Resolve<MainInitializationService>();

        public Main()
        {
            InitializeComponent();
            logger.LogInformation("Main form initialized.");
            Init();
        }

        private void Init()
        {
            InitOptions();
        }

        #region Init

        private void InitOptions()
        {
            logger.LogInformation("Initializing options...");

            try
            {
                var options = initializationService.GetOptions();
                BindComboBox(cB_ListMediaType, options, "ListMediaType");
                BindComboBox(cB_ListSourceType, options, "ListSourceType");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize options.");
                MessageBox.Show("選項載入失敗，請確認資料庫連線。", "初始化錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var SelectedSourceType = ((KeyValuePair<string, string>)cB_ListSourceType.SelectedItem).Value;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve selected options.");
            }
        }
    }
}