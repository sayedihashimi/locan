namespace Locan.Translate.Common {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [Serializable]
    public class MissingRequiredValueException : Exception {
        public MissingRequiredValueException() { }
        public MissingRequiredValueException(string message) : base(message) { }
        public MissingRequiredValueException(string message, Exception inner) : base(message, inner) { }
        protected MissingRequiredValueException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
