namespace Locan.Translate {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Locan.Translate.BingTranslatorService;
    using Locan.Translate.Common;

    public class BingLocanTranslator : ILocanTranslator, IAsyncTranslator {
        private static IDictionary<int, ITranslation> translationCache = new Dictionary<int, ITranslation>();
        private static object translationCacheLock = new object();
        private object lockAsyncTranslationHandlers = new object();

        private IList<ILanguage> supportedLanguages;
        private object lockSupportedLanguages = new object();
        
        public string ApiKey { get; private set; }
        private TranslateOptions TranslateOptions { get; set; }
        private IDictionary<Guid, List<IAsyncTranslationComplete>> AsyncTranslationHandlers;
        private int CustomTranslationRating { get; set; }

        public BingLocanTranslator(string apiKey) {
            if (string.IsNullOrEmpty(apiKey)) { throw new ArgumentNullException("apiKey"); }

            this.AsyncTranslationHandlers = new Dictionary<Guid, List<IAsyncTranslationComplete>>();
            this.ApiKey = apiKey;
            this.TranslateOptions = new TranslateOptions { Category = "tech", ContentType = "text/plain", User = "default user" };
            this.CustomTranslationRating = 7;
        }

        public ITranslation Translate(string stringToTranslate, ILanguage destLanguage, ILanguage sourceLanguage = null) {
            if (string.IsNullOrEmpty(stringToTranslate)) { throw new ArgumentNullException("stringToTranslate"); }

            if (sourceLanguage == null) {
                sourceLanguage = this.DetectLanguage(stringToTranslate);
            }

            ITranslation translation = BingLocanTranslator.GetTranslationFromCache(stringToTranslate, sourceLanguage, destLanguage);
            if (translation == null) {
                string translationResult = null;
                using (BingTranslatorService.SoapService client = new BingTranslatorService.SoapService()) {
                    translationResult = client.Translate(
                        this.ApiKey,
                        stringToTranslate,
                        sourceLanguage.Language,
                        destLanguage.Language,
                        this.TranslateOptions.ContentType,
                        this.TranslateOptions.Category);
                }

                translation = new Translation(sourceLanguage, destLanguage, stringToTranslate, translationResult);
            }

            return translation;
        }

        public TranslateArrayResponse[] TranslateSync(IEnumerable<string> stringsToTranslate, ILanguage destLanguage, ILanguage sourceLanguage = null)
        {
            if (stringsToTranslate == null) { throw new ArgumentNullException("stringsToTranslate"); }

            // Convert stringsToTranslate to a list because I need to send them all at once
            //      to the translation engine. We can only get benefit of IEnumberable in one direction
            List<string> stringList = stringsToTranslate.ToList();

            if (sourceLanguage == null)
            {
                // detect language from the first string in the list to translate                
                sourceLanguage = this.DetectLanguage(stringsToTranslate.First());
            }

            Guid translationGuid = Guid.NewGuid();

         
            // TODO: Do we want this to be done async? if so update this code
            using (BingTranslatorService.SoapService client = new BingTranslatorService.SoapService())
            {
                return client.TranslateArray(
                    this.ApiKey,
                    stringList.ToArray(),
                    sourceLanguage.Language,
                    destLanguage.Language,
                    this.TranslateOptions);
            }
        }

