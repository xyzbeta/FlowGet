using FlowGet.Abstractions.M3u8;

namespace FlowGet.Abstractions.M3uDownloaders
{
    public interface IDownloadService
    {
        ValueTask Initialization(CancellationToken cancellationToken = default);

        Task StartDownload(IM3uFileInfo m3UFileInfo, CancellationToken cancellationToken = default);

        Task<bool> DownloadM3uMediaInfo(IM3uMediaInfo m3UMediaInfo, IEnumerable<KeyValuePair<string, string>>? headers, string mediaPath, CancellationToken cancellationToken = default);

        Func<Stream, CancellationToken, Task<Stream>> HandleDataFunc { get; set; }

        Func<string, Stream, CancellationToken, Task> WriteToFileFunc { get; set; }
    }
}
