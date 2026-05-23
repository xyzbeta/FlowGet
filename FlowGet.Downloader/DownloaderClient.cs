using FlowGet.Downloader.M3uDownloaders;
using FlowGet.Abstractions.Common;
using FlowGet.Downloader.MediaDownloads;
using FlowGet.Abstractions.M3u8;
using FlowGet.Common.Extensions;
using FlowGet.Abstractions.Models;
using FlowGet.Abstractions.M3uDownloaders;


namespace FlowGet.Downloader
{
    public class DownloaderClient(IDownloadContext context)
    {
        private IDownloadService? _m3u8downloader;
        private DownloaderBase? _mediaDownloader;

        public IDialogProgress DialogProgress { get; set; } = default!;
        public IM3uFileInfo M3UFileInfo { get; set; } = default!;
        public Func<TimeSpan,CancellationToken, Task<IM3uFileInfo>> GetLiveFileInfoFunc { get; set; } = default!;

        public IDownloadService M3u8Downloader
        {
            get
            {
                if (_m3u8downloader is null)
                {
                    IDownloadService m3U8Downloader = new M3u8Downloader(context, DialogProgress);

                    if (!M3UFileInfo.IsVod())
                    {
                        LiveM3uDownloader liveM3UDownloader = new(m3U8Downloader,context, DialogProgress)
                        {
                            GetLiveFileInfoFunc = GetLiveFileInfoFunc
                        };
                        m3U8Downloader = liveM3UDownloader;
                    }
                    else if (M3UFileInfo.Key is not null)
                        m3U8Downloader = new CryptM3uDownloader(m3U8Downloader, context);

                _m3u8downloader = m3U8Downloader;
                }
                return _m3u8downloader;
            }
        }

        public DownloaderBase MediaDownloader
        {
            get
            {
                IMediaDownloadParam mediaDownloadParam = (IMediaDownloadParam)context.DownloadParam;
                if(_mediaDownloader is null)
                {
                    if (mediaDownloadParam.IsVideoStream)
                        _mediaDownloader = new MediaDownloader(context);
                    else
                        _mediaDownloader = new LiveVideoDownloader(context);
                }

                _mediaDownloader.DialogProgress = DialogProgress;
                return _mediaDownloader;
            }
        }
    }
}
