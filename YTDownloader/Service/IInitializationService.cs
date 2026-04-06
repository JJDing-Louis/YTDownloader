using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTDownloader.Service
{
    public interface IInitializationService
    {
        public new Dictionary<string, List<KeyValuePair<string, string>>> GetOptions();
    }
}
