using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using JJNET.Utility.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using System.Runtime.CompilerServices;
using YTDownloader;
using YTDownloader.Model;

namespace YTDownloaderTest.ToolTest
{
    public class DBToolTest
    {
        private string? originalCurrentDirectory;
        private string? originalEnvironment;
        private string? testDatabaseDirectory;

        [SetUp]
        public void Setup()
        {
            originalCurrentDirectory = Environment.CurrentDirectory;
            originalEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var testDirectory = TestContext.CurrentContext.TestDirectory;
            var seedDatabasePath = FindSeedDatabasePath(testDirectory);
            testDatabaseDirectory = Path.Combine(Path.GetTempPath(), "YTDownloaderTest", TestContext.CurrentContext.Test.ID);
            Directory.CreateDirectory(testDatabaseDirectory);
            var testDatabasePath = Path.Combine(testDatabaseDirectory, "JJNET.db");

            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            File.Copy(seedDatabasePath, testDatabasePath, overwrite: true);

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "YTDownloaderTest");
            Environment.CurrentDirectory = testDatabaseDirectory;
        }

        [TearDown]
        public void TearDown()
        {
            if (originalCurrentDirectory != null)
                Environment.CurrentDirectory = originalCurrentDirectory;

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalEnvironment);

            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (!string.IsNullOrWhiteSpace(testDatabaseDirectory) && Directory.Exists(testDatabaseDirectory))
                Directory.Delete(testDatabaseDirectory, recursive: true);
        }

        private static string FindSeedDatabasePath(string testDirectory)
        {
            var candidates = new[]
            {
                Path.GetFullPath(Path.Combine(testDirectory, "..", "..", "..", "JJNET.db")),
                Path.Combine(TestContext.CurrentContext.WorkDirectory, "JJNET.db"),
                Path.Combine(Environment.CurrentDirectory, "YTDownloaderTest", "JJNET.db"),
                Path.Combine(Environment.CurrentDirectory, "JJNET.db")
            };

            var seedDatabasePath = candidates.FirstOrDefault(File.Exists);
            if (seedDatabasePath == null)
                throw new FileNotFoundException("找不到測試用 JJNET.db。", string.Join(Environment.NewLine, candidates));

            return seedDatabasePath;
        }

        [Test]
        public void UpsertDownloadHistoryTest()
        {
            var downloadHistory = new DownloadHistory
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
                var result =  conn.QueryFirstOrDefault<DownloadHistory>(sqlcmd);
                Assert.That(result, Is.Not.Null);
            }
        }

        [Test]
        [Description("下載項目被取消時，DownloadHistory 狀態應更新為 Cancel 並保留取消當下進度")]
        public void UpdateDownloadProgress_CancelStatus_PersistsCancel()
        {
            const long taskId = 1001;
            InsertHistory(taskId, "InProgress", "35");

            YTDownloader.Tool.DBTool.UpdateDownloadProgress(taskId, 35, "Cancel");

            var result = GetHistory(taskId);
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Status, Is.EqualTo("Cancel"));
                Assert.That(result.Progress, Is.EqualTo("35"));
                Assert.That(result.CompleteDateTime, Is.Null);
            });
        }

        [Test]
        public void ClearTsqlLog_RemovesAllRows()
        {
            using (var conn = ConnectionTool.GetConnection())
            {
                conn.Execute("""
                             INSERT INTO TSQL_LOG (TS, TSQL, CLTID)
                             VALUES (@TS, @TSQL, @CLTID)
                             """, new { TS = DateTime.UtcNow, TSQL = "SELECT 1", CLTID = "test" });
            }

            Assert.That(CountRows("TSQL_LOG"), Is.GreaterThan(0));

            YTDownloader.Tool.DBTool.ClearTsqlLog();

            Assert.That(CountRows("TSQL_LOG"), Is.EqualTo(0));
        }

        [Test]
        public void ClearDownloadHistory_RemovesAllRows()
        {
            InsertHistory(2001, "Complete", "100");
            InsertHistory(2002, "Fail", "0");

            Assert.That(CountRows("DownloadHistory"), Is.GreaterThanOrEqualTo(2));

            YTDownloader.Tool.DBTool.ClearDownloadHistory();

            Assert.That(CountRows("DownloadHistory"), Is.EqualTo(0));
        }

        [Test]
        [Description("已取消的任務若後續背景流程又回報 InProgress，不應覆蓋資料庫中的 Cancel 狀態")]
        public void MainForm_UpdateDownloadProgress_AfterCancel_DoesNotOverwriteCancel()
        {
            const long taskId = 1002;
            InsertHistory(taskId, "InProgress", "42");
            var mainForm = CreateMainFormForCancellationTest();

            InvokePrivate(mainForm, "MarkDownloadCanceled", taskId, 42d);
            mainForm.UpdateDownloadProgress(taskId, 0, "InProgress");

            var result = GetHistory(taskId);
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Status, Is.EqualTo("Cancel"));
                Assert.That(result.Progress, Is.EqualTo("42"));
            });
        }

        private static MainForm CreateMainFormForCancellationTest()
        {
            var mainForm = (MainForm)RuntimeHelpers.GetUninitializedObject(typeof(MainForm));
            SetPrivateField(mainForm, "_canceledDownloadTaskIds", new HashSet<long>());
            SetPrivateField(mainForm, "_canceledDownloadTaskIdsLock", new object());
            SetPrivateField(mainForm, "_logger", NullLogger<MainForm>.Instance);
            return mainForm;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"找不到欄位 {fieldName}");
            field!.SetValue(target, value);
        }

        private static void InvokePrivate(object target, string methodName, params object[] args)
        {
            var method = target.GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"找不到方法 {methodName}");
            method!.Invoke(target, args);
        }

        private static void InsertHistory(long taskId, string status, string progress)
        {
            YTDownloader.Tool.DBTool.InsertDownloadHistory(new DownloadHistory
            {
                TaskID = taskId,
                Title = $"test-{taskId}",
                FileName = $"test-{taskId}.mp4",
                Type = "Video",
                URL = "https://example.com/video",
                DownloadDateTime = DateTime.UtcNow,
                Path = "/path/to/test.mp4",
                Status = status,
                Progress = progress
            });
        }

        private static DownloadHistory? GetHistory(long taskId)
        {
            using var conn = ConnectionTool.GetConnection();
            const string sqlcmd = """
                                  SELECT 
                                      * 
                                  FROM DownloadHistory
                                  WHERE TaskID = @TaskID
                                  """;
            return conn.QueryFirstOrDefault<DownloadHistory>(sqlcmd, new { TaskID = taskId });
        }

        private static int CountRows(string tableName)
        {
            using var conn = ConnectionTool.GetConnection();
            return conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM {tableName}");
        }
    }
}
