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

    public class VerbIsMissingError : IError
    {
        public string Message => "No verb provided.";
    }

    public class UnknownVerbError(string verb) : IError
    {
        public string ProvidedVerb => verb;

        public string Message => $"Unknown verb \"{ProvidedVerb}\"";
    }

    public class UnexpectedExceptionError(Exception exception) : IError
    {
        public Exception Exception => exception;

        public string Message => "Unexpected Exception: " + Exception.Message;
    }

    public class RequiredParameterMissingError(IVerb verb, string parameterName) : IError
    {
        public IVerb Verb => verb;
        public string ParameterName => parameterName;

        public string Message => $"The required parameter \"{ParameterName}\" is missing.";
    }

    public class NotEnoughElementsError(IVerb verb, string parameterName, int minimumCount) : IError
    {
        public IVerb Verb => verb;
        public string ParameterName => parameterName;
        public int MinimumCount => minimumCount;

        public string Message => $"The number of arguments for the parameter \"{ParameterName}\" is less than {MinimumCount}";
    }

    public class OptionForArgumentNotFoundError(IVerb verb, string argument, int argumentPosition) : IError
    {
        public IVerb Verb => verb;
        public string Argument => argument;
        public int ArgumentPosition => argumentPosition;

        public string Message => $"No option with the name \"{Argument}\" and no positional option at position {ArgumentPosition}.";
    }

}
