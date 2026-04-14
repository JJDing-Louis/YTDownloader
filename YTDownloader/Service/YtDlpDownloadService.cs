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
        /// 下載音訊
        /// </summary>
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
        /// 批次下載多個 URL
        /// </summary>
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
        /// 取得 metadata 並轉成自有 DTO
        /// </summary>
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
        /// 直接用 metadata 判斷 URL 是單一影片或播放清單
        /// </summary>
        public async Task<ResourceDetectionResult> DetectResourceAsync(
            string url,
            int bufferKb = 1024,
            CancellationToken cancellationToken = default)
        {
            ValidateUrl(url);

            try
            {
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
        /// 取得格式清單
        /// </summary>
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
        /// 自動挑選最佳影音格式後下載
        /// </summary>
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
        /// 下載時自動依 metadata 分流：
        /// Playlist -> 下載整個播放清單
        /// SingleVideo -> 下載單一影片
        /// </summary>
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
        /// <param name="url">播放清單 URL（也接受單一影片 URL，會包成單項清單回傳）</param>
        /// <param name="progress">
        /// 可選的進度回報，參數為 (目前索引, 總數, 目前影片標題)，
        /// 適合用來更新 ProgressBar 或 Label。
        /// </param>
        /// <param name="bufferKb">讀取 metadata 時的緩衝大小，大型清單建議調高（預設 8192 KB）</param>
        /// <param name="cancellationToken">取消權杖</param>
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
                            IsSelected = true
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
                    IsSelected = true
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