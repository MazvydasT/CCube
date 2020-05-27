using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace CCube
{
    public class ParamFileParsingNotification : Notification
    {
        public ParamFileParsingNotification(XElement element, string attributeName)
            : base(GenerateMissingAttributeMessage(element, attributeName), NotificationTypes.Error) { }

        public ParamFileParsingNotification(XAttribute attribute, string[] expectedAttributeValues)
            : base(GenerateWrongAttributeValueMessage(attribute, expectedAttributeValues), NotificationTypes.Error) { }

        public ParamFileParsingNotification(XAttribute attribute, string expectedAttributeValue)
            : this(attribute, new string[] { expectedAttributeValue }) { }

        private static string GenerateWrongAttributeValueMessage(XAttribute attribute, string[] expectedAttributeValues)
        {
            var message = $"Attribute '{attribute.Name}' value '{attribute.Value}' does not match expectation of {string.Join(" or ", expectedAttributeValues.Select(v => $"'{v ?? ""}'"))}.";

            message = AppendLineData(attribute, message);
            message = AppendSourceFilePath(attribute, message);

            return message;
        }

        private static string GenerateMissingAttributeMessage(XElement element, string attributeName)
        {
            var message = $"Expected '{attributeName}' attribute was not found within {element.Name} element.";

            message = AppendLineData(element, message);
            message = AppendSourceFilePath(element, message);

            return message;
        }

        private static string AppendLineData(XObject xObject, string message = null)
        {
            IXmlLineInfo lineInfo = xObject ?? throw new ArgumentNullException("xObject");
            if (lineInfo.HasLineInfo())
            {
                var lineData = $"Line: {lineInfo.LineNumber}, position: {lineInfo.LinePosition}.";

                message = message == null ? lineData : $"{message} {lineData}";
            }

            return message;
        }

        private static string AppendSourceFilePath(XObject xObject, string message = null)
        {
            var baseUri = (xObject ?? throw new ArgumentNullException("xObject")).Document.BaseUri;
            if (baseUri.Length > 0)
            {
                var sourceFilePath = $"File: '{new Uri(baseUri).LocalPath}'.";
                message = message == null ? sourceFilePath : $"{message} {sourceFilePath}";
            }

            return message;
        }
    }
}