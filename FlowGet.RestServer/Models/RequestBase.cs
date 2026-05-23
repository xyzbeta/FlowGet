using System.Text.Json.Serialization;
using FlowGet.RestServer.Attributes;

namespace FlowGet.RestServer.Models
{
    internal class RequestBase : IValidate
    {
        [JsonPropertyName("name")]
        public string VideoName { get; set; } = default!;

        public string SavePath { get; set; } = string.Empty;

        public IDictionary<string,string>? Headers { get; set; }
    }
}
