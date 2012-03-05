namespace Locan.Translate.Common {
    using System;

    [Serializable]
    public class ReaderNotFoundException : Exception {
        public ReaderNotFoundException() { }
        public ReaderNotFoundException(string message) : base(message) { }
        public ReaderNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected ReaderNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}


