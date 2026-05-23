using FlowGet.Abstractions.Common;

namespace FlowGet.Abstractions.Downloader
{
    public interface IDownloader
    {
        ValueTask StartDownload(Action<int> StateAction,IDialogProgress dialogProgress, CancellationToken cancellationToken);
    }
}
