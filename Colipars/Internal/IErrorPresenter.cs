using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Internal
{
    public interface IErrorPresenter
    {
        void Present(IEnumerable<IError> errors);
    }
}
