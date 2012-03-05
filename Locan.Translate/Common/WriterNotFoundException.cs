namespace Locan.Translate.Common {
    using System;

    [Serializable]
    public class WriterNotFoundException : Exception {
        public WriterNotFoundException() { }
        public WriterNotFoundException(string message) : base(message) { }
        public WriterNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected WriterNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
