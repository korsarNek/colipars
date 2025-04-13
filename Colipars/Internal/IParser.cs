using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colipars.Internal
{
    public interface IParser
    {
        IParseResult ShowHelp(IVerb verb);
        IParseResult ShowHelp();
        IParseResult Parse(IEnumerable<string> args);
    }

    public interface IParser<TResult> : IParser where TResult : IParseResult
    {
        new TResult ShowHelp(IVerb verb);
        new TResult ShowHelp();
        new TResult Parse(IEnumerable<string> args);
    }
}
