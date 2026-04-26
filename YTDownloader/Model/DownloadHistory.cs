namespace YTDownloader.Model
{
    /// <summary>
    /// 代表一筆已完成下載的歷史紀錄。
    /// </summary>
    public class DownloadHistory
    {
        /// <summary>下載流水號，與 DownloadTask.ID 對應一致。</summary>
        public int ID { get; set; }

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

        /// <summary>執行結果，例如 Success / Failed。</summary>
        public string? Result { get; set; }
    }
}
