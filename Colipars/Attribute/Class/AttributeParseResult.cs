using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Colipars.Internal;

namespace Colipars.Attribute.Class
{
    public sealed class AttributeParseResult : IParseResult
    {
        private readonly object? _customObject = null;
        private readonly ErrorHandler _defaultErrorHandler;

        public IVerb? Verb { get; }
        public IEnumerable<IError> Errors { get; }
        public bool HelpRequested { get; private set; } = false;

        private AttributeParseResult(IVerb verb, ErrorHandler defaultErrorHandler, object customObject)
        {
            Verb = verb ?? throw new ArgumentNullException(nameof(verb));
            Errors = new IError[0];
            _defaultErrorHandler = defaultErrorHandler ?? throw new ArgumentNullException(nameof(defaultErrorHandler));
            _customObject = customObject ?? throw new ArgumentNullException(nameof(customObject));
        }

        private AttributeParseResult(IVerb? verb, ErrorHandler defaultErrorHandler, IEnumerable<IError> errors)
        {
            Verb = verb;
            _defaultErrorHandler = defaultErrorHandler ?? throw new ArgumentNullException(nameof(defaultErrorHandler));
            Errors = errors;
        }

        #region Map

        /// <summary>
        /// Maps the options object to an exit code.
        /// </summary>
        /// <typeparam name="TOption"></typeparam>
        /// <param name="optionHandler"></param>
        /// <returns></returns>
        public int Map<TOption>(Func<TOption, int> optionHandler)
        {
            return Map(optionHandler, _defaultErrorHandler);
        }

        public int Map<TOption1, TOption2>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler)
        {
            return Map(option1Handler, option2Handler, _defaultErrorHandler);
        }

