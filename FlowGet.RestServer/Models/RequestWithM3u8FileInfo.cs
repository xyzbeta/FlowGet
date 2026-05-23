using FlowGet.Abstractions.Common;
using FlowGet.Abstractions.M3u8;
using FlowGet.Common.DownloadPrams;
using FlowGet.Common.M3u8Infos;
using FlowGet.RestServer.Attributes;
using System.Text.Json.Serialization;

namespace FlowGet.RestServer.Models
{
    internal class RequestWithM3u8FileInfo :RequestBase
    {
        [JsonPropertyName("content")]
        [Required(ExceptionMsg = "m3UFileInfo解析失败")]
        public IM3uFileInfo M3UFileInfos { get; set; } = default!;

        public IDownloadParamBase ToDownloadParam()
        {
            if (!M3UFileInfos.MediaFiles.Any())
                throw new ArgumentException("m3u8的数据不能为空");

            return new DownloadParamsBase(M3UFileInfos.MediaFiles[0].Uri, VideoName, SavePath, "mp4", Headers);
        }
   
    }
}
