using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                CompleteDateTime = DateTime.UtcNow.ToString("o"),
                DownloadDateTime = DateTime.UtcNow.ToString("o"),
                Path = "/path/to/test.mp4",
                Result = "Success"
            };
            YTDownloader.Tool.DBTool.UpsertDownloadHistory(downloadHistory);
            // 這裡可以加入查詢資料庫的程式碼來驗證資料是否正確插入或更新
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
                DownloadDateTime = DateTime.UtcNow.ToString("o"),
                CompleteDateTime = DateTime.UtcNow.ToString("o")
            };
            YTDownloader.Tool.DBTool.UpsertDownloadTask(downloadTask);
            // 這裡可以加入查詢資料庫的程式碼來驗證資料是否正確插入或更新

        }
    }
}
