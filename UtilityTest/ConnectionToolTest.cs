using System.Data;
using System.Reflection;
using SqlKata.Compilers;
using Utility.Tools;

namespace UtilityTest
{
    /// <summary>
    /// ConnectionTool 單元測試。
    ///
    /// 情境檔說明：
    ///   appsettings.json            — 基底，無 DBType（用於測試 DBType 缺少的情境）
    ///   appsettings.SQLiteTest.json — DBType=SQLite，連接 :memory:
    ///   appsettings.UnsupportedDB.json — DBType=MySQL（不支援）
    ///
    /// ParameterTool 有 static config cache，每個測試前後皆清除，
    /// 並透過 ASPNETCORE_ENVIRONMENT 切換不同 appsettings。
    /// </summary>
    [TestFixture]
    [NonParallelizable]
    public class ConnectionToolTest
    {
        private string? _originalEnv;

        [SetUp]
        public void Setup()
        {
            _originalEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            ClearConfigCache();
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", _originalEnv);
            ClearConfigCache();
        }

        // ── ResolveProvider 錯誤情境 ────────────────────────────────────

        [Test]
        public void GetConnection_DBTypeMissing_ThrowsException()
        {
            // appsettings.json 基底無 DBType，appsettings.NoDB.json 不存在（optional）
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "NoDB");

            var ex = Assert.Throws<Exception>(() => ConnectionTool.GetConnection());
            Assert.That(ex!.Message, Does.Contain("DBType"));
        }

        [Test]
        public void GetConnection_DBTypeUnsupported_ThrowsNotSupportedException()
        {
            // appsettings.UnsupportedDB.json: DBType = "MySQL"
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "UnsupportedDB");

            var ex = Assert.Throws<NotSupportedException>(() => ConnectionTool.GetConnection());
            Assert.That(ex!.Message, Does.Contain("MySQL"));
        }

        // ── SQLite 正常情境 ─────────────────────────────────────────────

        [Test]
        public void GetConnection_SQLite_ReturnsSQLiteConnection()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "SQLiteTest");

            using var conn = ConnectionTool.GetConnection();

            Assert.That(conn.GetType().Name, Is.EqualTo("SqliteConnection"));
            Assert.That(conn.State, Is.EqualTo(ConnectionState.Open));
        }

        [Test]
        public void GetConnection_SQLite_OpenFalse_ReturnsClosedConnection()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "SQLiteTest");

            using var conn = ConnectionTool.GetConnection(openConnection: false);

            Assert.That(conn.State, Is.EqualTo(ConnectionState.Closed));
        }

        [Test]
        public void GetQueryFactory_SQLite_ReturnsQueryFactoryWithSqliteCompiler()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "SQLiteTest");

            var qf = ConnectionTool.GetQueryFactory();

            Assert.That(qf, Is.Not.Null);
            Assert.That(qf.Compiler, Is.InstanceOf<SqliteCompiler>());
        }

        // ── 工具方法 ────────────────────────────────────────────────────

        /// <summary>
        /// 清除 ParameterTool 的 static config cache，確保每個測試都重新讀取 appsettings。
        /// </summary>
        private static void ClearConfigCache()
        {
            var field = typeof(ParameterTool).GetField(
                "configs", BindingFlags.NonPublic | BindingFlags.Static);
            var dict = field?.GetValue(null) as System.Collections.IDictionary;
            dict?.Clear();
        }
    }
}
