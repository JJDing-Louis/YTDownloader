# YtDlpDownloadService 使用說明

`YtDlpDownloadService` 是封裝 [ManuHub.Ytdlp.NET](https://github.com/manusoft/Ytdlp.NET) 函式庫的下載服務，  
提供影片下載、音訊提取、播放清單讀取、格式查詢等功能，並統一處理錯誤回傳與 Log 記錄。

---

## 目錄

- [前置需求](#前置需求)
- [建構子](#建構子)
- [公開方法總覽](#公開方法總覽)
- [方法詳細說明](#方法詳細說明)
    - [DownloadVideoAsync](#downloadvideoasync)
    - [DownloadAudioAsync](#downloadaudioasync)
    - [DownloadVideosAsync](#downloadvideosasync)
    - [DownloadBestMuxedVideoAsync](#downloadbestmuxedvideoasync)
    - [DownloadByDetectedTypeAsync](#downloadbydetectedtypeasync)
    - [GetPlaylistVideosAsync](#getplaylistvideosasync)
    - [GetMetadataAsync](#getmetadataasync)
    - [DetectResourceAsync](#detectresourceasync)
    - [GetFormatsAsync](#getformatsasync)
- [相關 Model](#相關-model)
- [錯誤處理](#錯誤處理)
- [注意事項](#注意事項)

---

## 前置需求

| 項目                          | 說明                                                           |
|-----------------------------|--------------------------------------------------------------|
| **yt-dlp 執行檔**              | 需事先下載並放置於固定路徑，官方下載：https://github.com/yt-dlp/yt-dlp/releases |
| **ffmpeg**（選用）              | 若需要影音合併（muxing）或音訊轉檔，需額外提供 ffmpeg 資料夾路徑                      |
| **.NET 8**                  | 目標框架 `net8.0-windows`                                        |
| **ManuHub.Ytdlp.NET 3.0.x** | NuGet 套件，已由專案引用                                              |

---

## 建構子

```csharp
public YtDlpDownloadService(string ytDlpPath, string? ffmpegFolder = null)
```

| 參數             | 型別        | 必填 | 說明                                     |
|----------------|-----------|----|----------------------------------------|
| `ytDlpPath`    | `string`  | ✅  | `yt-dlp.exe` 的完整路徑                     |
| `ffmpegFolder` | `string?` | ❌  | ffmpeg 執行檔所在的**資料夾**路徑（非執行檔路徑），留空則跳過設定 |

**例外：** `ytDlpPath` 為空白時拋出 `ArgumentException`。

```csharp
// 範例
var service = new YtDlpDownloadService(
    ytDlpPath:   @"C:\tools\yt-dlp.exe",
    ffmpegFolder: @"C:\tools\ffmpeg\bin"
);
```

---

## 公開方法總覽

| 方法                            | 回傳型別                      | 用途                  |
|-------------------------------|---------------------------|---------------------|
| `DownloadVideoAsync`          | `DownloadResult`          | 下載單一影片（或整個播放清單）     |
| `DownloadAudioAsync`          | `DownloadResult`          | 提取並下載音訊             |
| `DownloadVideosAsync`         | `BatchDownloadResult`     | 批次下載多個 URL          |
| `DownloadBestMuxedVideoAsync` | `DownloadResult`          | 自動選最佳影音格式後下載        |
| `DownloadByDetectedTypeAsync` | `DownloadResult`          | 偵測類型後自動分流下載         |
| `GetPlaylistVideosAsync`      | `PlaylistFetchResult`     | 讀取播放清單所有影片資訊供 UI 勾選 |
| `GetMetadataAsync`            | `YtDlpMetadata?`          | 取得影片或播放清單的 metadata |
| `DetectResourceAsync`         | `ResourceDetectionResult` | 判斷 URL 是單一影片或播放清單   |
| `GetFormatsAsync`             | `List<VideoFormatDto>`    | 取得可用的影音格式清單         |

---

## 方法詳細說明

---

### DownloadVideoAsync

下載單一影片。若傳入播放清單 URL，yt-dlp 會直接處理整個清單（無法中途選擇）。  
若需先讓使用者挑選清單項目，請改用 [`GetPlaylistVideosAsync`](#getplaylistvideosasync)。

```csharp
Task<DownloadResult> DownloadVideoAsync(
    string url,
    string outputFolder,
    string format              = "best",
    string? outputTemplate     = "%(title)s.%(ext)s",
    bool downloadThumbnail     = false,
    bool embedMetadata         = false,
    CancellationToken cancellationToken = default)
```

| 參數                  | 預設值                   | 說明                                            |
|---------------------|-----------------------|-----------------------------------------------|
| `url`               | —                     | 影片 URL（必填）                                    |
| `outputFolder`      | —                     | 輸出資料夾路徑，不存在時自動建立（必填）                          |
| `format`            | `"best"`              | yt-dlp 格式選擇字串，例如 `"bestvideo+bestaudio/best"` |
| `outputTemplate`    | `"%(title)s.%(ext)s"` | 輸出檔名模板，使用 yt-dlp 的 `%(field)s` 語法             |
| `downloadThumbnail` | `false`               | 是否一併下載縮圖                                      |
| `embedMetadata`     | `false`               | 是否將 metadata 嵌入影片檔案                           |

```csharp
// 範例：下載最佳畫質，並嵌入 metadata
var result = await service.DownloadVideoAsync(
    url:           "https://www.youtube.com/watch?v=xxxxxxx",
    outputFolder:  @"D:\Downloads\Videos",
    embedMetadata: true
);

if (result.IsSuccess)
    Console.WriteLine($"儲存至：{result.OutputFolder}");
else
    Console.WriteLine($"失敗：{result.Message}");
```

---

### DownloadAudioAsync

提取影片中的音訊並轉檔後儲存。

```csharp
Task<DownloadResult> DownloadAudioAsync(
    string url,
    string outputFolder,
    AudioFormat audioFormat    = AudioFormat.Mp3,
    int audioQuality           = 5,
    string? outputTemplate     = "%(title)s.%(ext)s",
    bool embedMetadata         = true,
    bool embedThumbnail        = false,
    CancellationToken cancellationToken = default)
```

| 參數               | 預設值     | 說明                                          |
|------------------|---------|---------------------------------------------|
| `audioFormat`    | `Mp3`   | 輸出音訊格式，可選 `Mp3`、`M4a`、`Flac`、`Wav`、`Opus` 等 |
| `audioQuality`   | `5`     | 音質等級，`0` 最佳，`10` 最差（VBR 模式）                 |
| `embedMetadata`  | `true`  | 是否嵌入 metadata（標題、作者等）                       |
| `embedThumbnail` | `false` | 是否將縮圖嵌入音訊檔作為封面                              |

```csharp
// 範例：下載為高品質 MP3 並嵌入封面
var result = await service.DownloadAudioAsync(
    url:            "https://www.youtube.com/watch?v=xxxxxxx",
    outputFolder:   @"D:\Downloads\Music",
    audioFormat:    AudioFormat.Mp3,
    audioQuality:   0,
    embedThumbnail: true
);
```

---

### DownloadVideosAsync

批次下載多個 URL，支援並發控制。空白或重複的 URL 會自動過濾。

```csharp
Task<BatchDownloadResult> DownloadVideosAsync(
    IEnumerable<string> urls,
    string outputFolder,
    string format        = "best",
    int maxConcurrency   = 3,
    CancellationToken cancellationToken = default)
```

| 參數               | 預設值 | 說明                       |
|------------------|-----|--------------------------|
| `urls`           | —   | URL 清單，空白項目自動略過，重複項目自動去除 |
| `maxConcurrency` | `3` | 同時下載的最大數量                |

**例外：**

- `urls` 為 `null` → `ArgumentNullException`
- 過濾後清單為空 → `ArgumentException`

```csharp
// 範例：從播放清單選取勾選項目後批次下載
var selectedUrls = playlistItems
    .Where(v => v.IsSelected && v.WebpageUrl != null)
    .Select(v => v.WebpageUrl!)
    .ToList();

var result = await service.DownloadVideosAsync(
    urls:           selectedUrls,
    outputFolder:   @"D:\Downloads\Batch",
    maxConcurrency: 2
);

Console.WriteLine($"共 {result.TotalCount} 筆，{(result.IsSuccess ? "成功" : "失敗")}");
```

---

### DownloadBestMuxedVideoAsync

先查詢最佳影像與音訊格式 ID，再以 `{bestVideo}+{bestAudio}/best` 的格式字串下載，  
自動確保影音合併、品質最佳，適合不想手動設定格式的場景。

```csharp
Task<DownloadResult> DownloadBestMuxedVideoAsync(
    string url,
    string outputFolder,
    int maxHeight                       = 1080,
    CancellationToken cancellationToken = default)
```

| 參數          | 預設值    | 說明                                 |
|-------------|--------|------------------------------------|
| `maxHeight` | `1080` | 影像解析度上限（像素），例如 `720`、`1080`、`2160` |

```csharp
// 範例：下載不超過 720p 的最佳品質影片
var result = await service.DownloadBestMuxedVideoAsync(
    url:          "https://www.youtube.com/watch?v=xxxxxxx",
    outputFolder: @"D:\Downloads\Videos",
    maxHeight:    720
);
```

> **注意：** 此方法會呼叫兩次 yt-dlp（先查格式、再下載），網路較差時耗時較長。

---

### DownloadByDetectedTypeAsync

先呼叫 [`DetectResourceAsync`](#detectresourceasync) 判斷 URL 類型，  
再交給 [`DownloadVideoAsync`](#downloadvideoasync) 下載。  
若無法判斷類型（`Unknown`），直接回傳失敗結果，不進行下載。

```csharp
Task<DownloadResult> DownloadByDetectedTypeAsync(
    string url,
    string outputFolder,
    string format              = "best",
    string? outputTemplate     = "%(title)s.%(ext)s",
    bool downloadThumbnail     = false,
    bool embedMetadata         = false,
    CancellationToken cancellationToken = default)
```

```csharp
// 範例：不管是影片還是播放清單，通通丟進來處理
var result = await service.DownloadByDetectedTypeAsync(
    url:          userInputUrl,
    outputFolder: @"D:\Downloads"
);
```

---

### GetPlaylistVideosAsync

讀取播放清單內所有影片的基本資訊，回傳 `PlaylistFetchResult`，  
讓 UI 層將結果顯示為可勾選的清單，使用者確認後再呼叫下載方法。

**單一影片 URL 也支援**，會自動包成只有一筆的清單回傳，UI 層不需特判。

```csharp
Task<PlaylistFetchResult> GetPlaylistVideosAsync(
    string url,
    IProgress<(int Current, int Total, string? CurrentTitle)>? progress = null,
    int bufferKb                        = 8192,
    CancellationToken cancellationToken = default)
```

| 參數         | 預設值    | 說明                                         |
|------------|--------|--------------------------------------------|
| `progress` | `null` | 進度回報，每解析完一筆影片時觸發，含（目前索引、總數、影片標題）           |
| `bufferKb` | `8192` | metadata 讀取緩衝，大型播放清單（100 部以上）建議調高至 `16384` |

**完整 UI 使用流程：**

```csharp
// 1. 顯示載入中，綁定進度回報
var progress = new Progress<(int Current, int Total, string? CurrentTitle)>(p =>
{
    labelStatus.Text    = $"載入中 {p.Current}/{p.Total}：{p.CurrentTitle}";
    progressBar.Maximum = p.Total;
    progressBar.Value   = p.Current;
});

// 2. 讀取播放清單
var result = await service.GetPlaylistVideosAsync(
    url:      playlistUrl,
    progress: progress,
    cancellationToken: _cts.Token
);

if (!result.IsSuccess)
{
    MessageBox.Show(result.Message);
    return;
}

// 3. 將影片清單綁定到 CheckedListBox（預設全選）
checkedListBox.Items.Clear();
foreach (var video in result.Videos)
    checkedListBox.Items.Add(video, video.IsSelected);

// 4. 使用者取消勾選後，點擊「下載」
private async void btnDownload_Click(object sender, EventArgs e)
{
    var toDownload = result.Videos
        .Where((v, i) => checkedListBox.GetItemChecked(i) && v.WebpageUrl != null)
        .Select(v => v.WebpageUrl!)
        .ToList();

    await service.DownloadVideosAsync(toDownload, outputFolder);
}
```

**`PlaylistVideoItem` 主要欄位：**

| 屬性               | 型別        | 說明                                |
|------------------|-----------|-----------------------------------|
| `Index`          | `int`     | 在播放清單中的順序（從 1 開始）                 |
| `Title`          | `string?` | 影片標題                              |
| `Uploader`       | `string?` | 上傳者 / 頻道名稱                        |
| `Duration`       | `int?`    | 長度（秒）                             |
| `DurationString` | `string`  | 格式化時間，例如 `"03:45"` 或 `"01:02:30"` |
| `Thumbnail`      | `string?` | 縮圖 URL                            |
| `WebpageUrl`     | `string?` | 影片頁面 URL（下載時使用）                   |
| `IsSelected`     | `bool`    | UI 勾選狀態，預設 `true`                 |
| `DisplayTitle`   | `string`  | 顯示用標題，為空時 fallback 至 ID           |

---

### GetMetadataAsync

取得影片或播放清單的完整 metadata，回傳自有 DTO `YtDlpMetadata`。  
失敗或 URL 無效時回傳 `null`，不拋出例外。

```csharp
Task<YtDlpMetadata?> GetMetadataAsync(
    string url,
    int bufferKb                        = 1024,
    CancellationToken cancellationToken = default)
```

```csharp
var meta = await service.GetMetadataAsync("https://www.youtube.com/watch?v=xxxxxxx");

if (meta != null)
{
    Console.WriteLine($"標題：{meta.Title}");
    Console.WriteLine($"上傳者：{meta.Uploader}");
    Console.WriteLine($"是否為播放清單：{meta.IsPlaylist}");
}
```

**`YtDlpMetadata` 主要欄位：**

| 屬性              | 說明                   |
|-----------------|----------------------|
| `Id`            | 影片 ID                |
| `Title`         | 標題                   |
| `Uploader`      | 上傳者                  |
| `Duration`      | 長度（秒）                |
| `Thumbnail`     | 縮圖 URL               |
| `WebpageUrl`    | 影片頁面 URL             |
| `PlaylistId`    | 播放清單 ID（單一影片時為 null） |
| `PlaylistTitle` | 播放清單標題               |
| `PlaylistCount` | 播放清單宣告總數             |
| `EntryCount`    | 實際解析到的 Entry 數量      |
| `IsPlaylist`    | 綜合以上欄位判斷是否為播放清單      |

---

### DetectResourceAsync

呼叫 `GetMetadataAsync` 後，根據 metadata 判斷 URL 為「單一影片」或「播放清單」，  
回傳結構化的 `ResourceDetectionResult`。

```csharp
Task<ResourceDetectionResult> DetectResourceAsync(
    string url,
    int bufferKb                        = 1024,
    CancellationToken cancellationToken = default)
```

```csharp
var detected = await service.DetectResourceAsync(userInputUrl);

switch (detected.ResourceType)
{
    case UrlResourceType.SingleVideo:
        Console.WriteLine($"單一影片：{detected.Title}");
        break;
    case UrlResourceType.Playlist:
        Console.WriteLine($"播放清單：{detected.PlaylistTitle}（共 {detected.PlaylistCount} 部）");
        break;
    case UrlResourceType.Unknown:
        Console.WriteLine($"無法判斷：{detected.Message}");
        break;
}
```

---

### GetFormatsAsync

取得指定 URL 所有可用的影音格式清單，適合讓使用者手動選擇格式後再下載。

```csharp
Task<List<VideoFormatDto>> GetFormatsAsync(
    string url,
    int bufferKb                        = 1024,
    CancellationToken cancellationToken = default)
```

失敗時回傳空清單，不拋出例外。

```csharp
var formats = await service.GetFormatsAsync("https://www.youtube.com/watch?v=xxxxxxx");

foreach (var fmt in formats)
{
    Console.WriteLine($"[{fmt.Id}] {fmt.Resolution} | 影像:{fmt.VCodec} 音訊:{fmt.ACodec} | {fmt.Extension}");
}

// 取得想要的格式 ID 後傳給 DownloadVideoAsync
var selectedFormatId = "137+140";
await service.DownloadVideoAsync(url, outputFolder, format: selectedFormatId);
```

**`VideoFormatDto` 欄位：**

| 屬性           | 說明                           |
|--------------|------------------------------|
| `Id`         | yt-dlp 格式 ID（用於 `format` 參數） |
| `Extension`  | 副檔名，例如 `mp4`、`webm`          |
| `Resolution` | 解析度字串，例如 `1920x1080`         |
| `VCodec`     | 影像編碼，例如 `avc1`、`vp9`         |
| `ACodec`     | 音訊編碼，例如 `mp4a`、`opus`        |
| `FormatNote` | 補充說明，例如 `1080p`、`DASH audio` |

---

## 相關 Model

```
YTDownloader.Model
├── DownloadResult          下載結果（IsSuccess, Url, OutputFolder, Message）
├── BatchDownloadResult     批次下載結果（IsSuccess, TotalCount, OutputFolder, Message）
├── YtDlpMetadata           影片 / 播放清單 metadata
├── ResourceDetectionResult 類型判斷結果（ResourceType, Title, PlaylistId...）
├── VideoFormatDto          單一格式資訊
├── PlaylistFetchResult     播放清單讀取結果（IsSuccess, Videos, PlaylistTitle...）
└── PlaylistVideoItem       播放清單中的單一影片（含 IsSelected 供 UI 勾選）
```

---

## 錯誤處理

所有下載與查詢方法都遵循以下統一原則：

| 情境                                        | 行為                                               |
|-------------------------------------------|--------------------------------------------------|
| `url` 為空或格式不正確                            | 直接拋出 `ArgumentException`，**不進入** yt-dlp 流程       |
| `outputFolder` 為空                         | 直接拋出 `ArgumentException`                         |
| `urls` 為 `null`（批次下載）                     | 拋出 `ArgumentNullException`                       |
| 使用者取消（`CancellationToken`）                | 回傳 `IsSuccess = false`，`Message` 說明已取消，**不拋出例外** |
| yt-dlp 執行期間發生錯誤                           | 記錄 Log，回傳 `IsSuccess = false`，**不拋出例外**          |
| `GetMetadataAsync` / `GetFormatsAsync` 失敗 | 回傳 `null` 或空清單，**不拋出例外**                         |

取消下載範例：

```csharp
private CancellationTokenSource _cts = new();

// 開始下載
var result = await service.DownloadVideoAsync(url, folder, cancellationToken: _cts.Token);

// 取消下載（例如按下「停止」按鈕）
private void btnCancel_Click(object sender, EventArgs e)
{
    _cts.Cancel();
    _cts = new CancellationTokenSource(); // 重置供下次使用
}
```

---

## 注意事項

- **yt-dlp 版本相容性：** 本服務以 `ManuHub.Ytdlp.NET 3.0.x` 測試，yt-dlp 本身請維持在較新版本以確保站台支援。
- **播放清單大型清單：** `GetPlaylistVideosAsync` 預設 `bufferKb = 8192`，清單超過 200 部時建議調高至 `16384` 以避免截斷。
- **ffmpeg 必要性：** `DownloadBestMuxedVideoAsync` 會將影像流與音訊流分開下載後合併，**必須** 提供 ffmpeg 路徑，否則合併步驟會失敗。
- **輸出資料夾：** 所有下載方法在資料夾不存在時會自動建立，無需事先手動建立。
- **執行緒安全：** 每次方法呼叫都會建立獨立的 `Ytdlp` 實例，多個下載同時執行不會互相干擾。
