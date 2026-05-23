using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FlowGet.ViewModels;
using FlowGet.ViewModels.Dialogs;
using FlowGet.ViewModels.Downloads;
using FlowGet.ViewModels.Menus;
using FlowGet.ViewModels.Windows;
using FlowGet.Views.Dialogs;
using FlowGet.Views.Downloads;
using FlowGet.Views.Menus;
using FlowGet.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FlowGet.FrameWork
{
    public class ViewLocator(IServiceProvider serviceProvider) : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if (param is not ViewModelBase viewModel)
                return null;

            var view = CreateView(param);
            view?.DataContext ??= viewModel;

            return view;
        }

        private Control? CreateView(object? param)
        {
            return param switch
            {
                MainWindowViewModel => serviceProvider.GetRequiredService<MainWindowView>(),
                SettingsViewModel => serviceProvider.GetRequiredService<SettingsView>(),
                ExtensionViewModel => serviceProvider.GetRequiredService<ExtensionView>(),
                M3u8WindowViewModel => serviceProvider.GetRequiredService<M3u8WindowView>(),
                MediaWindowViewModel => serviceProvider.GetRequiredService<MediaWindowView>(),
                DownloadViewModel => new DownloadView(),
                DeleteDialogViewModel => new DeleteDialogView(),
                _ => null
            };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
