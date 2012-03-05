namespace Locan.Translate.IO {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represent a row of data that needs to be or has been translated
    /// </summary>
    public interface ILocanRow {
        /// <summary>
        /// Unique identifier for this row
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Text value which should be translated.
        /// This value may be <c>null</c> when returning the translated string in <c>TranslatedString</c>
        /// </summary>
        string StringToTranslate { get; }
        /// <summary>
        /// Text value which contains the translated value.
        /// This value may be <c>null</c> when the row is being translated.
        /// </summary>
        string TranslatedString { get; }
    }

    [Serializable]
    public class LocanRow : ILocanRow {
        public LocanRow(){}
        public LocanRow(string id, string stringToTranslate = null, string translatedString = null)
            :this() {
            this.Id = id;
            this.StringToTranslate = stringToTranslate;
            this.TranslatedString = translatedString;
        }

        public string Id { get; set; }
        public string StringToTranslate { get; set; }
        public string TranslatedString { get; set; }
    }
}