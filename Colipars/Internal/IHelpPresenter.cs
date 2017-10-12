using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Internal
{
    public interface IHelpPresenter
    {
        void Present(string verb);

        void Present();
    }
}
