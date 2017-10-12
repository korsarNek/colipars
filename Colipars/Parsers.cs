using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars
{
    public static class Parsers
    {
        /// <summary>
        /// Extension point for different parser setups.
        /// </summary>
        public static SetupHelper Setup { get => SetupHelper._instance; }

        public class SetupHelper
        {
            internal static SetupHelper _instance = new SetupHelper();
        }
    }
}
