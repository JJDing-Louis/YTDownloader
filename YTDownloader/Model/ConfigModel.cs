using System.Text.Json.Serialization;

namespace YTDownloader.Model;

public sealed class ConfigModel
{
    /// <summary>
    ///     一般設定
    /// </summary>
    public GeneralConfigModel General { get; set; } = new();

    /// <summary>
    ///     外觀設定
    /// </summary>
    public AppearanceConfigModel Appearance { get; set; } = new();

    /// <summary>
    ///     下載執行序設定
    /// </summary>
    public ThreadConfigModel Thread { get; set; } = new();

    /// <summary>
    ///     儲存設定
    /// </summary>
    public SaveConfigModel Save { get; set; } = new();
}

public sealed class GeneralConfigModel
{
    /// <summary>
    ///     UI 顯示語言
    /// </summary>
    public string Language { get; set; } = "zh-TW";
}

public sealed class AppearanceConfigModel
{
    /// <summary>
    ///     是否為黑暗模式
    /// </summary>
    public bool IsDarkMode { get; set; } = false;

    /// <summary>
    ///     背景顏色
    /// </summary>
    public string BackColor { get; set; } = "#F0F0F0";

    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public Color BackColorValue
    {
        get
        {
            if (string.IsNullOrWhiteSpace(BackColor))
                return SystemColors.Control;

            try
            {
                var color = ColorTranslator.FromHtml(BackColor.Trim());
                return color.IsEmpty ? SystemColors.Control : color;
            }
            catch (Exception)
            {
                return SystemColors.Control;
            }
        }
    }

    /// <summary>
    ///     背景圖片
    /// </summary>
    public string BackGroundImage { get; set; } = string.Empty;

    /// <summary>
    ///     字型
    /// </summary>
    public string Font { get; set; } = string.Empty;

    /// <summary>
    ///     字型大小
    /// </summary>
    public int FontSize { get; set; } = 12;
}

public sealed class ThreadConfigModel
{
    /// <summary>
    ///     初始執行緒數量
    /// </summary>
    public int InitialCount { get; set; } = 3;

    /// <summary>
    ///     最大執行緒數量
    /// </summary>
    public int MaxCount { get; set; } = 3;
}

public sealed class SaveConfigModel
{
    /// <summary>
    ///     存檔資料夾
    /// </summary>
    public string DownloadPath { get; set; } = Environment.CurrentDirectory;
}