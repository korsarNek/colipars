using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Colipars.Internal;

namespace Colipars.Attribute.Method
{
    public sealed class AttributeParseResult : IParseResult
    {
        public IVerb? Verb { get; }

        public IEnumerable<IError> Errors { get; }

        public bool HelpRequested { get; } = false;

        private readonly ErrorHandler _defaultErrorHandler;
        private readonly MethodInfo? _method;
        private readonly object? _instance;
        private readonly object?[] _parameters;

        /// <summary>
        /// Constructor for the case that help was requested.
        /// </summary>
        /// <param name="verb"></param>
        private AttributeParseResult(IVerb? verb, ErrorHandler defaultErrorHandler)
        {
            Verb = verb;
            _defaultErrorHandler = defaultErrorHandler ?? throw new ArgumentNullException(nameof(defaultErrorHandler));
            HelpRequested = true;
            Errors = [];
            _parameters = [];
        }

        private AttributeParseResult(IVerb? verb, ErrorHandler defaultErrorHandler, IEnumerable<IError> errors)
        {
            //Verb not necessarily given in case of an error with the verb.
            Verb = verb;
            _defaultErrorHandler = defaultErrorHandler ?? throw new ArgumentNullException(nameof(defaultErrorHandler));
            Errors = errors ?? [];
            _parameters = [];
        }

        private AttributeParseResult(IVerb verb, ErrorHandler defaultErrorHandler, MethodInfo method, object? instance, object?[] parameters)
        {
            Verb = verb ?? throw new ArgumentNullException(nameof(verb));
            Errors = [];
            _defaultErrorHandler = defaultErrorHandler ?? throw new ArgumentNullException(nameof(defaultErrorHandler));
            _method = method;
            _instance = instance;
            _parameters = parameters;
        }

        /// <summary>
        /// Execute the method identified by the verb if there are no errors and no help was requested.
        /// </summary>
        /// <returns></returns>
        public int Execute()
        {
            return Execute(_defaultErrorHandler);
        }

        public int Execute(ErrorHandler errorHandler)
        {
            if (HelpRequested)
                return 0;

            if (Errors.Any())
                return errorHandler(Errors);

            if (_method == null)
                throw new LogicException("No help was requested and no errors exist, but no target method exists.");

            var result = _method.Invoke(_instance, _parameters);
            if (result is int exitCode)
                return exitCode;
            if (result is bool success)
                return success ? 0 : 1;
            if (result is IAsyncResult)
                throw new InvalidOperationException($"The method \"{_method.Name}\" was executed synchronously, but returned an async result, call ExecuteAsync instead.");
            if (result is not null)
                throw new InvalidOperationException($"The method \"{_method.Name}\" was executed, but returned neither void, null, int nor bool.");

            return 0;
        }

        public Task<int> ExecuteAsync()
        {
            return ExecuteAsync(_defaultErrorHandler);
        }

        public async Task<int> ExecuteAsync(ErrorHandler errorHandler)
        {
            if (HelpRequested)
                return 0;

            if (Errors.Any())
                return errorHandler(Errors);

            if (_method == null)
                throw new LogicException("No help was requested and no errors exist, but no target method exists.");

            var potentialTask = _method.Invoke(_instance, _parameters);
            if (potentialTask is Task task)
            {
                await task;
                return 0;
            }
            else if (potentialTask is Task<int> intTask)
                return await intTask;
            else if (potentialTask is Task<bool> booleanTask)
                return await booleanTask ? 0 : 1;
            else if (potentialTask is not IAsyncResult)
                throw new InvalidOperationException($"The method \"{_method.Name}\" was executed asynchronously, but did not return an async result, call Execute instead.");
            else
                throw new InvalidOperationException($"The method \"{_method.Name}\" was executed asynchronously, but returned neither Task, Task<int> nor Task<bool>.");
        }

        /// <summary>
        /// Execute the method identified by the verb if there are no errors and no help was requested. If there is an exception, an error handler takes care of it.
        /// </summary>
        /// <param name="exitCode"></param>
        /// <returns>Returns true if no exception was thrown, otherwise false.</returns>
        public bool TryExecute(out int exitCode)
        {
            return TryExecute(_defaultErrorHandler, out exitCode);
        }

        public bool TryExecute(ErrorHandler errorHandler, out int exitCode)
        {
            try
            {
                exitCode = Execute();
                return true;
            }
            catch (Exception exc)
            {
                exitCode = errorHandler([new UnexpectedExceptionError(exc)]);
                return false;
            }
        }

        public static AttributeParseResult CreateHelpRequest(IVerb? verb, ErrorHandler errorHandler)
        {
            return new AttributeParseResult(verb, errorHandler);
        }

        public static AttributeParseResult CreateErrorResult(IVerb? verb, ErrorHandler errorHandler, IEnumerable<IError> errors)
        {
            return new AttributeParseResult(verb, errorHandler, errors);
        }

        public static AttributeParseResult CreateSuccessResult(IVerb verb, ErrorHandler errorHandler, MethodInfo method, object? instance, object?[] parameters)
        {
            return new AttributeParseResult(verb, errorHandler, method, instance, parameters);
        }
    }
}
