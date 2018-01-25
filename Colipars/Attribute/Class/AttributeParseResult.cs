using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Colipars.Internal;

namespace Colipars.Attribute.Class
{
    public class AttributeParseResult : IParseResult
    {
        private object _handledOption;

        public IVerb Verb { get; }
        public IEnumerable<IError> Errors { get; }
        public bool HelpRequested { get; } = false;

        /// <summary>
        /// The error handler assigned as default for handling errors, can be null.
        /// Use the ErrorHandlerFunc if you want the HandleErrors function of it.
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

        public AttributeParseResult(IVerb verb, IErrorHandler errorHandler, object handledOption)
        {
            Verb = verb ?? throw new ArgumentNullException(nameof(verb));
            Errors = new IError[0];
            ErrorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _handledOption = handledOption ?? throw new ArgumentNullException(nameof(handledOption));
        }

        /// <summary>
        /// Constructor in case help was requested.
        /// </summary>
        /// <param name="verb"></param>
        /// <param name="errorHandler"></param>
        protected AttributeParseResult(IVerb verb)
        {
            Verb = verb;
            Errors = new IError[0];
            HelpRequested = true;
        }

        public AttributeParseResult(IVerb verb, IErrorHandler errorHandler, IEnumerable<IError> errors)
        {
            Verb = verb;
            ErrorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            Errors = errors ?? new IError[0];
        }

        #region Map

        /// <summary>
        /// Maps the verb object to an exit code.
        /// </summary>
        /// <param name="verbHandler"></param>
        /// <returns></returns>
        public int Map(Func<IVerb, int> verbHandler)
        {
            return Map(verbHandler, ErrorHandlerFunc);
        }

        /// <summary>
        /// Maps the options object to an exit code.
        /// </summary>
        /// <typeparam name="TOption"></typeparam>
        /// <param name="optionHandler"></param>
        /// <returns></returns>
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

        public virtual int Map(Func<IVerb, int> verbHandler, Func<IEnumerable<IError>, int> errorsHandler)
        {
            if (verbHandler == null) throw new ArgumentNullException(nameof(verbHandler));
            if (errorsHandler == null) throw new ArgumentNullException(nameof(errorsHandler));

            return MapInternal(() =>
            {
                return verbHandler(Verb);
            }, errorsHandler);
        }

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
                return 0;

            if (Errors.Any())
                return errorsHandler(Errors);

            if (_handledOption == null)
                throw new InvalidOperationException("The parse result is no object, but neither help was request nor was there an error.");

            return handler();
        }

        #region TryMap

        /// <summary>
        /// Maps the options object to an exit code. If an exception happens in the <paramref name="optionHandler"/>, an error handler takes care of it and this function returns false.
        /// </summary>
        /// <typeparam name="TOption"></typeparam>
        /// <param name="optionHandler"></param>
        /// <param name="exitCode"></param>
        /// <returns>True if no exception was thrown, false otherwise.</returns>
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
        /// Maps the options object to an exit code. If an exception happens in the <paramref name="optionHandler"/>, the given <paramref name="errorsHandler"/> gets called and this function returns false.
        /// </summary>
        /// <typeparam name="TOption"></typeparam>
        /// <param name="optionHandler"></param>
        /// <param name="errorsHandler">Gets called if any error happens.</param>
        /// <param name="exitCode"></param>
        /// <returns>True if no exception was thrown, false otherwise.</returns>
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

        public static AttributeParseResult CreateHelpRequest(IVerb verb = null)
        {
            return new AttributeParseResult(verb);
        }
    }
}
