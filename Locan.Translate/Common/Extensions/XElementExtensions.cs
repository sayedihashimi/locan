namespace Locan.Translate.Common.Extensions {
    using System.Xml.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class XElementExtensions {
        public static string SafeGetAttributeValue(this XElement element, string attributeName, string defaultValue = null) {
            if (element == null) { throw new System.ArgumentNullException("element"); }
            if (string.IsNullOrEmpty(attributeName)) { throw new System.ArgumentNullException("attributeName"); }

            string result = null;
            XAttribute attribute = element.Attribute(attributeName);
            if (attribute != null) {
                result = attribute.Value;
            }

            if (!string.IsNullOrWhiteSpace(defaultValue) && string.IsNullOrEmpty(result)) {
                result = defaultValue;
            }

            return result;
        }

        public static string GetRequiredAttributeValue(this XElement element, string attributeName) {
            string result = SafeGetAttributeValue(element, attributeName);

            if (string.IsNullOrEmpty(result)) {
                string message = string.Format(
                    "Missing [{0}] attribute on [{1}] element.",
                    element.Name,
                    attributeName);
                throw new MissingRequiredValueException(message);
            }

            return result;
        }

    }
}
