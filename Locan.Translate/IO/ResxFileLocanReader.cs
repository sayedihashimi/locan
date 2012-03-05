namespace Locan.Translate.IO {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Resources;
    using System.Collections;

    public class ResxFileLocanReader : ILocanReader, IDisposable {
        protected internal ResxFileLocanReader(string filepath) {
            if (string.IsNullOrEmpty(filepath)) { throw new ArgumentNullException("filepath"); }

            this.Filepath = filepath;
        }

        public string Filepath { get; private set; }

        private ResXResourceReader ResxReader { get; set; }
        private object lockResxReader = new object();

        public IEnumerable<ILocanRow> GetRowsToBeTranslated() {
            this.ResxReader = new ResXResourceReader(this.Filepath);
            foreach (DictionaryEntry entry in this.ResxReader) {
                // conver the entry into an ILocanRow
                ILocanRow row = new LocanRow(Convert.ToString(entry.Key), Convert.ToString(entry.Value), null);
                yield return row;
            }
        }

        public IEnumerable<ILocanRow> GetRowsTranslated() {
            this.ResxReader = new ResXResourceReader(this.Filepath);
            foreach (DictionaryEntry entry in this.ResxReader) {
                // conver the entry into an ILocanRow
                ILocanRow row = new LocanRow(Convert.ToString(entry.Key), null, Convert.ToString(entry.Value));
                yield return row;
            }
        }

        #region IDisposable items
        ~ResxFileLocanReader() {
            this.Dispose(false);
        }
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // free up managed resources here
                if (this.ResxReader != null) {
                    lock (this.lockResxReader) {
                        if (this.ResxReader != null) {
                            IDisposable disposable = this.ResxReader as IDisposable;
                            if (disposable != null) {
                                disposable.Dispose();
                            }
                            this.ResxReader = null;
                        }
                    }
                }
            }
            // free up any native resources here
        }
        #endregion
    }
}