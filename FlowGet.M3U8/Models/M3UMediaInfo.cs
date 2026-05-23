using System;
using System.Net.Http.Headers;
using FlowGet.Abstractions.M3u8;

namespace FlowGet.Common.M3u8Infos
{
    public partial class M3UMediaInfo : IM3uMediaInfo
    {
        public float Duration { get; set; }

        public string Title { get; set; } = default!;

        public Uri Uri { get; set; } = default!;

        public RangeHeaderValue? RangeValue { get; set; }

    }

    public partial class M3UMediaInfo : IEquatable<M3UMediaInfo>
    {
        public bool Equals(M3UMediaInfo? other) => StringComparer.Ordinal.Equals(Uri, other?.Uri);
        public override bool Equals(object? obj) => obj is M3UMediaInfo other && Equals(other);
        public override int GetHashCode() => Uri.GetHashCode();

        public static bool operator ==(M3UMediaInfo m3UMediaInfo, M3UMediaInfo m3UMediaInfo1) =>  m3UMediaInfo.Equals(m3UMediaInfo1);
        public static bool operator !=(M3UMediaInfo m3UMediaInfo, M3UMediaInfo m3UMediaInfo1) => !(m3UMediaInfo == m3UMediaInfo1);

    }
}