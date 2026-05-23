using DialogHostAvalonia;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FlowGet.FrameWork
{
    public class DialogManager
    {
        public DialogManager()
        {
            
        }

        public static async Task<T?> ShowDialogAsync<T>(DialogViewModelBase<T> dialogScreen)
        {
            void OnDialogOpened(object? openSender, DialogOpenedEventArgs openArgs)
            {
                void OnScreenClosed(object? openSender,EventArgs closeArgs)
                {
                    openArgs.Session.Close();
                    dialogScreen.Closed -= OnScreenClosed;
                }
                dialogScreen.Closed += OnScreenClosed;
            }

            await DialogHost.Show(dialogScreen, OnDialogOpened);

            return dialogScreen.DialogResult;
        }
    }
}
