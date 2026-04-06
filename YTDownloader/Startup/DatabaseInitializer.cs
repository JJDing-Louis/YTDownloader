using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.SQLite;
using YTDownloader.Startup;

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
            EnsureTables(connectionString);
            EnsureStaticData(connectionString);

        }

        private void EnsureDatabase(string connectionString)
        {
            _logger.LogInformation("Initializing database...");
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            _logger.LogInformation("Database ready.");
        }

        private void EnsureTables(string connectionString)
        {
            _logger.LogInformation("Ensuring tables...");

            var commands = new[]
            {
                """
                CREATE TABLE IF NOT EXISTS DownloadHistory (
                    ID                INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    FileName    TEXT    NOT NULL,  -- 下載的檔案名稱
                    URL             TEXT    NOT NULL,  -- 來源網址
                    Type            TEXT,             -- 檔案類型，例如 video / audio
                    Path             TEXT,             -- 本地儲存路徑
                    DownloadDateTime TEXT,             -- 開始下載時間 (ISO 8601)
                    CompleteDateTime TEXT,              -- 完成下載時間 (ISO 8601)
                    Resulr          TEXT           -- 下載結果，例如 Success / Failed
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS ListMediaType (
                    Name         TEXT NOT NULL PRIMARY KEY,
                    Desc         TEXT 
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS ListSourceType (
                    Name         TEXT NOT NULL PRIMARY KEY,
                    Desc         TEXT 
                );
                """,
            };

            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            foreach (var sql in commands)
            {
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }

            _logger.LogInformation("Tables ready.");
        }

        private void EnsureStaticData(string connectionString) 
        {
            _logger.LogInformation("Ensuring Static Data...");

            var commands = new[]
            {
                """
                INSERT INTO ListMediaType (Name, "Desc") VALUES ('Audio', '音訊')
                    ON CONFLICT(Name) DO UPDATE SET "Desc" = excluded."Desc";

                INSERT INTO ListMediaType (Name, "Desc") VALUES ('Video', '視訊')
                    ON CONFLICT(Name) DO UPDATE SET "Desc" = excluded."Desc";
                """,
                """
                INSERT INTO ListSourceType (Name, Desc) VALUES ('VideoOnly', '單一影片')
                ON CONFLICT(Name) DO UPDATE SET "Desc" = excluded."Desc";

                INSERT INTO ListSourceType (Name, Desc) VALUES ('PlayList', '播放清單')
                ON CONFLICT(Name) DO UPDATE SET "Desc" = excluded."Desc";
                """
            };

            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            foreach (var sql in commands)
            {
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }

            _logger.LogInformation("Static Data.");
        }
    }
}
