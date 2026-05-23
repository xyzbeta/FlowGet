using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FlowGet.Models
{
    public class MyLog(ObservableCollection<LogParams> Logs) : Abstractions.Common.ILog
    {
        private void AddLog(LogParams log)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (Logs.Count >= 500)
                {
                    for (int i = 0; i < 100; i++)
                        Logs.RemoveAt(0);
                }
                Logs.Add(log);
            });
        }

        public virtual void Info(string format, params object[] args)
        {
            AddLog(new LogParams(LogType.Info, string.Format(format, args)));
        }

        public virtual void Warn(string format, params object[] args)
        {
            AddLog(new LogParams(LogType.Warning, string.Format(format, args)));
        }

        public virtual void Error(Exception exception)
        {
            AddLog(new LogParams(LogType.Error, exception.ToString()));
        }
    }
}
