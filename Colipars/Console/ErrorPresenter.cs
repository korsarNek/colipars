using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Colipars.Internal;

namespace Colipars.Console
{
    public class ErrorPresenter : IErrorPresenter
    {
        public void Present(IEnumerable<IError> errors)
        {
            if (!errors.Any())
                return;

            System.Console.Error.WriteLine("Error occured!");
            foreach (var error in errors)
            {
                System.Console.Error.WriteLine(error.GetType().Name + ": " + error.Message);
            }
        }
    }
}
