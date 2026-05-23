using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowGet.RestServer;
using FlowGet.Services;
using System;
using System.Diagnostics;
using System.IO;

namespace FlowGet.ViewModels.Menus
{
    public partial class ExtensionViewModel : ViewModelBase
    {
        private readonly HttpListenService httpListenService = HttpListenService.Instance;

        [ObservableProperty]
        public partial bool IsExtensionConnected { get; set; }

        [ObservableProperty]
        public partial int? HttpServicePort { get; set; }

        public ExtensionViewModel()
        {
            httpListenService.RequestReceived += OnRequestReceived;
            if (httpListenService.LastRequestTime is not null)
                IsExtensionConnected = true;
        }

        private void OnRequestReceived(object? sender, EventArgs e)
        {
            HttpServicePort = httpListenService.Port;
            IsExtensionConnected = true;
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task DownloadExtension()
        {
            try
            {
                if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                    desktop.MainWindow?.StorageProvider is not { } storage)
                    return;

                var folder = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "选择保存目录"
                });

                if (folder.Count == 0) return;

                var targetPath = Path.Combine(folder[0].Path.LocalPath, "flowget-extension.zip");
                ExtensionService.SaveToFile(targetPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Extension download failed: {ex}");
            }
        }
    }
}
