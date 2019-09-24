using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colipars.Internal
{
    public interface IParseResult
    {
        /// <summary>
        /// The parsed IVerb of the operation. Might be null if there was an error with the verb.
        /// </summary>
        IVerb? Verb { get; }
        IEnumerable<IError> Errors { get; }
        bool HelpRequested { get; }
    }
}
