namespace Locan.Translate {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The interface for translation values.
    /// Each sub-class <b>>must override Equals() and GetHashCode()</b>.
    /// </summary>
    public interface ITranslation {
        ILanguage SourceLanguage { get;}
        ILanguage DestLanguage { get;}
        string StringToTranslate { get;}
        string TrnaslatedString { get;}
    }

    public class Translation :ITranslation {
        public Translation(ILanguage sourceLang, ILanguage destLanguage, string stringToTranslate,string translatedString) {
            this.SourceLanguage = sourceLang;
            this.DestLanguage = destLanguage;
            this.StringToTranslate = stringToTranslate;
            this.TrnaslatedString = translatedString;
        }

        public ILanguage SourceLanguage { get; private set; }
        public ILanguage DestLanguage { get; private set; }
        public string StringToTranslate { get; private set; }
        public string TrnaslatedString { get; private set; }

        public override bool Equals(object obj) {
            bool areEqual = true;
            ITranslation objTranslation = obj as ITranslation;

            if (objTranslation != null) {
                if (this.StringToTranslate != null) {
                    if (!this.StringToTranslate.Equals(objTranslation.StringToTranslate)) {
                        areEqual = false;
                    }
                }
                else if (this.StringToTranslate == null) {
                    if (objTranslation.StringToTranslate != null) {
                        areEqual = false;
                    }
                }
            }

            if (areEqual && this.SourceLanguage != null) {
                if (!this.SourceLanguage.Equals(objTranslation.SourceLanguage)) {
                    areEqual = false;
                }
            }
            else if (areEqual && this.SourceLanguage == null) {
                if (objTranslation != null) {
                    areEqual = false;
                }
            }

            if (areEqual && this.DestLanguage != null) {
                if (!this.DestLanguage.Equals(objTranslation.DestLanguage)) {
                    areEqual = false;
                }
            }
            else if (areEqual && this.DestLanguage == null) {
                if (objTranslation != null) {
                    areEqual = false;
                }
            }

            // do not factor in TranslatedString because that truly can be different from instance to instance
            // and that is OK for equality

            return areEqual;
        }

        public override int GetHashCode() {
            int result = 1;
            if (this.SourceLanguage != null) { result += this.SourceLanguage.GetHashCode(); }
            if (this.DestLanguage != null) { result += this.DestLanguage.GetHashCode(); }
            if (this.StringToTranslate != null) { result += this.StringToTranslate.GetHashCode(); }
            if (this.TrnaslatedString != null) { result += this.StringToTranslate.GetHashCode(); }

            return base.GetHashCode();
        }
    }
}
