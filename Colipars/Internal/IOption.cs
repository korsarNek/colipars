using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Internal
{
    public interface IOption
    {
        string Name { get; }

        string Alias { get; }

        string Description { get; }

        bool Required { get; }
    }
}
