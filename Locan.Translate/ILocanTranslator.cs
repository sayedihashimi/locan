namespace Locan.Translate {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface ILocanTranslator {
        IEnumerable<ILanguage> GetSupportedLanguages();
        ILanguage DetectLanguage(string text);

        /// <summary>
        /// Translates the given string
        /// </summary>
        /// <param name="stringToTranslate">string to translate</param>
        /// <param name="sourceLanguage">Language that the text is in, if not provided the
        ///     language will be detected if the translator supports it, othererwise an Exception will be raised.</param>
        /// <returns></returns>
        ITranslation Translate(string stringToTranslate, ILanguage destLanguage, ILanguage sourceLanguage = null);

        /// <summary>
        /// Translates the given strings. 
        /// </summary>
        /// <param name="stringsToTranslate">strings to translate</param>
        /// <param name="destLanguage">Language that the strings should be translated to</param>
        /// <param name="sourceLanguage">Language that the text is in, if not provided the
        //     language will be detected from the first string in the list if the translator 
        //     supports it, othererwise an Exception will be raised.</param>
        /// <returns>An object which allows the caller to attach any async event handlers that are necessary</returns>
        IAsyncContinuation Translate(IEnumerable<string> stringsToTranslate, ILanguage destLanguage, ILanguage sourceLanguage = null);

        /// <summary>
        /// This will send custom translations to bing.
        /// </summary>
        /// <param name="sourceLanguage">The source language</param>
        /// <param name="destLanguage">The dest language</param>
        /// <param name="customTranslations">The dictionary containing all the custom translations. 
        /// <c>Key</c> = string to translate
        /// <c>Value</c> = custom translated string
        /// </param>
        void AddCustomTranslations(ILanguage sourceLanguage, ILanguage destLanguage, IDictionary<string, string> customTranslations);
    }

    internal interface IAsyncTranslator : ILocanTranslator {
        void AddTranslationHandler(Guid id, IAsyncTranslationComplete handler);
    }
}