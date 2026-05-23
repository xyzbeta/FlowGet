using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FlowGet.FrameWork;
using FlowGet.Services;
using FlowGet.ViewModels;
using FlowGet.ViewModels.Menus;
using FlowGet.ViewModels.Windows;
using FlowGet.Views;
using FlowGet.Views.Menus;
using FlowGet.Views.Windows;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace FlowGet
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider = default!;

        public App()
        {
            var services = new ServiceCollection();

            services.AddSingleton<SettingsService>();
            services.AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));

            services.AddSingleton<DashboardWindowViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<ExtensionViewModel>();

            services.AddSingleton<MainWindowView>();
            services.AddSingleton<SettingsView>();
            services.AddSingleton<ExtensionView>();
            services.AddSingleton<M3u8WindowView>();
            services.AddSingleton<MediaWindowView>();

            _serviceProvider = services.BuildServiceProvider();

        }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var settings = _serviceProvider.GetRequiredService<SettingsService>();
            settings.Load();
            ApplyTheme(settings.IsDarkMode);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new DashboardWindow
                {
                    DataContext = _serviceProvider.GetRequiredService<DashboardWindowViewModel>()
                };
            }

            DataTemplates.Add(new ViewLocator(_serviceProvider));

            base.OnFrameworkInitializationCompleted();
        }

        public static void ApplyTheme(bool isDark)
        {
            if (Current is not null)
                Current.RequestedThemeVariant = isDark ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }
}
