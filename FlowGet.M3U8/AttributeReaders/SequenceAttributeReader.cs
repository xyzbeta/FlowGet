using FlowGet.Common.M3u8Infos;
using FlowGet.M3U8.AttributeReader.Attributes;
using System.Collections.Generic;
using System;
using FlowGet.M3U8.Utilities;

namespace FlowGet.M3U8.AttributeReaders
{
    [M3U8Reader("#EXT-X-MEDIA-SEQUENCE", typeof(SequenceAttributeReader))]
    internal class SequenceAttributeReader : AttributeReader
    {
        public override void Write(M3UFileInfo fileInfo, string value, IEnumerator<string> reader, Uri baseUri)
        {
            fileInfo.MediaSequence = To.Value<int>(value);
        }
    }
}