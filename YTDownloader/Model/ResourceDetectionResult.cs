using YTDownloader.Service;

namespace YTDownloader.Model
{
    /// <summary>
    /// 類型判斷結果
    /// </summary>
    public class ResourceDetectionResult
    {
        private const string OptionListSourceType = "ListSourceType";

        /// <summary>
        /// 原始 URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 類型
        /// </summary>
        public string ResourceType { get; set; } = string.Empty;

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
        public bool IsPlaylist => ResourceType == GetSourceTypeName("播放清單");

        /// <summary>
        /// 是否為單一影片
        /// </summary>
        public bool IsSingleVideo => ResourceType == GetSourceTypeName("單一影片");

        /// <summary>
        /// 結果訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        public static string GetSourceTypeName(string desc)
        {
            return OptionService.GetOptionName(OptionListSourceType, desc);
        }
    }
}
