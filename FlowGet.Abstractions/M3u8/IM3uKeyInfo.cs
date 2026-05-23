using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowGet.Abstractions.M3u8
{
    public partial interface IM3uKeyInfo 
    {
        public string Method { get; }

        public Uri Uri { get; }

        public byte[] BKey { get; }

        public byte[] IV { get; }
    }

}
