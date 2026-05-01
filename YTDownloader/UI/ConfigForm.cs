using YTDownloader.Model;
using YTDownloader.Service;
using YTDownloader.Tool;

namespace YTDownloader.UI;

public partial class ConfigForm : Form
{
    private const string BackgroundImageFolderName = "BackgroundImg";
    private readonly ConfigService _configService;
    private ConfigModel _settings = new();

    public ConfigForm() : this(new ConfigService())
    {
    }

    public ConfigForm(ConfigService configService)
    {
        _configService = configService;
        _settings = IsInDesignMode() ? new ConfigModel() : _configService.Load();
        InitializeForm();
    }

    public event EventHandler<ConfigModel>? SettingsApplied;

    private void InitializeForm()
    {
        if (!IsInDesignMode())
            GUITool.ApplyStartupFont(this, _settings);

        InitializeComponent();

        if (IsInDesignMode())
            return;

        PopulateFontComboBox();
        LoadSettings();
        GUITool.Apply(this, _settings);
    }

    private static bool IsInDesignMode()
    {
        return System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;
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

    private void PopulateFontComboBox()
    {
        _fontComboBox.Items.Clear();
        _fontComboBox.Items.AddRange(FontFamily.Families.Select(font => font.Name).Cast<object>().ToArray());
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
        SettingsApplied?.Invoke(this, _settings);
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

    private void CategoryList_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ShowSelectedPanel();
    }

    private void BrowseFolderButton_Click(object? sender, EventArgs e)
    {
        BrowseDownloadPath();
    }

    private void CacheClearButton_Click(object? sender, EventArgs e)
    {
        ClearTableWithConfirmation("快取", "TSQL_LOG", DBTool.ClearTsqlLog);
    }

    private void HistoryClearButton_Click(object? sender, EventArgs e)
    {
        ClearTableWithConfirmation("歷史紀錄", "DownloadHistory", DBTool.ClearDownloadHistory);
    }

    private void BackColorTextBox_TextChanged(object? sender, EventArgs e)
    {
        UpdateBackColorPreview();
    }

    private void ColorButton_Click(object? sender, EventArgs e)
    {
        PickBackColor();
    }

    private void BrowseImageButton_Click(object? sender, EventArgs e)
    {
        BrowseBackgroundImage();
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        SaveAndClose();
    }

    private void ApplyButton_Click(object? sender, EventArgs e)
    {
        ApplySettings();
    }

    private void CancelButton_Click(object? sender, EventArgs e)
    {
        Close();
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

    private void ClearTableWithConfirmation(string itemName, string tableName, Func<int> clearAction)
    {
        var confirmResult = MessageBox.Show(
            $"確定要清空{itemName}資料嗎？此動作會刪除 {tableName} 的所有資料且無法復原。",
            "確認清空",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (confirmResult != DialogResult.Yes)
            return;

        try
        {
            var affectedRows = clearAction();
            MessageBox.Show($"{itemName}已清空，共刪除 {affectedRows} 筆資料。", "清空完成", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{itemName}清空失敗：{ex.Message}", "清空失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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
