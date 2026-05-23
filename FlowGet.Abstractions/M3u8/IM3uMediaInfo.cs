using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FlowGet.Abstractions.M3u8
{
    public interface IM3uMediaInfo
    {
        public float Duration { get;  }

        public string Title { get;  }

        public Uri Uri { get;  } 

        public RangeHeaderValue? RangeValue { get; }
    }
}
