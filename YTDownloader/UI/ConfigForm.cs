using YTDownloader.Model;
using YTDownloader.Service;
using YTDownloader.Tool;

namespace YTDownloader.UI;

public partial class ConfigForm : Form
{
    private const string BackgroundImageFolderName = "BackgroundImg";
    private readonly Panel _appearancePanel = new();
    private readonly Panel _backColorPreview = new();
    private readonly TextBox _backColorTextBox = new();
    private readonly TextBox _backgroundImageTextBox = new();

    private readonly ListBox _categoryList = new();

    private readonly ConfigService _configService;
    private readonly Panel _contentPanel = new();

    private readonly TextBox _downloadPathTextBox = new();
    private readonly ComboBox _fontComboBox = new();
    private readonly NumericUpDown _fontSizeNumeric = new();
    private readonly Panel _generalPanel = new();

    private readonly NumericUpDown _initialThreadNumeric = new();

    private readonly CheckBox _isDarkModeCheckBox = new();
    private readonly ComboBox _languageComboBox = new();
    private readonly NumericUpDown _maxThreadNumeric = new();
    private readonly Panel _savePanel = new();
    private readonly Panel _threadPanel = new();
    private ConfigModel _settings = new();

    public ConfigForm() : this(new ConfigService())
    {
    }

    public ConfigForm(ConfigService configService)
    {
        _configService = configService;
        _settings = _configService.Load();
        InitializeForm();
    }

    private void InitializeForm()
    {
        GUITool.ApplyStartupFont(this, _settings);
        InitializeComponent();
        BuildLayout();
        LoadSettings();
        GUITool.Apply(this, _settings);
    }

    private void BuildLayout()
    {
        SuspendLayout();

        Text = "偏好設定";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(760, 430);
        ClientSize = new Size(860, 460);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(16, 14, 16, 12)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _categoryList.Dock = DockStyle.Fill;
        _categoryList.IntegralHeight = false;
        _categoryList.Items.AddRange(new object[] { "一般", "外觀", "下載執行序", "儲存" });
        _categoryList.SelectedIndexChanged += (_, _) => ShowSelectedPanel();

        _contentPanel.Dock = DockStyle.Fill;
        _contentPanel.Padding = new Padding(16, 0, 0, 0);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            MinimumSize = new Size(0, Math.Max(58, Font.Height + 30)),
            Padding = new Padding(0, 8, 0, 0)
        };

        var saveButton = CreateFooterButton("儲存並關閉");
        saveButton.Click += (_, _) => SaveAndClose();

        var applyButton = CreateFooterButton("套用");
        applyButton.Click += (_, _) => ApplySettings();

        var cancelButton = CreateFooterButton("取消");
        cancelButton.Click += (_, _) => Close();

        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(applyButton);
        buttonPanel.Controls.Add(cancelButton);

        BuildGeneralPanel();
        BuildAppearancePanel();
        BuildThreadPanel();
        BuildSavePanel();

        _contentPanel.Controls.Add(_generalPanel);
        _contentPanel.Controls.Add(_appearancePanel);
        _contentPanel.Controls.Add(_threadPanel);
        _contentPanel.Controls.Add(_savePanel);

        root.Controls.Add(_categoryList, 0, 0);
        root.Controls.Add(_contentPanel, 1, 0);
        root.Controls.Add(buttonPanel, 0, 1);
        root.SetColumnSpan(buttonPanel, 2);

        Controls.Clear();
        Controls.Add(root);

