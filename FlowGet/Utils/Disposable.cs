using System;
using System.Collections.Generic;
using System.Text;

namespace FlowGet.Utils
{
    internal class Disposable(Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }
}
