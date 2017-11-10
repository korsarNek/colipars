using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Colipars
{
    public class Configuration
    {
        /// <summary>
        /// Possible arguments to any verb that trigger the help.
        /// </summary>
        public IEnumerable<string> HelpArguments { get; set; } = new[] { "h", "help" };

        public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;

        public IVerb DefaultVerb { get; set; }

        public int HelpExitCode { get; set; } = 0;

        public IServiceProvider Services { get; }

        public Configuration(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
    }
}
