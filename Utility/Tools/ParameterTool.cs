using Microsoft.Extensions.Configuration;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Utility.Tools
{
    public class ParameterTool
    {
        private static readonly Dictionary<string, IConfiguration> configs = new();
        private static readonly ILogger logger = LoggerTool.GetLogger("ParameterTools");




        public static IConfiguration GetConfiguration()
        {
            var name = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
            if (configs.TryGetValue(name, out var configuration))
                return configuration;
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{name}.json", true, true)
                .Build();
            configs[name] = config;
            return config;
        }


        public static void WriteParam(string key, object val, string description = null)
        {
            string TVal;
            string DataType;
            if (string.IsNullOrEmpty(description))
            {
                var p = ReadParam(key);
                description = p.Description;
            }

            if (val != null)
            {
                switch (val)
                {
                    case int _val:
                        TVal = _val.ToString("D");
                        DataType = "int";
                        break;
                    case decimal _val:
                        TVal = _val.ToString("F");
                        DataType = "decimal";
                        break;
                    case DateTime _val:
                        TVal = _val.ToString("O");
                        DataType = "DateTime";
                        break;
                    case TimeSpan _val:
                        TVal = _val.ToString("g");
                        DataType = "TimeSpan";
                        break;
                    case bool _val:
                        TVal = _val.ToString();
                        DataType = "bool";
                        break;
                    default:
                        TVal = val.ToString();
                        DataType = "string";
                        break;
                }
            }
            else
            {
                TVal = null;
                DataType = "string";
            }

            try
            {
                using (var conn = ConnectionTool.GetConnection())
                {
                    conn.Execute($@"
                    merge {SystemEnum.SPParameter} as target
                    using (select @TKey TKey,@Desc Description,@TVal TVal,@DataType DataType) as Src 
                        on target.TKey = @TKey
                    when not matched then
                         insert (TKey,Description,DataType,TVal,RID) values (@Tkey,@Desc,@DataType,@TVal,@TKey) 
                    when matched then
                        update set Description = isnull(@Desc,target.Description),DataType = @DataType,TVal = @TVal,RID=@TKey;
                    ", new
                    {
                        TKey = key,
                        TVal = val,
                        DataType = DataType,
                        Desc = description
                    });
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error in write Parameter for key {key} with value {val}");
            }
        }

        public static bool Exists(string key)
        {
            using (var conn = ConnectionTool.GetConnection())
            {
                return conn.Query<dynamic>("select * from " + SystemEnum.SPParameter + " where TKey=@TKey", new { TKey = key }).Any();
            }
        }

        public static decimal? ReadDecimalParam(string key, decimal? DefaultNullValue = null)
        {
            var parameter = ReadParam(key);
            if (parameter == null)
                return DefaultNullValue;


            try
            {
                var val = StringTool.ParseDecimal(parameter.TVal);
                return val.Value;
            }
            catch (Exception)
            {
            }

            return DefaultNullValue;
        }

        public static string ReadStringParam(string key, string? DefaultNullValue = null)
        {
            var parameter = ReadParam(key);
            if (parameter == null)
                return DefaultNullValue;

            return parameter.TVal;
        }

        public static int? ReadIntParam(string key, int? DefaultNullValue = null)
        {
            var parameter = ReadParam(key);
            if (parameter == null)
                return DefaultNullValue;

            try
            {
                var val = int.Parse(parameter.TVal);
                return val;
            }
            catch (Exception)
            {
            }

            return DefaultNullValue;
        }

        public static DateTime? ReadDateTimeParam(string key, DateTime? DefaultNullValue = null)
        {
            var parameter = ReadParam(key);
            if (parameter == null)
                return DefaultNullValue;

            try
            {
                var val = StringTool.ParseISO8601String(parameter.TVal);
                return val.Value;
            }
            catch (Exception)
            {
            }

            return DefaultNullValue;
        }


        public static bool? ReadBoolParam(string key, bool? DefaultNullValue = null)
        {
            var parameter = ReadParam(key);
            if (parameter == null)
                return DefaultNullValue;

            try
            {
                var val = StringTool.ParseBoolean(parameter.TVal);
                return val.Value;
            }
            catch (Exception)
            {
            }

            return DefaultNullValue;
        }


        public static TimeSpan? ReadTimeParam(string key, TimeSpan? DefaultNullValue = null)
        {
            var parameter = ReadParam(key);
            if (parameter == null)
                return DefaultNullValue;

            try
            {
                var val = StringTool.ParseISO8601String(parameter.TVal);
                return val.Value - DateTime.Today;
            }
            catch (Exception)
            {
            }

            return DefaultNullValue;
        }

        public static Parameter ReadParam(string key)
        {
            try
            {
                using (var conn = ConnectionTool.GetConnection())
                {
                    return conn.Query<Parameter>("select * from " + SystemEnum.SPParameter + " where TKey=@key", new { key })
                        .FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error in read Parameter for key {key}");
                return null;
            }
        }

        public static List<Parameter> ReadAllParam()
        {
            try
            {
                using (var conn = ConnectionTool.GetConnection())
                {
                    return conn.Query<Parameter>("select * from " + SystemEnum.SPParameter).ToList();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in read all Parameters ");
                return null;
            }
        }
    }

    public class Parameter
    {
        public string DataType;
        public string Description;
        public string TKey;
        public string TVal;

        public override string ToString()
        {
            return
                $"{nameof(TKey)}: {TKey}, {nameof(Description)}: {Description}, {nameof(DataType)}: {DataType}, {nameof(TVal)}: {TVal}";
        }
    }
}
