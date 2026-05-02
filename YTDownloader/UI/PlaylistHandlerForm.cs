using Autofac;
using JJNET.Utility.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using YTDownloader.Model;
using YTDownloader.Service;
using YTDownloader.Tool;

namespace YTDownloader;

public partial class PlaylistHandlerForm : Form
{
    private const string OptionListMediaType = "ListMediaType";
    private const string SelectColumnName = "colSelected";
    private readonly ConfigService _configService;
    private readonly YtDlpDownloadService? _downloadService;
    private readonly ILogger _logger;
    private readonly ConfigModel _settings;
    private readonly MainForm mainForm = null!;
    private readonly string mediaType = string.Empty;
    private readonly string mediaTypeDisplay = string.Empty;
    private readonly string playlistUrl = string.Empty;
    private bool _isSelectAllChecked;
    private bool _updatingSelectAllState;
    private IConfiguration config = null!;
    private string downloadDir = string.Empty;
    private string ffmpegPath = string.Empty;
    private List<PlaylistVideoItem> playlistVideos = new();
    private string ytDlpPath = string.Empty;

    public PlaylistHandlerForm() : this(
        new ConfigService(),
        NullLogger<PlaylistHandlerForm>.Instance,
        null)
    {
    }

    public PlaylistHandlerForm(
        ConfigService configService,
        ILogger<PlaylistHandlerForm> logger,
        YtDlpDownloadService? downloadService = null)
    {
        _configService = configService;
        _downloadService = downloadService;
        _settings = _configService.Load();
        _logger = logger;
        InitializeForm();
    }

    public PlaylistHandlerForm(string playlistUrl, MainForm mainForm, string mediaType, string mediaTypeDisplay) :
        this()
    {
        this.playlistUrl = playlistUrl;
        this.mainForm = mainForm;
        this.mediaType = OptionService.GetOptionName(OptionListMediaType, mediaType);
        this.mediaTypeDisplay = string.IsNullOrWhiteSpace(mediaTypeDisplay)
            ? OptionService.GetOptionDesc(OptionListMediaType, this.mediaType)
            : mediaTypeDisplay;
    }

    public PlaylistHandlerForm(
        ConfigService configService,
        ILogger<PlaylistHandlerForm> logger,
        YtDlpDownloadService? downloadService,
        string playlistUrl,
        MainForm mainForm,
        string mediaType,
        string mediaTypeDisplay) : this(configService, logger, downloadService)
    {
        this.playlistUrl = playlistUrl;
        this.mainForm = mainForm;
        this.mediaType = OptionService.GetOptionName(OptionListMediaType, mediaType);
        this.mediaTypeDisplay = string.IsNullOrWhiteSpace(mediaTypeDisplay)
            ? OptionService.GetOptionDesc(OptionListMediaType, this.mediaType)
            : mediaTypeDisplay;
    }

    private void InitializeForm()
    {
        if (!IsInDesignMode())
            GUITool.ApplyStartupFont(this, _settings);

        InitializeComponent();

        if (IsInDesignMode())
            return;

        GUITool.Apply(this, _settings);
        _logger.LogInformation("PlaylistHandlerForm form initialized.");
        Init();
    }

    private static bool IsInDesignMode()
    {
        return System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;
    }

    #region Public Methods

    /// <summary>
    ///     非同步取得播放清單資訊，並填入 DataGridView。
    ///     避免在 UI Thread 上使用 .Result/.GetAwaiter().GetResult()，防止 async deadlock。
    /// </summary>
    /// <returns>(IsSuccess, Message)</returns>
    public async Task<(bool IsSuccess, string Message)> GetPlaylistInfoAsync()
    {
        try
        {
            var ffmpegDir = File.Exists(ffmpegPath)
                ? Path.GetDirectoryName(ffmpegPath)!
                : ffmpegPath;
            var YTDownloadService = _downloadService ?? new YtDlpDownloadService(ytDlpPath, ffmpegDir);
            var playlist = await YTDownloadService.GetPlaylistVideosAsync(playlistUrl);
            dGV_PlayList.Rows.Clear();
            if (playlist.Videos != null && playlist.Videos.Count > 0)
            {
                //暫存目前取得的播放清單媒體資訊
                playlistVideos = playlist.Videos.DeepClone();
                foreach (var video in playlistVideos) video.IsSelected = false;

                //UI顯示
                foreach (var video in playlist.Videos)
                    dGV_PlayList.Rows.Add(
                        false, // CheckBox 欄
                        video.Index, // #
                        video.Id, //Id（隱藏欄位）
                        video.DisplayTitle, // 標題（自動 fallback 到 ID）
                        video.Uploader, // 頻道
                        video.DurationString, // 時長（mm:ss / hh:mm:ss）
                        video.WebpageUrl // 連結（隱藏欄位）
                    );
            }

            UpdateSelectAllCheckBoxState();

            // 若有不可播放的影片（私人 / 刪除 / 地區限制），跳出一次性提示
            if (playlist.IsSuccess && playlist.SkippedCount > 0)
            {
                var privateCount = playlist.UnavailableEntries
                    .Count(e => e.Contains("[Private video]", StringComparison.OrdinalIgnoreCase));
                var subscriberOnlyCount = playlist.UnavailableEntries
                    .Count(e => e.Contains("會員專屬", StringComparison.OrdinalIgnoreCase));

                ShowPlaylistUnavailableSummary(privateCount, subscriberOnlyCount, playlist.TotalCount);
            }

            return (playlist.IsSuccess, playlist.Message ?? string.Empty);
        }
        catch (Exception ex)
        {
            return (false, $"獲取播放列表資訊時發生錯誤: {ex.Message}");
        }
    }

