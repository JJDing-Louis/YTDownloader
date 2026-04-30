using JJNET.DataAccess.Entity;
using JJNET.Utility.Tools;
using YTDownloader.Model;

namespace YTDownloader.Tool;

public class DBTool
{
    public static void InsertDownloadHistory(DownloadHistory downloadHistory)
    {
        using (var conn = ConnectionTool.GetConnection())
        {
            conn.InsertOrUpdate("DownloadHistory", downloadHistory);
        }
    }

    public static void UpdateDownloadProgress(long TaskID, int Progress, string Status,
        DateTime? CompleteDateTime = null)
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
            conn.Execute(sqlcmd, param);
        }
    }
}