using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Tools;

namespace YTDownloader.Service
{
    public class PlaylistHandlerInitializationService : IInitializationService
    {
        public PlaylistHandlerInitializationService()
        {
        }

        public IConfiguration GetConfig()
        {
            var config = ParameterTool.GetConfiguration();
            return config;
        }

        public Dictionary<string, List<KeyValuePair<string, string>>> GetOptions()
        {
            return null;
        }
    }
}
