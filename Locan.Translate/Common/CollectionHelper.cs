namespace Locan.Translate.Common {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;


    public interface ICollectionHelper {
        IDictionary<string, object> ConvertToDictionary(object obj);
    }

    public class CollectionHelper : ICollectionHelper {
        private static CollectionHelper instance = new CollectionHelper();

        // Singleton
        private CollectionHelper() { }

        public static ICollectionHelper Instance {
            get {
                return instance;
            }
        }

        /// <summary>
        /// This takes an object and converts it into a dictionary. The names of the properties on the object
        /// will be the keys on the dictionary and the value of the properties will be the
        /// values of the associated property.
        /// This is especially useful for dealing with anonymous objects.
        /// </summary>
        public IDictionary<string, object> ConvertToDictionary(object obj) {
            if (obj == null) { throw new ArgumentNullException("obj"); }
            if (obj is IDictionary<string, object>)
                return (IDictionary<string, object>)obj;

            IDictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj)) {
                dictionary.Add(descriptor.Name, descriptor.GetValue(obj));
            }
            return dictionary;
        }
    }
}
