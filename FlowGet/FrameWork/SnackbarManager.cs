using Avalonia.Threading;
using FlowGet.Abstractions.Models;
using Material.Styles.Controls;
using Material.Styles.Models;
using System;

namespace FlowGet.FrameWork;

public class SnackbarManager(string hostname,TimeSpan duration) : INotificationService
{
    public void Info(string message)
    {
        SnackbarHost.Post(
            new SnackbarModel(message, duration),
            hostname,
            DispatcherPriority.Normal
            );
    }
}
