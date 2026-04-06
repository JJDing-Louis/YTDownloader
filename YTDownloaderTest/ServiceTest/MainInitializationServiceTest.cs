using Autofac;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using YTDownloader;
using YTDownloader.Service;

namespace YTDownloaderTest.ServiceTest
{
    public class MainInitializationServiceTest
    {
        public MainInitializationService TestedService;
        private static string _dbPath;

        [OneTimeSetUp]
        public static void OneTimeSetup()
        {
            Program.Startup.Run();

            var config = Program.Startup.Container.Resolve<IConfiguration>();
            var connStr = config.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Missing connection string 'Default' in appsettings.json");

            _dbPath = new SqliteConnectionStringBuilder(connStr).DataSource;
        }

        [OneTimeTearDown]
        public static void OneTimeTeardown()
        {
            SqliteConnection.ClearAllPools();
            if (_dbPath != null && _dbPath != ":memory:" && File.Exists(_dbPath))
                File.Delete(_dbPath);
        }

        [SetUp]
        public void Setup()
        {
            TestedService = new();
        }

        [Test]
        public void GetListMediaTypeTest()
        {
            var result = TestedService.GetListMediaType();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void GetListSourceTypeTest()
        {
            var result = TestedService.GetListSourceType();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
        }
    }
}
