namespace YTDownloader.Model;

/// <summary>
///     播放清單中的單一影片資訊，供 UI 勾選使用
/// </summary>
public class PlaylistVideoItem
{
    /// <summary>
    ///     在播放清單中的順序（從 1 開始）
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    ///     影片唯一識別碼
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    ///     影片標題
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    ///     上傳者 / 頻道名稱
    /// </summary>
    public string? Uploader { get; set; }

    /// <summary>
    ///     影片長度（秒）
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    ///     縮圖 URL
    /// </summary>
    public string? Thumbnail { get; set; }

    /// <summary>
    ///     影片頁面 URL（用於下載）
    /// </summary>
    public string? WebpageUrl { get; set; }

    /// <summary>
    ///     UI 勾選狀態，預設為已勾選
    /// </summary>
    public bool IsSelected { get; set; } = true;

    /// <summary>
    ///     格式化後的時間長度字串（mm:ss 或 hh:mm:ss），供 UI 顯示用
    /// </summary>
    public string DurationString
    {
        get
        {
            if (Duration == null || Duration <= 0)
                return "--:--";

            var ts = TimeSpan.FromSeconds(Duration.Value);

            return ts.TotalHours >= 1
                ? ts.ToString(@"hh\:mm\:ss")
                : ts.ToString(@"mm\:ss");
        }
    }

    /// <summary>
    ///     顯示用標題（若標題為空則顯示 ID）
    /// </summary>
    public string DisplayTitle =>
        !string.IsNullOrWhiteSpace(Title) ? Title : Id ?? "(未知標題)";

    /// <summary>
    ///     回傳此物件的深複製。
    ///     <para>
    ///         由於所有屬性均為不可變字串或值型別，成員複製即等同深複製。
    ///     </para>
    /// </summary>
    public PlaylistVideoItem Clone()
    {
        return new PlaylistVideoItem
        {
            Index = Index,
            Id = Id,
            Title = Title,
            Uploader = Uploader,
            Duration = Duration,
            Thumbnail = Thumbnail,
            WebpageUrl = WebpageUrl,
            IsSelected = IsSelected
        };
    }
}

/// <summary>
///     <see cref="List{T}" /> 的 <see cref="PlaylistVideoItem" /> 擴充方法。
/// </summary>
public static class PlaylistVideoItemListExtensions
{
    /// <summary>
    ///     回傳清單的深複製（每個元素各自呼叫 <see cref="PlaylistVideoItem.Clone" />）。
    /// </summary>
    public static List<PlaylistVideoItem> DeepClone(this List<PlaylistVideoItem> source)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        return source.Select(v => v.Clone()).ToList();
    }
}