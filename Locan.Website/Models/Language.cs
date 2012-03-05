using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Locan.Translate;

namespace Locan.Website.Models
{
    public class Language
    {
        [Key]
        public string IsoCode { get; set; }
        public IEnumerable<string> Emails { get; set; }
        public CultureInfo Culture
        {
            get
            {
                return CultureInfo.GetCultureInfoByIetfLanguageTag(this.IsoCode);
            }
        }

        public static IEnumerable<Language> GetLanguages(string apiKey)
        {
            BingLocanTranslator bingTranslator = new BingLocanTranslator(apiKey);
            var result = bingTranslator.GetSupportedLanguages()
                .Select(l => l.Language.Replace("zh-CHT", "zh"))
                .Where(l => l != "ht" && l != "zh-CHS");

            return result.Select(l => new Language() { IsoCode = l, Emails = new string[0] }).OrderBy(l => l.Culture.EnglishName);
        }
    }
}