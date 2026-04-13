namespace YTDownloader.Model
{
    /// <summary>
    /// 批次下載結果
    /// </summary>
    public class BatchDownloadResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 輸出資料夾
        /// </summary>
        public string? OutputFolder { get; set; }

        /// <summary>
        /// 總下載筆數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 執行訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}