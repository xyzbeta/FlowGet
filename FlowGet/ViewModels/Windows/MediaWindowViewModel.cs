using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowGet.Abstractions.Common;
using FlowGet.Common.DownloadPrams;
using FlowGet.Extensions;
using FlowGet.FrameWork;
using FlowGet.Models;
using FlowGet.Services;
using FlowGet.Utils;
using FlowGet.ViewModels.Downloads;
using System;
using System.Diagnostics;
using System.Net.Http;

namespace FlowGet.ViewModels.Windows
{
    public partial class MediaWindowViewModel : ViewModelBase
    {
        private readonly SettingsService settingsService;
        private readonly ViewModelManager viewModelManager;
        private readonly SnackbarManager notification;

        public MediaDownloadInfo MediaDownloadInfo { get; } = new MediaDownloadInfo();
        public Action<DownloadViewModel> EnqueueDownloadAction { get; set; } = default!;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ProcessMediaDownloadCommand))]
        public partial bool IsBusy { get; private set; }

        public MediaWindowViewModel(SettingsService settingsService, ViewModelManager viewModelManager, SnackbarManager Notifications)
        {
            this.settingsService = settingsService;
            this.viewModelManager = viewModelManager;
            notification = Notifications;
        }

        private bool CanProcessMediaDownload() => !IsBusy;

        [RelayCommand(CanExecute = nameof(CanProcessMediaDownload))]
        private void ProcessMediaDownload(MediaDownloadInfo mediaDownloadInfo)
        {
            IsBusy = true;
            try
            {
                Uri VideoUri = MediaDownloadInfo.GetVideoRequestUri();
                Uri? AudioUri = MediaDownloadInfo.GetAudioRequestUri();
                MediaDownloadParams mediaDownloadParams = new(settingsService.SavePath, VideoUri, AudioUri, mediaDownloadInfo.VideoName, settingsService.Headers)
                {
                    IsVideoStream = mediaDownloadInfo.StreamIndex == 0
                };
                ProcessMediaDownload(null,mediaDownloadParams);

                mediaDownloadInfo.Reset(settingsService.IsResetAddress, settingsService.IsResetName);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                notification.Info(e.ToString());
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ProcessMediaDownload(HttpClient? httpClient, IMediaDownloadParam mediaDownloadParams)
        {
            FileEx.EnsureFileNotExist(mediaDownloadParams.VideoFullName);

            DownloadViewModel download = viewModelManager.CreateDownloadViewModel(httpClient,mediaDownloadParams);
            if (download is null) return;

            EnqueueDownloadAction(download);
        }
    }
}
