namespace YTDownloader.Service
{
    public interface IInitializationService
    {
        public new Dictionary<string, List<KeyValuePair<string, string>>> GetOptions();
    }
}