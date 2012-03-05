namespace Locan.Translate {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal interface IAsyncTranslationComplete {
        Action<IAsyncTranslationPayload,IEnumerable<ITranslation>> OnTranslationComplete { get; }
        IAsyncTranslationPayload TranslationAsync { get; }
    }

    internal class AsyncTranslationComplete : IAsyncTranslationComplete {
        private Action<IAsyncTranslationPayload,IEnumerable<ITranslation>> onTranslationComplete;
        private IAsyncTranslationPayload translationAsync;

        public AsyncTranslationComplete(IAsyncTranslationPayload translationAsync, Action<IAsyncTranslationPayload,IEnumerable<ITranslation>> onTranslationComplete) {
            if (translationAsync == null) { throw new ArgumentNullException("translationAsync"); }
            if (onTranslationComplete == null) { throw new ArgumentNullException("onTranslationComplete"); }

            this.TranslationAsync = translationAsync;
            this.OnTranslationComplete = onTranslationComplete;
        }

        public Action<IAsyncTranslationPayload, IEnumerable<ITranslation>> OnTranslationComplete {
            get { return this.onTranslationComplete; }
            private set { this.onTranslationComplete = value; }
        }

        public IAsyncTranslationPayload TranslationAsync {
            get { return this.translationAsync; }
            private set { this.translationAsync = value; }
        }
    }
}
