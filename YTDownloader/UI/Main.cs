using Autofac;
using Microsoft.Extensions.Logging;

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
    }
}
