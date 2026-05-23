using FlowGet.Common.M3u8Infos;
using FlowGet.Abstractions.M3u8;
using System.Collections.Generic;
using System;
using FlowGet.Abstractions.M3uDownloaders;

namespace FlowGet.M3U8.AttributeReaders
{
    internal abstract class AttributeReader : IAttributeReader
    {
        public virtual bool ShouldTerminate => false;

        public abstract void Write(M3UFileInfo m3UFileInfo, string value, IEnumerator<string> reader, Uri baseUri);

        public void Write(IM3uFileInfo m3UFileInfo, string value, IEnumerator<string> reader, Uri baseUri)
        {
            Write((M3UFileInfo)m3UFileInfo, value, reader, baseUri);
        }
    }
}