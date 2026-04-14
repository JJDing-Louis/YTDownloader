using Microsoft.Extensions.Configuration;

namespace YTDownloader.Service
{
    public interface IInitializationService
    {
        public  Dictionary<string, List<KeyValuePair<string, string>>> GetOptions();
        public IConfiguration GetConfig();
    }
}