using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTDownloader.Model
{
    /// <summary>
    /// 下載結果模型
    /// </summary>
    public class DownloadResult
    {
        /// <summary>
        /// 是否下載成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 原始影片 URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 下載輸出資料夾路徑
        /// </summary>
        /// <remarks>
        /// 成功時才會有值，失敗時通常為 null
        /// </remarks>
        public string? OutputFolder { get; set; }

        /// <summary>
        /// 執行結果訊息（成功或錯誤訊息）
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 建立成功結果
        /// </summary>
        public static DownloadResult Success(string url, string outputFolder, string message)
        {
            return new DownloadResult
            {
                IsSuccess = true,
                Url = url,
                OutputFolder = outputFolder,
                Message = message
            };
        }

        /// <summary>
        /// 建立失敗結果
        /// </summary>
        public static DownloadResult Fail(string url, string message)
        {
            return new DownloadResult
            {
                IsSuccess = false,
                Url = url,
                Message = message
            };
        }
    }
}