        public int Map<TOption1, TOption2, TOption3>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler)
        {
            return Map(option1Handler, option2Handler, option3Handler, _defaultErrorHandler);
        }

        public int Map<TOption1, TOption2, TOption3, TOption4>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler, Func<TOption4, int> option4Handler)
        {
            return Map(option1Handler, option2Handler, option3Handler, option4Handler, _defaultErrorHandler);
        }

        #endregion

        #region Map with error handler

        public int Map<TOption>(Func<TOption, int> optionHandler, ErrorHandler errorHandler)
        {
            if (optionHandler == null) throw new ArgumentNullException(nameof(optionHandler));
            if (errorHandler == null) throw new ArgumentNullException(nameof(errorHandler));

            return MapInternal(() =>
            {
                if (_customObject is TOption) return optionHandler((TOption)_customObject);

                throw new InvalidOperationException($"The handled option was of type \"{GetCustomObject().GetType()}\" and not of one the requested types.");
            }, errorHandler);
        }

        public int Map<TOption1, TOption2>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, ErrorHandler errorHandler)
        {
            if (option1Handler == null) throw new ArgumentNullException(nameof(option1Handler));
            if (option2Handler == null) throw new ArgumentNullException(nameof(option2Handler));
            if (errorHandler == null) throw new ArgumentNullException(nameof(errorHandler));

            return MapInternal(() =>
            {
                if (_customObject is TOption1) return option1Handler((TOption1)_customObject);
                if (_customObject is TOption2) return option2Handler((TOption2)_customObject);

                throw new InvalidOperationException($"The handled option was of type \"{GetCustomObject().GetType()}\" and not of one the requested types.");
            }, errorHandler);
        }

        public int Map<TOption1, TOption2, TOption3>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler, ErrorHandler errorHandler)
        {
            if (option1Handler == null) throw new ArgumentNullException(nameof(option1Handler));
            if (option2Handler == null) throw new ArgumentNullException(nameof(option2Handler));
            if (option3Handler == null) throw new ArgumentNullException(nameof(option3Handler));
            if (errorHandler == null) throw new ArgumentNullException(nameof(errorHandler));

            return MapInternal(() =>
            {
                if (_customObject is TOption1) return option1Handler((TOption1)_customObject);
                if (_customObject is TOption2) return option2Handler((TOption2)_customObject);
                if (_customObject is TOption3) return option3Handler((TOption3)_customObject);

                throw new InvalidOperationException($"The handled option was of type \"{GetCustomObject().GetType()}\" and not of one the requested types.");
            }, errorHandler);
        }

        public int Map<TOption1, TOption2, TOption3, TOption4>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler, Func<TOption4, int> option4Handler, ErrorHandler errorHandler)
        {
            if (option1Handler == null) throw new ArgumentNullException(nameof(option1Handler));
            if (option2Handler == null) throw new ArgumentNullException(nameof(option2Handler));
            if (option3Handler == null) throw new ArgumentNullException(nameof(option3Handler));
            if (option4Handler == null) throw new ArgumentNullException(nameof(option4Handler));
            if (errorHandler == null) throw new ArgumentNullException(nameof(errorHandler));

            return MapInternal(() =>
            {
                if (_customObject is TOption1) return option1Handler((TOption1)_customObject);
                if (_customObject is TOption2) return option2Handler((TOption2)_customObject);
                if (_customObject is TOption3) return option3Handler((TOption3)_customObject);
                if (_customObject is TOption4) return option4Handler((TOption4)_customObject);

                throw new InvalidOperationException($"The handled option was of type \"{GetCustomObject().GetType()}\" and not of one the requested types.");
            }, errorHandler);
        }

        #endregion

        private int MapInternal(Func<int> handler, ErrorHandler errorHandler)
        {
            if (HelpRequested)
                return 0;

            if (Errors.Any())
                return errorHandler(Errors);

            if (_customObject == null)
                throw new InvalidOperationException("The parse result is no object, but neither help was requested nor was there an error.");

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
            return TryMap<TOption>(optionHandler, _defaultErrorHandler, out exitCode);
        }

        public bool TryMap<TOption1, TOption2>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, out int exitCode)
        {
            return TryMap<TOption1, TOption2>(option1Handler, option2Handler, _defaultErrorHandler, out exitCode);
        }

        public bool TryMap<TOption1, TOption2, TOption3>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler, out int exitCode)
        {
            return TryMap<TOption1, TOption2, TOption3>(option1Handler, option2Handler, option3Handler, _defaultErrorHandler, out exitCode);
        }

        public bool TryMap<TOption1, TOption2, TOption3, TOption4>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler, Func<TOption4, int> option4Handler, out int exitCode)
        {
            return TryMap<TOption1, TOption2, TOption3, TOption4>(option1Handler, option2Handler, option3Handler, option4Handler, _defaultErrorHandler, out exitCode);
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
        public bool TryMap<TOption>(Func<TOption, int> optionHandler, ErrorHandler errorHandler, out int exitCode)
        {
            if (optionHandler == null) throw new ArgumentNullException(nameof(optionHandler));
            if (errorHandler == null) throw new ArgumentNullException(nameof(errorHandler));

            try
            {
                exitCode = Map(optionHandler, errorHandler);
                return true;
            }
            catch (Exception exc)
            {
                exitCode = errorHandler(new[] { new UnexpectedExceptionError(exc) });
                return false;
            }
        }

        public bool TryMap<TOption1, TOption2>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, ErrorHandler errorHandler, out int exitCode)
        {
            if (option1Handler == null) throw new ArgumentNullException(nameof(option1Handler));
            if (option2Handler == null) throw new ArgumentNullException(nameof(option2Handler));
            if (errorHandler == null) throw new ArgumentNullException(nameof(errorHandler));

            try
            {
                exitCode = Map(option1Handler, option2Handler, errorHandler);
                return true;
            }
            catch (Exception exc)
            {
                exitCode = errorHandler(new[] { new UnexpectedExceptionError(exc) });
                return false;
            }
        }

        public bool TryMap<TOption1, TOption2, TOption3>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler, ErrorHandler errorHandler, out int exitCode)
        {
            if (option1Handler == null) throw new ArgumentNullException(nameof(option1Handler));
            if (option2Handler == null) throw new ArgumentNullException(nameof(option2Handler));
            if (option3Handler == null) throw new ArgumentNullException(nameof(option3Handler));
            if (errorHandler == null) throw new ArgumentNullException(nameof(errorHandler));

            try
            {
                exitCode = Map(option1Handler, option2Handler, option3Handler, errorHandler);
                return true;
            }
            catch (Exception exc)
            {
                exitCode = errorHandler(new[] { new UnexpectedExceptionError(exc) });
                return false;
            }
        }

        public bool TryMap<TOption1, TOption2, TOption3, TOption4>(Func<TOption1, int> option1Handler, Func<TOption2, int> option2Handler, Func<TOption3, int> option3Handler, Func<TOption4, int> option4Handler, ErrorHandler errorHandler, out int exitCode)
        {
            if (option1Handler == null) throw new ArgumentNullException(nameof(option1Handler));
            if (option2Handler == null) throw new ArgumentNullException(nameof(option2Handler));
            if (option3Handler == null) throw new ArgumentNullException(nameof(option3Handler));
            if (option4Handler == null) throw new ArgumentNullException(nameof(option4Handler));
            if (errorHandler == null) throw new ArgumentNullException(nameof(errorHandler));

            try
            {
                exitCode = Map(option1Handler, option2Handler, option3Handler, option4Handler, errorHandler);
                return true;
            }
            catch (Exception exc)
            {
                exitCode = errorHandler(new[] { new UnexpectedExceptionError(exc) });
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
        /// If the help was requested or the parsing was unsuccessful because of an error, an <see cref="InvalidOperationException"/> gets thrown.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public object GetCustomObject()
        {
            if (HelpRequested)
                throw new InvalidOperationException("Can't request the verb object, user requested showing the help.");

            if (_customObject == null)
                if (Errors.Any())
                    throw new InvalidOperationException($"There are errors which prevented a successful parse; {string.Join(",", Errors.Select(e => e.Message))}");
                else
                    throw new LogicException("The parse didn't create errrors but no target object is available.");

            return _customObject;
        }

        public static AttributeParseResult CreateHelpRequest(IVerb? verb, ErrorHandler errorHandler)
        {
            return new AttributeParseResult(verb, errorHandler, new IError[0]) { HelpRequested = true };
        }

        public static AttributeParseResult CreateErrorResult(IVerb? verb, ErrorHandler errorHandler, IEnumerable<IError> errors)
        {
            return new AttributeParseResult(verb, errorHandler, errors);
        }

        public static AttributeParseResult CreateSuccessResult(IVerb verb, ErrorHandler errorHandler, object customObject)
        {
            return new AttributeParseResult(verb, errorHandler, customObject);
        }
    }
}
