using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Colipars.Internal
{
    public abstract class Parser<TResult> where TResult : ParseResult
    {
        public Parser(Settings settings, Configuration configuration)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Settings Settings { get; }

        public Configuration Configuration { get; }

        public TResult Parse(IEnumerable<string> args)
        {
            var firstParam = args.FirstOrDefault();
            if (firstParam == null)
                return CreateErrorResult(null, new VerbIsMissingError());

            if (Configuration.HelpArguments.Contains(firstParam))
                return ShowHelp();

            IVerb selectedVerb = Settings.Verbs.FirstOrDefault((x) => x.Name == firstParam);
            if (selectedVerb == null)
            {
                if (Configuration.DefaultVerb != null)
                    selectedVerb = Configuration.DefaultVerb;
                else
                    return CreateErrorResult(null, new UnknownVerbError(firstParam));
            }
            else
            {
                args = args.Skip(1);
            }

            if (args.Any((x) => Configuration.HelpArguments.Contains(x)))
                return ShowHelp(selectedVerb);

            return ProcessArguments(selectedVerb, args);
        }
        
        public abstract TResult ShowHelp(IVerb verb);

        public virtual TResult ShowHelp()
        {
            return ShowHelp(null);
        }

        protected abstract TResult ProcessArguments(IVerb verb, IEnumerable<string> arguments);

        protected abstract TResult CreateErrorResult(IVerb verb, IEnumerable<IError> errors);

        protected TResult CreateErrorResult(IVerb verb, IError error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            return CreateErrorResult(verb, new[] { error });
        }
    }
}
