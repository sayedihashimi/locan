namespace Locan.Translate.IO {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Resources;
    using Locan.Translate.Common;

    /// <summary>
    /// This will write the resx file to the destination specified. This writer 
    /// uses the <c>Id</c> property as the resource key and the 
    /// <c>TranslatedString</c> as the value
    /// </summary>
    public class ResxFileLocanWriter : ILocanWriter {
        private object lockWriter = new object();
        
        public ResxFileLocanWriter(string filepath) {
            this.Filepath = filepath;
            this.Disposed = false;
        }

        private string Filepath { get; set; }
        private bool Disposed { get; set; }
        private ResXResourceWriter ResxWriter { get; set; }

        public void WriteRow(ILocanRow row) {
            if (row == null) { throw new ArgumentNullException("row"); }

            if (this.ResxWriter == null) {
                lock (this.lockWriter) {
                    if (this.ResxWriter == null && !this.Disposed) {
                        this.ResxWriter = new ResXResourceWriter(this.Filepath);
                    }
                }
            }

            if (this.Disposed) {
                string message = string.Format("The write has been disposed");
                throw new UnexpectedStateException(message);
            }

            ResxWriter.AddResource(row.Id, row.TranslatedString);
        }

        public void WriteRows(IEnumerable<ILocanRow> rows) {
            if (rows == null) { throw new ArgumentNullException("rows"); }

            foreach (ILocanRow row in rows) {
                this.WriteRow(row);
            }
        }

        #region IDisposable items
        ~ResxFileLocanWriter() {
            this.Dispose(false);
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // free up managed resources here
                if (this.ResxWriter != null) {
                    lock (this.lockWriter) {
                        if (this.ResxWriter != null) {
                            ResxWriter.Dispose();
                        }
                        this.ResxWriter = null;
                    }
                }    
            }

            // free up any native resources here

            this.Disposed = true;
        }

        #endregion
        
    }
}
