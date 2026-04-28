namespace YTDownloader.Model
{
    
    /// <summary>
    /// 類型判斷結果
    /// </summary>
    public class ResourceDetectionResult
    {
        /// <summary>
        /// 原始 URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 類型
        /// </summary>
        public UrlResourceType ResourceType { get; set; }

        /// <summary>
        /// 標題
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// 播放清單 ID
        /// </summary>
        public string? PlaylistId { get; set; }

        /// <summary>
        /// 播放清單標題
        /// </summary>
        public string? PlaylistTitle { get; set; }

        /// <summary>
        /// 播放清單數量
        /// </summary>
        public int? PlaylistCount { get; set; }

        /// <summary>
        /// Entries 數量
        /// </summary>
        public int? EntryCount { get; set; }

        /// <summary>
        /// 是否為播放清單
        /// </summary>
        public bool IsPlaylist => ResourceType == UrlResourceType.Playlist;

        /// <summary>
        /// 是否為單一影片
        /// </summary>
        public bool IsSingleVideo => ResourceType == UrlResourceType.SingleVideo;

        /// <summary>
        /// 結果訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

}
