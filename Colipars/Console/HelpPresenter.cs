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
        public Configuration Configuration { get; }

        private readonly IParameterFormatter _parameterFormatter;

        public HelpPresenter(Configuration configuration, IParameterFormatter parameterFormatter)
        {
            Configuration = configuration;
            _parameterFormatter = parameterFormatter;
        }

        public void Present(IVerb verb)
        {
            WriteLine("usage: [verb] [parameters]");
            WriteLine($"{verb.Name} parameters:");

            foreach (var option in Configuration.GetOptions(verb))
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

            foreach (var verb in Configuration.Verbs)
            {
                WriteLine("\t" + verb.Name + "\t" + verb.Description);//TODO: output description
            }

            WriteLine();
            WriteLine("For detailed usage use [verb] " + _parameterFormatter.Format(Configuration.HelpArguments.First()));
        }
    }
}
