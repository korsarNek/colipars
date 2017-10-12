using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Colipars.Internal;

namespace Colipars.Attribute
{
    public class AttributeParseResult : ParseResult
    {
        private object _handledOption;

        public AttributeParseResult(string verb, object handledOption)
            : base(verb, null, null, helpRequested: false)
        {
            _handledOption = handledOption ?? throw new ArgumentNullException(nameof(handledOption));
        }

        /// <summary>
        /// Constructor in case help was requested.
        /// </summary>
        /// <param name="verb"></param>
        public AttributeParseResult(string verb)
            : base(verb, null, null, helpRequested: true)
        {
        }

        public AttributeParseResult(string verb, IErrorHandler errorHandler, IEnumerable<IError> errors)
            : base(verb, errorHandler, errors, helpRequested: false)
        {

        }

        public int Map<TOption>(Func<TOption, int> optionHandler)
        {
            return Map<TOption>(optionHandler, ErrorHandler.HandleErrors);
        }

        public int Map<TOption>(Func<TOption, int> optionHandler, Func<IEnumerable<IError>, int> errorsHandler)
        {
            if (optionHandler == null) throw new ArgumentNullException(nameof(optionHandler));
            if (errorsHandler == null) throw new ArgumentNullException(nameof(errorsHandler));

            if (HelpRequested)
                return EXIT_CODE_HELP;

            if (_handledOption is TOption)
                return optionHandler((TOption)_handledOption);

            throw new InvalidOperationException($"The handled option was not of type \"{typeof(TOption)}\"");
        }

        public bool TryMap<TOption>(Func<TOption, int> optionHandler, out int exitCode)
        {
            return TryMap<TOption>(optionHandler, ErrorHandler.HandleErrors, out exitCode);
        }

        /// <summary>
        /// Returns false if an exception was thrown while calling mapping the result, otherwise true.
        /// </summary>
        /// <typeparam name="TOption"></typeparam>
        /// <param name="optionHandler"></param>
        /// <param name="exceptionExitCode"></param>
        /// <param name="exitCode"></param>
        /// <returns></returns>
        public bool TryMap<TOption>(Func<TOption, int> optionHandler, Func<IEnumerable<IError>, int> errorsHandler, out int exitCode)
        {
            try
            {
                exitCode = Map<TOption>(optionHandler, errorsHandler);
                return true;
            }
            catch (Exception exc)
            {
                exitCode = errorsHandler(new[] { new UnexpectedExceptionError(exc) });
                return false;
            }
        }

        /// <summary>
        /// The instance of the type that was successfully parsed.
        /// If the help was shown, a InvalidOperationException gets thrown.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public T GetVerbObject<T>()
        {
            return (T)GetVerbObject();
        }

        /// <summary>
        /// The instance of the type that was successfully parsed.
        /// If the help was shown, a InvalidOperationException gets thrown.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public object GetVerbObject()
        {
            if (HelpRequested)
                throw new InvalidOperationException("Can't request the verb object, user requested showing the help.");

            return _handledOption;
        }
    }
}
