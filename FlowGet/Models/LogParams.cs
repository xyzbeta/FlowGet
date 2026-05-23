using System;

namespace FlowGet.Models
{
    public enum LogType
    {
        Info,
        Warning,
        Error
    }

    public readonly record struct LogParams(LogType Type, string Message)
    {
        public DateTime Time { get; } = DateTime.Now;
    }
}
