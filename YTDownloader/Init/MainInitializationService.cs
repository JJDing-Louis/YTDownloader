using Microsoft.Extensions.Configuration;
using JJNET.DataAccess;
using JJNET.Utility.Tools;

namespace YTDownloader.Init
{
    public class MainInitializationService : IInitializationService
    {
        public MainInitializationService()
        {
        }

        public Dictionary<string, List<KeyValuePair<string, string>>> GetOptions()
        {
            return new Dictionary<string, List<KeyValuePair<string, string>>>
            {
                ["ListMediaType"] = GetListMediaType(),
                ["ListSourceType"] = GetListSourceType(),
                ["ListDownloadStatus"] = GetListDownloadStatus()
            };
        }

        public IConfiguration GetConfig()
        {
            var config =ParameterTool.GetConfiguration();
            return config;
        }

        public List<KeyValuePair<string, string>> GetListMediaType()
        {
            return GetListOptions("ListMediaType");
        }

        public List<KeyValuePair<string, string>> GetListSourceType()
        {
            return GetListOptions("ListSourceType");
        }

        public List<KeyValuePair<string, string>> GetListDownloadStatus()
        {
            return GetListOptions("ListDownloadStatus", ignoreMissingTable: true);
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
                        {
                            options.Add(new KeyValuePair<string, string>(reader["Desc"].ToString()!, reader["Name"].ToString()!));
                        }
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
}
