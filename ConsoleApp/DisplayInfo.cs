using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace ConsoleApplication1
{
    public class DisplayInfo : IDisposable
    {
        public MemoryMappedFile MMFile { get; set; }
        public MemoryMappedViewAccessor MMViewAccessor { get; set; }

        public int[] Index { get; set; }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

               
                if (MMFile != null)
                {
                    try
                    {
                        MMFile.Dispose();
                    }
                    finally { };
                }
                MMFile = null;

                Index = null;

                disposedValue = true;
            }
        }

        ~DisplayInfo()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
