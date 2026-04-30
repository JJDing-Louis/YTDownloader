using Microsoft.Extensions.Logging.Abstractions;
using YTDownloader.Model;
using YTDownloader.Service;
using System.IO;

namespace YTDownloaderTest.ServiceTest
{
    public class YtDlpDownloadServiceTest
    {
        // ── 常數 ──────────────────────────────────────────────────────────────
        /// <summary>不存在的假路徑，僅供測試「不需啟動 yt-dlp」的輸入驗證情境</summary>
        private const string FakeYtDlpPath = "fake-yt-dlp.exe";

        private const string ValidVideoUrl    = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        private const string ValidPlaylistUrl = "https://www.youtube.com/playlist?list=PLrEnWoR732-BHrPp_Pm8_VleD68f9s14-";
        private const string MembersOnlyPlaylistUrl = "https://www.youtube.com/playlist?list=PLfaO5EsbbgdnRn2P7eTAj244MLNEFYqil";
        private const string UnavailableVideoUrl = "https://www.youtube.com/watch?v=ES6JF6cGliQ&t=6909s";
        private const string InvalidUrl       = "this-is-not-a-url";
        private const string FakeOutputFolder = @"C:\FakeOutputFolder";
        private const string SourceTypePlaylist = "PlayList";
        private const string SourceTypeSingleVideo = "VideoOnly";

        private YtDlpDownloadService _service = null!;

        // ── Setup / Teardown ──────────────────────────────────────────────────

        [SetUp]
        public void Setup()
        {
            _service = new YtDlpDownloadService(
                FakeYtDlpPath,
                logger: NullLogger<YtDlpDownloadService>.Instance);
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
        [Description("播放清單 URL 應回傳多筆影片資訊，且每筆 URL 不為空")]
        public async Task GetPlaylistVideosAsync_RealPlaylistUrl_ReturnsMultipleVideos()
        {
            var service = new YtDlpDownloadService(
                GetRealYtDlpPath(),
                logger: NullLogger<YtDlpDownloadService>.Instance);

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
        [Description("單一影片 URL 傳入 GetPlaylistVideosAsync 應包成 1 筆清單回傳")]
        public async Task GetPlaylistVideosAsync_SingleVideoUrl_ReturnsOneItem()
        {
            var service = new YtDlpDownloadService(
                GetRealYtDlpPath(),
                logger: NullLogger<YtDlpDownloadService>.Instance);
            var result  = await service.GetPlaylistVideosAsync(ValidVideoUrl);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess,  Is.True);
                Assert.That(result.TotalCount, Is.EqualTo(1));
                Assert.That(result.Videos,     Has.Count.EqualTo(1));
            });
        }

