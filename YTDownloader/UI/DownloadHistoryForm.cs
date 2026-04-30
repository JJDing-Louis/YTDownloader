using Autofac;
using JJNET.DataAccess.Entity;
using JJNET.Utility.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using YTDownloader.Model;
using YTDownloader.Service;
using YTDownloader.Tool;

namespace YTDownloader;

public partial class DownloadHistoryForm : Form
{
    private const string OptionListMediaType = "ListMediaType";
    private const string OptionListDownloadStatus = "ListDownloadStatus";
    private const string SelectColumnName = "colSelect";
    private readonly ConfigService _configService;
    private readonly ILogger _logger;
    private readonly MainForm? _mainForm;
    private readonly OptionService _optionService;
    private readonly ConfigModel _settings;
    private bool _isSelectAllChecked;
    private bool _updatingSelectAllState;
    private IConfiguration config = null!;

    public DownloadHistoryForm()
    {
        _configService = new ConfigService();
        _settings = IsInDesignMode() ? new ConfigModel() : _configService.Load();
        _optionService = IsInDesignMode()
            ? null!
            : Program.Startup.Container.Resolve<OptionService>();
        _logger = IsInDesignMode()
            ? NullLogger<DownloadHistoryForm>.Instance
            : Program.Startup.Container.Resolve<ILogger<DownloadHistoryForm>>();
        InitializeForm();
    }

    public DownloadHistoryForm(MainForm mainForm) : this()
    {
        _mainForm = mainForm;
    }

    public DownloadHistoryForm(ConfigService configService, OptionService optionService,
        ILogger<DownloadHistoryForm> logger, MainForm? mainForm = null)
    {
        _configService = configService;
        _settings = _configService.Load();
        _optionService = optionService;
        _logger = logger;
        _mainForm = mainForm;
        InitializeForm();
    }

    private void InitializeForm()
    {
        if (!IsInDesignMode())
            GUITool.ApplyStartupFont(this, _settings);

        InitializeComponent();

        if (IsInDesignMode())
            return;

        LockWindowSize();
        _logger.LogInformation("DownloadHistoryForm form initialized.");
        Init();
    }

    private static bool IsInDesignMode()
    {
        return System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;
    }

    private void Init()
    {
        InitConfig();
        InitOptions();
        InitUI();
    }

    private void InitUI()
    {
        ConfigureSearchLayout();
    }

