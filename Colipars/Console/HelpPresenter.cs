using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static System.Console;
using Colipars.Internal;

namespace Colipars.Console
{
    public class HelpPresenter : IHelpPresenter
    {
        public Settings Settings { get; }

        public Configuration Configuration { get; }

        private IParameterFormatter _parameterFormatter;

        public HelpPresenter(Settings settings, Configuration configuration, IParameterFormatter parameterFormatter)
        {
            Settings = settings;
            Configuration = configuration;
            _parameterFormatter = parameterFormatter;
        }

        public void Present(string verb)
        {
            WriteLine("usage: [verb] [parameters]");
            WriteLine($"{verb} parameters:");

            var options = Settings.GetOptions(verb);
            foreach (var option in options)
            {
                var line = _parameterFormatter.Format(option.Name);
                if (!string.IsNullOrWhiteSpace(option.Alias))
                    line += " (" + _parameterFormatter.Format(option.Alias) + ")";

                line += "\t" + option.Description;

                WriteLine("\t" + line);
            }
        }

        public void Present()
        {
            WriteLine("usage: [verb] [parameters]");
            WriteLine("verbs:");

            foreach (var verb in Settings.Verbs)
            {
                WriteLine("\t" + verb);
            }

            WriteLine();
            WriteLine("For detailed usage use [verb] " + _parameterFormatter.Format(Configuration.HelpArguments.First()));
        }
    }
}
