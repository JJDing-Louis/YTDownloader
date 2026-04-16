using YTDownloader;
using YTDownloader.Model;
using YTDownloader.Service;

namespace YTDownloaderTest.ServiceTest
{
    public class YtDlpDownloadServiceTest
    {
        // ── 常數 ──────────────────────────────────────────────────────────────
        /// <summary>不存在的假路徑，僅供測試「不需啟動 yt-dlp」的輸入驗證情境</summary>
        private const string FakeYtDlpPath = "fake-yt-dlp.exe";

        private const string ValidVideoUrl    = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        private const string ValidPlaylistUrl = "https://www.youtube.com/playlist?list=PLrEnWoR732-BHrPp_Pm8_VleD68f9s14-";
        private const string InvalidUrl       = "this-is-not-a-url";
        private const string FakeOutputFolder = @"C:\FakeOutputFolder";

        private YtDlpDownloadService _service = null!;

        // ── Setup / Teardown ──────────────────────────────────────────────────

        [OneTimeSetUp]
        public static void OneTimeSetup()
        {
            Program.Startup.Run();
        }

        [SetUp]
        public void Setup()
        {
            _service = new YtDlpDownloadService(FakeYtDlpPath);
        }

        // =====================================================================
        // Constructor
        // =====================================================================

        [Test]
        [Description("空字串的 yt-dlp 路徑應拋出 ArgumentException")]
        public void Constructor_EmptyYtDlpPath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new YtDlpDownloadService(""));
        }

        [Test]
        [Description("純空白的 yt-dlp 路徑應拋出 ArgumentException")]
        public void Constructor_WhitespacePath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new YtDlpDownloadService("   "));
        }

        [Test]
        [Description("合法路徑（不論檔案是否存在）不應在建構時拋出例外")]
        public void Constructor_ValidPath_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new YtDlpDownloadService(FakeYtDlpPath));
        }

        [Test]
        [Description("指定 ffmpegFolder 的多載建構不應拋出例外")]
        public void Constructor_WithFfmpegFolder_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new YtDlpDownloadService(FakeYtDlpPath, @"C:\ffmpeg\bin"));
        }

        // =====================================================================
        // DownloadVideoAsync — 輸入驗證
        // =====================================================================

        [Test]
        [Description("空白 URL 應拋出 ArgumentException，不應進入下載流程")]
        public void DownloadVideoAsync_EmptyUrl_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DownloadVideoAsync("", FakeOutputFolder));
        }

        [Test]
        [Description("格式不正確的 URL 應拋出 ArgumentException")]
        public void DownloadVideoAsync_InvalidUrl_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DownloadVideoAsync(InvalidUrl, FakeOutputFolder));
        }

        [Test]
        [Description("空白的輸出資料夾路徑應拋出 ArgumentException")]
        public void DownloadVideoAsync_EmptyOutputFolder_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DownloadVideoAsync(ValidVideoUrl, ""));
        }

        // =====================================================================
        // DownloadAudioAsync — 輸入驗證
        // =====================================================================

        [Test]
        [Description("空白 URL 應拋出 ArgumentException")]
        public void DownloadAudioAsync_EmptyUrl_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DownloadAudioAsync("", FakeOutputFolder));
        }

        [Test]
        [Description("格式不正確的 URL 應拋出 ArgumentException")]
        public void DownloadAudioAsync_InvalidUrl_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DownloadAudioAsync(InvalidUrl, FakeOutputFolder));
        }

        [Test]
        [Description("空白的輸出資料夾路徑應拋出 ArgumentException")]
        public void DownloadAudioAsync_EmptyOutputFolder_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DownloadAudioAsync(ValidVideoUrl, ""));
        }

        // =====================================================================
        // DownloadVideosAsync — 輸入驗證
        // =====================================================================

        [Test]
        [Description("null 的 URL 清單應拋出 ArgumentNullException")]
        public void DownloadVideosAsync_NullUrls_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.DownloadVideosAsync(null!, FakeOutputFolder));
        }

        [Test]
        [Description("空的 URL 清單應拋出 ArgumentException")]
        public void DownloadVideosAsync_EmptyList_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DownloadVideosAsync(new List<string>(), FakeOutputFolder));
        }

        [Test]
        [Description("全部都是空白的 URL 清單，Distinct + Where 後為空，應拋出 ArgumentException")]
        public void DownloadVideosAsync_AllBlankUrls_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DownloadVideosAsync(new[] { "", "   ", "" }, FakeOutputFolder));
        }

        // =====================================================================
        // GetMetadataAsync — 輸入驗證
        // =====================================================================

        [Test]
        [Description("空白 URL 應拋出 ArgumentException")]
        public void GetMetadataAsync_EmptyUrl_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _service.GetMetadataAsync(""));
        }

        [Test]
        [Description("格式不正確的 URL 應拋出 ArgumentException")]
        public void GetMetadataAsync_InvalidUrl_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _service.GetMetadataAsync(InvalidUrl));
        }

        // =====================================================================
        // DetectResourceAsync — 輸入驗證
        // =====================================================================

        [Test]
        [Description("空白 URL 應拋出 ArgumentException")]
        public void DetectResourceAsync_EmptyUrl_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _service.DetectResourceAsync(""));
        }

        [Test]
        [Description("格式不正確的 URL 應拋出 ArgumentException")]
        public void DetectResourceAsync_InvalidUrl_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _service.DetectResourceAsync(InvalidUrl));
        }

        // =====================================================================
        // GetFormatsAsync — 輸入驗證
        // =====================================================================

        [Test]
        [Description("空白 URL 應拋出 ArgumentException")]
        public void GetFormatsAsync_EmptyUrl_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _service.GetFormatsAsync(""));
        }

        [Test]
        [Description("格式不正確的 URL 應拋出 ArgumentException")]
        public void GetFormatsAsync_InvalidUrl_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _service.GetFormatsAsync(InvalidUrl));
        }

        // =====================================================================
        // GetPlaylistVideosAsync — 輸入驗證
        // =====================================================================

        [Test]
        [Description("空白 URL 應拋出 ArgumentException")]
        public void GetPlaylistVideosAsync_EmptyUrl_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _service.GetPlaylistVideosAsync(""));
        }

        [Test]
        [Description("格式不正確的 URL 應拋出 ArgumentException")]
        public void GetPlaylistVideosAsync_InvalidUrl_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _service.GetPlaylistVideosAsync(InvalidUrl));
        }

        // =====================================================================
        // DownloadBestMuxedVideoAsync — 輸入驗證
        // =====================================================================

        [Test]
        [Description("空白 URL 應拋出 ArgumentException")]
        public void DownloadBestMuxedVideoAsync_EmptyUrl_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DownloadBestMuxedVideoAsync("", FakeOutputFolder));
        }

        [Test]
        [Description("空白輸出資料夾應拋出 ArgumentException")]
        public void DownloadBestMuxedVideoAsync_EmptyOutputFolder_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DownloadBestMuxedVideoAsync(ValidVideoUrl, ""));
        }

        // =====================================================================
        // PlaylistVideoItem — Model 行為測試
        // =====================================================================

        [Test]
        [TestCase(null,  "--:--",    Description = "Duration 為 null 時應顯示預設值")]
        [TestCase(0,     "--:--",    Description = "Duration 為 0 時應顯示預設值")]
        [TestCase(-1,    "--:--",    Description = "Duration 為負數時應顯示預設值")]
        [TestCase(59,    "00:59",    Description = "不足一分鐘")]
        [TestCase(65,    "01:05",    Description = "一分五秒")]
        [TestCase(3599,  "59:59",    Description = "不足一小時")]
        [TestCase(3600,  "01:00:00", Description = "剛好一小時，需包含小時段")]
        [TestCase(3661,  "01:01:01", Description = "一小時一分一秒")]
        [TestCase(36000, "10:00:00", Description = "十小時整")]
        public void PlaylistVideoItem_DurationString_ReturnsCorrectFormat(int? duration, string expected)
        {
            var item = new PlaylistVideoItem { Duration = duration };
            Assert.That(item.DurationString, Is.EqualTo(expected));
        }

        [Test]
        [Description("有標題時 DisplayTitle 應回傳標題")]
        public void PlaylistVideoItem_DisplayTitle_WithTitle_ReturnsTitle()
        {
            var item = new PlaylistVideoItem { Id = "abc123", Title = "測試影片標題" };
            Assert.That(item.DisplayTitle, Is.EqualTo("測試影片標題"));
        }

        [Test]
        [Description("標題為 null 時 DisplayTitle 應 fallback 回 Id")]
        public void PlaylistVideoItem_DisplayTitle_NullTitle_ReturnsId()
        {
            var item = new PlaylistVideoItem { Id = "abc123", Title = null };
            Assert.That(item.DisplayTitle, Is.EqualTo("abc123"));
        }

        [Test]
        [Description("標題為空白時 DisplayTitle 應 fallback 回 Id")]
        public void PlaylistVideoItem_DisplayTitle_WhitespaceTitle_ReturnsId()
        {
            var item = new PlaylistVideoItem { Id = "abc123", Title = "   " };
            Assert.That(item.DisplayTitle, Is.EqualTo("abc123"));
        }

        [Test]
        [Description("Id 與 Title 皆為 null 時應回傳固定的 fallback 字串")]
        public void PlaylistVideoItem_DisplayTitle_BothNull_ReturnsFallback()
        {
            var item = new PlaylistVideoItem { Id = null, Title = null };
            Assert.That(item.DisplayTitle, Is.EqualTo("(未知標題)"));
        }

        [Test]
        [Description("新建的 PlaylistVideoItem，IsSelected 預設應為 true")]
        public void PlaylistVideoItem_IsSelected_DefaultTrue()
        {
            var item = new PlaylistVideoItem();
            Assert.That(item.IsSelected, Is.True);
        }

        // =====================================================================
        // PlaylistFetchResult — Factory 方法測試
        // =====================================================================

        [Test]
        [Description("Success Factory 應正確設定所有欄位")]
        public void PlaylistFetchResult_Success_SetsAllFields()
        {
            var videos = new List<PlaylistVideoItem>
            {
                new() { Id = "1", Title = "影片一" },
                new() { Id = "2", Title = "影片二" }
            };

            var result = PlaylistFetchResult.Success("PLtest", "測試播放清單", videos);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess,      Is.True);
                Assert.That(result.PlaylistId,     Is.EqualTo("PLtest"));
                Assert.That(result.PlaylistTitle,  Is.EqualTo("測試播放清單"));
                Assert.That(result.TotalCount,     Is.EqualTo(2));
                Assert.That(result.Videos,         Has.Count.EqualTo(2));
                Assert.That(result.Message,        Does.Contain("2"));
            });
        }

        [Test]
        [Description("Success Factory 帶入自訂訊息時應使用自訂訊息")]
        public void PlaylistFetchResult_Success_CustomMessage_UsesCustomMessage()
        {
            var videos = new List<PlaylistVideoItem> { new() { Id = "1", Title = "影片" } };
            var result = PlaylistFetchResult.Success("PLtest", "清單", videos, "自訂成功訊息");

            Assert.That(result.Message, Is.EqualTo("自訂成功訊息"));
        }

        [Test]
        [Description("Success Factory 傳入空清單時 TotalCount 應為 0")]
        public void PlaylistFetchResult_Success_EmptyVideos_TotalCountIsZero()
        {
            var result = PlaylistFetchResult.Success(null, null, new List<PlaylistVideoItem>());

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess,  Is.True);
                Assert.That(result.TotalCount, Is.EqualTo(0));
                Assert.That(result.Videos,     Is.Empty);
            });
        }

        [Test]
        [Description("Fail Factory 應正確設定失敗旗標與訊息")]
        public void PlaylistFetchResult_Fail_SetsIsSuccessFalseAndMessage()
        {
            var result = PlaylistFetchResult.Fail("發生錯誤：找不到播放清單");

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Message,   Is.EqualTo("發生錯誤：找不到播放清單"));
                Assert.That(result.Videos,    Is.Empty);
            });
        }

        [Test]
        [Description("Fail Factory 的 TotalCount 預設應為 0")]
        public void PlaylistFetchResult_Fail_TotalCountIsZero()
        {
            var result = PlaylistFetchResult.Fail("錯誤");
            Assert.That(result.TotalCount, Is.EqualTo(0));
        }

        // =====================================================================
        // 整合測試（需要實際 yt-dlp 執行檔，預設略過）
        // =====================================================================

        [Test]
        [Category("Integration")]
        [Ignore("需要實際 yt-dlp 執行檔，請手動執行整合測試")]
        [Description("對真實 YouTube URL 取得 metadata，驗證基本欄位不為空")]
        public async Task GetMetadataAsync_RealUrl_ReturnsMetadata()
        {
            var service = new YtDlpDownloadService(GetRealYtDlpPath());
            var result = await service.GetMetadataAsync(ValidVideoUrl);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Title, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        [Category("Integration")]
        //[Ignore("需要實際 yt-dlp 執行檔，請手動執行整合測試")]
        [Description("播放清單 URL 應回傳多筆影片資訊，且每筆 URL 不為空")]
        public async Task GetPlaylistVideosAsync_RealPlaylistUrl_ReturnsMultipleVideos()
        {
            var service = new YtDlpDownloadService(GetRealYtDlpPath());

            var progressLog = new List<(int, int, string?)>();
            var progress = new Progress<(int Current, int Total, string? CurrentTitle)>(p =>
                progressLog.Add(p));

            var result = await service.GetPlaylistVideosAsync(ValidPlaylistUrl, progress);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess,  Is.True,       "IsSuccess 應為 true");
                Assert.That(result.Videos,     Is.Not.Empty,  "影片清單不應為空");
                Assert.That(result.TotalCount, Is.GreaterThan(0));
                Assert.That(result.Videos.All(v => v.WebpageUrl != null), Is.True, "所有影片應有 URL");
                Assert.That(result.Videos.All(v => v.IsSelected), Is.True, "所有影片預設應為已勾選");
                Assert.That(progressLog, Is.Not.Empty, "應有進度回報記錄");
            });
        }

        [Test]
        [Category("Integration")]
        //[Ignore("需要實際 yt-dlp 執行檔，請手動執行整合測試")]
        [Description("單一影片 URL 傳入 GetPlaylistVideosAsync 應包成 1 筆清單回傳")]
        public async Task GetPlaylistVideosAsync_SingleVideoUrl_ReturnsOneItem()
        {
            var service = new YtDlpDownloadService(GetRealYtDlpPath());
            var result  = await service.GetPlaylistVideosAsync(ValidVideoUrl);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess,  Is.True);
                Assert.That(result.TotalCount, Is.EqualTo(1));
                Assert.That(result.Videos,     Has.Count.EqualTo(1));
            });
        }

        /// <summary>
        /// 取得系統上實際的 yt-dlp 路徑（供整合測試使用）。
        /// 可依實際環境修改此路徑。
        /// </summary>
        private static string GetRealYtDlpPath()
        {
            // 優先嘗試 PATH 中的 yt-dlp
            var candidates = new[] { "yt-dlp", "yt-dlp.exe" };
            foreach (var candidate in candidates)
            {
                var resolved = Environment.GetEnvironmentVariable("PATH")?
                    .Split(Path.PathSeparator)
                    .Select(dir => Path.Combine(dir, candidate))
                    .FirstOrDefault(File.Exists);

                if (resolved != null)
                    return resolved;
            }

            // fallback：常見安裝位置
            return @"C:\yt-dlp\yt-dlp.exe";
        }
    }
}
