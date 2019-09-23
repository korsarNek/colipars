using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Colipars.Internal;

namespace Colipars.Attribute.Method
{
    public class AttributeParseResult : IParseResult
    {
        public IVerb Verb { get; }

        public IEnumerable<IError> Errors { get; }

        public bool HelpRequested { get; } = false;

        private IErrorHandler _errorHandler;
        private MethodInfo _method;
        private object _instance;
        private object[] _parameters;

        /// <summary>
        /// Constructor for the case that help was requested.
        /// </summary>
        /// <param name="verb"></param>
        protected AttributeParseResult(IVerb verb)
        {
            Verb = verb;
            HelpRequested = true;
        }

        public AttributeParseResult(IVerb verb, IErrorHandler errorHandler, IEnumerable<IError> errors)
        {
            //Verb not necessarily given in case of an error with the verb.
            Verb = verb;
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            Errors = errors ?? new IError[0];
        }

        public AttributeParseResult(IVerb verb, IErrorHandler errorHandler, MethodInfo method, object instance, object[] parameters)
        {
            Verb = verb ?? throw new ArgumentNullException(nameof(verb));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            Errors = new IError[0];
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
            return Execute(_errorHandler.HandleErrors);
        }

        public int Execute(Func<IEnumerable<IError>, int> errorHandler)
        {
            if (errorHandler == null)
                throw new ArgumentNullException(nameof(errorHandler));

            if (HelpRequested)
                return 0;

            if (Errors.Any())
                return errorHandler(Errors);

            var result = _method.Invoke(_instance, _parameters);
            if (result is int exitCode)
                return exitCode;

            return 0;
        }

        /// <summary>
        /// Execute the method identified by the verb if there are no errors and no help was requested. If there is an exception, an error handler takes care of it.
        /// </summary>
        /// <param name="exitCode"></param>
        /// <returns>Returns true if no exception was thrown, otherwise false.</returns>
        public bool TryExecute(out int exitCode)
        {
            return TryExecute(_errorHandler.HandleErrors, out exitCode);
        }

        public bool TryExecute(Func<IEnumerable<IError>, int> errorHandler, out int exitCode)
        {
            if (errorHandler == null)
                throw new ArgumentNullException(nameof(errorHandler));

            try
            {
                exitCode = Execute();
                return true;
            }
            catch (Exception exc)
            {
                exitCode = errorHandler(new[] { new UnexpectedExceptionError(exc) });
                return false;
            }
        }

        public static AttributeParseResult CreateHelpRequest(IVerb verb = null)
        {
            return new AttributeParseResult(verb);
        }
    }
}
