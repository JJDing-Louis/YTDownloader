using Autofac;
using ManuHub.Ytdlp.NET;
using Microsoft.Extensions.Logging;
using YTDownloader.Model;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace YTDownloader.Service
{
    /// <summary>
    /// Ytdlp.NET 3.0.3 下載服務
    /// </summary>
    public class YtDlpDownloadService
    {
        private readonly string _ytDlpPath;
        private readonly string? _ffmpegFolder;
        private readonly ILogger logger = Program.Startup.Container.Resolve<ILogger<YtDlpDownloadService>>();

        /// <summary>
        /// 初始化 <see cref="YtDlpDownloadService"/>。
        /// </summary>
        /// <param name="ytDlpPath">
        /// yt-dlp 執行檔的完整路徑，例如 <c>C:\tools\yt-dlp.exe</c>。
        /// </param>
        /// <param name="ffmpegFolder">
        /// ffmpeg 執行檔所在的資料夾路徑（非執行檔本身），例如 <c>C:\tools\ffmpeg\bin</c>。
        /// 用於影音合併（muxing）或音訊轉檔；若不需要則留 <c>null</c>。
        /// </param>
        /// <exception cref="ArgumentException"><paramref name="ytDlpPath"/> 為空白時拋出。</exception>
        public YtDlpDownloadService(
            string ytDlpPath,
            string? ffmpegFolder = null)
        {
            if (string.IsNullOrWhiteSpace(ytDlpPath))
                throw new ArgumentException("yt-dlp 路徑不可為空", nameof(ytDlpPath));

            _ytDlpPath = ytDlpPath;
            _ffmpegFolder = ffmpegFolder;
        }

        /// <summary>
        /// 下載單一影片或播放清單中的內容。
        /// 若 URL 指向播放清單，yt-dlp 會依其行為處理整個播放清單。
        /// </summary>
        /// <param name="url">
        /// 影片或播放清單的 URL，必須為絕對 URI 格式。
        /// </param>
        /// <param name="outputFolder">
        /// 下載檔案的儲存資料夾路徑；若資料夾不存在，會自動建立。
        /// </param>
        /// <param name="format">
        /// yt-dlp 格式選擇字串，預設為 <c>"best"</c>（最佳單一流）。
        /// 可指定如 <c>"bestvideo+bestaudio/best"</c> 以分別選取最佳影像與音訊流再合併。
        /// </param>
        /// <param name="outputTemplate">
        /// 輸出檔名模板，使用 yt-dlp 的 <c>%(field)s</c> 語法，預設為 <c>"%(title)s.%(ext)s"</c>。
        /// </param>
        /// <param name="downloadThumbnail">
        /// 是否一併下載縮圖，預設為 <c>false</c>。
        /// </param>
        /// <param name="embedMetadata">
        /// 是否將 metadata（標題、作者等）嵌入影片檔案，預設為 <c>false</c>。
        /// </param>
        /// <param name="cancellationToken">用於取消非同步操作的權杖。</param>
        /// <returns>
        /// <see cref="DownloadResult"/>，包含：
        /// <list type="bullet">
        ///   <item><see cref="DownloadResult.IsSuccess"/> — 是否下載成功。</item>
        ///   <item><see cref="DownloadResult.OutputFolder"/> — 儲存路徑。</item>
        ///   <item><see cref="DownloadResult.Message"/> — 成功說明或錯誤原因。</item>
        /// </list>
        /// 取消操作或執行期間發生錯誤時，回傳 <c>IsSuccess = false</c>，不拋出例外。
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="url"/> 為空白或格式不正確，或 <paramref name="outputFolder"/> 為空白時拋出。
        /// </exception>
        public async Task<DownloadResult> DownloadVideoAsync(
            string url,
            string outputFolder,
            string format = "best",
            string? outputTemplate = "%(title)s.%(ext)s",
            bool downloadThumbnail = false,
            bool embedMetadata = false,
            CancellationToken cancellationToken = default)
        {
            ValidateUrl(url);
            EnsureDirectory(outputFolder);

            try
            {
                await using var ytdlp = BuildVideoDownloader(
                    outputFolder: outputFolder,
                    format: format,
                    outputTemplate: outputTemplate,
                    downloadThumbnail: downloadThumbnail,
                    embedMetadata: embedMetadata);

                AttachEvents(ytdlp);

                await ytdlp.DownloadAsync(url, cancellationToken);

                return DownloadResult.Success(
                    url: url,
                    outputFolder: outputFolder,
                    message: "影片下載完成");
            }
            catch (OperationCanceledException)
            {
                return DownloadResult.Fail(url, "影片下載已取消");
            }
            catch (Exception ex)
            {
               logger.LogError($"DownloadVideoAsync 失敗: {ex}");
                return DownloadResult.Fail(url, $"影片下載失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 提取影片中的音訊並以指定格式轉檔後儲存。
        /// </summary>
        /// <param name="url">
        /// 影片的 URL，必須為絕對 URI 格式。
        /// </param>
        /// <param name="outputFolder">
        /// 下載檔案的儲存資料夾路徑；若資料夾不存在，會自動建立。
        /// </param>
        /// <param name="audioFormat">
        /// 輸出音訊格式，預設為 <see cref="AudioFormat.Mp3"/>。
        /// 可選 <c>M4a</c>、<c>Flac</c>、<c>Wav</c>、<c>Opus</c> 等。
        /// </param>
        /// <param name="audioQuality">
        /// 音質等級（VBR 模式），範圍 <c>0</c>（最佳）到 <c>10</c>（最差），預設為 <c>5</c>。
        /// </param>
        /// <param name="outputTemplate">
        /// 輸出檔名模板，使用 yt-dlp 的 <c>%(field)s</c> 語法，預設為 <c>"%(title)s.%(ext)s"</c>。
        /// </param>
        /// <param name="embedMetadata">
        /// 是否將 metadata（標題、作者等）嵌入音訊檔案，預設為 <c>true</c>。
        /// </param>
        /// <param name="embedThumbnail">
        /// 是否將縮圖嵌入音訊檔案作為封面，預設為 <c>false</c>。
        /// </param>
        /// <param name="cancellationToken">用於取消非同步操作的權杖。</param>
        /// <returns>
        /// <see cref="DownloadResult"/>，包含：
        /// <list type="bullet">
        ///   <item><see cref="DownloadResult.IsSuccess"/> — 是否下載成功。</item>
        ///   <item><see cref="DownloadResult.OutputFolder"/> — 儲存路徑。</item>
        ///   <item><see cref="DownloadResult.Message"/> — 成功說明或錯誤原因，成功時會標示音訊格式。</item>
        /// </list>
        /// 取消操作或執行期間發生錯誤時，回傳 <c>IsSuccess = false</c>，不拋出例外。
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="url"/> 為空白或格式不正確，或 <paramref name="outputFolder"/> 為空白時拋出。
        /// </exception>
        public async Task<DownloadResult> DownloadAudioAsync(
            string url,
            string outputFolder,
            AudioFormat audioFormat = AudioFormat.Mp3,
            int audioQuality = 5,
            string? outputTemplate = "%(title)s.%(ext)s",
            bool embedMetadata = true,
            bool embedThumbnail = false,
            CancellationToken cancellationToken = default)
        {
            ValidateUrl(url);
            EnsureDirectory(outputFolder);

            try
            {
                await using var ytdlp = BuildAudioDownloader(
                    outputFolder: outputFolder,
                    audioFormat: audioFormat,
                    audioQuality: audioQuality,
                    outputTemplate: outputTemplate,
                    embedMetadata: embedMetadata,
                    embedThumbnail: embedThumbnail);

                AttachEvents(ytdlp);

                await ytdlp.DownloadAsync(url, cancellationToken);

                return DownloadResult.Success(
                    url: url,
                    outputFolder: outputFolder,
                    message: $"音訊下載完成，格式: {audioFormat}");
            }
            catch (OperationCanceledException)
            {
                return DownloadResult.Fail(url, "音訊下載已取消");
            }
            catch (Exception ex)
            {
               logger.LogError($"DownloadAudioAsync 失敗: {ex}");
                return DownloadResult.Fail(url, $"音訊下載失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次下載多個 URL，支援並發控制。
        /// 空白及重複的 URL 會在執行前自動過濾。
        /// </summary>
        /// <param name="urls">
        /// 要下載的影片 URL 清單。空白項目會自動略過，重複項目會自動去除。
        /// </param>
        /// <param name="outputFolder">
        /// 下載檔案的儲存資料夾路徑；若資料夾不存在，會自動建立。
        /// </param>
        /// <param name="format">
        /// yt-dlp 格式選擇字串，預設為 <c>"best"</c>。
        /// </param>
        /// <param name="maxConcurrency">
        /// 同時進行下載的最大數量，預設為 <c>3</c>。
        /// </param>
        /// <param name="cancellationToken">用於取消非同步操作的權杖。</param>
        /// <returns>
        /// <see cref="BatchDownloadResult"/>，包含：
        /// <list type="bullet">
        ///   <item><see cref="BatchDownloadResult.IsSuccess"/> — 是否全部下載成功。</item>
        ///   <item><see cref="BatchDownloadResult.TotalCount"/> — 實際執行下載的 URL 數量（過濾後）。</item>
        ///   <item><see cref="BatchDownloadResult.OutputFolder"/> — 儲存路徑。</item>
        ///   <item><see cref="BatchDownloadResult.Message"/> — 成功說明或錯誤原因。</item>
        /// </list>
        /// 取消操作或執行期間發生錯誤時，回傳 <c>IsSuccess = false</c>，不拋出例外。
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="urls"/> 為 <c>null</c> 時拋出。</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="urls"/> 過濾後為空，或 <paramref name="outputFolder"/> 為空白時拋出。
        /// </exception>
        public async Task<BatchDownloadResult> DownloadVideosAsync(
            IEnumerable<string> urls,
            string outputFolder,
            string format = "best",
            int maxConcurrency = 3,
            CancellationToken cancellationToken = default)
        {
            if (urls == null)
                throw new ArgumentNullException(nameof(urls));

            var urlList = urls
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            if (urlList.Count == 0)
                throw new ArgumentException("urls 不可為空", nameof(urls));

            EnsureDirectory(outputFolder);

            try
            {
                await using var ytdlp = BuildVideoDownloader(
                    outputFolder: outputFolder,
                    format: format,
                    outputTemplate: "%(title)s.%(ext)s",
                    downloadThumbnail: false,
                    embedMetadata: false);

                AttachEvents(ytdlp);

                await ytdlp.DownloadBatchAsync(urlList, maxConcurrency, cancellationToken);

                return new BatchDownloadResult
                {
                    IsSuccess = true,
                    OutputFolder = outputFolder,
                    TotalCount = urlList.Count,
                    Message = "批次下載完成"
                };
            }
            catch (OperationCanceledException)
            {
                return new BatchDownloadResult
                {
                    IsSuccess = false,
                    OutputFolder = outputFolder,
                    TotalCount = urlList.Count,
                    Message = "批次下載已取消"
                };
            }
            catch (Exception ex)
            {
               logger.LogError($"DownloadVideosAsync 失敗: {ex}");

                return new BatchDownloadResult
                {
                    IsSuccess = false,
                    OutputFolder = outputFolder,
                    TotalCount = urlList.Count,
                    Message = $"批次下載失敗: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 取得影片或播放清單的 metadata，並轉換為自有 DTO。
        /// </summary>
        /// <param name="url">
        /// 影片或播放清單的 URL，必須為絕對 URI 格式。
        /// </param>
        /// <param name="bufferKb">
        /// 讀取 metadata 時的輸出緩衝大小（KB），預設為 <c>1024</c>。
        /// 播放清單項目較多時可適當調高。
        /// </param>
        /// <param name="cancellationToken">用於取消非同步操作的權杖。</param>
        /// <returns>
        /// 解析成功時回傳 <see cref="YtDlpMetadata"/>，包含標題、上傳者、長度、播放清單資訊等欄位。
        /// 若 URL 無法解析或發生錯誤，回傳 <c>null</c>，不拋出例外。
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="url"/> 為空白或格式不正確時拋出。
        /// </exception>
        public async Task<YtDlpMetadata?> GetMetadataAsync(
            string url,
            int bufferKb = 1024,
            CancellationToken cancellationToken = default)
        {
            ValidateUrl(url);

            try
            {
                await using var ytdlp = CreateBaseClient();

                var raw = await ytdlp.GetMetadataAsync(url, cancellationToken, bufferKb: bufferKb);
                if (raw == null)
                    return null;

                return MapMetadata(raw);
            }
            catch (Exception ex)
            {
               logger.LogError($"GetMetadataAsync 失敗: {ex}");
                return null;
            }
        }

        /// <summary>
        /// 透過 metadata 判斷 URL 指向的是單一影片或播放清單。
        /// </summary>
        /// <param name="url">
        /// 要判斷的影片或播放清單 URL，必須為絕對 URI 格式。
        /// </param>
        /// <param name="bufferKb">
        /// 讀取 metadata 時的輸出緩衝大小（KB），預設為 <c>1024</c>。
        /// </param>
        /// <param name="cancellationToken">用於取消非同步操作的權杖。</param>
        /// <returns>
        /// <see cref="ResourceDetectionResult"/>，包含：
        /// <list type="bullet">
        ///   <item><see cref="ResourceDetectionResult.ResourceType"/> — 判斷結果，<see cref="UrlResourceType.SingleVideo"/>、<see cref="UrlResourceType.Playlist"/> 或 <see cref="UrlResourceType.Unknown"/>。</item>
        ///   <item><see cref="ResourceDetectionResult.Title"/> — 影片或播放清單標題。</item>
        ///   <item><see cref="ResourceDetectionResult.PlaylistCount"/> — 播放清單宣告的影片總數（單一影片時為 <c>null</c>）。</item>
        ///   <item><see cref="ResourceDetectionResult.Message"/> — 判斷說明或錯誤原因。</item>
        /// </list>
        /// 無法取得 metadata 時，<c>ResourceType</c> 為 <see cref="UrlResourceType.Unknown"/>，不拋出例外。
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="url"/> 為空白或格式不正確時拋出。
        /// </exception>
        public async Task<ResourceDetectionResult> DetectResourceAsync(
            string url,
            int bufferKb = 1024,
            CancellationToken cancellationToken = default)
        {
            ValidateUrl(url);

            try
            {
                // ── 快速路徑：純解析 URL，不呼叫 yt-dlp ──────────────────
                // 當 URL 帶有 list= 等明確播放清單參數時，可以在毫秒內完成判斷，
                // 避免 yt-dlp 把整個播放清單的所有影片 metadata 全部 dump 出來。
                var uri = new Uri(url);
                var quickType = TryDetectFromUrlPattern(uri);
                if (quickType.HasValue)
                {
                    return new ResourceDetectionResult
                    {
                        Url          = url,
                        ResourceType = quickType.Value,
                        Message      = quickType.Value == UrlResourceType.Playlist
                            ? "URL 特徵判斷為播放清單（快速路徑）"
                            : "URL 特徵判斷為單一影片（快速路徑）"
                    };
                }

                // ── Fallback：呼叫 yt-dlp 取得完整 metadata ──────────────
                var metadata = await GetMetadataAsync(url, bufferKb, cancellationToken);

                if (metadata == null)
                {
                    return new ResourceDetectionResult
                    {
                        Url = url,
                        ResourceType = UrlResourceType.Unknown,
                        Message = "無法取得 metadata"
                    };
                }

                return new ResourceDetectionResult
                {
                    Url = url,
                    ResourceType = metadata.IsPlaylist
                        ? UrlResourceType.Playlist
                        : UrlResourceType.SingleVideo,
                    Title = metadata.Title,
                    PlaylistId = metadata.PlaylistId,
                    PlaylistTitle = metadata.PlaylistTitle,
                    PlaylistCount = metadata.PlaylistCount,
                    EntryCount = metadata.EntryCount,
                    Message = metadata.IsPlaylist
                        ? "判斷為播放清單"
                        : "判斷為單一影片"
                };
            }
            catch (Exception ex)
            {
               logger.LogError($"DetectResourceAsync 失敗: {ex}");

                return new ResourceDetectionResult
                {
                    Url = url,
                    ResourceType = UrlResourceType.Unknown,
                    Message = $"判斷失敗: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 取得指定 URL 所有可用的影音格式清單。
        /// 適合讓使用者手動選擇格式 ID 後，再傳入 <see cref="DownloadVideoAsync"/> 的 <c>format</c> 參數。
        /// </summary>
        /// <param name="url">
        /// 影片的 URL，必須為絕對 URI 格式。
        /// </param>
        /// <param name="bufferKb">
        /// 讀取格式資訊時的輸出緩衝大小（KB），預設為 <c>1024</c>。
        /// </param>
        /// <param name="cancellationToken">用於取消非同步操作的權杖。</param>
        /// <returns>
        /// 可用格式的清單，每筆 <see cref="VideoFormatDto"/> 包含格式 ID、副檔名、解析度、影音編碼等資訊。
        /// 發生錯誤時回傳空清單，不拋出例外。
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="url"/> 為空白或格式不正確時拋出。
        /// </exception>
        public async Task<List<VideoFormatDto>> GetFormatsAsync(
            string url,
            int bufferKb = 1024,
            CancellationToken cancellationToken = default)
        {
            ValidateUrl(url);

            try
            {
                await using var ytdlp = CreateBaseClient();

                var formats = await ytdlp.GetFormatsAsync(url, cancellationToken, bufferKb: bufferKb);

                return formats.Select(item => new VideoFormatDto
                {
                    Id = item.Id,
                    Extension = item.Extension,
                    Resolution = item.Resolution,
                    VCodec = item.VideoCodec,
                    ACodec = item.AudioCodec,
                    FormatNote = item.Note
                }).ToList();
            }
            catch (Exception ex)
            {
               logger.LogError($"GetFormatsAsync 失敗: {ex}");
                return new List<VideoFormatDto>();
            }
        }

        /// <summary>
        /// 自動查詢最佳影像與音訊格式 ID，以 <c>{bestVideo}+{bestAudio}/best</c> 合併後下載。
        /// 適合不想手動選擇格式、追求最高品質的場景。
        /// </summary>
        /// <remarks>
        /// 此方法會呼叫兩次 yt-dlp（先查格式、再下載），需提供 ffmpeg 路徑以執行影音合併。
        /// </remarks>
        /// <param name="url">
        /// 影片的 URL，必須為絕對 URI 格式。
        /// </param>
        /// <param name="outputFolder">
        /// 下載檔案的儲存資料夾路徑；若資料夾不存在，會自動建立。
        /// </param>
        /// <param name="maxHeight">
        /// 影像解析度上限（像素高度），預設為 <c>1080</c>。
        /// 例如指定 <c>720</c> 則選取不超過 720p 的最佳格式。
        /// </param>
        /// <param name="cancellationToken">用於取消非同步操作的權杖。</param>
        /// <returns>
        /// <see cref="DownloadResult"/>，包含：
        /// <list type="bullet">
        ///   <item><see cref="DownloadResult.IsSuccess"/> — 是否下載成功。</item>
        ///   <item><see cref="DownloadResult.OutputFolder"/> — 儲存路徑。</item>
        ///   <item><see cref="DownloadResult.Message"/> — 成功時包含最終使用的格式字串，失敗時為錯誤原因。</item>
        /// </list>
        /// 執行期間發生錯誤時，回傳 <c>IsSuccess = false</c>，不拋出例外。
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="url"/> 為空白或格式不正確，或 <paramref name="outputFolder"/> 為空白時拋出。
        /// </exception>
        public async Task<DownloadResult> DownloadBestMuxedVideoAsync(
            string url,
            string outputFolder,
            int maxHeight = 1080,
            CancellationToken cancellationToken = default)
        {
            ValidateUrl(url);
            EnsureDirectory(outputFolder);

            try
            {
                await using var probe = CreateBaseClient();

                string bestVideo = await probe.GetBestVideoFormatIdAsync(
                    url,
                    maxHeight,
                    cancellationToken,
                    bufferKb: 1024);

                string bestAudio = await probe.GetBestAudioFormatIdAsync(
                    url,
                    cancellationToken,
                    bufferKb: 1024);

                string finalFormat = $"{bestVideo}+{bestAudio}/best";

                await using var ytdlp = BuildVideoDownloader(
                    outputFolder,
                    finalFormat,
                    "%(title)s.%(ext)s",
                    downloadThumbnail: false,
                    embedMetadata: true);

                AttachEvents(ytdlp);

                await ytdlp.DownloadAsync(url, cancellationToken);

                return DownloadResult.Success(url, outputFolder, $"影片下載完成，格式: {finalFormat}");
            }
            catch (Exception ex)
            {
               logger.LogError($"DownloadBestMuxedVideoAsync 失敗: {ex}");
                return DownloadResult.Fail(url, $"影片下載失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 先偵測 URL 類型，再自動分流下載。
        /// <list type="bullet">
        ///   <item><see cref="UrlResourceType.Playlist"/> — 下載整個播放清單。</item>
        ///   <item><see cref="UrlResourceType.SingleVideo"/> — 下載單一影片。</item>
        ///   <item><see cref="UrlResourceType.Unknown"/> — 直接回傳失敗，不進行下載。</item>
        /// </list>
        /// </summary>
        /// <param name="url">
        /// 影片或播放清單的 URL，必須為絕對 URI 格式。
        /// </param>
        /// <param name="outputFolder">
        /// 下載檔案的儲存資料夾路徑；若資料夾不存在，會自動建立。
        /// </param>
        /// <param name="format">
        /// yt-dlp 格式選擇字串，預設為 <c>"best"</c>。
        /// </param>
        /// <param name="outputTemplate">
        /// 輸出檔名模板，使用 yt-dlp 的 <c>%(field)s</c> 語法，預設為 <c>"%(title)s.%(ext)s"</c>。
        /// </param>
        /// <param name="downloadThumbnail">
        /// 是否一併下載縮圖，預設為 <c>false</c>。
        /// </param>
        /// <param name="embedMetadata">
        /// 是否將 metadata 嵌入檔案，預設為 <c>false</c>。
        /// </param>
        /// <param name="cancellationToken">用於取消非同步操作的權杖。</param>
        /// <returns>
        /// <see cref="DownloadResult"/>，包含：
        /// <list type="bullet">
        ///   <item><see cref="DownloadResult.IsSuccess"/> — 是否下載成功。</item>
        ///   <item><see cref="DownloadResult.OutputFolder"/> — 儲存路徑。</item>
        ///   <item><see cref="DownloadResult.Message"/> — 成功說明或錯誤原因。</item>
        /// </list>
        /// URL 類型無法判斷時，立即回傳 <c>IsSuccess = false</c>，不拋出例外。
        /// </returns>
        public async Task<DownloadResult> DownloadByDetectedTypeAsync(
            string url,
            string outputFolder,
            string format = "best",
            string? outputTemplate = "%(title)s.%(ext)s",
            bool downloadThumbnail = false,
            bool embedMetadata = false,
            CancellationToken cancellationToken = default)
        {
            var detected = await DetectResourceAsync(url, 1024, cancellationToken);

            if (detected.ResourceType == UrlResourceType.Unknown)
            {
                return DownloadResult.Fail(url, $"無法判斷資源類型: {detected.Message}");
            }

            return await DownloadVideoAsync(
                url: url,
                outputFolder: outputFolder,
                format: format,
                outputTemplate: outputTemplate,
                downloadThumbnail: downloadThumbnail,
                embedMetadata: embedMetadata,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// 讀取播放清單的所有影片資訊，供 UI 勾選後決定下載項目。
        /// <para>
        /// 回傳的 <see cref="PlaylistFetchResult.Videos"/> 中每個 <see cref="PlaylistVideoItem"/>
        /// 預設 <c>IsSelected = true</c>，UI 可讓使用者取消勾選後，
        /// 只把 IsSelected == true 且 WebpageUrl 不為空的項目送給下載方法。
        /// </para>
        /// </summary>
        /// <param name="url">
        /// 播放清單的 URL，必須為絕對 URI 格式。
        /// 也接受單一影片 URL，此時會自動包成只有一筆的清單回傳，UI 層無需特判。
        /// </param>
        /// <param name="progress">
        /// 可選的進度回報介面，每解析完一筆影片時觸發，回報內容為
        /// <c>(Current: 目前索引, Total: 總數, CurrentTitle: 目前影片標題)</c>，
        /// 適合用來更新 <c>ProgressBar</c> 或 <c>Label</c>。
        /// </param>
        /// <param name="bufferKb">
        /// 讀取 metadata 時的輸出緩衝大小（KB），預設為 <c>8192</c>。
        /// 播放清單超過 100 部時建議調高至 <c>16384</c> 以避免資料截斷。
        /// </param>
        /// <param name="cancellationToken">用於取消非同步操作的權杖。</param>
        /// <returns>
        /// <see cref="PlaylistFetchResult"/>，包含：
        /// <list type="bullet">
        ///   <item><see cref="PlaylistFetchResult.IsSuccess"/> — 是否成功讀取。</item>
        ///   <item><see cref="PlaylistFetchResult.PlaylistTitle"/> — 播放清單標題。</item>
        ///   <item><see cref="PlaylistFetchResult.TotalCount"/> — 實際解析到的影片數量。</item>
        ///   <item><see cref="PlaylistFetchResult.Videos"/> — 影片清單，每筆含標題、URL、時長、縮圖及 <c>IsSelected</c> 勾選狀態。</item>
        ///   <item><see cref="PlaylistFetchResult.Message"/> — 成功說明或錯誤原因。</item>
        /// </list>
        /// 取消操作或執行期間發生錯誤時，回傳 <c>IsSuccess = false</c>，不拋出例外。
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="url"/> 為空白或格式不正確時拋出。
        /// </exception>
        public async Task<PlaylistFetchResult> GetPlaylistVideosAsync(
            string url,
            IProgress<(int Current, int Total, string? CurrentTitle)>? progress = null,
            int bufferKb = 8192,
            CancellationToken cancellationToken = default)
        {
            ValidateUrl(url);

            try
            {
                await using var ytdlp = CreateBaseClient();

                var raw = await ytdlp.GetMetadataAsync(url, cancellationToken, bufferKb: bufferKb);

                if (raw == null)
                    return PlaylistFetchResult.Fail("無法取得播放清單資料，請確認 URL 是否正確");

                var videos = new List<PlaylistVideoItem>();

                // ── 播放清單：逐一解析 Entries ────────────────────────────
                bool hasEntries = false;
                try { hasEntries = raw.Entries != null; } catch { }

                if (hasEntries)
                {
                    int total = 0;
                    try { total = (int)raw.Entries.Count; } catch { }

                    int index = 0;
                    foreach (var entry in raw.Entries)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        index++;
                        string? title = TryGetString(() => entry.Title);

                        progress?.Report((index, total, title));

                        // Entry.Url 就是影片頁面網址（Entry 沒有 WebpageUrl 屬性）
                        string? videoUrl = TryGetString(() => entry.Url);

                        // Entry.Thumbnails 是集合，取第一個的 Url 作為縮圖
                        string? thumbnailUrl = null;
                        try
                        {
                            if (entry.Thumbnails != null && entry.Thumbnails.Count > 0)
                                thumbnailUrl = TryGetString(() => entry.Thumbnails[0].Url);
                        }
                        catch { }

                        videos.Add(new PlaylistVideoItem
                        {
                            Index      = index,
                            Id         = TryGetString(() => entry.Id),
                            Title      = title,
                            Uploader   = TryGetString(() => entry.Uploader),
                            Duration   = TryGetNullableInt(() => entry.Duration),
                            Thumbnail  = thumbnailUrl,
                            WebpageUrl = videoUrl,
                            IsSelected = false
                        });
                    }

                    return PlaylistFetchResult.Success(
                        playlistId:    TryGetString(() => raw.Id),
                        playlistTitle: TryGetString(() => raw.Title),
                        videos:        videos);
                }

                // ── 單一影片：包成單項清單回傳 ────────────────────────────
                var single = new PlaylistVideoItem
                {
                    Index      = 1,
                    Id         = TryGetString(() => raw.Id),
                    Title      = TryGetString(() => raw.Title),
                    Uploader   = TryGetString(() => raw.Uploader),
                    Duration   = TryGetNullableInt(() => raw.Duration),
                    Thumbnail  = TryGetString(() => raw.Thumbnail),
                    WebpageUrl = TryGetString(() => raw.WebpageUrl),
                    IsSelected = false
                };
                videos.Add(single);

                progress?.Report((1, 1, single.Title));

                return PlaylistFetchResult.Success(
                    playlistId:    null,
                    playlistTitle: single.Title,
                    videos:        videos,
                    message:       "URL 為單一影片，已轉為單項清單");
            }
            catch (OperationCanceledException)
            {
                return PlaylistFetchResult.Fail("播放清單載入已取消");
            }
            catch (Exception ex)
            {
                logger.LogError($"GetPlaylistVideosAsync 失敗: {ex}");
                return PlaylistFetchResult.Fail($"播放清單載入失敗: {ex.Message}");
            }
        }

        private Ytdlp CreateBaseClient()
        {
            var client = new Ytdlp(_ytDlpPath);

            if (!string.IsNullOrWhiteSpace(_ffmpegFolder))
            {
                client = client.WithFFmpegLocation(_ffmpegFolder);
            }

            return client;
        }

        private Ytdlp BuildVideoDownloader(
            string outputFolder,
            string format,
            string? outputTemplate,
            bool downloadThumbnail,
            bool embedMetadata)
        {
            var ytdlp = CreateBaseClient()
                .WithFormat(format)
                .WithOutputFolder(outputFolder);

            if (!string.IsNullOrWhiteSpace(outputTemplate))
            {
                ytdlp = ytdlp.WithOutputTemplate(outputTemplate);
            }

            if (downloadThumbnail)
            {
                ytdlp = ytdlp.WithThumbnails();
            }

            if (embedMetadata)
            {
                ytdlp = ytdlp.WithEmbedMetadata();
            }

            return ytdlp;
        }

        private Ytdlp BuildAudioDownloader(
            string outputFolder,
            AudioFormat audioFormat,
            int audioQuality,
            string? outputTemplate,
            bool embedMetadata,
            bool embedThumbnail)
        {
            var ytdlp = CreateBaseClient()
                .WithExtractAudio(audioFormat, audioQuality)
                .WithOutputFolder(outputFolder);

            if (!string.IsNullOrWhiteSpace(outputTemplate))
            {
                ytdlp = ytdlp.WithOutputTemplate(outputTemplate);
            }

            if (embedMetadata)
            {
                ytdlp = ytdlp.WithEmbedMetadata();
            }

            if (embedThumbnail)
            {
                ytdlp = ytdlp.WithEmbedThumbnail();
            }

            return ytdlp;
        }

        /// <summary>
        /// 把 library metadata 轉成自己的 DTO
        /// </summary>
        private YtDlpMetadata MapMetadata(dynamic raw)
        {
            int? entryCount = null;

            try
            {
                if (raw.Entries != null)
                {
                    entryCount = raw.Entries.Count;
                }
            }
            catch
            {
                entryCount = null;
            }

            return new YtDlpMetadata
            {
                Id = TryGetString(() => raw.Id),
                Title = TryGetString(() => raw.Title),
                Uploader = TryGetString(() => raw.Uploader),
                Duration = TryGetNullableInt(() => raw.Duration),
                Thumbnail = TryGetString(() => raw.Thumbnail),
                WebpageUrl = TryGetString(() => raw.WebpageUrl),

                PlaylistId = TryGetString(() => raw.PlaylistId),
                PlaylistTitle = TryGetString(() => raw.PlaylistTitle),
                PlaylistCount = TryGetNullableInt(() => raw.PlaylistCount),
                EntryCount = entryCount
            };
        }

        private void AttachEvents(Ytdlp ytdlp)
        {
            ytdlp.OnProgressDownload += (_, e) =>
            {
                logger.LogInformation(
                    $"[Download] {e.Percent:F2}% | Speed={e.Speed} | ETA={e.ETA}");
            };

            ytdlp.OnProgressMessage += (_, msg) =>
            {
                logger.LogInformation($"[Progress] {msg}");
            };

            ytdlp.OnCompleteDownload += (_, msg) =>
            {
                logger.LogInformation($"[Complete] {msg}");
            };

            ytdlp.OnPostProcessingStart += (_, msg) =>
            {
                logger.LogInformation($"[PostStart] {msg}");
            };

            ytdlp.OnPostProcessingComplete += (_, msg) =>
            {
                logger.LogInformation($"[PostComplete] {msg}");
            };

            ytdlp.OnOutputMessage += (_, msg) =>
            {
                logger.LogInformation($"[Output] {msg}");
            };

            ytdlp.OnErrorMessage += (_, err) =>
            {
               logger.LogError($"[Error] {err}");
            };

            ytdlp.OnCommandCompleted += (_, e) =>
            {
                logger.LogInformation($"[CommandCompleted] {e.Success}");
            };
        }

        private static string? TryGetString(Func<object?> getter)
        {
            try
            {
                var value = getter();
                return value?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static int? TryGetNullableInt(Func<object?> getter)
        {
            try
            {
                var value = getter();
                if (value == null)
                    return null;

                if (value is int intValue)
                    return intValue;

                if (int.TryParse(value.ToString(), out var parsed))
                    return parsed;

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 純解析 URL 字串判斷資源類型，無需呼叫 yt-dlp。
        /// 支援 YouTube、YouTube Music、Bilibili 等常見平台的播放清單 URL 特徵。
        /// </summary>
        /// <returns>
        /// 能從 URL 確定類型時回傳 <see cref="UrlResourceType"/>；
        /// 無法判斷時回傳 <c>null</c>，由呼叫端 fallback 至 yt-dlp。
        /// </returns>
        private static UrlResourceType? TryDetectFromUrlPattern(Uri uri)
        {
            var host = uri.Host.ToLowerInvariant();
            var path = uri.AbsolutePath.ToLowerInvariant();
            var query = uri.Query;

            // ── YouTube / YouTube Music ───────────────────────────────────
            if (host.EndsWith("youtube.com") || host == "youtu.be" || host.EndsWith("music.youtube.com"))
            {
                // 明確的播放清單頁面：youtube.com/playlist?list=xxx
                if (path == "/playlist")
                    return UrlResourceType.Playlist;

                // 頻道/用戶頁面：通常為多部影片集合
                if (path.StartsWith("/channel/") ||
                    path.StartsWith("/user/")    ||
                    path.StartsWith("/@"))
                    return UrlResourceType.Playlist;

                // youtu.be 短網址（無 list 參數）→ 單一影片
                if (host == "youtu.be" && !HasQueryParam(query, "list"))
                    return UrlResourceType.SingleVideo;

                // watch?v=xxx&list=yyy → 帶播放清單 context 的影片頁面，視為播放清單
                if (path == "/watch")
                {
                    bool hasVideo = HasQueryParam(query, "v");
                    bool hasList  = HasQueryParam(query, "list");

                    if (hasList)  return UrlResourceType.Playlist;
                    if (hasVideo) return UrlResourceType.SingleVideo;
                }
            }

            // ── Bilibili ─────────────────────────────────────────────────
            if (host.EndsWith("bilibili.com"))
            {
                // 合集/播放清單頁面
                if (path.StartsWith("/list/") ||
                    path.Contains("/channel/") ||
                    path.Contains("/medialist/"))
                    return UrlResourceType.Playlist;

                // 一般影片頁面
                if (path.StartsWith("/video/"))
                    return UrlResourceType.SingleVideo;
            }

            // 其他平台無法從 URL 特徵判斷，交由 yt-dlp
            return null;
        }

        /// <summary>
        /// 判斷 URL 的 query string 中是否包含指定的參數名稱。
        /// </summary>
        private static bool HasQueryParam(string query, string paramName)
        {
            if (string.IsNullOrEmpty(query))
                return false;

            // query 格式為 "?key=val&key2=val2"
            // 確保比對的是完整的 key 而非子字串（e.g. "list" 不誤判 "playlist"）
            return query.Contains($"?{paramName}=",  StringComparison.OrdinalIgnoreCase) ||
                   query.Contains($"&{paramName}=",  StringComparison.OrdinalIgnoreCase);
        }

        private static void ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL 不可為空", nameof(url));

            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                throw new ArgumentException("URL 格式不正確", nameof(url));
        }

        private static void EnsureDirectory(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("輸出資料夾不可為空", nameof(folderPath));

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }
    

    }
}