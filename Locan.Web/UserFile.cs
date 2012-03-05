namespace Locan.Web {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.ComponentModel.DataAnnotations;

    public class UserFile {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ProjectName { get; set; }
        public string Filename { get; set; }
    }
}