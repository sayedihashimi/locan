namespace Locan.Website.Models.Data {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Data.Entity;

    public class LocanDataContext : DbContext {
        public DbSet<User> Users { get; set; }
        public DbSet<UserFile> UserFiles { get; set; }
        public DbSet<Phrase> Phrases { get; set; }

        public class DbInitalizer : DropCreateDatabaseAlways<LocanDataContext> {
            public DbInitalizer() {

            }

            protected override void Seed(LocanDataContext context) {
                base.Seed(context);
            }
        }
    }
}