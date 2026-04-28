using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using YTDownloader.Model;

namespace YTDownloader.Service;

public sealed class ConfigSettingService
{
    /// <summary>
    /// 設定檔預設路徑
    /// </summary>
    private readonly string _filePath;

    public ConfigSettingService(string? filePath = null)
    {
        _filePath = string.IsNullOrWhiteSpace(filePath)
            ? Path.Combine(AppContext.BaseDirectory, "Config.json")
            : filePath;
    }

    public ConfigModel Init()
    {
        if (!File.Exists(_filePath))
        {
            //直接建立一個預設設定檔
            Save(new ConfigModel());
        }

        return Load();
    }

    public ConfigModel Load()
    {
        if (!File.Exists(_filePath))
            return new ConfigModel();

        var json = File.ReadAllText(_filePath);

        return JsonConvert.DeserializeObject<ConfigModel>(json, GetJsonSettings())
               ?? new ConfigModel();
    }

    public void Save(ConfigModel settings)
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var json = JsonConvert.SerializeObject(settings, GetJsonSettings());
        File.WriteAllText(_filePath, json);
    }

    private static JsonSerializerSettings GetJsonSettings()
    {
        return new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
    }
}
