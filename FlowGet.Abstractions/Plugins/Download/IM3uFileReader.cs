using FlowGet.Abstractions.M3u8;

namespace FlowGet.Abstractions.M3uDownloaders
{
    public interface IM3uFileReader
    {
        void InitAttributeReade(IAttributeReaderCollection readers);

        IM3uFileInfo GetM3u8FileInfo(Stream stream);
    }
}
