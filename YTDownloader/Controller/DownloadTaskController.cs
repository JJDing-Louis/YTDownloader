using YTDownloader.Model;

namespace YTDownloader.Controller
{
    /// <summary>
    /// 管理單一下載任務的生命週期：取消 / 暫停 / 繼續。
    /// </summary>
    public class DownloadTaskController
    {
        /// <summary>
        /// 取消權杖來源。
        /// 暫停時呼叫 <see cref="CancellationTokenSource.Cancel"/>；
        /// 繼續時重新建立新的實例。
        /// </summary>
        public CancellationTokenSource Cts { get; set; } = new();

        /// <summary>是否目前處於暫停狀態。</summary>
        public bool IsPaused { get; set; }

        /// <summary>最後一次紀錄的下載百分比（0–100），供「繼續」後維持進度顯示。</summary>
        public double LastPercent { get; set; }

        /// <summary>
        /// 以新的 <see cref="CancellationToken"/> 重新啟動下載的工作方法。
        /// 由 PlaylistHandler 在發起下載時傳入，Main 在「繼續」時呼叫。
        /// </summary>
        public Func<CancellationToken, Task> RestartAction { get; set; } = null!;

        /// <summary>
        /// 原始下載請求資訊（標題、輸出目錄、媒體類型）。
        /// 供「繼續 / 重試」前清除暫存檔使用。
        /// </summary>
        public DownloadRequest? OriginalRequest { get; set; }
    }
}
