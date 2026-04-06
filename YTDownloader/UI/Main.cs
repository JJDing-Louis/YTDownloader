using Autofac;
using Microsoft.Extensions.Logging;
using Utility.Tools;

namespace YTDownloader
{
    public partial class Main : Form
    {
        ILogger logger = Program.Startup.Container.Resolve<ILogger<Main>>();

        public Main()
        {
            InitializeComponent();
            logger.LogInformation("Main form initialized.");
        }

        private void Init()
        {

        }

        #region  Init
        private void InitOptions()
        {
            logger.LogInformation("Initializing options...");
            var options = new List<KeyValuePair<string, string>>();

            cB_ListMediaType.Items.AddRange(options);
        }

        #endregion
    }
}
