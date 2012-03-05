namespace Locan.IntegrationTest {
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Locan.Translate;
    using System.Threading;

    [TestClass]
    public class TestBingTranslator : BaseTest {
        [TestMethod]
        public void TestTranslateSingleString() {
            BingLocanTranslator translator = this.GetNewBingLocanTranslator();

            string stringToTranslate = @"What is your name?";
            string expectedTranslation = @"Qual é seu nome?";
            ILanguage sourceLanguage = new BaseLanguage("en");
            ILanguage destLanguage = new BaseLanguage("pt");

            ITranslation translation = translator.Translate(stringToTranslate, destLanguage, sourceLanguage);
            Assert.IsNotNull(translation);
            Assert.AreEqual(stringToTranslate, translation.StringToTranslate);
            Assert.AreEqual(destLanguage, translation.DestLanguage);
            Assert.AreEqual(expectedTranslation, translation.TrnaslatedString);
        }

        [TestMethod]
        public void TestTranslateSingleString_NoDestLangSpecified() {
            BingLocanTranslator translator = this.GetNewBingLocanTranslator();

            string stringToTranslate = @"What is your name?";
            string expectedTranslation = @"Qual é seu nome?";
            ILanguage destLanguage = new BaseLanguage("pt");
            ITranslation translation = translator.Translate(stringToTranslate, destLanguage);
            Assert.IsNotNull(translation);
            Assert.AreEqual(stringToTranslate, translation.StringToTranslate);
            Assert.AreEqual(destLanguage, translation.DestLanguage);
            Assert.AreEqual(expectedTranslation, translation.TrnaslatedString);
        }

        [TestMethod]
        public void TestGetSupportedLanguages() {
            BingLocanTranslator translator = this.GetNewBingLocanTranslator();
            IEnumerable<ILanguage> supportedLanguages = translator.GetSupportedLanguages();
            Assert.IsNotNull(supportedLanguages);
            Assert.IsTrue(supportedLanguages.Count() > 0);
        }

        [TestMethod]
        public void TestTranslateAsync() {
            BingLocanTranslator translator = this.GetNewBingLocanTranslator();

            List<string> stringsToTranslate = new List<string> {
                    @"What is your name?",
                    @"How old are you?",
                    @"My name is Sayed."
                };

            List<string> expectedTranslations = new List<string> {
                    @"Qual é seu nome?",
                    @"São velhos ou não é?",
                    @"Meu nome é Sayed.",
                };


            ILanguage sourceLanguage = new BaseLanguage("en");
            ILanguage destLangage = new BaseLanguage("pt");

            int currentIndex = 0;
            translator
                .Translate(stringsToTranslate, destLangage, sourceLanguage)
                .OnTranslationComplete((payload, translationValues) => {
                    Assert.AreEqual(stringsToTranslate.Count, translationValues.Count());
                    for (int i = 0; i < stringsToTranslate.Count; i++) {
                        string stringToTranslate = stringsToTranslate[i];
                        string expectedTranslation = expectedTranslations[i];
                        ITranslation translation = translationValues.ElementAt(i);

                        Assert.AreEqual(stringToTranslate, translation.StringToTranslate);
                        Assert.AreEqual(expectedTranslation, translation.TrnaslatedString);

                        currentIndex++;
                    }
                });

            // must give the service time to perform the translations
            Thread.Sleep(10000);

            Assert.IsTrue(currentIndex == stringsToTranslate.Count);
        }


        [Ignore] // Used for one-off testing
        [TestMethod]
        public void TestAddTranslation() {
            IList<BingTranslatorService.Translation> translationList = new List<BingTranslatorService.Translation>{
                    new BingTranslatorService.Translation {
                    OriginalText="Original text",
                    TranslatedText="custom translation here",
                    Rating=1,
                    RatingSpecified=true,
                    Sequence=0,
                    SequenceSpecified=true
                    }
                };

            BingTranslatorService.TranslateOptions translateOptions = new BingTranslatorService.TranslateOptions {
                Category = "tech",
                ContentType = "text/plain",
                User = "default user"
            };

            string fromLanguage = "en";
            string toLanguage = "pt";

            using (BingTranslatorService.SoapService client = new BingTranslatorService.SoapService()) {
                client.AddTranslationArray(
                    this.BingApiKey,
                    translationList.ToArray(),
                    fromLanguage,
                    toLanguage,
                    translateOptions);

                foreach (BingTranslatorService.Translation translation in translationList) {
                    string result = client.Translate(
                        this.BingApiKey,
                        translation.OriginalText,
                        fromLanguage,
                        toLanguage,
                        translateOptions.ContentType,
                        translateOptions.Category);
                    BingTranslatorService.GetTranslationsResponse allTranslations = client.GetTranslations(
                        this.BingApiKey,
                        translation.OriginalText,
                        fromLanguage,
                        toLanguage,
                        10,
                        true,
                        translateOptions);

                    IList<BingTranslatorService.TranslationMatch> translationMatches = allTranslations.Translations.ToList();
                }
            }
        }

    }
}
