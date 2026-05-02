using JJNET.DataAccess.Entity;
using JJNET.Utility.Tools;
using YTDownloader.Model;

namespace YTDownloader.Service;

public class DownloadHistoryService : IService
{
    private const string OptionListMediaType = "ListMediaType";
    private const string OptionListDownloadStatus = "ListDownloadStatus";

    public DownloadHistorySearchCriteria CreateSearchCriteria(
        bool includeFileName,
        string fileName,
        bool includeDownloadDate,
        DateTime downloadStartDate,
        DateTime downloadEndDate,
        bool includeDownloadResult,
        string downloadResult,
        bool includeMediaType,
        bool includeAudio,
        bool includeVideo)
    {
        return new DownloadHistorySearchCriteria(
            includeFileName ? fileName.Trim() : null,
            includeDownloadDate ? downloadStartDate.Date : null,
            includeDownloadDate ? downloadEndDate.Date : null,
            includeDownloadResult ? downloadResult : null,
            includeMediaType && includeAudio ? "Audio" : null,
            includeMediaType && includeVideo ? "Video" : null);
    }

    public IReadOnlyList<DownloadHistorySearchItem> Search(DownloadHistorySearchCriteria criteria)
    {
        const string sqlcmd = """
                              SELECT
                                  *
                              FROM DownloadHistory
                              WHERE (@FileName IS NULL OR FileName LIKE '%' || @FileName || '%')
                              AND (@DownloadStartDate IS NULL OR DownloadDateTime >= @DownloadStartDate)
                              AND (@DownloadEndDate IS NULL OR DownloadDateTime <= @DownloadEndDate)
                              AND (@DownloadResult IS NULL OR Status = @DownloadResult)
                              AND (
                                  (@IsAudio IS NULL AND @IsVideo IS NULL)
                                  OR Type = @IsAudio
                                  OR Type = @IsVideo
                              )
                              ORDER BY DownloadDateTime DESC
                              """;

        using var conn = ConnectionTool.GetConnection();
        return conn.Query<DownloadHistory>(sqlcmd, criteria)
            .Select((item, index) => ToSearchItem(item, index + 1))
            .ToList();
    }

    public IReadOnlyList<DownloadRequest> CreateRedownloadRequests(
        IEnumerable<DownloadHistorySearchItem> selectedItems)
    {
        return selectedItems
            .Where(item => !string.IsNullOrWhiteSpace(item.Url))
            .Select(item =>
            {
                var mediaType = OptionService.GetOptionName(OptionListMediaType, item.MediaTypeDisplay);

                return new DownloadRequest
                {
                    Title = item.Title,
                    WebpageUrl = item.Url,
                    MediaType = mediaType,
                    MediaTypeDisplay = OptionService.GetOptionDesc(OptionListMediaType, mediaType),
                    DownloadDir = item.Path
                };
            })
            .ToList();
    }

    public void Dispose()
    {
    }

    public void Init()
    {
    }

    private static DownloadHistorySearchItem ToSearchItem(DownloadHistory item, int index)
    {
        return new DownloadHistorySearchItem(
            index,
            item.FileName ?? string.Empty,
            FormatLocalDateTime(item.DownloadDateTime),
            OptionService.GetOptionDesc(OptionListMediaType, item.Type ?? string.Empty),
            OptionService.GetOptionDesc(OptionListDownloadStatus, item.Status ?? string.Empty),
            item.TaskID,
            item.Title ?? string.Empty,
            item.URL ?? string.Empty,
            item.Path ?? string.Empty);
    }

    private static string FormatLocalDateTime(DateTime? dateTime)
    {
        if (dateTime == null) return string.Empty;

        var utcDateTime = dateTime.Value.Kind == DateTimeKind.Utc
            ? dateTime.Value
            : DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc);

        return utcDateTime.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss");
    }
}

public sealed record DownloadHistorySearchCriteria(
    string? FileName,
    DateTime? DownloadStartDate,
    DateTime? DownloadEndDate,
    string? DownloadResult,
    string? IsAudio,
    string? IsVideo);

public sealed record DownloadHistorySearchItem(
    int Index,
    string FileName,
    string DownloadDateTime,
    string MediaTypeDisplay,
    string StatusDisplay,
    long TaskId,
    string Title,
    string Url,
    string Path);
