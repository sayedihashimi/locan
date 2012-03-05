namespace Locan.Website.Models.Data {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class User {
        public Guid Id { get; set; }
        public string ApiKey { get; set; }
        public string Email { get; set; }
    }
}