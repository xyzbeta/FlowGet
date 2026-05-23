using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowGet.Models
{
    public class DeleteDialogResult(bool result, bool isdeletecache)
    {
        public bool DialogResult { get; } = result;
        public bool IsDeleteCache { get; } = isdeletecache;
    }
}
