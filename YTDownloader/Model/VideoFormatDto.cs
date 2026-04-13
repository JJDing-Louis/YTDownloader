using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTDownloader.Model
{
    /// <summary>
    /// 可用格式資訊
    /// </summary>
    public class VideoFormatDto
    {
        /// <summary>
        /// 格式 ID
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// 副檔名
        /// </summary>
        public string? Extension { get; set; }

        /// <summary>
        /// 解析度
        /// </summary>
        public string? Resolution { get; set; }

        /// <summary>
        /// 視訊編碼
        /// </summary>
        public string? VCodec { get; set; }

        /// <summary>
        /// 音訊編碼
        /// </summary>
        public string? ACodec { get; set; }

        /// <summary>
        /// 格式說明
        /// </summary>
        public string? FormatNote { get; set; }
    }
}
