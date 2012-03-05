using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Locan.Website.Models.Data;
using Locan.Translate;


namespace Locan.Website.Models
{
    public class TranslationItemFactory
    {
        public static IEnumerable<TranslationItem> GetTranslationItems(string isoCode, Guid fileId)
        {
            var dal = new LocanDal();
            var apiKey = dal.GetUserByFileId(fileId).ApiKey;
            var phrases = dal.GetPhrases(p => p.FileId == fileId).ToList();


            return Convert(phrases, apiKey, isoCode);
        }

        private static IEnumerable<TranslationItem> Convert(IEnumerable<Phrase> phrases, string apiKey, string isoCode)
        {
            BingLocanTranslator bingTranslator = new BingLocanTranslator(apiKey);
            var result = bingTranslator.TranslateSync(phrases.Select(p => p.StringToTranslate), new BaseLanguage(isoCode));

            for (int i = 0; i < phrases.Count(); i++)
            {
                Phrase phrase = phrases.ElementAt(i);
                string translation = result[i].TranslatedText;

                yield return new TranslationItem()
                {
                    ID = phrase.TranslateKey,
                    ValueFrom = phrase.StringToTranslate,
                    ValueTo = translation,
                    Comment = phrase.Comment
                };
            }
        }
    }
}