    #endregion

    #region Init

    private void Init()
    {
        InitConfig();
    }

    private void PlayList_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.ColumnIndex < 0 || dGV_PlayList.Columns[e.ColumnIndex].Name != SelectColumnName) return;

        SetAllRowsSelected(!_isSelectAllChecked);
    }

    private void SetAllRowsSelected(bool isSelected)
    {
        dGV_PlayList.EndEdit();
        _updatingSelectAllState = true;
        foreach (DataGridViewRow row in dGV_PlayList.Rows)
            if (!row.IsNewRow)
                row.Cells[SelectColumnName].Value = isSelected;

        _updatingSelectAllState = false;

        _isSelectAllChecked = isSelected;
        dGV_PlayList.InvalidateCell(dGV_PlayList.Columns[SelectColumnName].Index, -1);
    }

    private void PlayList_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        if (dGV_PlayList.IsCurrentCellDirty &&
            dGV_PlayList.CurrentCell?.OwningColumn?.Name == SelectColumnName)
            dGV_PlayList.CommitEdit(DataGridViewDataErrorContexts.Commit);
    }

    private void PlayList_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 ||
            e.ColumnIndex < 0 ||
            dGV_PlayList.Columns[e.ColumnIndex].Name != SelectColumnName ||
            _updatingSelectAllState)
            return;

        UpdateSelectAllCheckBoxState();
    }

    private void UpdateSelectAllCheckBoxState()
    {
        _isSelectAllChecked = dGV_PlayList.Rows.Count > 0 &&
                              dGV_PlayList.Rows
                                  .Cast<DataGridViewRow>()
                                  .Where(row => !row.IsNewRow)
                                  .All(row => Convert.ToBoolean(row.Cells[SelectColumnName].Value));
        dGV_PlayList.InvalidateCell(dGV_PlayList.Columns[SelectColumnName].Index, -1);
    }

    private void PlayList_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex != -1 ||
            e.ColumnIndex < 0 ||
            dGV_PlayList.Columns[e.ColumnIndex].Name != SelectColumnName)
            return;

        if (e.Graphics == null) return;

        e.Paint(e.CellBounds, e.PaintParts & ~DataGridViewPaintParts.ContentForeground);

        const int checkBoxSize = 14;
        const int gap = 4;
        var text = "全選";
        var cellStyle = e.CellStyle ?? dGV_PlayList.ColumnHeadersDefaultCellStyle;
        var font = cellStyle.Font ?? dGV_PlayList.Font;
        var textSize = TextRenderer.MeasureText(text, font);
        var totalWidth = checkBoxSize + gap + textSize.Width;
        var checkBoxLocation = new Point(
            e.CellBounds.Left + Math.Max(0, (e.CellBounds.Width - totalWidth) / 2),
            e.CellBounds.Top + (e.CellBounds.Height - checkBoxSize) / 2);
        var textLocation = new Point(
            checkBoxLocation.X + checkBoxSize + gap,
            e.CellBounds.Top + (e.CellBounds.Height - textSize.Height) / 2);

        ControlPaint.DrawCheckBox(
            e.Graphics,
            new Rectangle(checkBoxLocation, new Size(checkBoxSize, checkBoxSize)),
            _isSelectAllChecked ? ButtonState.Checked : ButtonState.Normal);

        TextRenderer.DrawText(
            e.Graphics,
            text,
            font,
            textLocation,
            cellStyle.ForeColor,
            TextFormatFlags.NoPadding);

        e.Handled = true;
    }

    private void InitConfig()
    {
        _logger.LogInformation("Initializing configuration...");
        config = ParameterTool.GetConfiguration();

        if (config == null)
        {
            _logger.LogError("Configuration object is null.");
            MessageBox.Show("載入設定失敗（config 為 null）。", "初始化錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // 以安全方式讀取設定 (支援 appsettings.json 的 "yt-dlp"/"ffmpeg" 與原本預期的 "ytDlpPath"/"ffmpegPath")
        var ytDlpRel = config["Path:yt-dlp"];
        var ffmpegRel = config["Path:ffmpeg"];

        if (string.IsNullOrWhiteSpace(ytDlpRel))
        {
            _logger.LogError("yt-dlp path is not configured (Path:yt-dlp or Path:ytDlpPath).");
            MessageBox.Show("yt-dlp 路徑未在設定中指定。", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            ytDlpPath = Path.Combine(Environment.CurrentDirectory, ytDlpRel.Trim());
            if (!File.Exists(ytDlpPath))
            {
                _logger.LogError("yt-dlp executable not found at path: {Path}", ytDlpPath);
                MessageBox.Show($"yt-dlp 可執行檔未找到，請確認路徑：{ytDlpPath}", "配置錯誤", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        if (string.IsNullOrWhiteSpace(ffmpegRel))
        {
            _logger.LogError("ffmpeg path is not configured (Path:ffmpeg or Path:ffmpegPath).");
            MessageBox.Show("ffmpeg 路徑未在設定中指定。", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            ffmpegPath = Path.Combine(Environment.CurrentDirectory, ffmpegRel.Trim());
            if (!File.Exists(ffmpegPath))
            {
                _logger.LogError("ffmpeg executable not found at path: {Path}", ffmpegPath);
                MessageBox.Show($"ffmpeg 可執行檔未找到，請確認路徑：{ffmpegPath}", "配置錯誤", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        var downloadDirRel = config["Path:DownLoadDir"];
        downloadDir = string.IsNullOrWhiteSpace(downloadDirRel)
            ? Path.Combine(Environment.CurrentDirectory, "Downloads")
            : Path.Combine(Environment.CurrentDirectory, downloadDirRel.Trim());
    }

    private void ShowPlaylistUnavailableSummary(
        int privateCount,
        int subscriberOnlyCount,
        int downloadableCount)
    {
        using var dialog = new Form
        {
            Text = "部分影片無法存取",
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowInTaskbar = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(18)
        };

        var messageLabel = new Label
        {
            AutoSize = true,
            MinimumSize = new Size(260, 0),
            Text = $"私人影片：{privateCount} 部{Environment.NewLine}" +
                   $"會員專屬：{subscriberOnlyCount} 部{Environment.NewLine}" +
                   $"可下載：{downloadableCount} 部"
        };

        var okButton = new Button
        {
            Text = "確定",
            AutoSize = true,
            DialogResult = DialogResult.OK,
            Anchor = AnchorStyles.Right
        };

        var layout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2,
            Dock = DockStyle.Fill
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(messageLabel, 0, 0);
        layout.Controls.Add(okButton, 0, 1);
        okButton.Margin = new Padding(0, 18, 0, 0);

        dialog.Controls.Add(layout);
        dialog.AcceptButton = okButton;
        GUITool.ApplyStartupFont(dialog, _settings);
        GUITool.Apply(dialog, _settings);
        dialog.ShowDialog(this);
    }

    #endregion

    #region UI Functions

    private void btn_Download_Click(object sender, EventArgs e)
    {
        // ── 1. 收集已勾選項目的 ID ──────────────────────────────────────
        var selectedItemsId = new List<string>();
        foreach (DataGridViewRow row in dGV_PlayList.Rows)
            if (row.Cells["colSelected"].Value is true)
                selectedItemsId.Add(row.Cells["colId"].Value?.ToString() ?? string.Empty);

        if (selectedItemsId.Count == 0)
        {
            MessageBox.Show("請至少選取一個項目。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // ── 2. 從暫存的 playlistVideos 查回完整物件 ──────────────────────
        var selectedVideoItems = selectedItemsId
            .Select(id => playlistVideos.FirstOrDefault(v => v.Id == id))
            .Where(v => v != null)
            .Cast<PlaylistVideoItem>()
            .ToList();

        if (selectedVideoItems.Count == 0)
        {
            MessageBox.Show("找不到對應的影片資訊，請重新載入播放清單。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // ── 3. 組成 DownloadRequest 清單，交給 MainForm 執行 ─────────────────
        var requests = selectedVideoItems.Select(item => new DownloadRequest
        {
            Title = item.DisplayTitle,
            WebpageUrl = item.WebpageUrl!,
            MediaType = mediaType,
            MediaTypeDisplay = mediaTypeDisplay,
            DownloadDir = downloadDir
        });

        mainForm.EnqueueDownloads(requests);

        // ── 4. 關閉視窗，下載在 MainForm 背景進行 ────────────────────────────
        _logger.LogInformation("已提交 {Count} 個下載任務，關閉 PlaylistHandlerForm。", selectedVideoItems.Count);
        Close();
    }

    private void btn_Cancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    #endregion
}
