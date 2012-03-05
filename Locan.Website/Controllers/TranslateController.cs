using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Locan.Website.Models;
using Locan.Website.Models.Data;
using Locan.Translate;

namespace Locan.Website.Controllers
{
    public class TranslateController : Controller
    {
        public ActionResult Index(string isoCode, Guid fileId)
        {
            return View(TranslationItemFactory.GetTranslationItems(isoCode, fileId));
        }

        [HttpPost]
        public ActionResult Save(string isoCode, Guid fileId)
        {
            var dal = new LocanDal();
            var apiKey = dal.GetUserByFileId(fileId).ApiKey;
            ILocanTranslator bingTranslator = new BingLocanTranslator(apiKey);
            Language language = Language.GetLanguages(apiKey).SingleOrDefault(l => l.IsoCode.Equals(isoCode, StringComparison.OrdinalIgnoreCase));

            var phrases = dal.GetPhrases(p => p.FileId == fileId).ToList();
            IDictionary<string, string> result = new Dictionary<string, string>();

            foreach (string key in Request.Form)
            {
                var phrase = phrases.SingleOrDefault(p => p.TranslateKey == key);
                if (phrase != null)
                {
                    result.Add(phrase.StringToTranslate, Request.Form[key]);
                }
            }

            ILanguage sourceLanguage = bingTranslator.DetectLanguage(phrases.First().StringToTranslate);
            ILanguage destLanguage = new Locan.Translate.BaseLanguage(isoCode);

            bingTranslator.AddCustomTranslations(sourceLanguage, destLanguage, result);

            return View("Done");
        }
    }
}
