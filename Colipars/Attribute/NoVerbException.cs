using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colipars.Attribute
{
    class NoVerbException : Exception
    {
        public NoVerbException(string message) : base(message)
        {
        }

        public NoVerbException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
