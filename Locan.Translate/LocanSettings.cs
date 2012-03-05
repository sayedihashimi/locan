namespace Locan.Translate {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using Locan.Translate.Common.Extensions;
    using Locan.Translate.Common;
    using System.IO;

    public interface ILocanSettings {
        string DefaultApiKey { get; set; }
        bool PreserveUpdates { get; set; }
        IList<ILanguage> SupportedLanguages { get; }
        void SaveAs(string xmlFilepath);
    }

    public class LocanSettings : ILocanSettings {
        public LocanSettings() {
            this.PreserveUpdates = true;
            this.SupportedLanguages = new List<ILanguage>();
        }

        public string DefaultApiKey { get; set; }
        public bool PreserveUpdates { get; set; }
        public IList<ILanguage> SupportedLanguages { get; private set; }

        public void SaveAs(string xmlFilepath) {
            if (string.IsNullOrEmpty(xmlFilepath)) { throw new ArgumentNullException("xmlFilepath"); }

            XNamespace ns = Consts.XmlNamespace;
            XDocument xmlDocument = new XDocument(
                                        new XElement(ns + Consts.Locan,
                                            new XElement(ns + Consts.Settings,
                                                new XAttribute(Consts.DefaultApiKey,this.DefaultApiKey),
                                                new XAttribute(Consts.PreserveUpdates,this.PreserveUpdates)),
                                            new XElement(ns + Consts.Languages,
                                                from l in this.SupportedLanguages
                                                select new XElement(ns + Consts.Language,
                                                    new XAttribute(Consts.Name,l.Language))
                                                )));

            if (File.Exists(xmlFilepath)) {
                File.Delete(xmlFilepath);
            }

            xmlDocument.Save(xmlFilepath);
        }

        #region static members
        public static ILocanSettings Load(string uri) {
            XDocument document = XDocument.Load(uri);
            XNamespace ns = Consts.XmlNamespace;
            XElement rootElement = document.Element(ns + Consts.Locan);

            var resultList = from settingsElement in rootElement.Elements(ns+Consts.Settings)
                             from languagesElement in rootElement.Elements(ns+Consts.Languages)
                             select new {
                                 DefaultApiKey = settingsElement.SafeGetAttributeValue(Consts.DefaultApiKey),
                                 PreserveUpdates = settingsElement.SafeGetAttributeValue(Consts.PreserveUpdates),
                                 Languages = from l in languagesElement
                                                 .Descendants(ns + Consts.Language)
                                             select l.SafeGetAttributeValue(Consts.Name)
                             };
            var result = resultList.FirstOrDefault();

            if (result == null) {
                string message = string.Format("Unable to read settings (no settings found) from uri [{0}].",uri);
                throw new UnableToReadSettingsException(message);
            }


            ILocanSettings settings = new LocanSettings();
            if (!string.IsNullOrEmpty(result.DefaultApiKey)) {
                settings.DefaultApiKey = result.DefaultApiKey;
            }
            if (!string.IsNullOrEmpty(result.PreserveUpdates)) {
                settings.PreserveUpdates = bool.Parse(result.PreserveUpdates);
            }
            if (result.Languages != null && result.Languages.Count() > 0) {
                foreach (var lang in result.Languages) {
                    if (!string.IsNullOrEmpty(lang)) {
                        settings.SupportedLanguages.Add(new BaseLanguage(lang));

                    }
                }
            }

            return settings;
        }
        #endregion


        // internal in case unit test needs access
        internal static class Consts {
            public const string XmlNamespace = @"http://schemas.microsoft.com/2011/04/Locan";
            public const string Locan = @"Locan";
            public const string DefaultApiKey = @"DefaultApiKey";
            public const string PreserveUpdates = @"PreserveUpdates";
            public const string Languages = @"Languages";
            public const string Language = @"Language";
            public const string Name = @"Name";
            public const string Settings = @"Settings";
        }
    }
}