        _categoryList.SelectedIndex = 0;
        ResumeLayout(false);
    }

    private Button CreateFooterButton(string text)
    {
        return new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(88, Math.Max(32, Font.Height + 14)),
            Margin = new Padding(8, 0, 0, 0)
        };
    }

    private void BuildGeneralPanel()
    {
        _generalPanel.Dock = DockStyle.Fill;

        var layout = CreateSectionLayout();
        var group = new GroupBox
        {
            Text = "一般",
            Dock = DockStyle.Top,
            Height = 92
        };

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(12, 12, 12, 12)
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        _languageComboBox.Dock = DockStyle.Left;
        _languageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _languageComboBox.Width = 160;
        _languageComboBox.Items.AddRange(new object[] { "zh-TW", "en-US" });

        fields.Controls.Add(new Label { Text = "顯示語言", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill },
            0, 0);
        fields.Controls.Add(_languageComboBox, 1, 0);

        group.Controls.Add(fields);
        layout.Controls.Add(group, 0, 0);
        _generalPanel.Controls.Add(layout);
    }

    private void BuildAppearancePanel()
    {
        _appearancePanel.Dock = DockStyle.Fill;

        var layout = CreateSectionLayout();
        layout.Controls.Add(CreateAppearanceGroup(), 0, 0);
        layout.Controls.Add(CreateFontGroup(), 0, 1);

        _appearancePanel.Controls.Add(layout);
    }

    private GroupBox CreateAppearanceGroup()
    {
        var group = new GroupBox
        {
            Text = "外觀",
            Dock = DockStyle.Top,
            Height = 180
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 4,
            Padding = new Padding(12, 10, 12, 10)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        _isDarkModeCheckBox.Text = "啟用黑暗模式";
        _isDarkModeCheckBox.AutoSize = true;

        _backColorTextBox.Dock = DockStyle.Fill;
        _backColorTextBox.TextChanged += (_, _) => UpdateBackColorPreview();

        _backColorPreview.BorderStyle = BorderStyle.FixedSingle;
        _backColorPreview.Dock = DockStyle.Fill;

        var colorButton = new Button { Text = "...", Dock = DockStyle.Fill };
        colorButton.Click += (_, _) => PickBackColor();

        _backgroundImageTextBox.Dock = DockStyle.Fill;

        var browseImageButton = new Button { Text = "...", Dock = DockStyle.Fill };
        browseImageButton.Click += (_, _) => BrowseBackgroundImage();

        layout.Controls.Add(_isDarkModeCheckBox, 1, 0);
        layout.SetColumnSpan(_isDarkModeCheckBox, 2);
        layout.Controls.Add(new Label { Text = "背景顏色", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill },
            0, 1);
        layout.Controls.Add(_backColorTextBox, 1, 1);
        layout.Controls.Add(colorButton, 2, 1);
        layout.Controls.Add(new Label { Text = "顏色預覽", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill },
            0, 2);
        layout.Controls.Add(_backColorPreview, 1, 2);
        layout.Controls.Add(new Label { Text = "背景圖片", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill },
            0, 3);
        layout.Controls.Add(_backgroundImageTextBox, 1, 3);
        layout.Controls.Add(browseImageButton, 2, 3);

        group.Controls.Add(layout);
        return group;
    }

    private GroupBox CreateFontGroup()
    {
        var group = new GroupBox
        {
            Text = "字型",
            Dock = DockStyle.Top,
            Height = 112
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(12, 10, 12, 10)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        _fontComboBox.Dock = DockStyle.Fill;
        _fontComboBox.DropDownStyle = ComboBoxStyle.DropDown;
        _fontComboBox.Items.AddRange(FontFamily.Families.Select(font => font.Name).Cast<object>().ToArray());

        _fontSizeNumeric.Dock = DockStyle.Left;
        _fontSizeNumeric.Minimum = 8;
        _fontSizeNumeric.Maximum = 48;
        _fontSizeNumeric.Width = 80;

        layout.Controls.Add(new Label { Text = "字型", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill },
            0, 0);
        layout.Controls.Add(_fontComboBox, 1, 0);
        layout.Controls.Add(new Label { Text = "字型大小", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill },
            0, 1);
        layout.Controls.Add(_fontSizeNumeric, 1, 1);

        group.Controls.Add(layout);
        return group;
    }

    private void BuildThreadPanel()
    {
        _threadPanel.Dock = DockStyle.Fill;

        var layout = CreateSectionLayout();
        var group = new GroupBox
        {
            Text = "下載執行序",
            Dock = DockStyle.Top,
            Height = 128
        };

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(12, 12, 12, 12)
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        ConfigureThreadNumeric(_initialThreadNumeric);
        ConfigureThreadNumeric(_maxThreadNumeric);

        fields.Controls.Add(new Label { Text = "初始數量", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill },
            0, 0);
        fields.Controls.Add(_initialThreadNumeric, 1, 0);
        fields.Controls.Add(new Label { Text = "最大數量", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill },
            0, 1);
        fields.Controls.Add(_maxThreadNumeric, 1, 1);

        group.Controls.Add(fields);
        layout.Controls.Add(group, 0, 0);
        _threadPanel.Controls.Add(layout);
    }

    private static void ConfigureThreadNumeric(NumericUpDown numeric)
    {
        numeric.Minimum = 1;
        numeric.Maximum = 32;
        numeric.Width = 80;
        numeric.Dock = DockStyle.Left;
    }

    private void BuildSavePanel()
    {
        _savePanel.Dock = DockStyle.Fill;

        var layout = CreateSectionLayout();
        var group = new GroupBox
        {
            Text = "儲存",
            Dock = DockStyle.Top,
            Height = 92
        };

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(12, 12, 12, 12)
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        _downloadPathTextBox.Dock = DockStyle.Fill;
        var browseFolderButton = new Button { Text = "...", Dock = DockStyle.Fill };
        browseFolderButton.Click += (_, _) => BrowseDownloadPath();

        fields.Controls.Add(
            new Label { Text = "下載資料夾", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
        fields.Controls.Add(_downloadPathTextBox, 1, 0);
        fields.Controls.Add(browseFolderButton, 2, 0);

        group.Controls.Add(fields);
        layout.Controls.Add(group, 0, 0);
        _savePanel.Controls.Add(layout);
    }

    private static TableLayoutPanel CreateSectionLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1));
        return layout;
    }

    private void LoadSettings()
    {
        _settings = _configService.Load();

        _isDarkModeCheckBox.Checked = _settings.Appearance.IsDarkMode;
        _backColorTextBox.Text = NormalizeColorText(_settings.Appearance.BackColor);
        _backgroundImageTextBox.Text = _settings.Appearance.BackGroundImage;
        _fontComboBox.Text = _settings.Appearance.Font;
        _fontSizeNumeric.Value =
            Clamp(_settings.Appearance.FontSize, _fontSizeNumeric.Minimum, _fontSizeNumeric.Maximum);

        _initialThreadNumeric.Value = Clamp(_settings.Thread.InitialCount, _initialThreadNumeric.Minimum,
            _initialThreadNumeric.Maximum);
        _maxThreadNumeric.Value =
            Clamp(_settings.Thread.MaxCount, _maxThreadNumeric.Minimum, _maxThreadNumeric.Maximum);

        _downloadPathTextBox.Text = _settings.Save.DownloadPath;
        _languageComboBox.SelectedItem = _settings.General.Language;
        if (_languageComboBox.SelectedIndex < 0)
            _languageComboBox.SelectedIndex = 0;

        UpdateBackColorPreview();
    }

    private void SaveAndClose()
    {
        if (!SaveCurrentSettings())
            return;

        DialogResult = DialogResult.OK;
        Close();
    }

    private void ApplySettings()
    {
        if (!SaveCurrentSettings())
            return;

        foreach (Form form in Application.OpenForms)
            GUITool.Apply(form, _settings);
    }

    private bool SaveCurrentSettings()
    {
        if (!TryValidateInputs())
            return false;

        _settings.Appearance.IsDarkMode = _isDarkModeCheckBox.Checked;
        _settings.Appearance.BackColor = NormalizeColorText(_backColorTextBox.Text);
        _settings.Appearance.BackGroundImage = _backgroundImageTextBox.Text.Trim();
        _settings.Appearance.Font = _fontComboBox.Text.Trim();
        _settings.Appearance.FontSize = (int)_fontSizeNumeric.Value;

        _settings.Thread.InitialCount = (int)_initialThreadNumeric.Value;
        _settings.Thread.MaxCount = (int)_maxThreadNumeric.Value;

        _settings.Save.DownloadPath = _downloadPathTextBox.Text.Trim();
        _settings.General.Language = _languageComboBox.SelectedItem?.ToString() ?? "zh-TW";

        _configService.Save(_settings);
        return true;
    }

    private bool TryValidateInputs()
    {
        try
        {
            ColorTranslator.FromHtml(_backColorTextBox.Text.Trim());
        }
        catch (Exception)
        {
            MessageBox.Show("背景顏色格式不正確，請輸入像 #F0F0F0、White 這類格式。", "設定錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _categoryList.SelectedIndex = 1;
            _backColorTextBox.Focus();
            return false;
        }

        if (_initialThreadNumeric.Value > _maxThreadNumeric.Value)
        {
            MessageBox.Show("初始數量不可大於最大數量。", "設定錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _categoryList.SelectedIndex = 2;
            _initialThreadNumeric.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(_downloadPathTextBox.Text))
        {
            MessageBox.Show("請指定下載資料夾。", "設定錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _categoryList.SelectedIndex = 3;
            _downloadPathTextBox.Focus();
            return false;
        }

        return true;
    }

    private void ShowSelectedPanel()
    {
        _generalPanel.Visible = _categoryList.SelectedIndex == 0;
        _appearancePanel.Visible = _categoryList.SelectedIndex == 1;
        _threadPanel.Visible = _categoryList.SelectedIndex == 2;
        _savePanel.Visible = _categoryList.SelectedIndex == 3;

        _generalPanel.BringToFront();
        if (_appearancePanel.Visible)
            _appearancePanel.BringToFront();
        if (_threadPanel.Visible)
            _threadPanel.BringToFront();
        if (_savePanel.Visible)
            _savePanel.BringToFront();
    }

    private void PickBackColor()
    {
        using var dialog = new ColorDialog
        {
            Color = GetBackColorFromText()
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            _backColorTextBox.Text = ToHexColor(dialog.Color);
    }

    private void BrowseBackgroundImage()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "圖片檔案|*.jpg;*.jpeg;*.png;*.bmp;*.gif|所有檔案|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            try
            {
                _backgroundImageTextBox.Text = CopyBackgroundImageToAppFolder(dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"背景圖片複製失敗：{ex.Message}", "設定錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
    }

    private static string CopyBackgroundImageToAppFolder(string sourcePath)
    {
        var backgroundImageFolder = Path.Combine(AppContext.BaseDirectory, BackgroundImageFolderName);
        Directory.CreateDirectory(backgroundImageFolder);

        var sourceFileName = Path.GetFileName(sourcePath);
        var destinationPath = Path.Combine(backgroundImageFolder, sourceFileName);
        destinationPath = GetAvailableFilePath(destinationPath);

        File.Copy(sourcePath, destinationPath);
        return destinationPath;
    }

    private static string GetAvailableFilePath(string filePath)
    {
        if (!File.Exists(filePath))
            return filePath;

        var directory = Path.GetDirectoryName(filePath) ?? AppContext.BaseDirectory;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);

        for (var index = 1;; index++)
        {
            var candidatePath = Path.Combine(directory, $"{fileNameWithoutExtension}_{index}{extension}");
            if (!File.Exists(candidatePath))
                return candidatePath;
        }
    }

    private void BrowseDownloadPath()
    {
        using var dialog = new FolderBrowserDialog
        {
            SelectedPath = Directory.Exists(_downloadPathTextBox.Text)
                ? _downloadPathTextBox.Text
                : Environment.CurrentDirectory
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            _downloadPathTextBox.Text = dialog.SelectedPath;
    }

    private void UpdateBackColorPreview()
    {
        _backColorPreview.BackColor = GetBackColorFromText();
    }

    private Color GetBackColorFromText()
    {
        try
        {
            var color = ColorTranslator.FromHtml(_backColorTextBox.Text.Trim());
            return color.IsEmpty ? SystemColors.Control : color;
        }
        catch (Exception)
        {
            return SystemColors.Control;
        }
    }

    private static decimal Clamp(int value, decimal minimum, decimal maximum)
    {
        return Math.Min(Math.Max(value, minimum), maximum);
    }

    private static string NormalizeColorText(string colorText)
    {
        if (string.IsNullOrWhiteSpace(colorText))
            return ToHexColor(SystemColors.Control);

        try
        {
            return ToHexColor(ColorTranslator.FromHtml(colorText.Trim()));
        }
        catch (Exception)
        {
            return ToHexColor(SystemColors.Control);
        }
    }

    private static string ToHexColor(Color color)
    {
        var normalizedColor = Color.FromArgb(color.R, color.G, color.B);
        return $"#{normalizedColor.R:X2}{normalizedColor.G:X2}{normalizedColor.B:X2}";
    }
}