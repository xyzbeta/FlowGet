using FlowGet.RestServer.Attributes;
using System.Text.Json.Serialization;

namespace FlowGet.RestServer.Models
{
    internal class RequestWithContent : RequestBase
    {
        [Required(ExceptionMsg = "content不能为空")]
        public string Content { get; set; } = default!;

        [JsonPropertyName("baseurl")]
        public Uri? Url { get; set; }
    }
}
