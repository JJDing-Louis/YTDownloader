using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using YTDownloader.Model;

namespace YTDownloader.Service;

public sealed class ConfigService
{
    /// <summary>
    /// 設定檔預設路徑
    /// </summary>
    private readonly string _filePath;
    
    private ConfigModel _configModel = new();

    public ConfigService(string? filePath = null)
    {
        _filePath = string.IsNullOrWhiteSpace(filePath)
            ? Path.Combine(AppContext.BaseDirectory, "Config.json")
            : filePath;
        if (!File.Exists(_filePath))
        {
            //直接建立一個預設設定檔
            Save(new ConfigModel());
        }
        Init();
    }
    
    private void Init()
    {
        if (!File.Exists(_filePath))
            Save(new ConfigModel()); 

        var json = File.ReadAllText(_filePath);

        _configModel = JsonConvert.DeserializeObject<ConfigModel>(json, GetJsonSettings())
                       ?? new ConfigModel();
    }

    public ConfigModel Load()
    {
        return _configModel;
    }

    public void Save(ConfigModel settings)
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var json = JsonConvert.SerializeObject(settings, GetJsonSettings());
        File.WriteAllText(_filePath, json);
        _configModel = settings;
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
