using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using JJNET.DataAccess.Entity;
using JJNET.Utility.Tools;
using YTDownloader.Startup;

namespace YTDownloader.Data
{
    internal class DatabaseInitializer : IAppInitializer
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseInitializer> _logger;

        #region InitSQL

        private string InitTEntity = """
                                     create table if not exists TEntity
                                     (
                                         MasterEntity     text               not null,
                                         SubEntity        text    default '' not null,
                                         HistLog          integer default 0  not null,
                                         AccessRestricted integer default 0  not null,
                                         AllowRID         integer default 1  not null,
                                         Description      text,
                                         Position         integer,
                                         constraint TEntity_PK
                                             primary key (MasterEntity, SubEntity)
                                     );
                                     """;
        private string InitTEntity_Columns = """
                                     create table if not exists TEntity_Columns
                                     (
                                         MasterEntity     text               not null,
                                         SubEntity        text    default '' not null,
                                         ColumnName       text               not null,
                                         DisplayType      text
                                             constraint TEntityColumn_TEntityDisplayType_FK
                                                 references TEntityDisplayType,
                                         DataType         text               not null,
                                         IsPK             integer default 0  not null,
                                         RefEntity        text,
                                         SecurityMask     text,
                                         Description      text,
                                         Position         integer,
                                         _LIST_IDX        numeric,
                                         ListSelectionIdx integer,
                                         constraint TEntityColumn_PK
                                             primary key (MasterEntity, SubEntity, ColumnName),
                                         constraint TEntityColumn_TEntity_FK
                                             foreign key (MasterEntity, SubEntity) references TEntity
                                     );
                                     """;
        private string InitTEntity_Mvt = """
                                     create table if not exists TEntity_Mvt
                                     (
                                         MasterEntity  text not null,
                                         SubEntity     text not null,
                                         _AUDIT_ID     integer,
                                         _AUDIT_ACTION text,
                                         _AUDIT_TS     blob,
                                         _AUDIT_TIME   text,
                                         _AUDIT_USER   text,
                                         constraint TEntity_Mvt_PK
                                             primary key (MasterEntity, SubEntity)
                                     );
                                     
                                     
                                     """;
        private string InitTEntityDisplayType = """
                                                create table if not exists TEntityDisplayType
                                                (
                                                    DisplayType text not null
                                                        constraint TEntityDisplayType_PK
                                                            primary key,
                                                    DataType    text
                                                );
                                                
                                                """;
        private string InitTSQL_LOG = """
                                      create table if not exists TSQL_LOG
                                      (
                                          ID                INTEGER
                                              primary key autoincrement,
                                          TS                DATETIME,
                                          ES                DATETIME,
                                          DUR               NUMERIC,
                                          USR               TEXT,
                                          WSID              TEXT,
                                          TSQL              TEXT,
                                          PARAM             TEXT,
                                          CALLER            TEXT,
                                          REC_CNT           INTEGER,
                                          ERR_MSG           TEXT,
                                          SP_ID             INTEGER,
                                          TX_ID             INTEGER,
                                          TIME_OUT          INTEGER,
                                          CLTID             TEXT not null,
                                          ACCESS_RESTRICTED INTEGER,
                                          REMOTE_IP         TEXT,
                                          CONTROLLER        TEXT,
                                          EVENT             TEXT,
                                          URL               TEXT,
                                          TEVENT_ID         INTEGER,
                                          ACTION            TEXT,
                                          FUN_DESC          TEXT,
                                          ROLES             TEXT,
                                          DEPUTY            TEXT,
                                          WEB_TRANSACTION   TEXT,
                                          CATALOG           TEXT
                                      );
                                      """;
        #endregion

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
            EnsureSystemTables(connectionString);
            EnsureSystemStaticData(connectionString);
            EnsureTEnitiyData(connectionString);
            UpdateTable();
            EnsureStaticData(connectionString);
        }
        
        private void EnsureDatabase(string connectionString)
        {
            _logger.LogInformation("Initializing database...");
            using var conn = ConnectionTool.GetConnection();
            conn.Open();
            _logger.LogInformation("Database ready.");
        }
        
        private void EnsureSystemTables(string connectionString)
        {
            _logger.LogInformation("Ensuring system tables...");

            var commands = new[]
            {
                InitTEntityDisplayType,
                InitTEntity,
                InitTEntity_Columns,
                InitTEntity_Mvt,
                InitTSQL_LOG
            };

            using (var conn = ConnectionTool.GetConnection())
            {
                foreach (var sql in commands)
                {
                    conn.Execute(sql);
                }
            }
            _logger.LogInformation("System tables ready.");
        }

        private void EnsureSystemStaticData(string connectionString)
        {
            var sql = """
                      INSERT OR IGNORE INTO TEntityDisplayType (DisplayType, DataType) VALUES ('TEXT', 'TEXT');
                      INSERT OR IGNORE INTO TEntityDisplayType (DisplayType, DataType) VALUES ('INTEGER', 'INTEGER');
                      INSERT OR IGNORE INTO TEntityDisplayType (DisplayType, DataType) VALUES ('DATETIME', 'DATETIME');
                      INSERT OR IGNORE INTO TEntityDisplayType (DisplayType, DataType) VALUES ('DOUBLE', 'REAL');
                      """;
            using (var conn = ConnectionTool.GetConnection())
            {
                conn.Execute(sql);
            }
        }
        
        /// <summary>
        /// 專案表格初始化流程
        /// </summary>
        /// <param name="connectionString"></param>
        private void EnsureTEnitiyData(string connectionString)
        {
            _logger.LogInformation("Ensuring tables...");

            var SQLTEntityData_ListMediaType = """
                                               INSERT OR IGNORE INTO TEntity (MasterEntity, SubEntity, HistLog, AccessRestricted, AllowRID, Description, Position) VALUES ('ListMediaType', '', 0, 1, 1, '媒體類型', 0);
                                               INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('ListMediaType', '', 'Name', 'TEXT', 'TEXT', 1, null, null, '名稱', 0, 0, null);
                                               INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('ListMediaType', '', 'Desc', 'TEXT', 'TEXT', 1, null, null, '敘述', 1, 0, null);
                                               """;
            var SQLTEntityData_ListSourceType = """
                                                INSERT OR IGNORE INTO TEntity (MasterEntity, SubEntity, HistLog, AccessRestricted, AllowRID, Description, Position) VALUES ('ListSourceType', '', 0, 1, 1, '來源類型', 1);
                                                INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('ListSourceType', '', 'Name', 'TEXT', 'TEXT', 1, null, null, '名稱', 0, 0, null);
                                                INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('ListSourceType', '', 'Desc', 'TEXT', 'TEXT', 1, null, null, '敘述', 1, 0, null);
                                                """;
            var SQLTEntityData_ListDownloadStatus = """
                                                    INSERT OR IGNORE INTO TEntity (MasterEntity, SubEntity, HistLog, AccessRestricted, AllowRID, Description, Position) VALUES ('ListDownloadStatus', '', 0, 1, 1, '下載狀態清單', 3);
                                                    INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('ListDownloadStatus', '', 'Name', 'TEXT', 'TEXT', 1, null, null, '名稱', 0, 0, null);
                                                    INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('ListDownloadStatus', '', 'Desc', 'TEXT', 'TEXT', 1, null, null, '敘述', 1, 0, null);
                                                    """;
            var SQLTEntityData_DownloadHistory = """
                                                 INSERT OR IGNORE INTO TEntity (MasterEntity, SubEntity, HistLog, AccessRestricted, AllowRID, Description, Position) VALUES ('DownloadHistory', '', 0, 1, 1, '下載歷史紀錄', 2);
                                                 INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('DownloadHistory', '', 'TaskID', 'INTEGER', 'INTEGER', 1, null, null, null, null, null, null);
                                                 INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('DownloadHistory', '', 'URL', 'TEXT', 'TEXT', 1, null, null, null, null, null, null);
                                                 INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('DownloadHistory', '', 'Status', 'TEXT', 'TEXT', 1, null, null, null, null, null, null);
                                                 INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('DownloadHistory', '', 'Title', 'TEXT', 'TEXT', 0, null, null, null, null, null, null);
                                                 INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('DownloadHistory', '', 'FileName', 'TEXT', 'TEXT', 0, null, null, null, null, null, null);
                                                 INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('DownloadHistory', '', 'Type', 'TEXT', 'TEXT', 0, null, null, null, null, null, null);
                                                 INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('DownloadHistory', '', 'DownloadDateTime', 'DATETIME', 'DATETIME', 0, null, null, null, null, null, null);
                                                 INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('DownloadHistory', '', 'CompleteDateTime', 'DATETIME', 'DATETIME', 0, null, null, null, null, null, null);
                                                 INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('DownloadHistory', '', 'Path', 'TEXT', 'TEXT', 0, null, null, null, null, null, null);
                                                 INSERT OR IGNORE INTO TEntity_Columns (MasterEntity, SubEntity, ColumnName, DisplayType, DataType, IsPK, RefEntity, SecurityMask, Description, Position, _LIST_IDX, ListSelectionIdx) VALUES ('DownloadHistory', '', 'Progress', 'DOUBLE', 'REAL', 0, null, null, null, null, null, null);
                                                 """;
            
            var commands = new[]{SQLTEntityData_ListSourceType, SQLTEntityData_ListMediaType, SQLTEntityData_ListDownloadStatus, SQLTEntityData_DownloadHistory};

            using (var conn = ConnectionTool.GetConnection())
            {
                foreach (var sql in commands)
                {
                    conn.Execute(sql);
                }
            }
            _logger.LogInformation("Tables ready.");
        }

        private void UpdateTable()
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
                var result = conn.Query<string>(sqlcmd);
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

        private void EnsureStaticData(string connectionString) 
        {
            _logger.LogInformation("Ensuring Static Data...");

            var commands = new[]
            {
                """
                INSERT OR IGNORE INTO ListMediaType (Name, "Desc") VALUES ('Audio', '音訊');
                INSERT OR IGNORE INTO ListMediaType (Name, "Desc") VALUES ('Video', '視訊');
                """,
                """
                INSERT OR IGNORE INTO ListSourceType (Name, Desc) VALUES ('SingleVideo', '單一影片');
                INSERT OR IGNORE INTO ListSourceType (Name, Desc) VALUES ('PlayList', '播放清單');
                """,
                """
                INSERT OR IGNORE INTO ListDownloadStatus (Name, Desc) VALUES ('Waiting', '等待中');
                INSERT OR IGNORE INTO ListDownloadStatus (Name, Desc) VALUES ('InProgress', '下載中');
                INSERT OR IGNORE INTO ListDownloadStatus (Name, Desc) VALUES ('Complete', '完成');
                INSERT OR IGNORE INTO ListDownloadStatus (Name, Desc) VALUES ('Fail', '失敗');
                INSERT OR IGNORE INTO ListDownloadStatus (Name, Desc) VALUES ('Pause', '已暫停');
                INSERT OR IGNORE INTO ListDownloadStatus (Name, Desc) VALUES ('Cancel', '已取消');
                """
            };

            using (var conn = ConnectionTool.GetConnection())
            {
                foreach (var sql in commands)
                {
                    conn.Execute(sql);
                }
            }
            _logger.LogInformation("Static Data.");
        }
    }
}
