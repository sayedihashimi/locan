namespace Locan.Translate.Common {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
   
    [Serializable]
    public class ApiKeyAlreadyInUseException : Exception {
        public ApiKeyAlreadyInUseException() { }
        public ApiKeyAlreadyInUseException(string message) : base(message) { }
        public ApiKeyAlreadyInUseException(string message, Exception inner) : base(message, inner) { }
        protected ApiKeyAlreadyInUseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
