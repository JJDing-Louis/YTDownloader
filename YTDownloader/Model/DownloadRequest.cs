namespace YTDownloader.Model
{
    /// <summary>
    /// 代表一筆待下載任務的資料，由 PlaylistHandler 建立後交給 Main 執行。
    /// </summary>
    public class DownloadRequest
    {
        /// <summary>顯示於下載清單的標題。</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>影片頁面網址（yt-dlp 下載來源）。</summary>
        public string WebpageUrl { get; set; } = string.Empty;

        /// <summary>顯示用媒體類型，如 "音訊" 或 "視訊"。</summary>
        public string MediaTypeDisplay { get; set; } = string.Empty;

        /// <summary>true = 下載音訊，false = 下載視訊。</summary>
        public bool IsAudio { get; set; }

        /// <summary>儲存目錄的完整路徑。</summary>
        public string DownloadDir { get; set; } = string.Empty;
    }
}
