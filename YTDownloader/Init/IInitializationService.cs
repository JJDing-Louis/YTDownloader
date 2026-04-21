using Microsoft.Extensions.Configuration;

namespace YTDownloader.Init
{
    public interface IInitializationService
    {
        public  Dictionary<string, List<KeyValuePair<string, string>>> GetOptions();
        public IConfiguration GetConfig();
    }
}