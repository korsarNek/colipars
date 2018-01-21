using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colipars.Internal
{
    public interface IParser
    {
        IParseResult Parse(IEnumerable<string> args);
    }

    public interface IParser<TResult> : IParser where TResult : IParseResult
    {
        new TResult Parse(IEnumerable<string> args);
    }
}