        [Test]
        [Category("Integration")]
        [Description("含會員專屬影片的播放清單應跳過不可下載項目，且不回傳該影片到 Videos")]
        public async Task GetPlaylistVideosAsync_MembersOnlyPlaylist_SkipsSubscriberOnlyVideo()
        {
            var service = new YtDlpDownloadService(
                GetRealYtDlpPath(),
                logger: NullLogger<YtDlpDownloadService>.Instance);

            var result = await service.GetPlaylistVideosAsync(MembersOnlyPlaylistUrl);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Videos, Is.Not.Empty);
                Assert.That(result.UnavailableEntries, Has.Some.Contains("會員專屬"));
                Assert.That(result.Videos.Select(v => v.Id), Does.Not.Contain("wqrfsF_OMKs"));
            });
        }

        [Test]
        [Category("Integration")]
        [Description("不可存取的單一影片應回傳 SkippedCount，不應回傳可下載影片")]
        public async Task GetPlaylistVideosAsync_UnavailableSingleVideo_ReturnsSkippedEntry()
        {
            var service = new YtDlpDownloadService(
                GetRealYtDlpPath(),
                logger: NullLogger<YtDlpDownloadService>.Instance);

            var result = await service.GetPlaylistVideosAsync(UnavailableVideoUrl);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Videos, Is.Empty);
                Assert.That(result.SkippedCount, Is.EqualTo(1));
                Assert.That(result.UnavailableEntries, Has.Some.Contains("[Unavailable video]"));
            });
        }

        // =====================================================================
        // URL 型別快速偵測 — DetectResourceAsync 快速路徑（不呼叫 yt-dlp）
        // =====================================================================
        // 快速路徑（TryDetectFromUrlPattern）純解析 URL，不啟動 yt-dlp 行程，
        // 因此即使 FakeYtDlpPath 指向不存在的檔案，這些測試也能正常執行。

        [Test]
        [TestCase(
            "https://www.youtube.com/playlist?list=PLEKDwEUpZdpN1Ho1W5nQB368PnN53llE3",
            SourceTypePlaylist,
            Description = "playlist?list= 格式（150 首夜店歌曲清單）應判斷為 Playlist")]
        [TestCase(
            "https://www.youtube.com/watch?v=uEJuoEs1UxY&list=PLEKDwEUpZdpN1Ho1W5nQB368PnN53llE3",
            SourceTypePlaylist,
            Description = "watch?v=&list= 同時帶有 list 參數應判斷為 Playlist")]
        [TestCase(
            "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            SourceTypeSingleVideo,
            Description = "watch?v= 無 list 參數應判斷為 SingleVideo")]
        [TestCase(
            "https://youtu.be/dQw4w9WgXcQ",
            SourceTypeSingleVideo,
            Description = "youtu.be 短網址無 list 參數應判斷為 SingleVideo")]
        [TestCase(
            MembersOnlyPlaylistUrl,
            SourceTypePlaylist,
            Description = "使用者提供的會員專屬混合播放清單 URL 應判斷為 Playlist")]
        [TestCase(
            UnavailableVideoUrl,
            SourceTypeSingleVideo,
            Description = "使用者提供的不可存取 watch URL（含 t 參數）應判斷為 SingleVideo")]
        [TestCase(
            "https://www.youtube.com/@SomeChannel/videos",
            SourceTypePlaylist,
            Description = "@ 頻道網址應判斷為 Playlist")]
        [TestCase(
            "https://www.youtube.com/channel/UCxxxxxxxxxxxxxxxxxxxxxx",
            SourceTypePlaylist,
            Description = "/channel/ 網址應判斷為 Playlist")]
        [TestCase(
            "https://www.youtube.com/user/SomeLegacyUser",
            SourceTypePlaylist,
            Description = "/user/ 舊式頻道網址應判斷為 Playlist")]
        public async Task DetectResourceAsync_QuickPath_ReturnsCorrectResourceType(
            string url, string expectedType)
        {
            var result = await _service.DetectResourceAsync(url);

            Assert.That(result.ResourceType, Is.EqualTo(expectedType),
                $"URL [{url}] 預期 {expectedType}，實際為 {result.ResourceType}（訊息：{result.Message}）");
        }

        // =====================================================================
        // IsUnavailableTitle — 不可播放條目標題過濾器
        // =====================================================================

        [Test]
        [TestCase("[Deleted video]",                         true,  Description = "已刪除影片應被標記為不可播放")]
        [TestCase("[Private video]",                         true,  Description = "私人影片應被標記為不可播放")]
        [TestCase("[Unavailable video]",                     true,  Description = "不可用影片應被標記為不可播放")]
        [TestCase("[Removed video]",                         true,  Description = "已移除影片應被標記為不可播放")]
        [TestCase("[Some Future Placeholder]",               true,  Description = "通用 [...] 格式應被通用規則標記為不可播放")]
        [TestCase("正常影片標題",                              false, Description = "正常中文標題不應被誤判")]
        [TestCase("Rick Astley - Never Gonna Give You Up",  false, Description = "正常英文標題不應被誤判")]
        [TestCase("[Brackets in Title] 但後面有文字",          false, Description = "開頭有 [ 但非獨立 [...] 結尾的標題不應被誤判")]
        [TestCase("",                                        false, Description = "空字串不應被標記為不可播放")]
        [TestCase(null,                                      false, Description = "null 不應被標記為不可播放")]
        [TestCase("   ",                                     false, Description = "純空白不應被標記為不可播放")]
        public void IsUnavailableTitle_VariousTitles_ReturnsExpected(string? title, bool expected)
        {
            bool actual = YtDlpDownloadService.IsUnavailableTitle(title);

            Assert.That(actual, Is.EqualTo(expected),
                $"標題 [{title ?? "(null)"}] 預期 IsUnavailable={expected}，實際={actual}");
        }

        [Test]
        [Description("會員專屬影片在 flat playlist 中仍有標題，但 availability=subscriber_only 時應排除")]
        public void IsUnavailablePlaylistEntry_SubscriberOnly_ReturnsTrue()
        {
            var actual = YtDlpDownloadService.IsUnavailablePlaylistEntry(
                "[Japanese-style jazz girl x relaxing BGM] 4Costumes&4-Scenes / Relaxing Japanese jazz lofi",
                "subscriber_only",
                out var reason);

            Assert.Multiple(() =>
            {
                Assert.That(actual, Is.True);
                Assert.That(reason, Is.EqualTo("會員專屬"));
            });
        }

        [Test]
        [Description("一般可公開觀看影片不應被不可播放條目過濾器排除")]
        public void IsUnavailablePlaylistEntry_PublicVideo_ReturnsFalse()
        {
            var actual = YtDlpDownloadService.IsUnavailablePlaylistEntry(
                "正常影片標題",
                null,
                out var reason);

            Assert.Multiple(() =>
            {
                Assert.That(actual, Is.False);
                Assert.That(reason, Is.Empty);
            });
        }

        // =====================================================================
        // PlaylistFetchResult — UnavailableEntries 與 SkippedCount
        // =====================================================================

        [Test]
        [Description("加入不可播放條目後 SkippedCount 應等於 UnavailableEntries 的數量")]
        public void PlaylistFetchResult_SkippedCount_ReflectsUnavailableEntriesCount()
        {
            var result = PlaylistFetchResult.Success(null, null, new List<PlaylistVideoItem>());
            result.UnavailableEntries.Add("#113：[Deleted video]");
            result.UnavailableEntries.Add("#116：[Private video]");

            Assert.That(result.SkippedCount, Is.EqualTo(2));
        }

        [Test]
        [Description("沒有不可播放條目時 SkippedCount 應為 0")]
        public void PlaylistFetchResult_SkippedCount_EmptyEntries_IsZero()
        {
            var result = PlaylistFetchResult.Success(null, null, new List<PlaylistVideoItem>());
            Assert.That(result.SkippedCount, Is.EqualTo(0));
        }

        [Test]
        [Description("UnavailableEntries 的條目描述應包含播放清單位置與原始佔位標題")]
        public void PlaylistFetchResult_UnavailableEntries_ContainsExpectedDescriptions()
        {
            var result = new PlaylistFetchResult();
            result.UnavailableEntries.Add("#113：[Deleted video]");
            result.UnavailableEntries.Add("#116：[Private video]");

            Assert.Multiple(() =>
            {
                Assert.That(result.UnavailableEntries, Has.Count.EqualTo(2));
                Assert.That(result.UnavailableEntries[0], Does.Contain("[Deleted video]"));
                Assert.That(result.UnavailableEntries[1], Does.Contain("[Private video]"));
            });
        }

        [Test]
        [Description("IsSuccess=true 的結果仍可帶有不可播放條目（部分可用清單）")]
        public void PlaylistFetchResult_Success_CanHaveUnavailableEntries()
        {
            var videos = new List<PlaylistVideoItem>
            {
                new() { Id = "vid1", Title = "正常影片" }
            };
            var result = PlaylistFetchResult.Success("PLtest", "測試清單", videos);
            result.UnavailableEntries.Add("#5：[Deleted video]");

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess,    Is.True);
                Assert.That(result.TotalCount,   Is.EqualTo(1));
                Assert.That(result.SkippedCount, Is.EqualTo(1));
            });
        }

        // =====================================================================
        // ffmpeg 路徑處理 — exe 路徑 → 資料夾路徑
        // =====================================================================

        [Test]
        [Description("ffmpeg.exe 完整路徑透過 Path.GetDirectoryName 應回傳所在資料夾，不含檔名")]
        public void FfmpegPath_GetDirectoryName_ReturnsParentFolder()
        {
            const string ffmpegExePath = @"C:\tools\ffmpeg\bin\ffmpeg.exe";
            var dir = Path.GetDirectoryName(ffmpegExePath);

            Assert.That(dir, Is.EqualTo(@"C:\tools\ffmpeg\bin"));
        }

        [Test]
        [Description("傳入 ffmpeg.exe 路徑時，取得 Directory 後應與直接傳資料夾的結果相同")]
        public void FfmpegPath_GetDirectoryName_MatchesFolderPath()
        {
            const string ffmpegExe  = @"D:\tools\ffmpeg.exe";
            const string ffmpegDir  = @"D:\tools";

            var derived = Path.GetDirectoryName(ffmpegExe);
            Assert.That(derived, Is.EqualTo(ffmpegDir));
        }

        [Test]
        [Description("建立 YtDlpDownloadService 時傳入資料夾路徑作為 ffmpegFolder，不應拋出例外")]
        public void Constructor_WithFfmpegFolderPath_DoesNotThrow()
        {
            const string ffmpegDir = @"C:\tools\ffmpeg\bin";
            Assert.DoesNotThrow(() => new YtDlpDownloadService(FakeYtDlpPath, ffmpegDir));
        }

        /// <summary>
        ///     取得執行環境下實際的 yt-dlp 路徑（供整合測試使用）。
        ///     搜尋順序：
        ///       1. 測試輸出目錄（dotnet build CopyToOutputDirectory=Always 的位置）
        ///       2. PATH 環境變數
        ///       3. Fallback 常見手動安裝位置
        /// </summary>
        private static string GetRealYtDlpPath()
        {
            const string ytDlpExe = "yt-dlp.exe";

            // 1. 優先查詢測試執行環境的輸出目錄
            //    yt-dlp.exe 透過主專案的 CopyToOutputDirectory=Always 複製至此。
            var outputDirCandidates = new[]
            {
                Path.Combine(TestContext.CurrentContext.TestDirectory, ytDlpExe),
                Path.Combine(AppContext.BaseDirectory,                 ytDlpExe),
            };

            var fromOutputDir = outputDirCandidates.FirstOrDefault(File.Exists);
            if (fromOutputDir != null)
                return fromOutputDir;

            // 2. 嘗試 PATH 中的 yt-dlp
            var fromPath = Environment.GetEnvironmentVariable("PATH")?
                .Split(Path.PathSeparator)
                .Select(dir => Path.Combine(dir, ytDlpExe))
                .FirstOrDefault(File.Exists);

            if (fromPath != null)
                return fromPath;

            // 3. Fallback：常見手動安裝位置
            return @"C:\yt-dlp\yt-dlp.exe";
        }
    }
}
