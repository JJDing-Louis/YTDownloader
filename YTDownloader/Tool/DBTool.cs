using JJNET.DataAccess.Entity;
using JJNET.Utility.Tools;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJNET.DataAccess;

namespace YTDownloader.Tool
{
    public class DBTool
    {
        public DBTool() { }

        public static void InitDB() 
        {
            var Entities = new List<string>();
            using (var conn = ConnectionTool.GetConnection()) 
            {
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
                        var migrator = new EntityTableCreator(conn);
                        migrator.UpdateTableSchema(entity);
                    }
                }
            }
        }

        public static void WriteDownloadHistory() 
        {
        }
    }
}
