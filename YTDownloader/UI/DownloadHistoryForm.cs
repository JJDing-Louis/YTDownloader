using Autofac;
using JJNET.DataAccess.Entity;
using JJNET.Utility.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    private IConfiguration config = null!;
    private bool _isSelectAllChecked;
    private bool _updatingSelectAllState;

    public DownloadHistoryForm()
    {
        _configService = new ConfigService();
        _settings = _configService.Load();
        _optionService = Program.Startup.Container.Resolve<OptionService>();
        _logger = Program.Startup.Container.Resolve<ILogger<DownloadHistoryForm>>();
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
        GUITool.ApplyStartupFont(this, _settings);
        InitializeComponent();
        LockWindowSize();
        _logger.LogInformation("DownloadHistoryForm form initialized.");
        Init();
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
        InitDataGridView();
    }

    private void ConfigureSearchLayout()
    {
        tableLayoutPanel2.Padding = new Padding(0, 4, 0, 0);
        var reDownloadButtonSize = GetButtonSize(btn_ReDownload, minWidth: 120, minHeight: 34);
        var criteriaRowHeight = Math.Max(48, Font.Height + 24);
        var searchPanelHeight = tableLayoutPanel2.Padding.Top + criteriaRowHeight * tableLayoutPanel2.RowCount + 8;
        var actionRowHeight = reDownloadButtonSize.Height + 16;

        tableLayoutPanel1.RowStyles[0].SizeType = SizeType.Absolute;
        tableLayoutPanel1.RowStyles[0].Height = searchPanelHeight;
        tableLayoutPanel1.RowStyles[1].SizeType = SizeType.Absolute;
        tableLayoutPanel1.RowStyles[1].Height = actionRowHeight;
        tableLayoutPanel1.RowStyles[2].SizeType = SizeType.Percent;
        tableLayoutPanel1.RowStyles[2].Height = 100;

        tableLayoutPanel2.Dock = DockStyle.Top;
        tableLayoutPanel2.Height = searchPanelHeight;
        tableLayoutPanel2.ColumnStyles[0].SizeType = SizeType.Absolute;
        tableLayoutPanel2.ColumnStyles[0].Width = Math.Max(136, TextRenderer.MeasureText("下載結果", Font).Width + 34);
        tableLayoutPanel2.ColumnStyles[1].SizeType = SizeType.Absolute;
        tableLayoutPanel2.ColumnStyles[1].Width = 560;
        tableLayoutPanel2.ColumnStyles[2].SizeType = SizeType.Absolute;
        tableLayoutPanel2.ColumnStyles[2].Width = Math.Max(130, TextRenderer.MeasureText("查詢", Font).Width + 58);

        foreach (RowStyle rowStyle in tableLayoutPanel2.RowStyles)
        {
            rowStyle.SizeType = SizeType.Absolute;
            rowStyle.Height = criteriaRowHeight;
        }

        ConfigureCriteriaCheckBox(cB_FileName);
        ConfigureCriteriaCheckBox(cB_DownloadDate);
        ConfigureCriteriaCheckBox(cB_DownloadResult);
        ConfigureCriteriaCheckBox(cB_MediaType);

        ConfigureInputControl(txt_Filename);
        ConfigureInputControl(cBO_DownloadResult);

        tableLayoutPanel3.Dock = DockStyle.Fill;
        tableLayoutPanel3.Margin = new Padding(0);
        tableLayoutPanel3.ColumnStyles[0].SizeType = SizeType.Absolute;
        tableLayoutPanel3.ColumnStyles[0].Width = Math.Max(230, TextRenderer.MeasureText("2026年  4月30日", Font).Width + 64);
        tableLayoutPanel3.ColumnStyles[1].SizeType = SizeType.Absolute;
        tableLayoutPanel3.ColumnStyles[1].Width = 48;
        tableLayoutPanel3.ColumnStyles[2].SizeType = SizeType.Absolute;
        tableLayoutPanel3.ColumnStyles[2].Width = tableLayoutPanel3.ColumnStyles[0].Width;
        dTP_DownLoadStartDate.Dock = DockStyle.None;
        dTP_DownLoadStartDate.Anchor = AnchorStyles.Left;
        dTP_DownLoadStartDate.Width = (int)tableLayoutPanel3.ColumnStyles[0].Width;
        dTP_DownLoadStartDate.Height = dTP_DownLoadStartDate.PreferredSize.Height;
        dTP_DownLoadEndDate.Dock = DockStyle.None;
        dTP_DownLoadEndDate.Anchor = AnchorStyles.Left;
        dTP_DownLoadEndDate.Width = (int)tableLayoutPanel3.ColumnStyles[2].Width;
        dTP_DownLoadEndDate.Height = dTP_DownLoadEndDate.PreferredSize.Height;
        label1.Dock = DockStyle.Fill;
        label1.TextAlign = ContentAlignment.MiddleCenter;

        tableLayoutPanel5.Dock = DockStyle.Fill;
        tableLayoutPanel5.Margin = new Padding(0, 0, 0, 0);
        cB_Audio.AutoSize = true;
        cB_Audio.Anchor = AnchorStyles.Left;
        cB_Video.AutoSize = true;
        cB_Video.Anchor = AnchorStyles.Left;

        btn_Search.Anchor = AnchorStyles.Left;
        btn_Search.Margin = new Padding(10, 0, 0, 0);
        btn_Search.Size = GetButtonSize(btn_Search, minWidth: 96, minHeight: 32);

        btn_ReDownload.Dock = DockStyle.None;
        btn_ReDownload.Anchor = AnchorStyles.Left;
        btn_ReDownload.Margin = new Padding(0);
        btn_ReDownload.Size = reDownloadButtonSize;
    }

    private static Size GetButtonSize(Button button, int minWidth, int minHeight)
    {
        var preferredSize = button.GetPreferredSize(Size.Empty);
        return new Size(
            Math.Max(minWidth, preferredSize.Width + 12),
            Math.Max(minHeight, preferredSize.Height + 6));
    }

    private static void ConfigureCriteriaCheckBox(CheckBox checkBox)
    {
        checkBox.AutoSize = false;
        checkBox.Dock = DockStyle.Fill;
        checkBox.Margin = new Padding(0);
        checkBox.TextAlign = ContentAlignment.MiddleLeft;
    }

    private static void ConfigureInputControl(Control control)
    {
        control.Dock = DockStyle.None;
        control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        control.Margin = new Padding(0, 0, 0, 0);
        control.Height = control.PreferredSize.Height;
    }

    private void InitDataGridView()
    {
        dGV_SearchResult.Columns.Clear();
        dGV_SearchResult.RowHeadersVisible = false;
        //勾選選項
        dGV_SearchResult.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = SelectColumnName,
            HeaderText = "",
            Width = 76,
            ReadOnly = false,
            Resizable = DataGridViewTriState.False
        });
        // # 序號
        dGV_SearchResult.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colIndex",
            HeaderText = "#",
            Width = 40,
            ReadOnly = true,
            Resizable = DataGridViewTriState.False
        });
        // 檔名
        dGV_SearchResult.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colFileName",
            HeaderText = "檔名",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            ReadOnly = true
        });
        // 下載日期
        dGV_SearchResult.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "DownloadDateTime",
            HeaderText = "下載日期",
            Width = 100,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            ReadOnly = true
        });
        // 類型（音訊 / 視訊）
        dGV_SearchResult.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colMediaType",
            HeaderText = "類型",
            Width = Math.Max(80, TextRenderer.MeasureText("類型", Font).Width + 32),
            ReadOnly = true,
            Resizable = DataGridViewTriState.False
        });
        // 狀態文字
        dGV_SearchResult.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colStatus",
            HeaderText = "狀態",
            Width = 200,
            ReadOnly = true,
            Resizable = DataGridViewTriState.True,
            DefaultCellStyle = new DataGridViewCellStyle
            { Alignment = DataGridViewContentAlignment.MiddleLeft }
        });
        // 隱藏的任務 ID（用於刪除列後仍能找到正確的 controller）
        dGV_SearchResult.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colTaskId",
            HeaderText = "TaskId",
            Visible = false
        });
        // 隱藏的URL（用於刪除列後仍能找到正確的 controller）
        dGV_SearchResult.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colTitle",
            HeaderText = "Title",
            Visible = false
        });
        // 隱藏的URL（用於刪除列後仍能找到正確的 controller）
        dGV_SearchResult.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colURL",
            HeaderText = "URL",
            Visible = false
        });
        dGV_SearchResult.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colPath",
            HeaderText = "Path",
            Visible = false
        });
        dGV_SearchResult.Rows.Clear();
        dGV_SearchResult.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        dGV_SearchResult.ColumnHeadersHeight = Math.Max(34, Font.Height + 14);
        InitSelectAllHeader();
    }

    private void InitSelectAllHeader()
    {
        dGV_SearchResult.CurrentCellDirtyStateChanged -= SearchResult_CurrentCellDirtyStateChanged;
        dGV_SearchResult.CurrentCellDirtyStateChanged += SearchResult_CurrentCellDirtyStateChanged;
        dGV_SearchResult.CellValueChanged -= SearchResult_CellValueChanged;
        dGV_SearchResult.CellValueChanged += SearchResult_CellValueChanged;
        dGV_SearchResult.CellPainting -= SearchResult_CellPainting;
        dGV_SearchResult.CellPainting += SearchResult_CellPainting;
        dGV_SearchResult.ColumnHeaderMouseClick -= SearchResult_ColumnHeaderMouseClick;
        dGV_SearchResult.ColumnHeaderMouseClick += SearchResult_ColumnHeaderMouseClick;
    }

    private void SearchResult_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.ColumnIndex < 0 || dGV_SearchResult.Columns[e.ColumnIndex].Name != SelectColumnName)
        {
            return;
        }

        SetAllRowsSelected(!_isSelectAllChecked);
    }

    private void SetAllRowsSelected(bool isSelected)
    {
        dGV_SearchResult.EndEdit();
        _updatingSelectAllState = true;
        foreach (DataGridViewRow row in dGV_SearchResult.Rows)
        {
            if (!row.IsNewRow)
            {
                row.Cells[SelectColumnName].Value = isSelected;
            }
        }
        _updatingSelectAllState = false;

        _isSelectAllChecked = isSelected;
        dGV_SearchResult.InvalidateCell(dGV_SearchResult.Columns[SelectColumnName].Index, -1);
    }

    private void SearchResult_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        if (dGV_SearchResult.IsCurrentCellDirty &&
            dGV_SearchResult.CurrentCell?.OwningColumn?.Name == SelectColumnName)
        {
            dGV_SearchResult.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }
    }

    private void SearchResult_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 ||
            e.ColumnIndex < 0 ||
            dGV_SearchResult.Columns[e.ColumnIndex].Name != SelectColumnName ||
            _updatingSelectAllState)
        {
            return;
        }

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
        {
            return;
        }

        if (e.Graphics == null)
        {
            return;
        }

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

    private void LockWindowSize()
    {
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimumSize = Size;
        MaximumSize = Size;
    }

    private static string FormatLocalDateTime(DateTime? dateTime)
    {
        if (dateTime == null)
        {
            return string.Empty;
        }

        var utcDateTime = dateTime.Value.Kind == DateTimeKind.Utc
            ? dateTime.Value
            : DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc);

        return utcDateTime.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss");
    }

    /// <summary>
    /// TODO:待測
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
                {
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
                }
                UpdateSelectAllCheckBoxState();
            }
        }
    }

    private void btn_ReDownload_Click(object sender, EventArgs e)
    {
        var selectedItem =  dGV_SearchResult.Rows
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
                DownloadDir = row.Cells["colPath"].Value?.ToString() ?? string.Empty,
            };
        }).ToList();
        _mainForm?.EnqueueDownloads(requests);
        _logger.LogInformation("已提交 {Count} 個下載任務，關閉 PlaylistHandlerForm。", selectedItem.Count);
    }

    private void cB_SearchCondition_CheckedChanged(object sender, EventArgs e)
    {
        if (cB_FileName.Checked)
        {
            txt_Filename.Enabled=true;
        } 
        else
        {
            txt_Filename.Enabled = false;
            txt_Filename.Text = string.Empty;
        }

        if (cB_DownloadDate.Checked) 
        {
            dTP_DownLoadStartDate.Enabled =true;
            dTP_DownLoadEndDate.Enabled =  true;
            dTP_DownLoadStartDate.Value = DateTime.Now;
            dTP_DownLoadEndDate.Value = DateTime.Now;
        }
        else
        {
            dTP_DownLoadStartDate.Enabled =false;
            dTP_DownLoadEndDate.Enabled = false;
            dTP_DownLoadStartDate.Value = DateTime.Now;
            dTP_DownLoadEndDate.Value = DateTime.Now;
        }

        if (cB_DownloadResult.Checked)
        {
            cBO_DownloadResult.Enabled = true;
            cBO_DownloadResult.Text = cBO_DownloadResult.Items.Count > 0 ? cBO_DownloadResult.Items[0].ToString() : string.Empty;
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
