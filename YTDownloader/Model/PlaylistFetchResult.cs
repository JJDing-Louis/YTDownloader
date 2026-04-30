namespace YTDownloader.Model;

/// <summary>
///     播放清單讀取結果
/// </summary>
public class PlaylistFetchResult
{
    /// <summary>
    ///     是否成功取得播放清單
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    ///     播放清單 ID
    /// </summary>
    public string? PlaylistId { get; set; }

    /// <summary>
    ///     播放清單標題
    /// </summary>
    public string? PlaylistTitle { get; set; }

    /// <summary>
    ///     播放清單宣告的影片總數（來自 yt-dlp 的 playlist_count 欄位）。
    /// </summary>
    public int DeclaredCount { get; set; }

    /// <summary>
    ///     實際成功解析的影片數量。
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    ///     因不可播放（私人 / 刪除 / 地區限制）而跳過的條目描述，
    ///     例如 ["#113：[Deleted video]", "#116：[Private video]"]。
    /// </summary>
    public List<string> UnavailableEntries { get; set; } = new();

    /// <summary>因不可播放而跳過的數量。</summary>
    public int SkippedCount => UnavailableEntries.Count;

    /// <summary>
    ///     所有影片資訊清單（含 IsSelected 供 UI 勾選）
    /// </summary>
    public List<PlaylistVideoItem> Videos { get; set; } = new();

    /// <summary>
    ///     結果訊息（成功說明 / 錯誤原因）
    /// </summary>
    public string? Message { get; set; }

    // ── 靜態工廠 ────────────────────────────────────────

    public static PlaylistFetchResult Success(
        string? playlistId,
        string? playlistTitle,
        List<PlaylistVideoItem> videos,
        string? message = null)
    {
        return new PlaylistFetchResult
        {
            IsSuccess = true,
            PlaylistId = playlistId,
            PlaylistTitle = playlistTitle,
            TotalCount = videos.Count,
            Videos = videos,
            Message = message ?? $"成功載入 {videos.Count} 部影片"
        };
    }

    public static PlaylistFetchResult Fail(string message)
    {
        return new PlaylistFetchResult
        {
            IsSuccess = false,
            Videos = new List<PlaylistVideoItem>(),
            Message = message
        };
    }
}