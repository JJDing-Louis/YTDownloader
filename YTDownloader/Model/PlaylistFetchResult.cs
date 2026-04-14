using System.Collections.Generic;

namespace YTDownloader.Model
{
    /// <summary>
    /// 播放清單讀取結果
    /// </summary>
    public class PlaylistFetchResult
    {
        /// <summary>
        /// 是否成功取得播放清單
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 播放清單 ID
        /// </summary>
        public string? PlaylistId { get; set; }

        /// <summary>
        /// 播放清單標題
        /// </summary>
        public string? PlaylistTitle { get; set; }

        /// <summary>
        /// 清單中的影片總數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 所有影片資訊清單（含 IsSelected 供 UI 勾選）
        /// </summary>
        public List<PlaylistVideoItem> Videos { get; set; } = new();

        /// <summary>
        /// 結果訊息（成功說明 / 錯誤原因）
        /// </summary>
        public string? Message { get; set; }

        // ── 靜態工廠 ────────────────────────────────────────

        public static PlaylistFetchResult Success(
            string? playlistId,
            string? playlistTitle,
            List<PlaylistVideoItem> videos,
            string? message = null) => new()
        {
            IsSuccess = true,
            PlaylistId = playlistId,
            PlaylistTitle = playlistTitle,
            TotalCount = videos.Count,
            Videos = videos,
            Message = message ?? $"成功載入 {videos.Count} 部影片"
        };

        public static PlaylistFetchResult Fail(string message) => new()
        {
            IsSuccess = false,
            Videos = new List<PlaylistVideoItem>(),
            Message = message
        };
    }
}
