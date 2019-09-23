using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colipars.Test
{
    class ConsoleCapturer : IDisposable
    {
        private TextWriter _previousTextWriter;
        private StringWriter _writer;

        public ConsoleCapturer()
        {
            _writer = new StringWriter();
            _previousTextWriter = System.Console.Out;
            System.Console.SetOut(_writer);
        }

        public override string ToString()
        {
            return _writer.ToString();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    System.Console.SetOut(_previousTextWriter);
                }

                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
