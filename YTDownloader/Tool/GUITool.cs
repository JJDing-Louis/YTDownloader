using YTDownloader.Model;
using YTDownloader.Service;

namespace YTDownloader.Tool;

internal static class GUITool
{
    public static void ApplyFromConfig(Form form)
    {
        var settings = new ConfigService().Load();
        Apply(form, settings.Appearance);
    }

    public static void ApplyStartupFontFromConfig(Form form)
    {
        var settings = new ConfigService().Load();
        ApplyStartupFont(form, settings.Appearance);
    }

    public static void ApplyStartupFont(Form form, ConfigModel settings)
    {
        ApplyStartupFont(form, settings.Appearance);
    }

    public static void ApplyStartupFont(Form form, AppearanceConfigModel appearance)
    {
        using var configuredFont = CreateConfiguredFont(form.Font, appearance.Font, appearance.FontSize);
        if (configuredFont != null)
            form.Font = new Font(configuredFont, configuredFont.Style);
    }

    public static void Apply(Form form, ConfigModel settings)
    {
        Apply(form, settings.Appearance);
    }

    public static void Apply(Form form, AppearanceConfigModel appearance)
    {
        var backColor = appearance.IsDarkMode
            ? Color.FromArgb(32, 32, 32)
            : appearance.BackColorValue;
        var foreColor = appearance.IsDarkMode ? Color.WhiteSmoke : SystemColors.ControlText;

        form.BackColor = backColor;
        form.ForeColor = foreColor;
        ApplyColorsToControls(form.Controls, backColor, foreColor, appearance.IsDarkMode);
        ApplyConfiguredFont(form, appearance.Font, appearance.FontSize);
        ApplyBackgroundImage(form, appearance.BackGroundImage);
    }

    public static string ResolveAppPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        return Path.GetFullPath(
            Path.IsPathRooted(path)
                ? path
                : Path.Combine(AppContext.BaseDirectory, path));
    }

    public static string GetComboBoxSelectedName(ComboBox comboBox)
    {
        return comboBox.SelectedItem is KeyValuePair<string, string> kv
            ? kv.Value
            : string.Empty;
    }

    public static string GetComboBoxSelectedDesc(ComboBox comboBox)
    {
        return comboBox.SelectedItem is KeyValuePair<string, string> kv
            ? kv.Key
            : string.Empty;
    }

    public static void BindComboBox(ComboBox comboBox, string optionKey)
    {
        var items = OptionService.GetOptions(optionKey);
        if (items.Count == 0)
            return;

        comboBox.DisplayMember = "Key";
        comboBox.Items.AddRange(items.Cast<object>().ToArray());
        if (comboBox.Items.Count > 0)
            comboBox.SelectedIndex = 0;
    }

    private static void ApplyConfiguredFont(Form form, string fontName, int fontSize)
    {
        using var configuredFont = CreateConfiguredFont(form.Font, fontName, fontSize);
        if (configuredFont == null)
            return;

        ApplyFontToControlTree(form, configuredFont);
        form.PerformAutoScale();
        form.PerformLayout();
    }

    private static Font? CreateConfiguredFont(Font fallbackFont, string fontName, int fontSize)
    {
        try
        {
            var normalizedFontSize = Math.Clamp(fontSize, 8, 48);
            var familyName = string.IsNullOrWhiteSpace(fontName)
                ? fallbackFont.FontFamily.Name
                : fontName.Trim();

            return new Font(familyName, normalizedFontSize, FontStyle.Regular, GraphicsUnit.Point);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static void ApplyFontToControlTree(Control control, Font font)
    {
        control.Font = new Font(font, font.Style);

        foreach (Control child in control.Controls)
            ApplyFontToControlTree(child, font);

        if (control is MenuStrip menuStrip)
        {
            foreach (ToolStripItem item in menuStrip.Items)
                ApplyFontToToolStripItem(item, font);
        }
    }

    private static void ApplyFontToToolStripItem(ToolStripItem item, Font font)
    {
        item.Font = new Font(font, font.Style);

        if (item is ToolStripDropDownItem dropDownItem)
        {
            foreach (ToolStripItem child in dropDownItem.DropDownItems)
                ApplyFontToToolStripItem(child, font);
        }
    }

    private static void ApplyBackgroundImage(Form form, string backgroundImage)
    {
        var oldImage = form.BackgroundImage;
        form.BackgroundImage = null;
        oldImage?.Dispose();

        var backgroundImagePath = ResolveAppPath(backgroundImage);
        if (string.IsNullOrWhiteSpace(backgroundImagePath) || !File.Exists(backgroundImagePath))
            return;

        using var image = Image.FromFile(backgroundImagePath);
        form.BackgroundImage = new Bitmap(image);
        form.BackgroundImageLayout = ImageLayout.Stretch;
    }

    private static void ApplyColorsToControls(Control.ControlCollection controls, Color backColor, Color foreColor, bool isDarkMode)
    {
        foreach (Control control in controls)
        {
            switch (control)
            {
                case Button button:
                    button.UseVisualStyleBackColor = false;
                    button.BackColor = isDarkMode ? Color.FromArgb(55, 55, 55) : SystemColors.Control;
                    button.ForeColor = foreColor;
                    break;
                case TextBox or ComboBox:
                    control.BackColor = isDarkMode ? Color.FromArgb(45, 45, 45) : SystemColors.Window;
                    control.ForeColor = foreColor;
                    break;
                case DataGridView grid:
                    ApplyColorsToDataGridView(grid, isDarkMode);
                    break;
                case MenuStrip menuStrip:
                    menuStrip.BackColor = backColor;
                    menuStrip.ForeColor = foreColor;
                    break;
                default:
                    control.BackColor = backColor;
                    control.ForeColor = foreColor;
                    break;
            }

            if (control.HasChildren)
                ApplyColorsToControls(control.Controls, backColor, foreColor, isDarkMode);
        }
    }

    private static void ApplyColorsToDataGridView(DataGridView grid, bool isDarkMode)
    {
        grid.BackgroundColor = isDarkMode ? Color.FromArgb(32, 32, 32) : SystemColors.AppWorkspace;
        grid.GridColor = isDarkMode ? Color.FromArgb(80, 80, 80) : SystemColors.ControlDark;
        grid.DefaultCellStyle.BackColor = isDarkMode ? Color.FromArgb(45, 45, 45) : SystemColors.Window;
        grid.DefaultCellStyle.ForeColor = isDarkMode ? Color.WhiteSmoke : SystemColors.ControlText;
        grid.DefaultCellStyle.SelectionBackColor = isDarkMode ? Color.FromArgb(75, 75, 75) : SystemColors.Highlight;
        grid.DefaultCellStyle.SelectionForeColor = isDarkMode ? Color.White : SystemColors.HighlightText;
        grid.ColumnHeadersDefaultCellStyle.BackColor = isDarkMode ? Color.FromArgb(55, 55, 55) : SystemColors.Control;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = isDarkMode ? Color.WhiteSmoke : SystemColors.ControlText;
        grid.EnableHeadersVisualStyles = !isDarkMode;
    }
}
