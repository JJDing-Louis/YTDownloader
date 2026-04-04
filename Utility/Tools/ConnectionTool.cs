using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Utility.Tools
{
    public class ConnectionTool
    {
        public static QueryFactory JJNET => new QueryFactory(GetConnection(), new SqlServerCompiler());

        public static SqlConnection GetConnection(string name = "Default", bool DefaultOpenConnection = true, bool MultipleActiveResultSets = true)
        {
            name ??= "Default";
            var conf = ParameterTool.GetConfiguration();
            SqlConnection conn = null;
            var retryCount = 3;
            while (retryCount >= 0)
                try
                {
                    var connectionString = conf.GetConnectionString(name);
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        retryCount = 0;
                        throw new Exception($"missing database connection string definition in appsettings.json {name}");
                    }

                    connectionString = ConfigurationProvider.DecryptSQLConnectionString(connectionString);

                    var builder = new SqlConnectionStringBuilder(connectionString);
                    var pw = builder.Password;
                    builder.MultipleActiveResultSets = MultipleActiveResultSets;

                    conn = new SqlConnection(builder.ConnectionString);
                    if (DefaultOpenConnection)
                        conn.Open();
                    break;
                }
                catch (Exception ex)
                {
                    retryCount--;
                    if (retryCount <= 0)
                        throw;
                    try
                    {
                        Thread.Sleep(5_000);
                    }
                    catch (Exception e)
                    {
                    }
                }

            string who;

            //login ID
            who = Thread.CurrentPrincipal?.Identity?.Name;
            if (string.IsNullOrEmpty(who))
                who = SecurityTool.GetLoginID();

            if (!string.IsNullOrEmpty(who))
                SqlMapper.Execute(conn, "sp_set_session_context 'sp_user', @who ,1", new { who });

            //代理人
            var user = SecurityTool.GetUserID();
            if (!string.IsNullOrEmpty(user))
                SqlMapper.Execute(conn, "sp_set_session_context 'sp_user2', @user ,1", new { user });

            return conn;
        }
    }
}
