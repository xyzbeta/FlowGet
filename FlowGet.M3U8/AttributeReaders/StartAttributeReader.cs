using FlowGet.M3U8.AttributeReader.Attributes;

namespace FlowGet.M3U8.AttributeReaders
{
    [M3U8Reader("#EXT-X-START", typeof(StartAttributeReader))]
    internal class StartAttributeReader : DiscontinuityAttributeReader
    {
    }
}
