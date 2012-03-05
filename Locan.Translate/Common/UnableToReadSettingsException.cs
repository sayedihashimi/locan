namespace Locan.Translate.Common {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [Serializable]
    public class UnableToReadSettingsException : Exception {
        public UnableToReadSettingsException() { }
        public UnableToReadSettingsException(string message) : base(message) { }
        public UnableToReadSettingsException(string message, Exception inner) : base(message, inner) { }
        protected UnableToReadSettingsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
