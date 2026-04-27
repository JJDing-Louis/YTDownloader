

using JJNET.DataAccess.Entity;
using JJNET.Utility.Tools;
using System.Data.Common;
using YTDownloader.Model;

namespace YTDownloader.Tool
{
    public class DBTool
    {
        public DBTool() { }

        public static void InitDB() 
        {
            var Entities = new List<string>();
            DbConnectionStringBuilder ConnectionStringBuilder = new();

            using (var conn = ConnectionTool.GetConnection()) 
            {
                ConnectionStringBuilder.ConnectionString = conn.ConnectionString;
                var sqlcmd = """
                                     SELECT 
                                         MasterEntity 
                                     FROM TEntity
                                     WHERE SubEntity = ''
                                     """;
                var result = DapperSqlMapperExt.Query<string>(conn, sqlcmd);
                if (result != null) 
                        Entities.AddRange(result);

                if (Entities.Count > 0)
                {
                    foreach (var entity in Entities)
                    {
                        var migrator = new EntityTableCreator(ConnectionStringBuilder);
                        migrator.UpdateTableSchema(entity);
                    }
                }
            }
        }

        public static void InsertDownloadHistory(DownloadHistory downloadHistory)
        {
            using (var conn = ConnectionTool.GetConnection())
            {
                conn.InsertOrUpdate("DownloadHistory",downloadHistory);
            }
        }
        
        public static void UpdateDownloadProgress(long TaskID,int Progress,string Status,DateTime? CompleteDateTime=null)
        {
            using (var conn = ConnectionTool.GetConnection())
            {
                var sqlcmd = """
                                     UPDATE DownloadHistory
                                     SET Progress = @Progress, 
                                         Status = @Status, 
                                         CompleteDateTime = @CompleteDateTime
                                     WHERE TaskID = @TaskID
                                     """;
                var param = new { Progress, Status, CompleteDateTime, TaskID };
                DapperSqlMapperExt.Execute(conn, sqlcmd, param);
            }
        }
    }
}
