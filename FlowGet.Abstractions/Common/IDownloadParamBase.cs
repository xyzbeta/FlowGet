using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowGet.Abstractions.Common
{
    public interface IDownloadParamBase
    {
        string CachePath { get; }

        //没有扩展名的名称
        //例如index
        string VideoName { get; set; }

        //包含完整路径的名称且带有后缀
        //例如:e:/desktop/download/index.mp4
        string VideoFullName { get;  }
        string SavePath { get;  }
        IDictionary<string, string>? Headers { get; }
    }
}
