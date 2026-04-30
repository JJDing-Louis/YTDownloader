using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Autofac;
using ManuHub.Ytdlp.NET;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using YTDownloader.Model;

namespace YTDownloader.Service;

/// <summary>
///     Ytdlp.NET 3.0.3 下載服務
/// </summary>
public class YtDlpDownloadService
{
    /// <summary>
    ///     yt-dlp 在 flat-playlist 模式下，對無法存取的影片會輸出佔位標題，
    ///     格式為 "[Xxx video]"。此方法用來偵測這類條目。
    /// </summary>
    private static readonly HashSet<string> _unavailableTitles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "[Deleted video]",
            "[Private video]",
            "[Unavailable video]",
            "[Removed video]"
        };

    // ─── 同名檔案偵測與序號命名 ──────────────────────────────────────────
    // 目的：若 outputFolder 中已存在同名檔，將 %(title)s 替換為安全化後的
    //       標題並附加 (N) 序號，避免被 yt-dlp 覆蓋或跳過。
    //       為讓檔名完全可預測，最終 outputTemplate 會以字面字串取代
    //       %(title)s；%(ext)s 仍交給 yt-dlp 依實際輸出格式填入。

    /// <summary>影片下載時視為「同名衝突」的常見影片容器副檔名（不含點號）。</summary>
    private static readonly IReadOnlyCollection<string> _videoCandidateExtensions = new[]
    {
        "mp4", "mkv", "webm", "mov", "avi", "flv", "ts", "m4v"
    };

    /// <summary>音訊下載時視為「同名衝突」的常見音訊格式副檔名（不含點號）。</summary>
    private static readonly IReadOnlyCollection<string> _audioCandidateExtensions = new[]
    {
        "mp3", "m4a", "aac", "flac", "wav", "opus", "ogg"
    };

    private readonly string? _ffmpegFolder;
    private readonly string _ytDlpPath;
    private readonly ILogger<YtDlpDownloadService> logger;

    /// <summary>
    ///     初始化 <see cref="YtDlpDownloadService" />。
    /// </summary>
    /// <param name="ytDlpPath">
    ///     yt-dlp 執行檔的完整路徑，例如 <c>C:\tools\yt-dlp.exe</c>。
    /// </param>
    /// <param name="ffmpegFolder">
    ///     ffmpeg 執行檔所在的資料夾路徑（非執行檔本身），例如 <c>C:\tools\ffmpeg\bin</c>。
    ///     用於影音合併（muxing）或音訊轉檔；若不需要則留 <c>null</c>。
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="ytDlpPath" /> 為空白時拋出。</exception>
    public YtDlpDownloadService(
        string ytDlpPath,
        string? ffmpegFolder = null,
        ILogger<YtDlpDownloadService>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(ytDlpPath))
            throw new ArgumentException("yt-dlp 路徑不可為空", nameof(ytDlpPath));

        _ytDlpPath = ytDlpPath;
        _ffmpegFolder = ffmpegFolder;
        this.logger = logger ?? ResolveLogger();
    }

    /// <summary>
    ///     下載單一影片或播放清單中的內容。
    ///     若 URL 指向播放清單，yt-dlp 會依其行為處理整個播放清單。
    /// </summary>
    /// <param name="url">
    ///     影片或播放清單的 URL，必須為絕對 URI 格式。
    /// </param>
    /// <param name="outputFolder">
    ///     下載檔案的儲存資料夾路徑；若資料夾不存在，會自動建立。
    /// </param>
    /// <param name="format">
    ///     yt-dlp 格式選擇字串，預設為 <c>"best"</c>（最佳單一流）。
    ///     可指定如 <c>"bestvideo+bestaudio/best"</c> 以分別選取最佳影像與音訊流再合併。
    /// </param>
    /// <param name="outputTemplate">
    ///     輸出檔名模板，使用 yt-dlp 的 <c>%(field)s</c> 語法，預設為 <c>"%(title)s.%(ext)s"</c>。
    /// </param>
    /// <param name="downloadThumbnail">
    ///     是否一併下載縮圖，預設為 <c>false</c>。
    /// </param>
    /// <param name="embedMetadata">
    ///     是否將 metadata（標題、作者等）嵌入影片檔案，預設為 <c>false</c>。
    /// </param>
    /// <param name="knownTitle">
    ///     已知的影片標題（通常來自播放清單 metadata）。若提供，會觸發同名檔案偵測：
    ///     目標資料夾若已存在同名影片檔（mp4/mkv/webm/... 等常見影片容器），
    ///     會自動在輸出檔名後加上 <c>(1)</c>、<c>(2)</c> … 等序號避免覆蓋。
    ///     留 <c>null</c> 則保留 yt-dlp 原生行為。
    /// </param>
    /// <param name="cancellationToken">用於取消非同步操作的權杖。</param>
    /// <returns>
    ///     <see cref="DownloadResult" />，包含：
    ///     <list type="bullet">
    ///         <item><see cref="DownloadResult.IsSuccess" /> — 是否下載成功。</item>
    ///         <item><see cref="DownloadResult.OutputFolder" /> — 儲存路徑。</item>
    ///         <item><see cref="DownloadResult.Message" /> — 成功說明或錯誤原因。</item>
    ///     </list>
    ///     取消操作或執行期間發生錯誤時，回傳 <c>IsSuccess = false</c>，不拋出例外。
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="url" /> 為空白或格式不正確，或 <paramref name="outputFolder" /> 為空白時拋出。
    /// </exception>
    public async Task<DownloadResult> DownloadVideoAsync(
        string url,
        string outputFolder,
        string format = "best",
        string? outputTemplate = "%(title)s.%(ext)s",
        bool downloadThumbnail = false,
        bool embedMetadata = false,
        string? knownTitle = null,
        Action<double>? onProgress = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteDownloadAsync(
            new DownloadExecutionRequest(
                url,
                outputFolder,
                "開始下載影片：{Url} → {Folder}",
                "影片下載完成",
                "影片下載已取消",
                "影片下載失敗"),
            () => BuildVideoDownloader(
                outputFolder,
                format,
                outputTemplate,
                downloadThumbnail,
                embedMetadata,
                knownTitle),
            onProgress,
            cancellationToken);
    }

    /// <summary>
    ///     提取影片中的音訊並以指定格式轉檔後儲存。
    /// </summary>
    /// <param name="url">
    ///     影片的 URL，必須為絕對 URI 格式。
    /// </param>
    /// <param name="outputFolder">
    ///     下載檔案的儲存資料夾路徑；若資料夾不存在，會自動建立。
    /// </param>
    /// <param name="audioFormat">
    ///     輸出音訊格式，預設為 <see cref="AudioFormat.Mp3" />。
    ///     可選 <c>M4a</c>、<c>Flac</c>、<c>Wav</c>、<c>Opus</c> 等。
    /// </param>
    /// <param name="audioQuality">
    ///     音質等級（VBR 模式），範圍 <c>0</c>（最佳）到 <c>10</c>（最差），預設為 <c>5</c>。
    /// </param>
    /// <param name="outputTemplate">
    ///     輸出檔名模板，使用 yt-dlp 的 <c>%(field)s</c> 語法，預設為 <c>"%(title)s.%(ext)s"</c>。
    /// </param>
    /// <param name="embedMetadata">
    ///     是否將 metadata（標題、作者等）嵌入音訊檔案，預設為 <c>true</c>。
    /// </param>
    /// <param name="embedThumbnail">
    ///     是否將縮圖嵌入音訊檔案作為封面，預設為 <c>false</c>。
    /// </param>
    /// <param name="knownTitle">
    ///     已知的影片標題（通常來自播放清單 metadata）。若提供，會觸發同名檔案偵測：
    ///     目標資料夾若已存在同名音訊檔（mp3/m4a/flac/... 等常見音訊格式），
    ///     會自動在輸出檔名後加上 <c>(1)</c>、<c>(2)</c> … 等序號避免覆蓋。
    ///     留 <c>null</c> 則保留 yt-dlp 原生行為。
    /// </param>
    /// <param name="cancellationToken">用於取消非同步操作的權杖。</param>
    /// <returns>
    ///     <see cref="DownloadResult" />，包含：
    ///     <list type="bullet">
    ///         <item><see cref="DownloadResult.IsSuccess" /> — 是否下載成功。</item>
    ///         <item><see cref="DownloadResult.OutputFolder" /> — 儲存路徑。</item>
    ///         <item><see cref="DownloadResult.Message" /> — 成功說明或錯誤原因，成功時會標示音訊格式。</item>
    ///     </list>
    ///     取消操作或執行期間發生錯誤時，回傳 <c>IsSuccess = false</c>，不拋出例外。
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="url" /> 為空白或格式不正確，或 <paramref name="outputFolder" /> 為空白時拋出。
    /// </exception>
    public async Task<DownloadResult> DownloadAudioAsync(
        string url,
        string outputFolder,
        AudioFormat audioFormat = AudioFormat.Mp3,
        int audioQuality = 5,
        string? outputTemplate = "%(title)s.%(ext)s",
        bool embedMetadata = true,
        bool embedThumbnail = false,
        string? knownTitle = null,
        Action<double>? onProgress = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteDownloadAsync(
            new DownloadExecutionRequest(
                url,
                outputFolder,
                "開始下載音訊：{Url} → {Folder}",
                $"音訊下載完成，格式：{audioFormat}",
                "音訊下載已取消",
                "音訊下載失敗"),
            () => BuildAudioDownloader(
                outputFolder,
                audioFormat,
                audioQuality,
                outputTemplate,
                embedMetadata,
                embedThumbnail,
                knownTitle),
            onProgress,
            cancellationToken);
    }

    /// <summary>
    ///     取得影片或播放清單的 metadata，並轉換為自有 DTO。
    /// </summary>
    /// <param name="url">
    ///     影片或播放清單的 URL，必須為絕對 URI 格式。
    /// </param>
    /// <param name="bufferKb">
    ///     讀取 metadata 時的輸出緩衝大小（KB），預設為 <c>1024</c>。
    ///     播放清單項目較多時可適當調高。
    /// </param>
    /// <param name="cancellationToken">用於取消非同步操作的權杖。</param>
    /// <returns>
    ///     解析成功時回傳 <see cref="YtDlpMetadata" />，包含標題、上傳者、長度、播放清單資訊等欄位。
    ///     若 URL 無法解析或發生錯誤，回傳 <c>null</c>，不拋出例外。
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="url" /> 為空白或格式不正確時拋出。
    /// </exception>
    private async Task<YtDlpMetadata?> GetMetadataAsync(
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
    ///     透過 metadata 判斷 URL 指向的是單一影片或播放清單。
    /// </summary>
    /// <param name="url">
    ///     要判斷的影片或播放清單 URL，必須為絕對 URI 格式。
    /// </param>
    /// <param name="bufferKb">
    ///     讀取 metadata 時的輸出緩衝大小（KB），預設為 <c>1024</c>。
    /// </param>
    /// <param name="cancellationToken">用於取消非同步操作的權杖。</param>
    /// <returns>
    ///     <see cref="ResourceDetectionResult" />，包含：
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="ResourceDetectionResult.ResourceType" /> — 判斷結果為 SingleVideo、Playlist 或 Unknown。
    ///         </item>
    ///         <item><see cref="ResourceDetectionResult.Title" /> — 影片或播放清單標題。</item>
    ///         <item><see cref="ResourceDetectionResult.PlaylistCount" /> — 播放清單宣告的影片總數（單一影片時為 <c>null</c>）。</item>
    ///         <item><see cref="ResourceDetectionResult.Message" /> — 判斷說明或錯誤原因。</item>
    ///     </list>
    ///     無法取得 metadata 時，<c>ResourceType</c> 為 Unknown，不拋出例外。
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="url" /> 為空白或格式不正確時拋出。
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
            if (!string.IsNullOrWhiteSpace(quickType))
                return new ResourceDetectionResult
                {
                    Url = url,
                    ResourceType = quickType,
                    Message = quickType == ResourceDetectionResult.GetSourceTypeName("播放清單")
                        ? "URL 特徵判斷為播放清單（快速路徑）"
                        : "URL 特徵判斷為單一影片（快速路徑）"
                };

            // ── Fallback：呼叫 yt-dlp 取得完整 metadata ──────────────
            var metadata = await GetMetadataAsync(url, bufferKb, cancellationToken);

            if (metadata == null)
                return new ResourceDetectionResult
                {
                    Url = url,
                    ResourceType = ResourceDetectionResult.GetSourceTypeName("Unknown"),
                    Message = "無法取得 metadata"
                };

            return new ResourceDetectionResult
            {
                Url = url,
                ResourceType = metadata.IsPlaylist
                    ? ResourceDetectionResult.GetSourceTypeName("播放清單")
                    : ResourceDetectionResult.GetSourceTypeName("單一影片"),
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
                ResourceType = ResourceDetectionResult.GetSourceTypeName("Unknown"),
                Message = $"判斷失敗: {ex.Message}"
            };
        }
    }

    /// <summary>
    ///     讀取播放清單的所有影片資訊，供 UI 勾選後決定下載項目。
    ///     <para>
    ///         回傳的 <see cref="PlaylistFetchResult.Videos" /> 中每個 <see cref="PlaylistVideoItem" />
    ///         預設 <c>IsSelected = true</c>，UI 可讓使用者取消勾選後，
    ///         只把 IsSelected == true 且 WebpageUrl 不為空的項目送給下載方法。
    ///     </para>
    /// </summary>
    /// <param name="url">
    ///     播放清單的 URL，必須為絕對 URI 格式。
    ///     也接受單一影片 URL，此時會自動包成只有一筆的清單回傳，UI 層無需特判。
    /// </param>
    /// <param name="progress">
    ///     可選的進度回報介面，每解析完一筆影片時觸發，回報內容為
    ///     <c>(Current: 目前索引, Total: 總數, CurrentTitle: 目前影片標題)</c>，
    ///     適合用來更新 <c>ProgressBar</c> 或 <c>Label</c>。
    /// </param>
    /// <param name="bufferKb">
    ///     讀取 metadata 時的輸出緩衝大小（KB），預設為 <c>8192</c>。
    ///     播放清單超過 100 部時建議調高至 <c>16384</c> 以避免資料截斷。
    /// </param>
    /// <param name="cancellationToken">用於取消非同步操作的權杖。</param>
    /// <returns>
    ///     <see cref="PlaylistFetchResult" />，包含：
    ///     <list type="bullet">
    ///         <item><see cref="PlaylistFetchResult.IsSuccess" /> — 是否成功讀取。</item>
    ///         <item><see cref="PlaylistFetchResult.PlaylistTitle" /> — 播放清單標題。</item>
    ///         <item><see cref="PlaylistFetchResult.TotalCount" /> — 實際解析到的影片數量。</item>
    ///         <item><see cref="PlaylistFetchResult.Videos" /> — 影片清單，每筆含標題、URL、時長、縮圖及 <c>IsSelected</c> 勾選狀態。</item>
    ///         <item><see cref="PlaylistFetchResult.Message" /> — 成功說明或錯誤原因。</item>
    ///     </list>
    ///     取消操作或執行期間發生錯誤時，回傳 <c>IsSuccess = false</c>，不拋出例外。
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="url" /> 為空白或格式不正確時拋出。
    /// </exception>
    public async Task<PlaylistFetchResult> GetPlaylistVideosAsync(
        string url,
        IProgress<(int Current, int Total, string? CurrentTitle)>? progress = null,
        int bufferKb = 32768, // 保留簽章相容性，此實作不使用 bufferKb
        CancellationToken cancellationToken = default)
    {
        ValidateUrl(url);

        try
        {
            // ── 用 --flat-playlist 模式直接執行 yt-dlp ───────────────
            // --flat-playlist : 只取基本資訊，不對每部影片發出完整 metadata 請求
            // --dump-json     : 每個項目輸出一行 JSON（EOF 代表結束）
            // --ignore-errors : 無法存取的影片（私人/刪除）跳過，不中斷整個清單
            var psi = new ProcessStartInfo
            {
                FileName = _ytDlpPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            psi.ArgumentList.Add("--flat-playlist");
            psi.ArgumentList.Add("--dump-json");
            psi.ArgumentList.Add("--ignore-errors");
            psi.ArgumentList.Add(url);

            logger.LogInformation("啟動 yt-dlp flat-playlist，URL={Url}", url);

            using var process = new Process { StartInfo = psi };
            var stderrSb = new StringBuilder();

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data == null) return;
                stderrSb.AppendLine(e.Data);
                logger.LogDebug("[yt-dlp] {Line}", e.Data);
            };

            process.Start();
            process.BeginErrorReadLine();

            var videos = new List<PlaylistVideoItem>();
            var unavailableEntries = new List<string>(); // 不可播放條目描述
            string? playlistTitle = null;
            string? playlistId = null;
            var declaredCount = 0; // playlist_count（yt-dlp 宣告的總數）
            var index = 0;

            // 逐行讀取：每行是一個獨立 JSON 物件（代表一部影片）
            while (true)
            {
                var line = await process.StandardOutput.ReadLineAsync(cancellationToken);
                if (line == null) break; // EOF
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    using var doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;

                    // 部分 yt-dlp 版本會先輸出一行 _type=playlist 的頭部資訊，跳過
                    if (root.TryGetProperty("_type", out var typeProp)
                        && typeProp.GetString() == "playlist")
                        continue;

                    // 從首筆 entry 讀取播放清單資訊
                    if (playlistTitle == null)
                        playlistTitle = GetJsonString(root, "playlist_title")
                                        ?? GetJsonString(root, "playlist");
                    if (playlistId == null)
                        playlistId = GetJsonString(root, "playlist_id");
                    if (declaredCount == 0
                        && root.TryGetProperty("playlist_count", out var pc)
                        && pc.ValueKind == JsonValueKind.Number)
                        declaredCount = pc.GetInt32();

                    // 影片頁面網址：優先 webpage_url，退而求其次用 url
                    var webpageUrl = GetJsonString(root, "webpage_url")
                                     ?? GetJsonString(root, "url");

                    // 時長（秒）
                    int? duration = null;
                    if (root.TryGetProperty("duration", out var durProp)
                        && durProp.ValueKind == JsonValueKind.Number)
                        duration = (int)durProp.GetDouble();

                    // 縮圖：先找 thumbnail 欄位，再找 thumbnails 陣列末項（通常最高解析度）
                    var thumbnail = GetJsonString(root, "thumbnail");
                    if (thumbnail == null
                        && root.TryGetProperty("thumbnails", out var thumbs)
                        && thumbs.ValueKind == JsonValueKind.Array
                        && thumbs.GetArrayLength() > 0)
                        thumbnail = thumbs[thumbs.GetArrayLength() - 1]
                            .TryGetProperty("url", out var tu)
                            ? tu.GetString()
                            : null;

                    var title = GetJsonString(root, "title");

                    // 取播放清單中的原始位置（供提示訊息顯示）
                    var playlistIndex = videos.Count + unavailableEntries.Count + 1;
                    if (root.TryGetProperty("playlist_index", out var piProp)
                        && piProp.ValueKind == JsonValueKind.Number)
                        playlistIndex = piProp.GetInt32();

                    var availability = GetJsonString(root, "availability");

                    // 過濾不可播放的佔位條目（[Deleted video]、[Private video] 等）
                    // 以及 yt-dlp 可讀到標題、但實際需會員權限才可下載的項目。
                    if (IsUnavailablePlaylistEntry(title, availability, out var unavailableReason))
                    {
                        unavailableEntries.Add($"#{playlistIndex}：{unavailableReason} - {title}");
                        logger.LogInformation(
                            "跳過不可播放條目 #{Index}：{Reason} - {Title}",
                            playlistIndex,
                            unavailableReason,
                            title);
                        continue;
                    }

                    index++;
                    progress?.Report((index, 0, title));

                    videos.Add(new PlaylistVideoItem
                    {
                        Index = index,
                        Id = GetJsonString(root, "id"),
                        Title = title,
                        Uploader = GetJsonString(root, "uploader")
                                   ?? GetJsonString(root, "channel"),
                        Duration = duration,
                        Thumbnail = thumbnail,
                        WebpageUrl = webpageUrl,
                        IsSelected = true
                    });
                }
                catch (JsonException ex)
                {
                    logger.LogWarning("跳過無法解析的 JSON 行：{Message}", ex.Message);
                }
            }

            await process.WaitForExitAsync(cancellationToken);

            if (videos.Count == 0)
            {
                var stderr = stderrSb.ToString();
                logger.LogError("播放清單解析完畢但無影片。stderr={Stderr}", stderr);
                if (unavailableEntries.Count == 0
                    && TryCreateUnavailableEntryFromYtDlpError(stderr, out var unavailableEntry))
                    unavailableEntries.Add(unavailableEntry);

                var failedResult = PlaylistFetchResult.Fail(
                    "播放清單為空，或所有影片均無法存取（私人 / 已刪除）。\n\n" +
                    $"yt-dlp 訊息：{(stderr.Length > 400 ? stderr[..400] + "…" : stderr)}");
                failedResult.DeclaredCount = declaredCount;
                failedResult.UnavailableEntries = unavailableEntries;
                return failedResult;
            }

            logger.LogInformation(
                "成功解析播放清單 [{Title}]，解析={Parsed} / 宣告={Declared}",
                playlistTitle, videos.Count, declaredCount);

            var result = PlaylistFetchResult.Success(playlistId, playlistTitle, videos);
            result.DeclaredCount = declaredCount;
            result.UnavailableEntries = unavailableEntries;
            return result;
        }
        catch (OperationCanceledException)
        {
            return PlaylistFetchResult.Fail("播放清單載入已取消");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetPlaylistVideosAsync 失敗");
            return PlaylistFetchResult.Fail($"播放清單載入失敗：{ex.Message}");
        }
    }

    /// <summary>
    ///     從 <see cref="JsonElement" /> 安全地讀取字串屬性，找不到或型別不符時回傳 null。
    /// </summary>
    private static string? GetJsonString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop)
               && prop.ValueKind == JsonValueKind.String
            ? prop.GetString()
            : null;
    }

    internal static bool IsUnavailableTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title)) return false;
        // 精確比對已知佔位標題
        if (_unavailableTitles.Contains(title)) return true;
        // 通用防禦：格式為 "[Xxx video]" 或 "[Xxx]" 的佔位標題
        return title.StartsWith('[') && title.EndsWith(']');
    }

    internal static bool IsUnavailablePlaylistEntry(
        string? title,
        string? availability,
        out string reason)
    {
        if (IsUnavailableTitle(title))
        {
            reason = "不可存取";
            return true;
        }

        if (string.Equals(availability, "subscriber_only", StringComparison.OrdinalIgnoreCase))
        {
            reason = "會員專屬";
            return true;
        }

        reason = string.Empty;
        return false;
    }

    private static bool TryCreateUnavailableEntryFromYtDlpError(
        string ytDlpError,
        out string unavailableEntry)
    {
        if (ytDlpError.Contains("Private video", StringComparison.OrdinalIgnoreCase)
            || ytDlpError.Contains("This is a private video", StringComparison.OrdinalIgnoreCase))
        {
            unavailableEntry = "#1：不可存取 - [Private video]";
            return true;
        }

        if (ytDlpError.Contains("members-only", StringComparison.OrdinalIgnoreCase)
            || ytDlpError.Contains("subscriber", StringComparison.OrdinalIgnoreCase)
            || ytDlpError.Contains("Join this channel", StringComparison.OrdinalIgnoreCase))
        {
            unavailableEntry = "#1：會員專屬 - [Members-only video]";
            return true;
        }

        if (ytDlpError.Contains("This video is not available", StringComparison.OrdinalIgnoreCase)
            || ytDlpError.Contains("Video unavailable", StringComparison.OrdinalIgnoreCase))
        {
            unavailableEntry = "#1：不可存取 - [Unavailable video]";
            return true;
        }

        unavailableEntry = string.Empty;
        return false;
    }

    private async Task<DownloadResult> ExecuteDownloadAsync(
        DownloadExecutionRequest request,
        Func<Ytdlp> createDownloader,
        Action<double>? onProgress,
        CancellationToken cancellationToken)
    {
        ValidateUrl(request.Url);
        EnsureDirectory(request.OutputFolder);

        try
        {
            await using var ytdlp = createDownloader();

            var ytSuccess = true;
            var ytErrors = string.Empty;
            AttachEvents(ytdlp, onProgress, (ok, err) =>
            {
                ytSuccess = ok;
                ytErrors = err;
            });

            logger.LogInformation(request.StartLogMessage, request.Url, request.OutputFolder);
            await ytdlp.DownloadAsync(request.Url, cancellationToken);

            return ytSuccess
                ? DownloadResult.Success(request.Url, request.OutputFolder, request.SuccessMessage)
                : DownloadResult.Fail(request.Url, BuildYtDlpFailureMessage(ytErrors));
        }
        catch (OperationCanceledException)
        {
            return DownloadResult.Fail(request.Url, request.CanceledMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Operation} 失敗", request.FailurePrefix);
            return DownloadResult.Fail(request.Url, $"{request.FailurePrefix}：{ex.Message}");
        }
    }

    private static string BuildYtDlpFailureMessage(string ytErrors)
    {
        return string.IsNullOrWhiteSpace(ytErrors)
            ? "yt-dlp 回報下載失敗（未知原因）"
            : $"yt-dlp 下載失敗：{ytErrors}";
    }

    private static ILogger<YtDlpDownloadService> ResolveLogger()
    {
        try
        {
            return Program.Startup.Container.Resolve<ILogger<YtDlpDownloadService>>();
        }
        catch
        {
            return NullLogger<YtDlpDownloadService>.Instance;
        }
    }

    private Ytdlp CreateBaseClient()
    {
        var client = new Ytdlp(_ytDlpPath);

        if (!string.IsNullOrWhiteSpace(_ffmpegFolder)) client = client.WithFFmpegLocation(_ffmpegFolder);

        return client;
    }

    private Ytdlp BuildVideoDownloader(
        string outputFolder,
        string format,
        string? outputTemplate,
        bool downloadThumbnail,
        bool embedMetadata,
        string? knownTitle)
    {
        var resolvedTemplate = ResolveUniqueOutputTemplate(
            outputFolder, outputTemplate, knownTitle, _videoCandidateExtensions);

        var ytdlp = CreateBaseClient()
            .WithFormat(format)
            .WithOutputFolder(outputFolder);

        if (!string.IsNullOrWhiteSpace(resolvedTemplate)) ytdlp = ytdlp.WithOutputTemplate(resolvedTemplate);

        if (downloadThumbnail) ytdlp = ytdlp.WithThumbnails();

        if (embedMetadata) ytdlp = ytdlp.WithEmbedMetadata();

        return ytdlp;
    }

    private Ytdlp BuildAudioDownloader(
        string outputFolder,
        AudioFormat audioFormat,
        int audioQuality,
        string? outputTemplate,
        bool embedMetadata,
        bool embedThumbnail,
        string? knownTitle)
    {
        var resolvedTemplate = ResolveUniqueOutputTemplate(
            outputFolder, outputTemplate, knownTitle, _audioCandidateExtensions);

        var ytdlp = CreateBaseClient()
            .WithExtractAudio(audioFormat, audioQuality)
            .WithOutputFolder(outputFolder);

        if (!string.IsNullOrWhiteSpace(resolvedTemplate)) ytdlp = ytdlp.WithOutputTemplate(resolvedTemplate);

        if (embedMetadata) ytdlp = ytdlp.WithEmbedMetadata();

        if (embedThumbnail) ytdlp = ytdlp.WithEmbedThumbnail();

        return ytdlp;
    }

    /// <summary>
    ///     在下載前計算不會覆蓋既有檔案的 outputTemplate。
    ///     <para>
    ///         當 <paramref name="knownTitle" /> 有值且 <paramref name="outputTemplate" />
    ///         包含 <c>%(title)s</c> 時，會：
    ///         <list type="number">
    ///             <item>將 <paramref name="knownTitle" /> 做 Windows 檔名安全化。</item>
    ///             <item>
    ///                 在 <paramref name="outputFolder" /> 檢查 <paramref name="candidateExtensions" />
    ///                 中任何副檔名是否已存在同名檔。
    ///             </item>
    ///             <item>若存在則逐一嘗試 <c>Title(1)</c>、<c>Title(2)</c> … 直到找到可用名稱。</item>
    ///             <item>以找到的名稱字面替換 <c>%(title)s</c> 後回傳新 template。</item>
    ///         </list>
    ///         其他情況（無 knownTitle、template 不含 %(title)s、資料夾不存在等）則回傳原 template。
    ///     </para>
    /// </summary>
    private string ResolveUniqueOutputTemplate(
        string outputFolder,
        string? outputTemplate,
        string? knownTitle,
        IReadOnlyCollection<string> candidateExtensions)
    {
        var template = string.IsNullOrWhiteSpace(outputTemplate)
            ? "%(title)s.%(ext)s"
            : outputTemplate!;

        // 沒有 knownTitle 或 template 不含 %(title)s 佔位符 → 交回 yt-dlp 原生行為
        if (string.IsNullOrWhiteSpace(knownTitle))
            return template;
        if (!template.Contains("%(title)s", StringComparison.Ordinal))
            return template;

        var safeTitle = SanitizeForWindowsFileName(knownTitle!);
        if (string.IsNullOrWhiteSpace(safeTitle))
            return template;

        // 資料夾不存在則一定沒有衝突，直接把 %(title)s 字面化為安全化後的標題
        if (!Directory.Exists(outputFolder))
            return template.Replace("%(title)s", safeTitle);

        var candidateTitle = safeTitle;
        var seq = 0;
        const int maxSeq = 9999;
        while (HasFileCollision(outputFolder, template, candidateTitle, candidateExtensions))
        {
            seq++;
            if (seq > maxSeq)
            {
                logger.LogWarning(
                    "同名檔名序號已超過 {MaxSeq}，放棄加序號以避免無限迴圈（標題：{Title}）",
                    maxSeq, safeTitle);
                break;
            }

            candidateTitle = $"{safeTitle}({seq})";
        }

        if (seq > 0)
            logger.LogInformation(
                "偵測到同名檔案，輸出檔名改為：{FinalTitle}（原：{OriginalTitle}）",
                candidateTitle, safeTitle);

        return template.Replace("%(title)s", candidateTitle);
    }

    /// <summary>
    ///     將指定 title 代入 template 後，逐一比對 candidateExtensions 中的副檔名，
    ///     檢查 outputFolder 內是否已存在對應檔案。
    /// </summary>
    private static bool HasFileCollision(
        string outputFolder,
        string template,
        string titleSubstitution,
        IReadOnlyCollection<string> candidateExtensions)
    {
        var baseTemplate = template.Replace("%(title)s", titleSubstitution);

        foreach (var ext in candidateExtensions)
        {
            var normalizedExt = ext.StartsWith('.') ? ext[1..] : ext;
            var candidatePath = Path.Combine(
                outputFolder,
                baseTemplate.Replace("%(ext)s", normalizedExt));

            if (File.Exists(candidatePath))
                return true;
        }

        return false;
    }

    /// <summary>
    ///     將字串轉為 Windows 合法檔名：
    ///     <list type="bullet">
    ///         <item>ASCII 受限字元（<c>\ / : * ? " &lt; &gt; |</c>）與其全形對應字元替換為 <c>_</c>。</item>
    ///         <item>控制字元（U+0000–U+001F、U+007F）替換為 <c>_</c>。</item>
    ///         <item>修剪尾端的空白與點號，避免 Windows 檔案系統拒絕。</item>
    ///     </list>
    ///     若整體為空則回傳 <c>"_"</c>，保證回傳值可作為檔名使用。
    /// </summary>
    private static string SanitizeForWindowsFileName(string title)
    {
        if (string.IsNullOrEmpty(title)) return string.Empty;

        var sb = new StringBuilder(title.Length);
        foreach (var c in title)
        {
            if (c < 0x20 || c == 0x7F)
            {
                sb.Append('_');
                continue;
            }

            var replaced = c switch
            {
                '/' or '／' => '_',
                '\\' or '＼' => '_',
                ':' or '：' => '_',
                '*' or '＊' => '_',
                '?' or '？' => '_',
                '"' or '＂' => '_',
                '<' or '＜' => '_',
                '>' or '＞' => '_',
                '|' or '｜' => '_',
                _ => c
            };
            sb.Append(replaced);
        }

        var result = sb.ToString().TrimEnd(' ', '.');
        return result.Length == 0 ? "_" : result;
    }

    /// <summary>
    ///     把 library metadata 轉成自己的 DTO
    /// </summary>
    private YtDlpMetadata MapMetadata(dynamic raw)
    {
        int? entryCount = null;

        try
        {
            if (raw.Entries != null) entryCount = raw.Entries.Count;
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

    /// <param name="onProgress">每次進度更新觸發，回傳 0–100 的百分比。</param>
    /// <param name="onCompleted">
    ///     yt-dlp 程序結束時觸發：<br />
    ///     第一個參數 = 是否成功（exit code 0）；<br />
    ///     第二個參數 = 彙整的 stderr 錯誤訊息（成功時為空字串）。
    /// </param>
    private void AttachEvents(
        Ytdlp ytdlp,
        Action<double>? onProgress = null,
        Action<bool, string>? onCompleted = null)
    {
        ytdlp.OnProgressDownload += (_, e) =>
        {
            logger.LogInformation(
                "[Download] {Percent:F2}% | Speed={Speed} | ETA={ETA}",
                e.Percent, e.Speed, e.ETA);
            onProgress?.Invoke(e.Percent);
        };

        ytdlp.OnProgressMessage += (_, msg) => logger.LogInformation("[Progress] {Msg}", msg);
        ytdlp.OnCompleteDownload += (_, msg) => logger.LogInformation("[Complete] {Msg}", msg);
        ytdlp.OnPostProcessingStart += (_, msg) => logger.LogInformation("[PostStart] {Msg}", msg);
        ytdlp.OnPostProcessingComplete += (_, msg) => logger.LogInformation("[PostComplete] {Msg}", msg);
        ytdlp.OnOutputMessage += (_, msg) => logger.LogInformation("[Output] {Msg}", msg);

        var errorSb = new StringBuilder();
        ytdlp.OnErrorMessage += (_, err) =>
        {
            logger.LogError("[yt-dlp Error] {Err}", err);
            errorSb.AppendLine(err);
        };

        ytdlp.OnCommandCompleted += (_, e) =>
        {
            logger.LogInformation("[CommandCompleted] Success={Success}", e.Success);
            onCompleted?.Invoke(e.Success, errorSb.ToString().Trim());
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
    ///     純解析 URL 字串判斷資源類型，無需呼叫 yt-dlp。
    ///     支援 YouTube、YouTube Music、Bilibili 等常見平台的播放清單 URL 特徵。
    /// </summary>
    /// <returns>
    ///     能從 URL 確定類型時回傳資源類型字串；
    ///     無法判斷時回傳 <c>null</c>，由呼叫端 fallback 至 yt-dlp。
    /// </returns>
    private static string? TryDetectFromUrlPattern(Uri uri)
    {
        var host = uri.Host.ToLowerInvariant();
        var path = uri.AbsolutePath.ToLowerInvariant();
        var query = uri.Query;

        // ── YouTube / YouTube Music ───────────────────────────────────
        if (host.EndsWith("youtube.com") || host == "youtu.be" || host.EndsWith("music.youtube.com"))
        {
            // 明確的播放清單頁面：youtube.com/playlist?list=xxx
            if (path == "/playlist")
                return ResourceDetectionResult.GetSourceTypeName("播放清單");

            // 頻道/用戶頁面：通常為多部影片集合
            if (path.StartsWith("/channel/") ||
                path.StartsWith("/user/") ||
                path.StartsWith("/@"))
                return ResourceDetectionResult.GetSourceTypeName("播放清單");

            // youtu.be 短網址（無 list 參數）→ 單一影片
            if (host == "youtu.be" && !HasQueryParam(query, "list"))
                return ResourceDetectionResult.GetSourceTypeName("單一影片");

            // watch?v=xxx&list=yyy → 帶播放清單 context 的影片頁面，視為播放清單
            if (path == "/watch")
            {
                var hasVideo = HasQueryParam(query, "v");
                var hasList = HasQueryParam(query, "list");

                if (hasList) return ResourceDetectionResult.GetSourceTypeName("播放清單");
                if (hasVideo) return ResourceDetectionResult.GetSourceTypeName("單一影片");
            }
        }

        // ── Bilibili ─────────────────────────────────────────────────
        if (host.EndsWith("bilibili.com"))
        {
            // 合集/播放清單頁面
            if (path.StartsWith("/list/") ||
                path.Contains("/channel/") ||
                path.Contains("/medialist/"))
                return ResourceDetectionResult.GetSourceTypeName("播放清單");

            // 一般影片頁面
            if (path.StartsWith("/video/"))
                return ResourceDetectionResult.GetSourceTypeName("單一影片");
        }

        // 其他平台無法從 URL 特徵判斷，交由 yt-dlp
        return null;
    }

    /// <summary>
    ///     判斷 URL 的 query string 中是否包含指定的參數名稱。
    /// </summary>
    private static bool HasQueryParam(string query, string paramName)
    {
        if (string.IsNullOrEmpty(query))
            return false;

        // query 格式為 "?key=val&key2=val2"
        // 確保比對的是完整的 key 而非子字串（e.g. "list" 不誤判 "playlist"）
        return query.Contains($"?{paramName}=", StringComparison.OrdinalIgnoreCase) ||
               query.Contains($"&{paramName}=", StringComparison.OrdinalIgnoreCase);
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

        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
    }

    private sealed record DownloadExecutionRequest(
        string Url,
        string OutputFolder,
        string StartLogMessage,
        string SuccessMessage,
        string CanceledMessage,
        string FailurePrefix);
}