using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowGet.Common.Models;
using FlowGet.Extensions;
using FlowGet.FrameWork;
using FlowGet.Models;
using FlowGet.RestServer;
using FlowGet.Services;
using FlowGet.Utils;
using FlowGet.ViewModels.Dialogs;
using FlowGet.ViewModels.Downloads;
using FlowGet.ViewModels.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace FlowGet.ViewModels.Menus
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly HttpListenService httpListenService = HttpListenService.Instance;
        private readonly SettingsService settingsService;
        private readonly ViewModelManager viewModelManager;
        private readonly List<IDisposable> _disposables = [];
        private readonly AppCommandService appCommandService;
        private readonly Queue<DownloadViewModel> _pausedQueue = new();
        public static SnackbarManager Notifications { get; } = new SnackbarManager("MainWindowHost",TimeSpan.FromSeconds(5));

        public ObservableCollection<ViewModelBase> SubWindows { get; } = [];

        public ObservableCollection<DownloadViewModel> Downloads { get; } = [];

        [ObservableProperty]
        public partial int? HttpServicePort { get; set; } = default!;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotPaused))]
        public partial bool IsGlobalPaused { get; set; }

        public bool IsNotPaused => !IsGlobalPaused;

        public MainWindowViewModel(SettingsService settingsService)
        {
            this.settingsService = settingsService;
            viewModelManager = new(settingsService);
            var m3U8WindowViewModel = new M3u8WindowViewModel(settingsService, viewModelManager, Notifications) { Title = "M3U8", EnqueueDownloadAction = EnqueueDownload };
            SubWindows.Add(m3U8WindowViewModel);
            var  mediaWindowViewModel = new MediaWindowViewModel(settingsService, viewModelManager, Notifications) { Title = "长视频", EnqueueDownloadAction = EnqueueDownload };
            SubWindows.Add(mediaWindowViewModel);
            appCommandService = InitAppCommand(m3U8WindowViewModel, mediaWindowViewModel);

            _disposables.Add(settingsService.WatchProperty(
                s => s.MaxConcurrentDownloadCount,
                () => ThrottlingSemaphore.Instance.MaxCount = settingsService.MaxConcurrentDownloadCount));

            _disposables.Add(settingsService.WatchProperty(
                s => s.ProxyInfo,
                () => Http.Instance.UpdateProxy(settingsService.ProxyInfo)));
        }

        ~MainWindowViewModel()
        {
            foreach (var item in _disposables)
            {
                item.Dispose();
            }
        }

        public AppCommandService InitAppCommand(M3u8WindowViewModel m3U8WindowViewModel, MediaWindowViewModel mediaWindowViewModel)
        {
            var appCommandService = new AppCommandService(
                (http, param) =>
                {
                    param.CompleteAttribute(settingsService);
                    Dispatcher.UIThread.Post(() =>
                    {
                        m3U8WindowViewModel.ProcessM3u8Download(http, param);
                    });
                },
                (http, param, fileinfo) =>
                {
                    param.CompleteAttribute(settingsService);
                    Dispatcher.UIThread.Post(() =>
                    {
                        m3U8WindowViewModel.ProcessM3u8Download(http, param, fileinfo);
                    });
                },
                (http, param) =>
                {
                    param.CompleteAttribute(settingsService);
                    Dispatcher.UIThread.Post(() =>
                    {
                        mediaWindowViewModel.ProcessMediaDownload(http, param);
                    });
                });

            return appCommandService;
        }

        public Task InitializeAsync()
        {
            try
            {
                settingsService.Load();

                httpListenService.Run(i => HttpServicePort = i);
                httpListenService.Initialization(appCommandService);
            }
            catch (Exception ex) {
                Notifications.Info($"初始化失敗\n {ex}");
            }
            return Task.FromResult(0);
        }

        [RelayCommand]
        private void PauseAll()
        {
            IsGlobalPaused = true;
            foreach (var download in Downloads.Where(d => d.IsActive))
            {
                download.CancelCommand.Execute(null);
                _pausedQueue.Enqueue(download);
            }
            Notifications.Info($"已暂停全部下载");
        }

        [RelayCommand]
        private void ResumeAll()
        {
            IsGlobalPaused = false;
            while (_pausedQueue.Count > 0)
            {
                var download = _pausedQueue.Dequeue();
                download.RestartCommand.Execute(null);
            }
            Notifications.Info("已恢复全部下载");
        }

        [RelayCommand]
        private void MoveUp(DownloadViewModel download)
        {
            var idx = Downloads.IndexOf(download);
            if (idx > 0)
                Downloads.Move(idx, idx - 1);
        }

        [RelayCommand]
        private void MoveDown(DownloadViewModel download)
        {
            var idx = Downloads.IndexOf(download);
            if (idx < Downloads.Count - 1)
                Downloads.Move(idx, idx + 1);
        }

        private void EnqueueDownload(DownloadViewModel download)
        {
            var existingDownloads = Downloads.FirstOrDefault(d => d == download);
            if (existingDownloads is not null)
            {
                Notifications.Info($"{download.VideoName} 已经在下载列表中!");
                return;
            }

            Downloads.Insert(0, download);

            if (IsGlobalPaused)
                _pausedQueue.Enqueue(download);
            else
                download.Start();
        }

        [RelayCommand]
        private void RestartDownloads(IList selectedDownloads)
        {
            foreach (DownloadViewModel item in selectedDownloads)
            {
                item.RestartCommand.Execute(null);
            }
        }

        [RelayCommand]
        private void StopDownloads(IList selectedDownload)
        {
            foreach (DownloadViewModel item in selectedDownload)
            {
                item.CancelCommand.Execute(null);
            }
        }

        [RelayCommand]
        private async Task RemoveDownloads(IList selectedDownload)
        {
            var tempSelectDwonload = selectedDownload.Cast<DownloadViewModel>().ToList();
            DeleteDialogViewModel deleteDialogViewModel = DeleteDialogViewModel.CreateDeleteDialogViewModel(tempSelectDwonload.Count);
            DeleteDialogResult? deleteDialogResult = await DialogManager.ShowDialogAsync(deleteDialogViewModel);
            if (deleteDialogResult is null)
                return;

            if (deleteDialogResult.DialogResult == false)
                return;

            foreach (DownloadViewModel download in tempSelectDwonload)
            {
                download.CancelCommand.Execute(null);
                Downloads.Remove(download);
                if (deleteDialogResult.IsDeleteCache)
                    download.DeleteCache();
            }
        }

        [RelayCommand]
        private void RemoveInactiveDownloads()
        {

            var inactiveDownloads = Downloads.Where(c => !c.IsActive).ToArray();
            foreach(var inactiveDownload in inactiveDownloads)
                Downloads.Remove(inactiveDownload);
        }

        [RelayCommand]
        private void RemoveSuccessDownloads()
        {
            var successDownloads = Downloads.Where(c => c.Status == DownloadStatus.Completed).ToArray();
            foreach (var successDownload in successDownloads)
                Downloads.Remove(successDownload);
        }

        [RelayCommand]
        private void RemoveFailedDownloads()
        {
            var failedDownloads = Downloads.Where(c => c.Status == DownloadStatus.Failed).ToArray();
            foreach (var failedDownload in failedDownloads)
                Downloads.Remove(failedDownload);
        }

        [RelayCommand]
        private void RestartFailedDownloads()
        {
            var failedDownloads = Downloads.Where(c => c.Status == DownloadStatus.Failed).ToArray();
            foreach (var failedDownload in failedDownloads)
                failedDownload.RestartCommand.Execute(null);
        }

        [RelayCommand]
        private async Task CopyUrl(DownloadViewModel download)
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow?.Clipboard is not { } provider)
                return;

            await provider.SetTextAsync(download.RequestUrl.OriginalString);

        }

        [RelayCommand]
        private async Task CopyTitle(DownloadViewModel download)
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.Clipboard is not { } provider)
                return;

            await  provider.SetTextAsync(download.VideoName);

        }

        [RelayCommand]
        private async Task CopyLogs(DownloadViewModel download)
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
           desktop.MainWindow?.Clipboard is not { } provider)
                return;

            await provider.SetTextAsync(download.CopyLog());
        }
    }
}
