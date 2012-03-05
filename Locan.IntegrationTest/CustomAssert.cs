namespace Locan.IntegrationTest {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
using Locan.Translate;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class CustomAssert {

        public static void AreEqual(ILanguage lang01, ILanguage lang02) {
            if (lang01 == null) { throw new ArgumentNullException("lang01"); }
            if (lang02 == null) { throw new ArgumentNullException("lang02"); }

            Assert.AreEqual(lang01.Language, lang02.Language);

        }

        // The action should throw an exception if they are not equal. 
        // This is an action instead of a Func because there should be no return type.
        // This is the same behavior of these AreEqual methods.
        public static void AreEqual<T>(IEnumerable<T> firstItemList, IEnumerable<T> secondItemList, Action<T, T> areEqual) {
            if (firstItemList == null) { throw new System.ArgumentNullException("firstItemList"); }
            if (secondItemList == null) { throw new System.ArgumentNullException("secondItemList"); }

            object toListLock = new object();

            List<T> firstList = null;
            List<T> secondList = null;
            // Prevent any deferred exec issues
            lock (toListLock) {
                firstList = firstItemList.ToList();
                secondList = secondItemList.ToList();
            }

            Assert.AreEqual(firstList.Count, secondList.Count);

            for (int i = 0; i < firstList.Count; i++) {
                areEqual(firstList[0], secondList[0]);
            }
        }
    }
}
