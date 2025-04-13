using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colipars.Internal
{
    /// <summary>
    /// Represents a case that hasn't been clearly covered through other language features and that should never occur according to the intent of the code.
    /// </summary>
    [ExcludeFromCodeCoverage]
    sealed class LogicException : Exception
    {
        public LogicException(string message):
            base (message)
        {

        }
    }
}
