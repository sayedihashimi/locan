namespace Locan.Translate {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Impelmentors must be Serializable
    /// </summary>
    public interface ILanguage {
        string Language { get; }
    }

    [Serializable]
    public class BaseLanguage :ILanguage {
        public BaseLanguage(string language) {
            if (string.IsNullOrEmpty(language)) { throw new ArgumentNullException("language"); }

            this.Language = language;
        }

        public string Language { get; set; }

        public override bool Equals(object obj) {
            bool areEqual = false;
            
            string objStr = obj as string;
            if (objStr != null) {
                areEqual = this.Language.Equals(objStr);
            }
            
            return areEqual;
        }

        public override int GetHashCode() {
            return this.Language.GetHashCode();
        }
    }
}
