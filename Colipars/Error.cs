using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars
{
    public interface IError
    {
        string Message { get; }
    }

    public interface IVerbError : IError
    {
        IVerb Verb { get; }
    }

    public class VerbIsMissingError : IError
    {
        public string Message => "No verb provided.";
    }

    public class UnknownVerbError : IError
    {
        public string ProvidedVerb;

        public UnknownVerbError(string verb)
        {
            ProvidedVerb = verb ?? throw new ArgumentNullException(nameof(verb));
        }

        public string Message => $"Unknown verb \"{ProvidedVerb}\"";
    }

    public class UnexpectedExceptionError : IError
    {
        public Exception Exception { get; }

        public UnexpectedExceptionError(Exception exception)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }

        public string Message => "Unexpected Exception: " + Exception.Message;
    }

    public class RequiredParameterMissingError : IVerbError
    {
        public IVerb Verb { get; }
        public string ParameterName { get; }

        public RequiredParameterMissingError(IVerb verb, string parameterName)
        {
            Verb = verb ?? throw new ArgumentNullException(nameof(verb));
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
        }

        public string Message => $"The required parameter \"{ParameterName}\" is missing.";
    }

    public class NotEnoughElementsError : IVerbError
    {
        public IVerb Verb { get; }
        public string ParameterName { get; }
        public int MinimumCount { get; }

        public NotEnoughElementsError(IVerb verb, string parameterName, int minimumCount)
        {
            Verb = verb;
            ParameterName = parameterName;
            MinimumCount = minimumCount;
        }

        public string Message => $"The number of arguments for the parameter \"{ParameterName}\" is less than {MinimumCount}";
    }

    public class OptionForArgumentNotFoundError : IVerbError
    {
        public IVerb Verb { get; }
        public string Argument { get; }
        public int ArgumentPosition { get; }

        public OptionForArgumentNotFoundError(IVerb verb, string argument, int argumentPosition)
        {
            Verb = verb ?? throw new ArgumentNullException(nameof(verb));
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
            ArgumentPosition = argumentPosition;
        }

        public string Message => $"No option with the name \"{Argument}\" and no positional option at position {ArgumentPosition}.";
    }

}
