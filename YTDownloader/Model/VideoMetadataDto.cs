using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTDownloader.Model
{
    /// <summary>
    /// 影片 Metadata
    /// </summary>
    public class VideoMetadataDto
    {
        /// <summary>
        /// 影片唯一識別碼
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// 影片標題
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// 上傳者 / 頻道名稱
        /// </summary>
        public string? Uploader { get; set; }

        /// <summary>
        /// 長度（通常為秒）
        /// </summary>
        public object? Duration { get; set; }

        /// <summary>
        /// 縮圖 URL
        /// </summary>
        public string? Thumbnail { get; set; }

        /// <summary>
        /// 原始頁面 URL
        /// </summary>
        public string? WebpageUrl { get; set; }
    }
}
