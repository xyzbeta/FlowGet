using FlowGet.Common.M3u8Infos;
using FlowGet.M3U8.AttributeReader.Attributes;
using System.Collections.Generic;
using System;

namespace FlowGet.M3U8.AttributeReaders
{
    [M3U8Reader("#EXT-X-PLAYLIST-TYPE", typeof(PlaylistTypeAttributeReader))]
    internal class PlaylistTypeAttributeReader : AttributeReader
    {
        public override void Write(M3UFileInfo fileInfo, string value, IEnumerator<string> reader, Uri baseUri)
        {
            fileInfo.PlaylistType = value;
        }
    }
}