        /// <summary>
        /// Translates the given strings.
        /// </summary>
        /// <param name="stringToTranslate">string to translate</param>
        /// <param name="sourceLanguage">Language that the text is in, if not provided the
        ///     language will be detected from the first string in the list if the translator 
        ///     supports it, othererwise an Exception will be raised.</param>
        /// <returns></returns>
        public IAsyncContinuation Translate(IEnumerable<string> stringsToTranslate, ILanguage destLanguage, ILanguage sourceLanguage = null) {
            if (stringsToTranslate == null) { throw new ArgumentNullException("stringsToTranslate"); }

            // Convert stringsToTranslate to a list because I need to send them all at once
            //      to the translation engine. We can only get benefit of IEnumberable in one direction
            List<string> stringList = stringsToTranslate.ToList();

            if (sourceLanguage == null) {
                // detect language from the first string in the list to translate                
                sourceLanguage = this.DetectLanguage(stringsToTranslate.First());
            }

            Guid translationGuid = Guid.NewGuid();

            IAsyncTranslationPayload asyncObj = new AsyncTranslationPayload(Guid.NewGuid(), stringList, sourceLanguage, destLanguage, this);
            BingTranslatorUserState userState = new BingTranslatorUserState {
                Id = asyncObj.Id,
                SourceLanguage = sourceLanguage as BaseLanguage,
                DestLanguage = destLanguage as BaseLanguage
            };

            // TODO: Do we want this to be done async? if so update this code
            using (BingTranslatorService.SoapService client = new BingTranslatorService.SoapService()) {
                client.TranslateArrayAsync(
                    this.ApiKey,
                    stringList.ToArray(),
                    sourceLanguage.Language,
                    destLanguage.Language,
                    this.TranslateOptions,
                    userState);
                client.TranslateArrayCompleted += HandleTranslationComplete;
            }

            return asyncObj;
        }

        public IAsyncContinuation Translate2(IEnumerable<string> stringsToTranslate, ILanguage destLanguage, ILanguage sourceLanguage = null) {
            if (stringsToTranslate == null) { throw new ArgumentNullException("stringsToTranslate"); }

            // Convert stringsToTranslate to a list because I need to send them all at once
            //      to the translation engine. We can only get benefit of IEnumberable in one direction
            List<string> stringList = stringsToTranslate.ToList();

            if (sourceLanguage == null) {
                // detect language from the first string in the list to translate                
                sourceLanguage = this.DetectLanguage(stringsToTranslate.First());
            }

            Guid translationGuid = Guid.NewGuid();

            IAsyncTranslationPayload asyncObj = new AsyncTranslationPayload(Guid.NewGuid(), stringList, sourceLanguage, destLanguage, this);
            BingTranslatorUserState userState = new BingTranslatorUserState {
                Id = asyncObj.Id,
                SourceLanguage = sourceLanguage as BaseLanguage,
                DestLanguage = destLanguage as BaseLanguage
            };

            // TODO: Do we want this to be done async? if so update this code
            using (BingTranslatorService.SoapService client = new BingTranslatorService.SoapService()) {
                client.TranslateArrayAsync(
                    this.ApiKey,
                    stringList.ToArray(),
                    sourceLanguage.Language,
                    destLanguage.Language,
                    this.TranslateOptions,
                    userState);
                client.TranslateArrayCompleted += HandleTranslationComplete;
            }

            return asyncObj;
        }

        internal void AddTranslationHandler(Guid id, IAsyncTranslationComplete handler) {
            lock (this.lockAsyncTranslationHandlers) {
                List<IAsyncTranslationComplete> handlers = null;
                this.AsyncTranslationHandlers.TryGetValue(id, out handlers);

                if (handlers == null) {
                    handlers = new List<IAsyncTranslationComplete>();
                }

                handlers.Add(handler);

                this.AsyncTranslationHandlers[id] = handlers;
            }
        }

        void IAsyncTranslator.AddTranslationHandler(Guid id, IAsyncTranslationComplete handler) {
            lock (this.lockAsyncTranslationHandlers) {
                List<IAsyncTranslationComplete> handlers = null;
                this.AsyncTranslationHandlers.TryGetValue(id, out handlers);

                if (handlers == null) {
                    handlers = new List<IAsyncTranslationComplete>();
                }

                handlers.Add(handler);

                this.AsyncTranslationHandlers[id] = handlers;
            }
        }

        #region Async work

        private class AsnycHandlerContainer {
            public IAsyncTranslationPayload TranslationAsync { get; set; }
            public IAsyncTranslationComplete OnTranslationComplete { get; set; }
        }

