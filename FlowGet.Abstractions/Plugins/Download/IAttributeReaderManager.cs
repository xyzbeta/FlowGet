using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace FlowGet.Abstractions.M3uDownloaders
{
    public interface IAttributeReaderCollection:
        IDictionary<string, IAttributeReader>,
        ICollection<KeyValuePair<string, IAttributeReader>>,
        IEnumerable<KeyValuePair<string, IAttributeReader>>,
        IEnumerable
    {

    }
}
