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

        public AttributeParseResult(IVerb verb, IErrorHandler errorHandler, object handledOption)
            : base(verb, errorHandler, null, helpRequested: false)
        {
            _handledOption = handledOption ?? throw new ArgumentNullException(nameof(handledOption));
        }

        /// <summary>
        /// Constructor in case help was requested.
        /// </summary>
        /// <param name="verb"></param>
        /// <param name="errorHandler"></param>
        public AttributeParseResult(IVerb verb)
            : base(verb, null, null, helpRequested: true)
        {
        }

        public AttributeParseResult(IVerb verb, IErrorHandler errorHandler, IEnumerable<IError> errors)
            : base(verb, errorHandler, errors, helpRequested: false)
        {

        }

        #region Map

        public int Map<TOption>(Func<TOption, int> optionHandler)
        {
            return Map(optionHandler, ErrorHandlerFunc);
        }

        public int Map<TOption1, TOption2>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler)
        {
            return Map(option1Handler, option2Handler, ErrorHandlerFunc);
        }

        public int Map<TOption1, TOption2, TOption3>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler)
        {
            return Map(option1Handler, option2Handler, option3Handler, ErrorHandlerFunc);
        }

        public int Map<TOption1, TOption2, TOption3, TOption4>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler, Func<TOption4, int> option4Handler)
        {
            return Map(option1Handler, option2Handler, option3Handler, option4Handler, ErrorHandlerFunc);
        }

        #endregion

        #region Map with error handler

        public int Map<TOption>(Func<TOption, int> optionHandler, Func<IEnumerable<IError>, int> errorsHandler)
        {
            if (optionHandler == null) throw new ArgumentNullException(nameof(optionHandler));
            if (errorsHandler == null) throw new ArgumentNullException(nameof(errorsHandler));

            return MapInternal(() =>
            {
                if (_handledOption is TOption) return optionHandler((TOption)_handledOption);

                throw new InvalidOperationException($"The handled option was of type \"{_handledOption.GetType()}\" and not of one the requested types.");
            }, errorsHandler);
        }

        public int Map<TOption1, TOption2>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<IEnumerable<IError>, int> errorsHandler)
        {
            if (option1Handler == null) throw new ArgumentNullException(nameof(option1Handler));
            if (option2Handler == null) throw new ArgumentNullException(nameof(option2Handler));
            if (errorsHandler == null) throw new ArgumentNullException(nameof(errorsHandler));

            return MapInternal(() =>
            {
                if (_handledOption is TOption1) return option1Handler((TOption1)_handledOption);
                if (_handledOption is TOption2) return option2Handler((TOption2)_handledOption);

                throw new InvalidOperationException($"The handled option was of type \"{_handledOption.GetType()}\" and not of one the requested types.");
            }, errorsHandler);
        }

        public int Map<TOption1, TOption2, TOption3>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler, Func<IEnumerable<IError>, int> errorsHandler)
        {
            if (option1Handler == null) throw new ArgumentNullException(nameof(option1Handler));
            if (option2Handler == null) throw new ArgumentNullException(nameof(option2Handler));
            if (option3Handler == null) throw new ArgumentNullException(nameof(option3Handler));
            if (errorsHandler == null) throw new ArgumentNullException(nameof(errorsHandler));

            return MapInternal(() =>
            {
                if (_handledOption is TOption1) return option1Handler((TOption1)_handledOption);
                if (_handledOption is TOption2) return option2Handler((TOption2)_handledOption);
                if (_handledOption is TOption3) return option3Handler((TOption3)_handledOption);

                throw new InvalidOperationException($"The handled option was of type \"{_handledOption.GetType()}\" and not of one the requested types.");
            }, errorsHandler);
        }

        public int Map<TOption1, TOption2, TOption3, TOption4>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler, Func<TOption4, int> option4Handler, Func<IEnumerable<IError>, int> errorsHandler)
        {
            if (option1Handler == null) throw new ArgumentNullException(nameof(option1Handler));
            if (option2Handler == null) throw new ArgumentNullException(nameof(option2Handler));
            if (option3Handler == null) throw new ArgumentNullException(nameof(option3Handler));
            if (option4Handler == null) throw new ArgumentNullException(nameof(option4Handler));
            if (errorsHandler == null) throw new ArgumentNullException(nameof(errorsHandler));

            return MapInternal(() =>
            {
                if (_handledOption is TOption1) return option1Handler((TOption1)_handledOption);
                if (_handledOption is TOption2) return option2Handler((TOption2)_handledOption);
                if (_handledOption is TOption3) return option3Handler((TOption3)_handledOption);
                if (_handledOption is TOption4) return option4Handler((TOption4)_handledOption);

                throw new InvalidOperationException($"The handled option was of type \"{_handledOption.GetType()}\" and not of one the requested types.");
            }, errorsHandler);
        }

        #endregion

        private int MapInternal(Func<int> handler, Func<IEnumerable<IError>, int> errorsHandler)
        {
            if (HelpRequested)
                return EXIT_CODE_HELP;

            if (Errors.Any())
                return errorsHandler(Errors);

            if (_handledOption == null)
                throw new InvalidOperationException("The parse result is no object, but neither help was request nor was there an error.");

            return handler();
        }

        #region TryMap

        public bool TryMap<TOption>(Func<TOption, int> optionHandler, out int exitCode)
        {
            return TryMap<TOption>(optionHandler, ErrorHandlerFunc, out exitCode);
        }

        public bool TryMap<TOption1, TOption2>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, out int exitCode)
        {
            return TryMap<TOption1, TOption2>(option1Handler, option2Handler, ErrorHandlerFunc, out exitCode);
        }

        public bool TryMap<TOption1, TOption2, TOption3>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler, out int exitCode)
        {
            return TryMap<TOption1, TOption2, TOption3>(option1Handler, option2Handler, option3Handler, ErrorHandlerFunc, out exitCode);
        }

        public bool TryMap<TOption1, TOption2, TOption3, TOption4>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler, Func<TOption4, int> option4Handler, out int exitCode)
        {
            return TryMap<TOption1, TOption2, TOption3, TOption4>(option1Handler, option2Handler, option3Handler, option4Handler, ErrorHandlerFunc, out exitCode);
        }

        #endregion

        #region TrypMap with error handler

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
            if (optionHandler == null) throw new ArgumentNullException(nameof(optionHandler));
            if (errorsHandler == null) throw new ArgumentNullException(nameof(errorsHandler));

            try
            {
                exitCode = Map(optionHandler, errorsHandler);
                return true;
            }
            catch (Exception exc)
            {
                exitCode = errorsHandler(new[] { new UnexpectedExceptionError(exc) });
                return false;
            }
        }

        public bool TryMap<TOption1, TOption2>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<IEnumerable<IError>, int> errorsHandler, out int exitCode)
        {
            if (option1Handler == null) throw new ArgumentNullException(nameof(option1Handler));
            if (option2Handler == null) throw new ArgumentNullException(nameof(option2Handler));
            if (errorsHandler == null) throw new ArgumentNullException(nameof(errorsHandler));

            try
            {
                exitCode = Map(option1Handler, option2Handler, errorsHandler);
                return true;
            }
            catch (Exception exc)
            {
                exitCode = errorsHandler(new[] { new UnexpectedExceptionError(exc) });
                return false;
            }
        }

        public bool TryMap<TOption1, TOption2, TOption3>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler, Func<IEnumerable<IError>, int> errorsHandler, out int exitCode)
        {
            if (option1Handler == null) throw new ArgumentNullException(nameof(option1Handler));
            if (option2Handler == null) throw new ArgumentNullException(nameof(option2Handler));
            if (option3Handler == null) throw new ArgumentNullException(nameof(option3Handler));
            if (errorsHandler == null) throw new ArgumentNullException(nameof(errorsHandler));

            try
            {
                exitCode = Map(option1Handler, option2Handler, option3Handler, errorsHandler);
                return true;
            }
            catch (Exception exc)
            {
                exitCode = errorsHandler(new[] { new UnexpectedExceptionError(exc) });
                return false;
            }
        }

        public bool TryMap<TOption1, TOption2, TOption3, TOption4>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler, Func<TOption4, int> option4Handler, Func<IEnumerable<IError>, int> errorsHandler, out int exitCode)
        {
            if (option1Handler == null) throw new ArgumentNullException(nameof(option1Handler));
            if (option2Handler == null) throw new ArgumentNullException(nameof(option2Handler));
            if (option3Handler == null) throw new ArgumentNullException(nameof(option3Handler));
            if (option4Handler == null) throw new ArgumentNullException(nameof(option4Handler));
            if (errorsHandler == null) throw new ArgumentNullException(nameof(errorsHandler));

            try
            {
                exitCode = Map(option1Handler, option2Handler, option3Handler, option4Handler, errorsHandler);
                return true;
            }
            catch (Exception exc)
            {
                exitCode = errorsHandler(new[] { new UnexpectedExceptionError(exc) });
                return false;
            }
        }

        #endregion

        /// <summary>
        /// The instance of the type that was successfully parsed.
        /// If the help was shown, a InvalidOperationException gets thrown.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public T GetCustomObject<T>()
        {
            return (T)GetCustomObject();
        }

        /// <summary>
        /// The instance of the type that was successfully parsed.
        /// If the help was shown, a InvalidOperationException gets thrown.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public object GetCustomObject()
        {
            if (HelpRequested)
                throw new InvalidOperationException("Can't request the verb object, user requested showing the help.");

            return _handledOption;
        }
    }
}
