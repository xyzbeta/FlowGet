using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowGet.Combiners.Utils
{
    internal static class DirEx
    {
        public static void CreateDirecotry(string savepath)
        {
            DirectoryInfo directoryInfo = new(savepath);
            if (directoryInfo.Exists)
                return;

            directoryInfo.Create();
        }
    }
}
