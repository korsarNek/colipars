using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Colipars.Internal
{
    public class DefaultErrorHandler
    {
        private readonly IErrorPresenter _errorPresenter;
        private readonly IHelpPresenter _helpPresenter;

        //TODO: get rid of custom error codes, other than 1.
        private const int EXIT_CODE_VERB_ERROR = 100;
        private const int EXIT_CODE_ARGUMENT_ERROR = 101;
        private const int EXIT_CODE_OTHER = 1;

        public DefaultErrorHandler(IErrorPresenter errorPresenter, IHelpPresenter helpPresenter)
        {
            _errorPresenter = errorPresenter;
            _helpPresenter = helpPresenter;
        }

        public int HandleErrors(IEnumerable<IError> errors)
        {
            int? exitCode = null;
            foreach (var error in errors)
            {
                if (error is VerbIsMissingError || error is UnknownVerbError)
                {
                    _helpPresenter.Present();
                    exitCode = EXIT_CODE_VERB_ERROR;
                    break;
                }
                else if (error is RequiredParameterMissingError missingError)
                {
                    _helpPresenter.Present(missingError.Verb);
                    exitCode = EXIT_CODE_ARGUMENT_ERROR;
                    break;
                }
                else if (error is OptionForArgumentNotFoundError argumentError)
                {
                    _helpPresenter.Present(argumentError.Verb);
                    exitCode = EXIT_CODE_ARGUMENT_ERROR;
                    break;
                }
            }

            _errorPresenter.Present(errors);
            if (exitCode != null)
                return exitCode.Value;

            return EXIT_CODE_OTHER;
        }
    }
}
