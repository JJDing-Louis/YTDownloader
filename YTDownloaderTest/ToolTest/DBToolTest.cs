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
        [Test]
        public void UpsertDownloadHistoryTest()
        {
            var downloadHistory = new YTDownloader.Model.DownloadHistory
            {
                ID = 1,
                FileName = "test.mp4",
                Type = "Video",
                URL = "https://example.com/video",
                CompleteDateTime = DateTime.UtcNow,
                DownloadDateTime = DateTime.UtcNow,
                Path = "/path/to/test.mp4",
                Result = "Success"
            };
            YTDownloader.Tool.DBTool.UpsertDownloadHistory(downloadHistory);
            // 這裡可以加入查詢資料庫的程式碼來驗證資料是否正確插入或更新
            
            //驗證
            using (var conn = ConnectionTool.GetConnection())
            {
                var sqlcmd = """
                             SELECT 
                                 * 
                             FROM DownloadHistory
                             WHERE ID = 1
                             """;
                var result =  conn.QueryFirstOrDefault<YTDownloader.Model.DownloadHistory>(sqlcmd);
                Assert.That(result, Is.Not.Null);
            }
        }

        [Test]
        public void UpsertDownloadTaskTest()
        {
            var downloadTask = new YTDownloader.Model.DownloadTask
            {
                ID = 1,
                URL = "https://example.com/video",
                Type = "Video",
                Status = "Pending",
                DownloadDateTime = DateTime.UtcNow,
                CompleteDateTime = DateTime.UtcNow
            };
            YTDownloader.Tool.DBTool.UpsertDownloadTask(downloadTask);
            // 這裡可以加入查詢資料庫的程式碼來驗證資料是否正確插入或更新

        }
    }
}
