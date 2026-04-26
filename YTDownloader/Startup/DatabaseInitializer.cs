using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.SQLite;
using YTDownloader.Startup;
using YTDownloader.Tool;

namespace YTDownloader.Data
{
    internal class DatabaseInitializer : IAppInitializer
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void Initialize()
        {
            string connectionString = _config.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' not found.");

            EnsureDatabase(connectionString);
            EnsureSchema(connectionString);
        }

        private void EnsureDatabase(string connectionString)
        {
            _logger.LogInformation("Initializing database...");
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            _logger.LogInformation("Database ready.");
        }

        private void EnsureSchema(string connectionString)
        {
            _logger.LogInformation("Ensuring database schema...");

            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            DownloadRecordSchema.EnsureInitialized(conn, _logger);

            _logger.LogInformation("Database schema ready.");
        }
    }
}
