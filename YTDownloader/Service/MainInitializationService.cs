using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Tools;

namespace YTDownloader.Service
{
    public class MainInitializationService : IInitializationService
    {
        public MainInitializationService()
        {
        
        }

        public new Dictionary<string, List<KeyValuePair<string, string>>> GetOptions() 
        {
            var options = new  Dictionary<string, List<KeyValuePair<string, string>>>();
            options.Add("ListMediaType", GetListMediaType());
            options.Add("ListSourceType",GetListSourceType());

            return options;
        }

        public List<KeyValuePair<string, string>> GetListMediaType() 
        {
            var options = new List<KeyValuePair<string, string>>();
            using (var con = ConnectionTool.GetConnection()) 
            {
                    var cmd = con.CreateCommand();
                     cmd.CommandText = """
                    SELECT
                        *
                    FROM ListMediaType;
                    """;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            options.Add(new KeyValuePair<string, string>(reader["Desc"].ToString()!, reader["Name"].ToString()!));
                        }
                }
            }
            return options;
        }

        public List<KeyValuePair<string, string>> GetListSourceType()
        {
            var options = new List<KeyValuePair<string, string>>();
            using (var con = ConnectionTool.GetConnection())
            {
                var cmd = con.CreateCommand();
                cmd.CommandText = """
                    SELECT
                        *
                    FROM ListSourceType;
                    """;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        options.Add(new KeyValuePair<string, string>(reader["Desc"].ToString()!, reader["Name"].ToString()!));
                    }
                }
            }
            return options;
        }
    }
}