        protected internal void HandleTranslationComplete(object sender, TranslateArrayCompletedEventArgs e) {
            // look up the id from e
            if (e.UserState == null) {
                string message = string.Format("UserState from async translation is null, expected it to be an istance of BingTranslatorUserState.");
                throw new UnexpectedStateException(message);
            }

            // BingTranslatorUserState userState = new BingTranslatorUserState(e.UserState as object[]);
            BingTranslatorUserState userState = e.UserState as BingTranslatorUserState;
            // see if there are any registered handlers
            List<IAsyncTranslationComplete> handlers = null;
            this.AsyncTranslationHandlers.TryGetValue(userState.Id, out handlers);

            // remove the id from the dictionary then call the events, they may take time to execute
            //      that is why they are removed first
            if (handlers != null) {
                lock (lockAsyncTranslationHandlers) {
                    this.AsyncTranslationHandlers[userState.Id] = null;
                }

                // create the object that will be passed to the handlers
                List<ITranslation> translationResult = new List<ITranslation>();

                foreach (IAsyncTranslationComplete handler in handlers) {
                    List<string> stringsToTranslate = handler.TranslationAsync.StringsToTranslate;
                    if (stringsToTranslate.Count != e.Result.Length) {
                        string message = string.Format(
                            "There was a mis-match between the count of strings to translate [{0}] and the translated strings [{1}].",
                            stringsToTranslate.Count,
                            e.Result.Length);
                        throw new UnexpectedStateException(message);
                    }

                    List<ITranslation> translations = new List<ITranslation>(stringsToTranslate.Count);

                    for (int i = 0; i < stringsToTranslate.Count; i++) {
                        translations.Add(
                            new Translation(
                                handler.TranslationAsync.SourceLanguage,
                                handler.TranslationAsync.DestLanguage,
                                stringsToTranslate[i],
                                e.Result[i].TranslatedText));
                    }

                    handler.OnTranslationComplete(handler.TranslationAsync,translations);
                }
            }
        }

        #endregion

        public ILanguage DetectLanguage(string text) {
            if (string.IsNullOrEmpty(text)) { throw new ArgumentNullException("text"); }

            ILanguage result = null;
            using (BingTranslatorService.SoapService client = new BingTranslatorService.SoapService()) {
                string lang = client.Detect(this.ApiKey, text);
                result = new BaseLanguage(lang);
            }
            return result;
        }

        public IEnumerable<ILanguage> GetSupportedLanguages() {
            if (this.supportedLanguages == null) {
                lock (lockSupportedLanguages) {
                    if (this.supportedLanguages == null) {
                        // Get languages
                        using (BingTranslatorService.SoapService client = new BingTranslatorService.SoapService()) {
                            IList<ILanguage> langList = new List<ILanguage>();

                            string[] languages = client.GetLanguagesForTranslate(this.ApiKey);
                            languages.ToList().ForEach(lang => {
                                langList.Add(
                                    new BaseLanguage(lang));
                            });

                            this.supportedLanguages = langList;
                        }
                    }
                }
            }

            return this.supportedLanguages;
        }
        
        public void AddCustomTranslations(ILanguage sourceLanguage, ILanguage destLanguage, IDictionary<string,string>customTranslations) {
            if (sourceLanguage == null) { throw new ArgumentNullException("soureLanguage"); }
            if (destLanguage == null) { throw new ArgumentNullException("destLanguage"); }
            if (customTranslations == null) { throw new ArgumentNullException("customTranslations"); }

            // TODO: This could be done Async

            IList<BingTranslatorService.Translation> translationList = new List<BingTranslatorService.Translation>();
            foreach (string key in customTranslations.Keys) {
                if (!string.IsNullOrWhiteSpace(key)) {
                    BingTranslatorService.Translation translation = new BingTranslatorService.Translation();
                    translation.OriginalText = key;
                    translation.TranslatedText = customTranslations[key];
                    // make it less than 5 because that is the max value for machine translations
                    translation.Rating = this.CustomTranslationRating;
                    translation.RatingSpecified = true;
                    translation.Sequence = 0;
                    translation.SequenceSpecified = true;
                    translationList.Add(translation);
                }
            }

            // TODO: We should batch these into 100 because according to http://msdn.microsoft.com/en-us/library/ff512409.aspx that is the limit

            using (BingTranslatorService.SoapService client = new BingTranslatorService.SoapService()) {
                client.AddTranslationArray(this.ApiKey, translationList.ToArray(), sourceLanguage.Language, destLanguage.Language, this.TranslateOptions);
            }
        }

