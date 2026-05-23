using FlowGet.Abstractions.Common;
using FlowGet.Abstractions.M3uDownloaders;


namespace FlowGet.Abstractions.Models
{
    public interface IDownloadContext
    {
        HttpClient HttpClient { get; }
        ILog Log { get; }
        IDownloadParamBase DownloadParam { get; }
        IDownloaderSetting DownloaderSetting { get; }
    }
}
