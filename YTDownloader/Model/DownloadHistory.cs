namespace YTDownloader.Model;

/// <summary>
///     代表一筆已完成下載的歷史紀錄。
/// </summary>
public class DownloadHistory
{
    /// <summary>下載流水號，與 DownloadTask.ID 對應一致。</summary>
    public long TaskID { get; set; }

    /// <summary>來源網址。</summary>
    public string URL { get; set; } = string.Empty;

    /// <summary>下載狀態。</summary>
    public string Status { get; set; } = string.Empty;

    public string? RID { get; set; }

    /// <summary> 顯示標題。</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>下載檔名。</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>下載輸出路徑。</summary>
    public string? Path { get; set; }

    /// <summary>下載進度。</summary>
    public string? Progress { get; set; }

    /// <summary>媒體類型，例如 Audio / Video。</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>完成時間（ISO 8601 UTC 格式）。</summary>
    public DateTime? CompleteDateTime { get; set; }

    /// <summary>開始下載時間（ISO 8601 UTC 格式）。</summary>
    public DateTime? DownloadDateTime { get; set; }
}