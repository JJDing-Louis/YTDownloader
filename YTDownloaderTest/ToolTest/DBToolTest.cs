using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using JJNET.Utility.Tools;

namespace YTDownloaderTest.ToolTest
{
    public class DBToolTest
    {
        private string? originalCurrentDirectory;
        private string? originalEnvironment;

        [SetUp]
        public void Setup()
        {
            originalCurrentDirectory = Environment.CurrentDirectory;
            originalEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var testDirectory = TestContext.CurrentContext.TestDirectory;
            var seedDatabasePath = Path.GetFullPath(Path.Combine(testDirectory, "..", "..", "..", "JJNET.db"));
            var testDatabasePath = Path.Combine(testDirectory, "JJNET.db");

            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            File.Copy(seedDatabasePath, testDatabasePath, overwrite: true);

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "YTDownloaderTest");
            Environment.CurrentDirectory = testDirectory;
        }

        [TearDown]
        public void TearDown()
        {
            if (originalCurrentDirectory != null)
                Environment.CurrentDirectory = originalCurrentDirectory;

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalEnvironment);
        }

        [Test]
        public void UpsertDownloadHistoryTest()
        {
            var downloadHistory = new YTDownloader.Model.DownloadHistory
            {
                TaskID = 1,
                Title = "test",
                FileName = "test.mp4",
                RID = "test-rid",
                Type = "Video",
                URL = "https://example.com/video",
                CompleteDateTime = DateTime.UtcNow,
                DownloadDateTime = DateTime.UtcNow,
                Path = "/path/to/test.mp4",
                Status = "Success"
            };
            YTDownloader.Tool.DBTool.InsertDownloadHistory(downloadHistory);
            // 這裡可以加入查詢資料庫的程式碼來驗證資料是否正確插入或更新
            
            //驗證
            using (var conn = ConnectionTool.GetConnection())
            {
                var sqlcmd = """
                             SELECT 
                                 * 
                             FROM DownloadHistory
                             WHERE TaskID = 1
                             """;
                var result =  conn.QueryFirstOrDefault<YTDownloader.Model.DownloadHistory>(sqlcmd);
                Assert.That(result, Is.Not.Null);
            }
        }
        
    }
}
