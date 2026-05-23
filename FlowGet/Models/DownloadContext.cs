using FlowGet.Abstractions.Common;
using FlowGet.Abstractions.M3uDownloaders;
using FlowGet.Abstractions.Models;
using System.Net.Http;

namespace FlowGet.Models
{
    public class DownloadContext(HttpClient httpClient,
        ILog log,
        IDownloadParamBase downloadParamBase,
        IDownloaderSetting downloaderSetting
            ) : IDownloadContext
    {
        public HttpClient HttpClient { get; private set; } = httpClient;

        public ILog Log { get; private set; } = log;

        public IDownloadParamBase DownloadParam { get; private set; } = downloadParamBase;

        public IDownloaderSetting DownloaderSetting { get; private set; } = downloaderSetting;
    }
}
