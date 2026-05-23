using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowGet.Abstractions.Common
{
    public interface IMediaDownloadParam : IDownloadParamBase
    {
        bool IsVideoStream { get; }
        IList<IStreamInfo> Medias { get; }
    }
}
