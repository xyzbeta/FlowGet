using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowGet.Abstractions.M3u8
{
    public interface IM3uStreamInfo
    {
        public int? ProgramId { get;  }

        public int? Bandwidth { get; }

        public string Codecs { get;  }

        public string Resolution { get;  } 

        public Uri Uri { get;  } 
    }
}
