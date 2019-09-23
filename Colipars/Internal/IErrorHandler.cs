using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Internal
{
    public interface IErrorHandler
    {
        int HandleErrors(IEnumerable<IError> errors);
    }
}
