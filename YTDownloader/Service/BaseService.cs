using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// TODO: 下個版本要把邏輯轉移到Service裡面，做UI解構
namespace YTDownloader.Service
{
    internal class BaseService : IDisposable
    {
        public BaseService() 
        {
        
        }

        public virtual void InitData()
        {
            //TODO: 這裡是用來初始化資料的，像是從資料庫撈資料，或是從檔案讀取資料等等
        }

        public virtual void Dispose()
        {
            //TODO: 這裡是用來釋放資源的，像是關閉資料庫連線，釋放檔案資源等等
        }
    }
}
