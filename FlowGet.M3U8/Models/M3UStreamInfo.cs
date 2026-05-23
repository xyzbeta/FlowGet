using System;
using FlowGet.Abstractions.M3u8;

namespace FlowGet.Common.M3u8Infos
{
    public class M3UStreamInfo : IM3uStreamInfo
    {
        public int? ProgramId { get; set; }

        public int? Bandwidth { get; set; }

        public string Codecs { get; set; } = default!;

        public string Resolution { get; set; } = default!;

        public Uri Uri { get; set; } = default!;
    }
}