using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace CCube
{
    public static class Utils
    {
        public static Dispatcher GUIDispatcher { get; set; }

        public static void AddInputsFromParamsXML(IEnumerable<string> pathsToParamsXMLFile, bool append = false)
        {
            var applicationDataService = ApplicationData.Service;

            uint count = append ? (uint)applicationDataService.Inputs.Count() : 0;

            var newInputs = pathsToParamsXMLFile.Select(pathToParamsXMLFile => (XMLManager.GetDocument(pathToParamsXMLFile) ?? new XDocument()).Descendants("CCCall")
                .Select(ccCallElement => new { ccCallObject = CreateCCCall(ccCallElement), ccCallElement })
                .Where(pair => pair.ccCallObject != null)
                .Select(pair =>
                {
                    var ccCallObject = pair.ccCallObject;

                    var ccCommandParameters = new CCCommandParameters(ccCallObject.ProjectId, ccCallObject.NodeExternalId, ccCallObject?.CheckInOptions.Version)
                    {
                        Incremental = ccCallObject.Incremental,
                        ThreeDMapping = ccCallObject.Skip3Dmapping == null ? null : !ccCallObject.Skip3Dmapping,
                        Comment = ccCallObject?.CheckInOptions.Comment
                    };

                    var ccCallElement = pair.ccCallElement;

                    XMLManager.GetAttributeValue(ccCallElement, "chunkName", null, (string)null, out string chunkName, true);

                    var input = new Input(++count, pathToParamsXMLFile, ccCommandParameters)
                    {
                        ChunkName = chunkName,
                        XMLElement = ccCallElement
                    };

                    input.AddIteration();

                    return input;
                })).SelectMany(s => s).ToArray();

            if (append)
            {
                applicationDataService.Inputs = applicationDataService.Inputs.Concat(newInputs).ToArray();
            }

            else
            {
                applicationDataService.Inputs = newInputs;
            }

            var statsService = applicationDataService.Stats;

            statsService.Reset();

            statsService.InputsTotal = count;
        }

        public static void WriteParamsXML(IEnumerable<Input> inputs, string xmlPath)
        {
            if (!(inputs?.Any() ?? false)) return;

            using (var fileStream = new FileStream(xmlPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            using (var streamWriter = new StreamWriter(fileStream))
            using (var xmlWriter = XmlWriter.Create(streamWriter, new XmlWriterSettings()
            {
                Indent = true,
                NewLineChars = "\n",
                IndentChars = "\t"
            }))
            {
                try
                {
                    xmlWriter.WriteStartElement("CCUpdateCalls");

                    foreach (var input in inputs)
                    {
                        input.XMLElement.WriteTo(xmlWriter);
                    }

                    xmlWriter.WriteEndElement();
                }

                finally
                {
                    fileStream?.SetLength(fileStream?.Position ?? 0); // Trunkates existing file
                }
            }
        }

        public static CCCall CreateCCCall(XElement ccCallElement)
        {
            var regexNonEmptyString = new Regex(@"^[^\s]+$", RegexOptions.Compiled);
            var regexPositiveNumber = new Regex(@"^\d+$", RegexOptions.Compiled);
            var regexYN = new Regex("^[ny]$", RegexOptions.Compiled);

            var expectedValueYN = new string[] { "y", "n" };

            var dataErrorFound = false;

            dataErrorFound |= !XMLManager.GetAttributeValue(ccCallElement ?? throw new ArgumentNullException("ccCallElement"), "projectID", regexPositiveNumber, "should be positive number", out string projectId);
            dataErrorFound |= !XMLManager.GetAttributeValue(ccCallElement, "nodeExternalId", regexNonEmptyString, "should not be blank", out string nodeExternalId);
            dataErrorFound |= !XMLManager.GetAttributeValue(ccCallElement, "incremental", regexYN, expectedValueYN, out string incremental, true);
            dataErrorFound |= !XMLManager.GetAttributeValue(ccCallElement, "skip3Dmapping", regexYN, expectedValueYN, out string skip3Dmapping, true);

            var checkInOptionsElement = ccCallElement.Element("CheckInOptions");

            CheckInOptions checkInOptionsObject = null;
            CCCall ccCallObject = null;

            if (checkInOptionsElement != null)
            {
                dataErrorFound |= !XMLManager.GetAttributeValue(checkInOptionsElement, "checkIn", regexYN, expectedValueYN, out string checkIn);
                dataErrorFound |= !XMLManager.GetAttributeValue(checkInOptionsElement, "checkInAsNew", regexYN, expectedValueYN, out string checkInAsNew, true);
                dataErrorFound |= !XMLManager.GetAttributeValue(checkInOptionsElement, "version", regexNonEmptyString, "should not be blank", out string version, checkInAsNew != "y");
                dataErrorFound |= !XMLManager.GetAttributeValue(checkInOptionsElement, "comment", null, (string)null, out string comment, true);
                dataErrorFound |= !XMLManager.GetAttributeValue(checkInOptionsElement, "keepCheckOut", regexYN, expectedValueYN, out string keepCheckOut, true);

                if (!dataErrorFound)
                {
                    checkInOptionsObject = checkInAsNew == "y" ? new CheckInOptions(version) : new CheckInOptions(checkIn == "y");
                    checkInOptionsObject.Comment = comment;
                    checkInOptionsObject.KeepCheckOut = keepCheckOut == null ? null : (bool?)(keepCheckOut == "y");
                }
            }

            if (!dataErrorFound)
            {
                ccCallObject = new CCCall(long.Parse(projectId), nodeExternalId)
                {
                    Incremental = incremental == null ? null : (bool?)(incremental == "y"),
                    Skip3Dmapping = skip3Dmapping == null ? null : (bool?)(skip3Dmapping == "y"),
                    CheckInOptions = checkInOptionsObject
                };
            }

            return ccCallObject;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T t)
                    {
                        yield return t;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static string SecureStringToString(SecureString secureString) => new NetworkCredential(string.Empty, secureString).Password;
    }
}