        #region Static items
        private static void AddTranslationToCache(ITranslation translation) {
            int hashCode = BingLocanTranslator.GetCacheKeyFor(translation);

            lock (BingLocanTranslator.translationCacheLock) {
                BingLocanTranslator.translationCache[hashCode] = translation;
            }
        }

        private static ITranslation GetTranslationFromCache(string stringToTranslate, ILanguage sourceLanguage, ILanguage destLanguage) {
            if (string.IsNullOrEmpty(stringToTranslate)) { throw new ArgumentNullException("stringToTranslate"); }
            if (sourceLanguage == null) { throw new ArgumentNullException("sourceLanguage"); }
            if (destLanguage == null) { throw new ArgumentNullException("destLanguage"); }

            ITranslation translation = null;
            ITranslation translationFromCache = null;
            BingLocanTranslator.translationCache.TryGetValue(
                BingLocanTranslator.GetCacheKeyFor(stringToTranslate, sourceLanguage, destLanguage),
                out translationFromCache);

            if (translationFromCache != null) {
                // double check the values because there may have been a collision on the hash code
                if (stringToTranslate.Equals(translationFromCache.StringToTranslate) &&
                    sourceLanguage.Equals(translationFromCache.SourceLanguage) &&
                    destLanguage.Equals(translationFromCache.DestLanguage)) {
                    translation = translationFromCache;
                }
            }

            return translation;
        }

        private static int GetCacheKeyFor(ITranslation translation) {
            return GetCacheKeyFor(translation.StringToTranslate, translation.SourceLanguage, translation.DestLanguage);
        }

        private static int GetCacheKeyFor(string stringToTranslate, ILanguage sourceLanguage, ILanguage destLanguage) {
            if (string.IsNullOrEmpty(stringToTranslate)) { throw new ArgumentNullException("stringToTranslate"); }
            if (sourceLanguage == null) { throw new ArgumentNullException("sourceLanguage"); }
            if (destLanguage == null) { throw new ArgumentNullException("destLanguage"); }

            int hashCode = stringToTranslate.GetHashCode() + sourceLanguage.GetHashCode() + destLanguage.GetHashCode();

            return hashCode;
        }
        #endregion

        [Serializable]
        private class BingTranslatorUserState {
            public BingTranslatorUserState() { }
            //public BingTranslatorUserState(object[] userStateFromBing) {
            //    if (userStateFromBing == null) { throw new ArgumentNullException("userStateFromBing"); }
            //    if (userStateFromBing.Length != 3) {
            //        string message = string.Format(
            //            "User state invalid, expected [{0}] values but received [{1}] values",
            //            3,
            //            userStateFromBing.Length);
            //        throw new UnexpectedStateException(message);
            //    }
            //    this.Id = new Guid(userStateFromBing[0] as string);
            //    this.SourceLanguage = new BaseLanguage(userStateFromBing[1] as string);
            //    this.DestLanguage = new BaseLanguage(userStateFromBing[2] as string);
            //}
            public Guid Id { get; set; }
            public BaseLanguage SourceLanguage { get; set; }
            public BaseLanguage DestLanguage { get; set; }

            //public object[] ToObjectArray() {
            //    return new object[] { Id, SourceLanguage.Language, DestLanguage.Language };
            //}
        }

    }
}