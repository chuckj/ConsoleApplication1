using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConsoleApplication1
{
    public class DisplayInfo : IDisposable
    {
        public FileStream FileStream { get; set; }
        public BinaryReader BinaryReader { get; set; }
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

                if (BinaryReader != null)
                {
                    try
                    {
                        BinaryReader.Close();
                    }
                    finally { };
                }
                if (FileStream != null)
                {
                    try
                    {
                        FileStream.Dispose();
                    }
                    finally { };
                }
                FileStream = null;
                BinaryReader = null;

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
