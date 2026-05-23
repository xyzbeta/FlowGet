using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowGet.Exceptions
{
    public class FileExistsException(string? message) : Exception(message)
    {
    }
}
