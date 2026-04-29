using JJNET.DataAccess.Entity;
using JJNET.Utility.Tools;

namespace YTDownloader.Service;

public class OptionService
{
    private static readonly object OptionsLock = new();
    private static Dictionary<string, List<KeyValuePair<string, string>>> options = new();

    public OptionService()
    {
        LoadOptions();
    }

    private static void LoadOptions()
    {
        var loadedOptions = new Dictionary<string, List<KeyValuePair<string, string>>>();
        var tables = GetListOptionTable();

        foreach (var table in tables) loadedOptions.Add(table, GetListOptions(table));

        lock (OptionsLock)
        {
            options = loadedOptions;
        }
    }

    public static Dictionary<string, List<KeyValuePair<string, string>>> GetOptions()
    {
        EnsureOptionsLoaded();
        return options;
    }

    /// <summary>
    /// 取得指定外部選單的全部項目。Key 為 Desc（顯示文字），Value 為 Name（資料庫值）。
    /// </summary>
    public static IReadOnlyList<KeyValuePair<string, string>> GetOptions(string listName)
    {
        EnsureOptionsLoaded();
        return options.TryGetValue(listName, out var items) && items != null
            ? items
            : Array.Empty<KeyValuePair<string, string>>();
    }

    /// <summary>
    /// 依 Name（資料庫值）取得 Desc（顯示文字）。
    /// </summary>
    public static string GetOptionDesc(string listName, string name)
    {
        var desc = GetOptions(listName)
            .FirstOrDefault(item => item.Value == name)
            .Key;

        return string.IsNullOrWhiteSpace(desc) ? name : desc;
    }

    /// <summary>
    /// 依 Desc（顯示文字）取得 Name（資料庫值）。
    /// </summary>
    public static string GetOptionName(string listName, string desc)
    {
        var name = GetOptions(listName)
            .FirstOrDefault(item => item.Key == desc)
            .Value;

        return string.IsNullOrWhiteSpace(name) ? desc : name;
    }

    private static void EnsureOptionsLoaded()
    {
        if (options.Count > 0) return;

        lock (OptionsLock)
        {
            if (options.Count == 0)
                LoadOptions();
        }
    }

    private static List<string> GetListOptionTable()
    {
        var tables = new List<string>();
        try
        {
            using (var conn = ConnectionTool.GetConnection())
            {
                var sql = """
                          SELECT 
                              MasterEntity 
                          FROM TEntity
                          WHERE MasterEntity LIKE 'List%'
                          """;
                tables = conn.Query<string>(sql).ToList();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return tables;
    }

    private static List<KeyValuePair<string, string>> GetListOptions(string tableName, bool ignoreMissingTable = false)
    {
        var options = new List<KeyValuePair<string, string>>();
        try
        {
            using (var con = ConnectionTool.GetConnection())
            {
                var cmd = con.CreateCommand();
                cmd.CommandText = $"""
                                   SELECT
                                       Name,
                                       Desc
                                   FROM {tableName}
                                   """;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        options.Add(new KeyValuePair<string, string>(reader["Desc"].ToString()!,
                            reader["Name"].ToString()!));
                }
            }
        }
        catch
        {
            if (!ignoreMissingTable)
                throw;
        }

        return options;
    }
}
