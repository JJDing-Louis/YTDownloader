/// <summary>
/// TODO: 下個版本要把邏輯轉移到Service裡面，做UI解構

namespace YTDownloader.Service;

internal interface IService : IDisposable
{
    //TODO: 這裡是用來初始化資料的，像是從資料庫撈資料，或是從檔案讀取資料等等
    public void InitData();

    //TODO: 這裡是用來釋放資源的，像是關閉資料庫連線，釋放檔案資源等等
    public void Dispose();
}