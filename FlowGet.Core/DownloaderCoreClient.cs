using FlowGet.Abstractions.Common;
using FlowGet.Abstractions.Downloader;
using FlowGet.Abstractions.M3u8;
using FlowGet.Abstractions.M3uDownloaders;
using FlowGet.Abstractions.Models;
using FlowGet.Core.Downloads;

namespace FlowGet.Core
{
    public class DownloaderCoreClient
    {
        public IDownloader Downloader { get; } = default!;

        public DownloaderCoreClient(IDownloadContext context)
        {
            Downloader = M3u8Downloader.CreateM3u8Downloader(context);
        }

        public DownloaderCoreClient(IDownloadContext context, IM3uFileInfo m3UFileInfo)
        {
            Downloader = M3u8Downloader.CreateM3u8Downloader(context, m3UFileInfo);
        }

        private DownloaderCoreClient(IDownloader downloader) { Downloader = downloader; }

        public static DownloaderCoreClient CreateMediaDownloader(IDownloadContext context)
        {
            return new DownloaderCoreClient(MediaDownloader.CreateMediaDownloader(context));
        }
    }
}
