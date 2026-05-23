using FlowGet.Abstractions.Common;
using FlowGet.Abstractions.M3u8;
using FlowGet.Core;
using FlowGet.Models;
using FlowGet.Services;
using FlowGet.Utils;
using FlowGet.ViewModels.Downloads;
using System.Net.Http;


namespace FlowGet.FrameWork;

public class ViewModelManager(SettingsService settingsService)
{
    public  DownloadViewModel CreateDownloadViewModel(
           HttpClient? httpClient,
           IM3u8DownloadParam m3U8DownloadParam)
    {
        DownloadViewModel viewModel = new(m3U8DownloadParam)
        {
            RequestUrl = m3U8DownloadParam.RequestUrl,
            VideoName = m3U8DownloadParam.VideoName
        };
        DownloadContext downloadContext = new(httpClient ?? Http.Instance.GetClient(), viewModel.Log,m3U8DownloadParam, settingsService.Clone<SettingsService>());
        viewModel.downloaderCoreClient = new(downloadContext);
        return viewModel;
    }

    public  DownloadViewModel CreateDownloadViewModel(
        HttpClient? httpClient,
        IM3uFileInfo m3UFileInfo,
        IDownloadParamBase m3U8DownloadParam)
    {
        DownloadViewModel viewModel = new(m3U8DownloadParam)
        {
            VideoName = m3U8DownloadParam.VideoName
        };

        DownloadContext downloadContext = new(httpClient ?? Http.Instance.GetClient(), viewModel.Log, m3U8DownloadParam, settingsService.Clone<SettingsService>());
        viewModel.downloaderCoreClient = new(downloadContext, m3UFileInfo);
        return viewModel;
    }

    public  DownloadViewModel CreateDownloadViewModel(
         HttpClient? httpClient,
         IMediaDownloadParam mediaDownloadParam)
    {
        DownloadViewModel viewModel = new(mediaDownloadParam)
        {
            RequestUrl = mediaDownloadParam.Medias[0].Url,
            VideoName = mediaDownloadParam.VideoName
        };
        DownloadContext downloadContext = new(httpClient ?? Http.Instance.GetClient(), viewModel.Log, mediaDownloadParam, settingsService.Clone<SettingsService>());
        viewModel.downloaderCoreClient = DownloaderCoreClient.CreateMediaDownloader(downloadContext);
        return viewModel;
    }
}
