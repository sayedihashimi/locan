namespace Locan.Translate {
    using System;
    using System.Collections.Generic;
    using Locan.Translate.Common;

    public interface IAsyncTranslationPayload: IAsyncContinuation {
        Guid Id { get; }
        List<string> StringsToTranslate { get;}
        ILanguage SourceLanguage { get; }
        ILanguage DestLanguage { get; }

        // void OnTranslationComplete(Action<IEnumerable<ITranslation>> onTranslationComplete);
    }

    public class AsyncTranslationPayload : IAsyncTranslationPayload {
        public AsyncTranslationPayload(Guid id, List<string> stringsToTranslate, ILanguage sourceLanguage, ILanguage destLanguage, ILocanTranslator translator) {
            if (id == Guid.Empty) {
                throw new ArgumentException("id cannot equal Guid.Empty");
            }
            if (stringsToTranslate == null) { throw new ArgumentNullException("stringsToTranslate"); }
            if (translator == null) { throw new ArgumentNullException("translator"); }
            if (sourceLanguage == null) { throw new ArgumentNullException("sourceLanguage"); }
            if (destLanguage == null) { throw new ArgumentNullException("destLanguage"); }

            this.Transloator = translator as IAsyncTranslator;
            if (this.Transloator == null) {
                throw new UnexpectedStateException("The translator provided doesn't support async operations");
            }

            this.Id = id;
            this.StringsToTranslate = stringsToTranslate;

            this.SourceLanguage = sourceLanguage;
            this.DestLanguage = destLanguage;
        }

        internal IAsyncTranslator Transloator { get; private set; }
        public Guid Id { get; private set; }
        public List<string> StringsToTranslate { get; private set; }
        public ILanguage SourceLanguage { get; private set; }
        public ILanguage DestLanguage { get; private set; }

        public void OnTranslationComplete(Action<IAsyncTranslationPayload, IEnumerable<ITranslation>> onTranslationComplete) {
            if (onTranslationComplete == null) { throw new ArgumentNullException("onTranslationComplete"); }

            IAsyncTranslationComplete tranzComplete = new AsyncTranslationComplete(this, onTranslationComplete);
            this.Transloator.AddTranslationHandler(this.Id, tranzComplete);
        }

    }
}