    private void SearchResult_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.ColumnIndex < 0 || dGV_SearchResult.Columns[e.ColumnIndex].Name != SelectColumnName) return;

        SetAllRowsSelected(!_isSelectAllChecked);
    }

    private void SetAllRowsSelected(bool isSelected)
    {
        dGV_SearchResult.EndEdit();
        _updatingSelectAllState = true;
        foreach (DataGridViewRow row in dGV_SearchResult.Rows)
            if (!row.IsNewRow)
                row.Cells[SelectColumnName].Value = isSelected;

        _updatingSelectAllState = false;

        _isSelectAllChecked = isSelected;
        dGV_SearchResult.InvalidateCell(dGV_SearchResult.Columns[SelectColumnName].Index, -1);
    }

    private void SearchResult_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        if (dGV_SearchResult.IsCurrentCellDirty &&
            dGV_SearchResult.CurrentCell?.OwningColumn?.Name == SelectColumnName)
            dGV_SearchResult.CommitEdit(DataGridViewDataErrorContexts.Commit);
    }

    private void SearchResult_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 ||
            e.ColumnIndex < 0 ||
            dGV_SearchResult.Columns[e.ColumnIndex].Name != SelectColumnName ||
            _updatingSelectAllState)
            return;

        UpdateSelectAllCheckBoxState();
    }

    private void UpdateSelectAllCheckBoxState()
    {
        _isSelectAllChecked = dGV_SearchResult.Rows.Count > 0 &&
                              dGV_SearchResult.Rows
                                  .Cast<DataGridViewRow>()
                                  .Where(row => !row.IsNewRow)
                                  .All(row => Convert.ToBoolean(row.Cells[SelectColumnName].Value));
        dGV_SearchResult.InvalidateCell(dGV_SearchResult.Columns[SelectColumnName].Index, -1);
    }

    private void SearchResult_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex != -1 ||
            e.ColumnIndex < 0 ||
            dGV_SearchResult.Columns[e.ColumnIndex].Name != SelectColumnName)
            return;

        if (e.Graphics == null) return;

        e.Paint(e.CellBounds, e.PaintParts & ~DataGridViewPaintParts.ContentForeground);

        const int checkBoxSize = 14;
        const int gap = 4;
        var text = "全選";
        var cellStyle = e.CellStyle ?? dGV_SearchResult.ColumnHeadersDefaultCellStyle;
        var font = cellStyle.Font ?? dGV_SearchResult.Font;
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
        }
    }

    private void InitOptions()
    {
        _logger.LogInformation("Initializing options...");
        try
        {
            GUITool.BindComboBox(cBO_DownloadResult, OptionListDownloadStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize options.");
            MessageBox.Show("選項載入失敗，請確認資料庫連線。", "初始化錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string FormatLocalDateTime(DateTime? dateTime)
    {
        if (dateTime == null) return string.Empty;

        var utcDateTime = dateTime.Value.Kind == DateTimeKind.Utc
            ? dateTime.Value
            : DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc);

        return utcDateTime.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss");
    }
    
    private void btn_Search_Click(object sender, EventArgs e)
    {
        var FileName = cB_FileName.Checked ? txt_Filename.Text.Trim() : null;
        var DownloadStartDate = cB_DownloadDate.Checked ? dTP_DownLoadStartDate.Value.Date : (DateTime?)null;
        var DownloadEndDate = cB_DownloadDate.Checked ? dTP_DownLoadEndDate.Value.Date : (DateTime?)null;
        var DownloadResult = cB_DownloadResult.Checked ? cBO_DownloadResult.SelectedValue : null;
        var IsAudio = cB_MediaType.Checked && cB_Audio.Checked ? "Audio" : null;
        var IsVideo = cB_MediaType.Checked && cB_Video.Checked ? "Video" : null;
        var sqlcmd = """
                       SELECT 
                           * 
                       FROM DownloadHistory
                       WHERE (@FileName IS NULL OR FileName LIKE '%' || @FileName || '%')
                       AND (@DownloadStartDate IS NULL OR DownloadDateTime >= @DownloadStartDate)
                       AND (@DownloadEndDate IS NULL OR DownloadDateTime <= @DownloadEndDate)
                       AND (@DownloadResult IS NULL OR Status = @DownloadResult)
                       AND (
                           (@IsAudio IS NULL AND @IsVideo IS NULL)
                           OR Type = @IsAudio
                           OR Type = @IsVideo
                       )
                       ORDER BY DownloadDateTime DESC
                     """;
        var Param = new { FileName, DownloadStartDate, DownloadEndDate, DownloadResult, IsAudio, IsVideo };
        var SearchResult = new List<DownloadHistory>();
        dGV_SearchResult.Rows.Clear();
        using (var conn = ConnectionTool.GetConnection())
        {
            var result = conn.Query<DownloadHistory>(sqlcmd, Param).ToList();
            if (result != null && result.Count > 0)
            {
                SearchResult.AddRange(result);
                foreach (var item in SearchResult)
                    dGV_SearchResult.Rows.Add(
                        false,
                        dGV_SearchResult.Rows.Count + 1,
                        item.FileName,
                        FormatLocalDateTime(item.DownloadDateTime),
                        OptionService.GetOptionDesc(OptionListMediaType, item.Type),
                        OptionService.GetOptionDesc(OptionListDownloadStatus, item.Status),
                        item.TaskID,
                        item.Title,
                        item.URL,
                        item.Path
                    );
                UpdateSelectAllCheckBoxState();
            }
        }
    }

    private void btn_ReDownload_Click(object sender, EventArgs e)
    {
        var selectedItem = dGV_SearchResult.Rows
            .Cast<DataGridViewRow>()
            .Where(row => !row.IsNewRow
                          && Convert.ToBoolean(row.Cells[SelectColumnName].Value)
                          && row.Cells["colTaskId"].Value != null
                          && row.Cells["colURL"].Value != null
            ).ToList();

        var requests = selectedItem.Select(row =>
        {
            var mediaType = OptionService.GetOptionName(
                OptionListMediaType,
                row.Cells["colMediaType"].Value?.ToString() ?? string.Empty);

            return new DownloadRequest
            {
                Title = row.Cells["colTitle"].Value?.ToString() ?? string.Empty,
                WebpageUrl = row.Cells["colURL"].Value?.ToString() ?? string.Empty,
                MediaType = mediaType,
                MediaTypeDisplay = OptionService.GetOptionDesc(OptionListMediaType, mediaType),
                DownloadDir = row.Cells["colPath"].Value?.ToString() ?? string.Empty
            };
        }).ToList();
        _mainForm?.EnqueueDownloads(requests);
        _logger.LogInformation("已提交 {Count} 個下載任務，關閉 PlaylistHandlerForm。", selectedItem.Count);
    }

    private void cB_SearchCondition_CheckedChanged(object sender, EventArgs e)
    {
        if (cB_FileName.Checked)
        {
            txt_Filename.Enabled = true;
        }
        else
        {
            txt_Filename.Enabled = false;
            txt_Filename.Text = string.Empty;
        }

        if (cB_DownloadDate.Checked)
        {
            dTP_DownLoadStartDate.Enabled = true;
            dTP_DownLoadEndDate.Enabled = true;
            dTP_DownLoadStartDate.Value = DateTime.Now;
            dTP_DownLoadEndDate.Value = DateTime.Now;
        }
        else
        {
            dTP_DownLoadStartDate.Enabled = false;
            dTP_DownLoadEndDate.Enabled = false;
            dTP_DownLoadStartDate.Value = DateTime.Now;
            dTP_DownLoadEndDate.Value = DateTime.Now;
        }

        if (cB_DownloadResult.Checked)
        {
            cBO_DownloadResult.Enabled = true;
            cBO_DownloadResult.Text = cBO_DownloadResult.Items.Count > 0
                ? cBO_DownloadResult.Items[0].ToString()
                : string.Empty;
        }
        else
        {
            cBO_DownloadResult.Enabled = false;
            cBO_DownloadResult.Text = string.Empty;
        }

        if (cB_MediaType.Checked)
        {
            cB_Audio.Enabled = true;
            cB_Video.Enabled = true;
        }
        else
        {
            cB_Audio.Enabled = false;
            cB_Video.Enabled = false;
            cB_Audio.Checked = false;
            cB_Video.Checked = false;
        }
    }
}
