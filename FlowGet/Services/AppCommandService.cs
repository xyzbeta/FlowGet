using FlowGet.Abstractions.Common;
using FlowGet.Abstractions.M3u8;
using System;
using System.Net.Http;

namespace FlowGet.Services
{
    using DownloadByM3uFileInfoActionType = Action<HttpClient?, IDownloadParamBase, IM3uFileInfo>;
    using DownloadByUrlActionType = Action<HttpClient?, IM3u8DownloadParam>;
    using DownloadMediaActionType = Action<HttpClient?, IMediaDownloadParam>;

    public class AppCommandService(
            DownloadByUrlActionType downloadByUrl,
            DownloadByM3uFileInfoActionType downloadByM3uFileInfo,
            DownloadMediaActionType downloadMedia) : IAppCommandService
    {
        public void DownloadByM3uFileInfo(HttpClient? httpClient, IDownloadParamBase downloadParamBase, IM3uFileInfo m3UFileInfo)
        {
            downloadByM3uFileInfo(httpClient, downloadParamBase, m3UFileInfo);
        }

        public void DownloadByUrl(HttpClient? httpClient, IM3u8DownloadParam m3U8DownloadParam)
        {
            downloadByUrl(httpClient, m3U8DownloadParam);
        }

        public void DownloadMedia(HttpClient? httpClient, IMediaDownloadParam mediaDownloadParam)
        {
            downloadMedia(httpClient, mediaDownloadParam);
        }
    }
}
