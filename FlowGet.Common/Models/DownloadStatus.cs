using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowGet.Common.Models
{
    public enum DownloadStatus
    {
        Enqueued,
        Parsed,
        StartedLive,
        StartedVod,
        Completed,
        Failed,
        Canceled
    }
}
