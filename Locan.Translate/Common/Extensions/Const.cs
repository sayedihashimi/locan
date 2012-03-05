namespace Locan.Translate.Common.Extensions {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Linq.Expressions;

    public class Const<T> {
        public static class Predicate {
            public static Predicate<T> AlwaysTrue {
                get {
                    return x => true;
                }
            }

            public static Predicate<T> AlwaysFalse {
                get {
                    return x => false;
                }
            }
        }

        public class LinqExpression {
            public static Expression<Func<T, bool>> LinqExpressionFuncAlwaysTrue {
                get {
                    return x => true;
                }
            }
        }
    }
}
