using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Colipars.Internal;

namespace Colipars
{
    public class ParseResult
    {
        public IVerb Verb { get; }
        public IEnumerable<IError> Errors { get; }
        public bool HelpRequested { get; } = false;

        /// <summary>
        /// The error handler assigned as default for handling errors, can be null.
        /// </summary>
        protected IErrorHandler ErrorHandler { get; } = null;

        /// <summary>
        /// An error handler function, uses the ErrorHandler if one exists, otherwise it returns a dummy function.
        /// </summary>
        protected Func<IEnumerable<IError>, int> ErrorHandlerFunc
        {
            get
            {
                if (ErrorHandler != null)
                    return ErrorHandler.HandleErrors;

                return (_) => 1;
            }
        }

        protected const int EXIT_CODE_HELP = 0;

        public ParseResult(IVerb verb, IErrorHandler errorHandler, IEnumerable<IError> errors, bool helpRequested)
        {
            Verb = verb;
            Errors = errors ?? new IError[0];
            ErrorHandler = errorHandler;
            HelpRequested = helpRequested;
        }

        public virtual int Map(Func<IVerb, int> verbHandler, Func<IEnumerable<IError>, int> errorsHandler)
        {
            if (verbHandler == null) throw new ArgumentNullException(nameof(verbHandler));
            if (errorsHandler == null) throw new ArgumentNullException(nameof(errorsHandler));

            if (HelpRequested)
                return EXIT_CODE_HELP;

            if (Errors.Any())
                return errorsHandler(Errors);

            return verbHandler(Verb);
        }

        public int Map(Func<IVerb, int> verbHandler)
        {
            return Map(verbHandler, ErrorHandlerFunc);
        }
    }
}
