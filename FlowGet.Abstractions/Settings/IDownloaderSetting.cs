using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowGet.Abstractions.M3uDownloaders
{
    public interface IDownloaderSetting
    {
        public int Timeouts { get; }
        public int RetryCount { get; }
        public bool SkipRequestError { get; }
        public int MaxThreadCount { get; }
        TimeSpan RecordDuration { get; }
        Dictionary<string, string> Headers { get; }
    }

}
