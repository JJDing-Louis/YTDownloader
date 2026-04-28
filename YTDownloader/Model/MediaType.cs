namespace YTDownloader.Model
{
    /// <summary>
    /// 下載的媒體類型。新增類型時，同步更新 MainForm.EnqueueDownloads() 的 switch。
    /// </summary>
    public enum MediaType
    {
        /// <summary>僅下載音訊（mp3 / m4a）。</summary>
        Audio,

        /// <summary>下載影片（含音訊）。</summary>
        Video,
    }
}
