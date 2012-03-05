using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Locan.Website.Models
{
    public class TranslationItem
    {
        public string ID { get; set; }
        public string ValueFrom { get; set; }
        public string ValueTo { get; set; }
        public string Comment { get; set; }
    }
}