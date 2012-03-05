namespace Locan.Website.Models.Data {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class Phrase {
        public Guid Id { get; set; }
        public Guid FileId { get; set; }
        public string TranslateKey { get; set; }
        public string StringToTranslate { get; set; }
        public string Comment { get; set; }
    }
}