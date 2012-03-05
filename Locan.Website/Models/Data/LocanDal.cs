namespace Locan.Website.Models.Data {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Linq.Expressions;
    using Locan.Translate.Common.Extensions;
    using Locan.Translate.Common;
    using Locan.Translate.IO;

    public class LocanDal {

        #region User related methods
        public IList<User> GetUsers(Expression<Func<User, bool>> predicate = null) {
            if (predicate == null) {
                predicate = Const<User>.LinqExpression.LinqExpressionFuncAlwaysTrue;
            }

            using (LocanDataContext ctx = this.GetDbContext()) {
                IList<User> result = (from u in ctx.Users
                                      select u)
                                    .Where(predicate)
                                    .ToList();

                return result;
            }
        }

        public User GetUniqueUser(Expression<Func<User, bool>> predicate, bool required) {
            if (predicate == null) { throw new ArgumentNullException("predicate"); }

            using (LocanDataContext ctx = this.GetDbContext()) {
                User result = null;
                var query = (from u in ctx.Users
                             select u)
                              .Where(predicate);

                if (required) {
                    result = query.Single();
                }
                else {
                    result = query.SingleOrDefault();
                }

                return result;
            }
        }

        public User AddUser(User user) {
            if (user == null) { throw new ArgumentNullException("user"); }
            using (LocanDataContext ctx = this.GetDbContext()) {
                // make sure that the api key is not already in use
                User existingUser = this.GetUniqueUser(u => u.ApiKey == user.ApiKey, false);
                if (existingUser != null) {
                    string message = string.Format("The ApiKey [{0}] is already being used by another user", user.ApiKey);
                    throw new ApiKeyAlreadyInUseException(message);
                }

                user.Id = Guid.NewGuid();
                user = ctx.Users.Add(user);
                ctx.SaveChanges();

                return user;
            }
        }

        public User UpdateUser(User user) {
            if (user == null) { throw new ArgumentNullException("user"); }
            using (LocanDataContext ctx = this.GetDbContext()) {
                User existingUser = this.GetUniqueUser(u => u.Id == user.Id, true);
                existingUser.ApiKey = user.ApiKey;
                existingUser.Email = user.Email;
                return existingUser;
            }
        }

        public User GetUserByFileId(Guid fileId) {
            using (LocanDataContext ctx = this.GetDbContext()) {
                User user = (from f in ctx.UserFiles
                             from u in ctx.Users
                             where f.Id == fileId
                             where u.Id == f.UserId
                             select u).SingleOrDefault();
                if (user == null) {
                    string message = string.Format("Unable to find user associated with File ID: {0}", fileId);
                    throw new MissingRequiredValueException(message);
                }

                return user;
            }
        }
        #endregion

        public Guid GetOrCreateIdForFile(Guid userId, string filename, string projectName) {
            if (string.IsNullOrWhiteSpace(filename)) { throw new ArgumentNullException("filename"); }
            if (string.IsNullOrWhiteSpace(projectName)) { throw new ArgumentNullException("projectName"); }

            // see if there is already an existing one for this file, if not create a new ID
            using (LocanDataContext ctx = this.GetDbContext()) {
                UserFile existingFile = (from uf in ctx.UserFiles
                                         where uf.UserId == userId
                                         where string.Compare(filename, uf.Filename, StringComparison.OrdinalIgnoreCase) == 0
                                         where string.Compare(projectName, uf.ProjectName, StringComparison.OrdinalIgnoreCase) == 0
                                         select uf).SingleOrDefault();
                if (existingFile == null) {
                    existingFile = new UserFile {
                        Id = Guid.NewGuid(),
                        ProjectName = projectName,
                        Filename = filename,
                        UserId = userId
                    };
                    ctx.UserFiles.Add(existingFile);
                    ctx.SaveChanges();
                }

                return existingFile.Id;
            }
        }

        public void DeleteAllPhrasesfor(Guid fileId) {
            using (LocanDataContext ctx = this.GetDbContext()) {
                var phrases = from p in ctx.Phrases
                              where p.FileId == fileId
                              select p;

                foreach (Phrase phrase in phrases) {
                    ctx.Phrases.Remove(phrase);
                }
                ctx.SaveChanges();
            }
        }

        public void AddPhrasesToFile(Guid fileId, IList<LocanRow> rows) {
            if (rows == null) { throw new ArgumentNullException("rows"); }
            using (LocanDataContext ctx = this.GetDbContext()) {
                rows.ToList().ForEach(r => {
                    ctx.Phrases.Add(new Phrase {
                        Id = Guid.NewGuid(),
                        TranslateKey = r.Id,
                        FileId = fileId,
                        StringToTranslate = r.StringToTranslate,
                    });
                });

                ctx.SaveChanges();
            }
        }

        public IList<Phrase> GetPhrases(Expression<Func<Phrase, bool>> predicate) {
            if (predicate == null) { throw new ArgumentNullException("predicate"); }

            using (LocanDataContext ctx = this.GetDbContext()) {
                var phrases = (from p in ctx.Phrases
                               select p)
                              .Where(predicate)
                              .ToList();

                return phrases;
            }
        }

        private LocanDataContext GetDbContext() {
            // TODO: Should we always be creating a new ctx?
            return new LocanDataContext();
        }
    }
}