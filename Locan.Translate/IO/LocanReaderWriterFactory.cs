namespace Locan.Translate.IO {
    using System;
    using System.Collections.Generic;
    using Locan.Translate.Common;

    public interface ILocanReaderWriterFactory {
        ILocanReader GetReader(object properties);

        ILocanWriter GetWriter(object properties);
    }

    public class LocanReaderWriterFactory : ILocanReaderWriterFactory {
        private static LocanReaderWriterFactory instance = new LocanReaderWriterFactory();

        private LocanReaderWriterFactory() { }

        public static ILocanReaderWriterFactory Instance {
            get { return instance; }
        }

        #region Reader Methods
        public ILocanReader GetReader(object properties) {
            if (properties == null) { throw new ArgumentNullException("properties"); }

            IDictionary<string, object> values = CollectionHelper.Instance.ConvertToDictionary(properties);
            ILocanReader reader = null;

            // get the file name
            string filepath = this.GetProperty(values, Consts.Filepath, true) as string;
            reader = this.GetReaderForFilepath(filepath);

            if (reader == null) {
                string message = string.Format("Couldn't find reader for filepath: [{0}]", filepath);
                throw new ReaderNotFoundException(message);
            }

            return reader;
        }

        private ILocanReader GetReaderForFilepath(string filepath) {
            if (string.IsNullOrEmpty(filepath)) { throw new ArgumentNullException("filepath"); }

            ILocanReader reader = null;
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(Consts.ResxRegexPattern);
            if (r.IsMatch(filepath)) {
                reader = new ResxFileLocanReader(filepath);
            }

            return reader;
        }

        #endregion

        #region Writer methods
        public ILocanWriter GetWriter(object properties) {
            if (properties == null) { throw new ArgumentNullException(Consts.Filepath); }
            
            IDictionary<string, object> values = CollectionHelper.Instance.ConvertToDictionary(properties);
            string filepath = this.GetProperty(values, Consts.Filepath, true) as string;
            ILocanWriter writer = this.GetWriterForFilepath(filepath);

            if (writer == null) {
                string message = string.Format("Couldn't find writer for filepath: [{0}]",filepath);
                throw new WriterNotFoundException(message);
            }

            return writer;
        }
        private ILocanWriter GetWriterForFilepath(string filepath) {
            if (string.IsNullOrEmpty(filepath)) { throw new ArgumentNullException("filepath"); }

            ILocanWriter writer = null;
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(Consts.ResxRegexPattern);
            if (r.IsMatch(filepath)) {
                writer = new ResxFileLocanWriter(filepath);
            }
            return writer;
        }
        #endregion

        protected internal object GetProperty(IDictionary<string, object> values, string valueName, bool required = false) {
            if (values == null) { throw new ArgumentNullException("values"); }
            if (string.IsNullOrEmpty(valueName)) { throw new ArgumentNullException("valueName"); }

            object value = null;
            values.TryGetValue(valueName, out value);
            if (value == null && required) {
                string message = string.Format("Missing value for key [{0}]", valueName);
                throw new MissingRequiredValueException(message);
            }

            return value;
        }
    }

    internal static class Consts {
        public const string ResxRegexPattern = @"\.resx";
        public const string Filepath = @"filepath";
    }
}
