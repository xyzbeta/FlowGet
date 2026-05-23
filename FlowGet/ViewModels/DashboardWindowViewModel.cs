using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowGet.Services;
using FlowGet.ViewModels.Menus;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FlowGet.ViewModels
{
    internal partial class DashboardWindowViewModel : ViewModelBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly MainWindowViewModel mainWindowViewModel;
        private readonly SettingsService settingsService;
        public string Version { get; } = Program.VersionString;

        [ObservableProperty]
        public partial ViewModelBase CurrentViewModel { get; set; } = default!;

        [ObservableProperty]
        public partial bool IsDarkMode { get; set; }

        public DashboardWindowViewModel(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            settingsService = serviceProvider.GetRequiredService<SettingsService>();
            mainWindowViewModel = serviceProvider.GetRequiredService<MainWindowViewModel>();
            CurrentViewModel = mainWindowViewModel;
            IsDarkMode = settingsService.IsDarkMode;
        }

        [RelayCommand]
        private void ToggleDarkMode()
        {
            IsDarkMode = !IsDarkMode;
            settingsService.IsDarkMode = IsDarkMode;
            settingsService.Save();
            App.ApplyTheme(IsDarkMode);
        }

        [RelayCommand]
        private async Task InitializeAsync()
        {
            Debug.WriteLine("InitializeAsync");
            await mainWindowViewModel.InitializeAsync();
        }

        [RelayCommand]
        private void Closed()
        {
            Debug.WriteLine("ClosedAsync");
        }

        [RelayCommand]
        private static void OpenProjectUrl()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/Harlan-H/M3u8Downloader_H",
                UseShellExecute = true
            });
        }

        [RelayCommand]
        private void NavigateTo(Type type)
        {
            if (CurrentViewModel.GetType() == type)
                return;

            CurrentViewModel = (ViewModelBase)serviceProvider.GetRequiredService(type);
        }
    }
}

