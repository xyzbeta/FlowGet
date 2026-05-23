using FlowGet.Abstractions.M3u8;

namespace FlowGet.Abstractions.Common
{
    public interface IM3u8FileInfoDownloadParam : IDownloadParamBase
    {
        IM3uFileInfo M3UFileInfos { get; }
    }
}
