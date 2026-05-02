using Microsoft.Extensions.Logging;
using YTDownloader.Model;
using YTDownloader.Service;
using YTDownloader.Tool;

namespace YTDownloader;

public partial class DownloadHistoryForm : Form
{
    private const string OptionListDownloadStatus = "ListDownloadStatus";
    private const string SelectColumnName = "colSelect";
    private readonly ConfigService _configService;
    private readonly DownloadHistoryService _downloadHistoryService;
    private readonly ILogger _logger;
    private readonly MainForm? _mainForm;
    private readonly ConfigModel _settings;
    private bool _isSelectAllChecked;
    private bool _updatingSelectAllState;

    public DownloadHistoryForm(
        ConfigService configService,
        DownloadHistoryService downloadHistoryService,
        ILogger<DownloadHistoryForm> logger,
        MainForm? mainForm = null)
    {
        _configService = configService;
        _downloadHistoryService = downloadHistoryService;
        _settings = _configService.Load();
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

    private void btn_Search_Click(object sender, EventArgs e)
    {
        var result = _downloadHistoryService.Search(CreateSearchCriteria());
        GUITool.BindDownloadHistoryRows(dGV_SearchResult, result);
        UpdateSelectAllCheckBoxState();
    }

    private void btn_ReDownload_Click(object sender, EventArgs e)
    {
        var selectedItems = GUITool.GetSelectedTaggedItems<DownloadHistorySearchItem>(
            dGV_SearchResult,
            SelectColumnName);
        var requests = _downloadHistoryService.CreateRedownloadRequests(selectedItems);

        _mainForm?.EnqueueDownloads(requests);
        _logger.LogInformation("已提交 {Count} 個下載任務。", requests.Count);
    }

    //TODO: 修改耦合
    private DownloadHistorySearchCriteria CreateSearchCriteria()
    {
        return _downloadHistoryService.CreateSearchCriteria(
            cB_FileName.Checked,
            txt_Filename.Text,
            cB_DownloadDate.Checked,
            dTP_DownLoadStartDate.Value,
            dTP_DownLoadEndDate.Value,
            cB_DownloadResult.Checked,
            GUITool.GetComboBoxSelectedName(cBO_DownloadResult),
            cB_MediaType.Checked,
            cB_Audio.Checked,
            cB_Video.Checked);
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
            if (cBO_DownloadResult.Items.Count > 0)
                cBO_DownloadResult.SelectedIndex = 0;
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
