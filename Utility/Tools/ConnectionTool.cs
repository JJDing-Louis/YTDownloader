using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SqlKata.Compilers;
using SqlKata.Execution;
using Microsoft.Data.Sqlite;

namespace Utility.Tools
{
    public enum DbProvider
    {
        SqlServer,
        SQLite
    }

    public class ConnectionTool
    {
        public static QueryFactory JJNET => GetQueryFactory("Default");

        public static QueryFactory GetQueryFactory(string name = "Default")
        {
            var provider = ResolveProvider(name);
            var conn = GetConnection(name);

            Compiler compiler;
            switch (provider)
            {
                case DbProvider.SqlServer:
                    compiler = new SqlServerCompiler();
                    break;
                case DbProvider.SQLite:
                    compiler = new SqliteCompiler();
                    break;
                default:
                    throw new NotSupportedException($"不支援的資料庫類型：{provider}。");
            }

            return new QueryFactory(conn, compiler);
        }

        public static IDbConnection GetConnection(
            string name = "Default",
            bool openConnection = true)
        {
            name ??= "Default";
            var conf = ParameterTool.GetConfiguration();
            var rawConnStr = conf.GetConnectionString(name);

            if (string.IsNullOrEmpty(rawConnStr))
                throw new Exception($"missing database connection string definition in appsettings.json: {name}");

            var provider = ResolveProvider(name);

            switch (provider)
            {
                case DbProvider.SqlServer:
                    return OpenSqlServer(rawConnStr, openConnection);
                case DbProvider.SQLite:
                    return OpenSQLite(rawConnStr, openConnection);
                default:
                    throw new NotSupportedException($"不支援的資料庫類型：{provider}。");
            }
        }

        // ── private helpers ──────────────────────────────────────────────

        /// <summary>
        /// 從 appsettings.json 讀取 "DBType" 決定資料庫類型。
        /// 範例：{ "DBType": "SQLite" }
        /// </summary>
        private static DbProvider ResolveProvider(string _ = null)
        {
            var conf = ParameterTool.GetConfiguration();
            var dbType = conf["DBType"];

            if (string.IsNullOrWhiteSpace(dbType))
                throw new Exception("appsettings.json 設定錯誤：缺少 \"DBType\" 欄位，請指定資料庫類型（例如 SqlServer 或 SQLite）。");

            if (!Enum.TryParse<DbProvider>(dbType, ignoreCase: true, out var provider))
                throw new NotSupportedException($"不支援的資料庫類型：\"{dbType}\"，目前支援：{string.Join(", ", Enum.GetNames<DbProvider>())}。");

            return provider;
        }

        private static IDbConnection OpenSqlServer(string rawConnStr, bool open)
        {
            SqlConnection conn = null;
            var retryCount = 3;

            while (retryCount >= 0)
                try
                {
                    // DecryptSQLConnectionString 內部用 SqlConnectionStringBuilder，僅限 SQL Server
                    var decrypted = ConfigurationProvider.DecryptSQLConnectionString(rawConnStr);
                    var builder = new SqlConnectionStringBuilder(decrypted)
                    {
                        MultipleActiveResultSets = true
                    };
                    conn = new SqlConnection(builder.ConnectionString);
                    if (open) conn.Open();
                    break;
                }
                catch
                {
                    retryCount--;
                    if (retryCount <= 0) throw;
                    try { Thread.Sleep(5_000); } catch { }
                }

            // SQL Server 專屬：設定 session context
            var who = Thread.CurrentPrincipal?.Identity?.Name ?? SecurityTool.GetLoginID();
            if (!string.IsNullOrEmpty(who))
                SqlMapper.Execute(conn, "sp_set_session_context 'sp_user', @who, 1", new { who });

            var user = SecurityTool.GetUserID();
            if (!string.IsNullOrEmpty(user))
                SqlMapper.Execute(conn, "sp_set_session_context 'sp_user2', @user, 1", new { user });

            return conn;
        }

        private static IDbConnection OpenSQLite(string rawConnStr, bool open)
        {
            var conn = new SqliteConnection(rawConnStr);
            if (open) conn.Open();
            return conn;
        }
    }
}
