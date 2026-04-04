using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utility.Tools;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Utility
{
    public class ConfigurationProvider : JsonConfigurationProvider
    {
        private JsonConfigurationSource source;

        public ConfigurationProvider(JsonConfigurationSource source) : base(source)
        {
            this.source = source;
        }

        public override void Load(Stream stream)
        {
            base.Load(stream);

            foreach (var p in Data)
            {
                if (p.Key.StartsWith("ConnectionStrings:"))
                {
                    var key = p.Key;
                    var value1 = (p.Value);
                    try
                    {
                        var value2 = DecryptSQLConnectionString(value1);
                        Data[key] = value2;
                    }
                    catch (Exception)
                    {
                        Data[key] = value1;
                    }
                }
            }
        }




        public static string DecryptSQLConnectionString(string value, string key = null, string iv = null)
        {
            var builder = new SqlConnectionStringBuilder(value);

            var pw = builder.Password;


            // 舊語法 $$開頭 $$結尾
            var pattern1 = @"\$\$(.*?)\$\$";


            // 定義正則表達式，匹配 AES256{...}
            var pattern3 = @"AES256\{(.*?)\}";


            var match1 = Regex.Match(pw, pattern1);

            var match3 = Regex.Match(pw, pattern3);


            if (match1.Success)
            {
                pw = match1.Groups[1].Value;
                pw = SecurityTool.SensitiveStringDecode(pw, key, iv);
                builder.Password = pw;
                value = builder.ConnectionString;
            }
            else if (match3.Success)
            {
                pw = match3.Groups[1].Value;
                pw = SecurityTool.DecryptAES256(pw, key, iv);
                builder.Password = pw;
                value = builder.ConnectionString;
            }
            //相容.net framework 4.x
            value = value.Replace("Trust Server Certificate", "TrustServerCertificate");
            //相容.net framework 4.x
            value = value.Replace("Multiple Active Result Sets", "MultipleActiveResultSets");
            return value;
        }
    }
}
