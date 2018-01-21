using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colipars.Internal
{
    public interface IParseResult
    {
        IVerb Verb { get; }
        IEnumerable<IError> Errors { get; }
        bool HelpRequested { get; }
    }
}
