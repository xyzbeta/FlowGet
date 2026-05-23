using FlowGet.Abstractions.Common;
using FlowGet.Abstractions.M3u8;
using FlowGet.Common.M3u8;

namespace FlowGet.Common.DownloadPrams
{
    public class M3u8DownloadParams : DownloadParamsBase, IM3u8DownloadParam
    {
        public Uri RequestUrl { get; } = default!;
        public IM3uKeyInfo? M3UKeyInfo { get; private set;}

        public M3u8DownloadParams(Uri url, string? videoname, string savePath, string selectFormat, IDictionary<string, string>? headers)
            : base(url, videoname, savePath, selectFormat, headers)
        {
            RequestUrl = url;
        }

        public M3u8DownloadParams(Uri url, string? videoname,  string savePath, string selectFormat, IDictionary<string, string>? headers, string method, string? key, string? iv)
            : base(url, videoname,  savePath, selectFormat, headers)
        {
            RequestUrl = url;
            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(method))
                M3UKeyInfo = M3uKeyInfoHelper.GetKeyInfoInstance(method, key!, iv!);
        }

        public void UpdateKeyInfo(string method, string? key, string? iv)
        {
            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(method))
                M3UKeyInfo = M3uKeyInfoHelper.GetKeyInfoInstance(method, key!, iv!);
        }
    }
}
