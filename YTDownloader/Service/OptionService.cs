using JJNET.DataAccess.Entity;
using JJNET.Utility.Tools;

namespace YTDownloader.Service;

public class OptionService
{
    public Dictionary<string, List<KeyValuePair<string, string>>> options = new();

    public OptionService()
    {
        var tables = GetListOptionTable();

        foreach (var table in tables) options.Add(table, GetListOptions(table));
    }

    public Dictionary<string, List<KeyValuePair<string, string>>> GetOptions()
    {
        return options;
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