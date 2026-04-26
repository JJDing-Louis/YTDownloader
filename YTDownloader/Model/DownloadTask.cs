namespace YTDownloader.Model
{
    /// <summary>
    /// 代表一筆目前仍在進行或等待重試的下載任務。
    /// </summary>
    public class DownloadTask
    {
        /// <summary>下載流水號，建立任務時自動產生。</summary>
        public int ID { get; set; }

        /// <summary>UI 任務流水號，用來對應畫面上的任務列。</summary>
        public int TaskID { get; set; }

        /// <summary>下載檔名或顯示標題。</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>媒體類型，例如 Audio / Video。</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>來源網址。</summary>
        public string URL { get; set; } = string.Empty;

        /// <summary>外部資源識別碼，例如 yt-dlp metadata id；目前可為空。</summary>
        public string? RID { get; set; }

        /// <summary>完成時間（ISO 8601 UTC 格式）。</summary>
        public string? CompleteDateTime { get; set; }

        /// <summary>開始下載時間（ISO 8601 UTC 格式）。</summary>
        public string? DownloadDateTime { get; set; }

        /// <summary>下載輸出路徑。</summary>
        public string? Path { get; set; }

        /// <summary>目前進度，例如 35.40。</summary>
        public string? Progress { get; set; }

        /// <summary>目前狀態，例如 下載中 / 完成 / 失敗 / 已取消。</summary>
        public string? Status { get; set; }
    }
}
