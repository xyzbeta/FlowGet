using FlowGet.Abstractions.M3u8;

namespace FlowGet.Abstractions.Common
{
    public interface IM3u8DownloadParam : IDownloadParamBase
    {
        Uri RequestUrl { get; }
        IM3uKeyInfo? M3UKeyInfo { get; }

    }
}
