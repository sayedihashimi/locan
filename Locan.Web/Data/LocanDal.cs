namespace Locan.Web.Data {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Linq.Expressions;
    using Locan.Translate.Common.Extensions;
    using Locan.Translate.Common;

    public class LocanDal {

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

        public User GetUniqueUser(Expression<Func<User, bool>> predicate,bool required) {
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
                    string message = string.Format("The ApiKey [{0}] is already being used by another user",user.ApiKey);
                    throw new ApiKeyAlreadyInUseException(message);
                }

                user.Id = Guid.NewGuid();
                user = ctx.Users.Add(user);
                ctx.SaveChanges();

                return user;
            }
        }

        private LocanDataContext GetDbContext() {
            // TODO: Should we always be creating a new ctx?
            return new LocanDataContext();
        }
    }
}