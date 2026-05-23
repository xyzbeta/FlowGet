using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowGet.Abstractions.Common;
using FlowGet.Abstractions.M3u8;
using FlowGet.Common.DownloadPrams;
using FlowGet.Extensions;
using FlowGet.FrameWork;
using FlowGet.Models;
using FlowGet.Services;
using FlowGet.Utils;
using FlowGet.ViewModels.Downloads;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FlowGet.ViewModels.Windows
{
    public partial class M3u8WindowViewModel(SettingsService settingsService, ViewModelManager viewModelManager, SnackbarManager Notification) : ViewModelBase
    {
        public M3u8DownloadInfo VideoDownloadInfo { get; } = new M3u8DownloadInfo();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ProcessM3u8DownloadCommand))]
        public partial bool IsBusy { get; private set; }

        public Action<DownloadViewModel> EnqueueDownloadAction { get; set; } = default!;

        public bool CanProcessM3u8Download => !IsBusy;

        [RelayCommand(CanExecute = nameof(CanProcessM3u8Download))]
        private void ProcessM3u8Download(M3u8DownloadInfo obj)
        {
            IsBusy = true;
            try
            {
                Uri uri = obj.GetRequestUri();
                M3u8DownloadParams m3U8DownloadParams = new(uri, obj.VideoName, settingsService.SavePath, settingsService.SelectedFormat, settingsService.Headers, obj.Method, obj.Key, obj.Iv);
                ProcessM3u8Download(null, m3U8DownloadParams);

                obj.Reset(settingsService.IsResetAddress, settingsService.IsResetName);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Notification.Info(e.ToString());
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ImportFromFile()
        {
            try
            {
                if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                    desktop.MainWindow?.StorageProvider is not { } storage)
                    return;

                var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "选择包含 URL 的文本文件",
                    AllowMultiple = false,
                    FileTypeFilter = [new FilePickerFileType("文本文件") { Patterns = ["*.txt", "*.csv", "*.m3u8", "*.json"] }]
                });

                if (files.Count == 0) return;

                var filePath = files[0].Path.LocalPath;
                var text = await File.ReadAllTextAsync(filePath);
                var urls = ParseUrls(text);

                if (urls.Count == 0)
                {
                    Notification.Info("文件中未找到有效的 URL");
                    return;
                }

                await EnqueueBatch(urls);
            }
            catch (Exception e)
            {
                Notification.Info($"文件导入失败: {e.Message}");
            }
        }

        private List<Uri> ParseUrls(string text)
        {
            var urls = new List<Uri>();
            var lines = text.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == "http" || uri.Scheme == "https"))
                {
                    urls.Add(uri);
                }
            }
            return urls;
        }

        private async Task EnqueueBatch(List<Uri> urls)
        {
            var count = 0;
            foreach (var uri in urls)
            {
                var param = new M3u8DownloadParams(uri, "", settingsService.SavePath, settingsService.SelectedFormat, settingsService.Headers, "", null, null);
                await Task.Run(() => ProcessM3u8Download(null, param));
                count++;
            }
            Notification.Info($"已添加 {count} 个下载任务");
        }

        //处理软件界面来的请求
        public void ProcessM3u8Download(HttpClient? httpClient, IM3u8DownloadParam m3U8DownloadParam)
        {
            FileEx.EnsureFileNotExist(m3U8DownloadParam.VideoFullName);

            DownloadViewModel download = viewModelManager.CreateDownloadViewModel(httpClient, m3U8DownloadParam);
            if (download is null) return;

            Dispatcher.UIThread.Post(() => EnqueueDownloadAction(download));
        }

        //处理接口过来的请求
        public void ProcessM3u8Download(HttpClient? httpClient, IDownloadParamBase m3U8DownloadParam, IM3uFileInfo m3UFileInfo)
        {
            FileEx.EnsureFileNotExist(m3U8DownloadParam.VideoFullName);

            DownloadViewModel download = viewModelManager.CreateDownloadViewModel(httpClient, m3UFileInfo, m3U8DownloadParam);
            if (download is null) return;

            Dispatcher.UIThread.Post(() => EnqueueDownloadAction(download));
        }

    }
}
