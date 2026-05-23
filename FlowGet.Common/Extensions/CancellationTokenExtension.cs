using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlowGet.Downloader.Utils.Extensions
{
    internal static class CancellationTokenExtension
    {
        public static CancellationTokenSource CancelTimeOut(this CancellationToken cancellationToken,int timeout)
        {
            CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cancellationTokenSource.CancelAfter(timeout);
            return cancellationTokenSource;
        }
    }
}
