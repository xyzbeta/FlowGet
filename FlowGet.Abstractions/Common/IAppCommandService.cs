using FlowGet.Abstractions.M3u8;
using System;

namespace FlowGet.Abstractions.Common
{
    public interface IAppCommandService
    {
        void DownloadByM3uFileInfo(HttpClient? httpClient, IDownloadParamBase downloadParamBase, IM3uFileInfo m3UFileInfo);
        void DownloadByUrl(HttpClient? httpClient, IM3u8DownloadParam m3U8DownloadParam);
        void DownloadMedia(HttpClient? httpClient, IMediaDownloadParam mediaDownloadParam);
    }
}
