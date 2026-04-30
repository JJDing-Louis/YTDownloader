namespace YTDownloader.Model
{
    /// <summary>
    /// 代表一筆待下載任務的資料，由 PlaylistHandlerForm 建立後交給 MainForm 執行。
    /// </summary>
    public class DownloadRequest
    {
        /// <summary>顯示於下載清單的標題。</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>預計輸出的下載檔名。</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>影片頁面網址（yt-dlp 下載來源）。</summary>
        public string WebpageUrl { get; set; } = string.Empty;

        /// <summary>顯示用媒體類型，如 "音訊" 或 "視訊"。</summary>
        public string MediaTypeDisplay { get; set; } = string.Empty;

        /// <summary>下載的媒體類型 Name，如 "Audio" 或 "Video"。</summary>
        public string MediaType { get; set; } = string.Empty;

        /// <summary>儲存目錄的完整路徑。</summary>
        public string DownloadDir { get; set; } = string.Empty;
    }
}
