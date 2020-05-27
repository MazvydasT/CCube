using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace CCube
{
    public static class XMLManager
    {
        public static XDocument GetDocument(string pathToXMLFile)
        {
            try
            {
                return XDocument.Load(pathToXMLFile, LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
            }

            catch (XmlException e)
            {
                var message = e.Message;

                if ((e.SourceUri ?? "").Length > 0)
                    message += $" File: {new Uri(e.SourceUri).LocalPath}";

                ApplicationData.Service.Notifier.Log(message, Notification.NotificationTypes.Error);

                return null;
            }
        }

        public static bool GetAttributeValue(XElement element, string attributeName, Regex validationRegex, string expectedAttributeValue, out string value, bool optional = false)
        { return GetAttributeValue(element, attributeName, validationRegex, new string[] { expectedAttributeValue }, out value, optional); }

        public static bool GetAttributeValue(XElement element, string attributeName, Regex validationRegex, string[] expectedAttributeValues, out string value, bool optional = false)
        {
            value = null;

            var notifier = ApplicationData.Service.Notifier;

            var attribute = (element ?? throw new ArgumentNullException("element")).Attribute(attributeName ?? throw new ArgumentNullException("attributeName"));
            var attributeValue = (string)attribute;

            if (!optional && attribute == null)
            {
                notifier.Log(new ParamFileParsingNotification(element, attributeName));
                return false;
            }

            else if (attribute != null && validationRegex != null && !validationRegex.IsMatch(attributeValue))
            {
                notifier.Log(new ParamFileParsingNotification(attribute, expectedAttributeValues));
                return false;
            }

            value = attributeValue;
            return true;
        }
    }
}
