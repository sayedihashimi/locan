namespace Locan.Translate {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IAsyncContinuation {
        void OnTranslationComplete(Action<IAsyncTranslationPayload,IEnumerable<ITranslation>> onTranslationComplete);
    }
}
