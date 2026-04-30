namespace YTDownloader.Model;

/// <summary>
///     自有 metadata DTO
/// </summary>
public class YtDlpMetadata
{
    /// <summary>
    ///     影片唯一識別碼
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    ///     標題
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    ///     上傳者 / 頻道名稱
    /// </summary>
    public string? Uploader { get; set; }

    /// <summary>
    ///     秒數
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    ///     縮圖 URL
    /// </summary>
    public string? Thumbnail { get; set; }

    /// <summary>
    ///     原始頁面 URL
    /// </summary>
    public string? WebpageUrl { get; set; }

    /// <summary>
    ///     播放清單 ID
    /// </summary>
    public string? PlaylistId { get; set; }

    /// <summary>
    ///     播放清單標題
    /// </summary>
    public string? PlaylistTitle { get; set; }

    /// <summary>
    ///     播放清單總數
    /// </summary>
    public int? PlaylistCount { get; set; }

    /// <summary>
    ///     Entries 數量
    /// </summary>
    public int? EntryCount { get; set; }

    /// <summary>
    ///     是否為播放清單
    /// </summary>
    public bool IsPlaylist =>
        (EntryCount ?? 0) > 0 ||
        !string.IsNullOrWhiteSpace(PlaylistId) ||
        !string.IsNullOrWhiteSpace(PlaylistTitle) ||
        (PlaylistCount ?? 0) > 0;
}