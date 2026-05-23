using FlowGet.Abstractions.M3u8;

namespace FlowGet.Abstractions.M3uDownloaders
{
    public interface IAttributeReader
    {
        bool ShouldTerminate { get; }

        void Write(IM3uFileInfo m3UFileInfo, string value, IEnumerator<string> reader, Uri baseUri);
    }
}
