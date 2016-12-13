////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationBaseMtLibrary
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.XPath;

    public static class CheckRunDataHandles
    {
        private static char MachineNameDelimiterChar = '.';

        /// <summary>
        /// Creates an identifier string followed by the origin machine name (from where the MetaAutomation service is called to start the check or sub-check) 
        /// and the destination machine name (where the check or subcheck actually runs). Stripped of the machine names, the identifer has 3 uses: It is the name of the named semaphore on the origin machine ,
        /// and the identifier of both the CheckRunLaunch object and the CheckRunArtifact object as they are stored temporarily by the MetaAutomation service.
        /// The machine names are appended to the end of the identifier and always passed around as such to identify the OS instance or machine on which the named semaphore exists, i.e. 
        /// the origin machine, and the name of the OS instance or machine on which the CheckRunLaunch and CheckRunArtifact objects are stored in the MetaAutomation service,
        /// i.e. the destination machine.
        /// </summary>
        /// <param name="xDocumentCRLorCRA"></param>
        /// <returns>The identifier string for the crx data object, and the origin and destination machine names inline with '.' delimiters</returns>
        public static string CreateIdFromCrxXDocument(XDocument xDocumentCRLorCRA)
        {
            const int MaxNameLength = 260;
            StringBuilder resultBuilder = new StringBuilder();

            // Get these from XElement "CheckRunData" although root element is different for CRL as for CRA
            XElement elementWithDataChildren = xDocumentCRLorCRA.Root.Element(DataStringConstants.ElementNames.CheckRunData);
            string checkMethodRunGuid = null;
            string originMachineName = null;
            string destinationMachineName = null;
            string checkMethodGuid = null;
            string reserved_SubCheckCounter = null;

            foreach (XElement el in elementWithDataChildren.Elements(DataStringConstants.ElementNames.DataElement))
            {
                switch (el.Attribute(DataStringConstants.AttributeNames.Name).Value)
                {
                    case DataStringConstants.NameAttributeValues.CheckRunGuid:
                        {
                            checkMethodRunGuid = el.Attribute(DataStringConstants.AttributeNames.Value).Value;
                            break;
                        }
                    case DataStringConstants.NameAttributeValues.DestinationMachine:
                        {
                            destinationMachineName = el.Attribute(DataStringConstants.AttributeNames.Value).Value;
                            break;
                        }
                    case DataStringConstants.NameAttributeValues.CheckMethodGuid:
                        {
                            checkMethodGuid = el.Attribute(DataStringConstants.AttributeNames.Value).Value;
                            break;
                        }
                    case DataStringConstants.NameAttributeValues.OriginMachine:
                        {
                            originMachineName = el.Attribute(DataStringConstants.AttributeNames.Value).Value;
                            break;
                        }
                    case DataStringConstants.NameAttributeValues.Reserved_SubCheckMap:
                        {
                            reserved_SubCheckCounter = el.Attribute(DataStringConstants.AttributeNames.Value).Value;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            if (string.IsNullOrWhiteSpace(checkMethodRunGuid))
            {
                throw new CheckInfrastructureBaseException(string.Format("The check run data element named '{0}' for checkMethodRunGuid is missing.", DataStringConstants.NameAttributeValues.CheckRunGuid));
            }

            if (string.IsNullOrWhiteSpace(destinationMachineName))
            {
                throw new CheckInfrastructureBaseException(string.Format("The check run data element named '{0}' for destinationMachineName is missing.", DataStringConstants.NameAttributeValues.DestinationMachine));
            }

            if (string.IsNullOrWhiteSpace(checkMethodGuid))
            {
                throw new CheckInfrastructureBaseException(string.Format("The check run data element named '{0}' for filePathAndName is missing.", DataStringConstants.NameAttributeValues.CheckMethodGuid));
            }

            if (string.IsNullOrWhiteSpace(originMachineName))
            {
                throw new CheckInfrastructureBaseException(string.Format("The check run data element named '{0}' for filePathAndName is missing.", DataStringConstants.NameAttributeValues.OriginMachine));
            }

            if (string.IsNullOrWhiteSpace(reserved_SubCheckCounter))
            {
                throw new CheckInfrastructureBaseException(string.Format("The check run data element named '{0}' for filePathAndName is missing.", DataStringConstants.NameAttributeValues.Reserved_SubCheckMap));
            }

            resultBuilder.Append(checkMethodRunGuid);
            resultBuilder.Append(originMachineName);
            resultBuilder.Append(reserved_SubCheckCounter);
            resultBuilder.Append(destinationMachineName);
            resultBuilder.Append(checkMethodGuid);
            char[] charsForLabel = resultBuilder.ToString().ToCharArray();
            resultBuilder.Clear();

            // Limit the string to numbers and letters, to make it safe and unambiguous
            var charEnum = from char c in charsForLabel where Char.IsLetterOrDigit(c) select c;

            foreach (char b in charEnum)
            {
                resultBuilder.Append(b);
            }

            // Add the scope qualifier. This is needed for semaphores that are global to a Windows instance
            string resultLabel = @"Global\" + resultBuilder.ToString().ToLower();

            if (resultLabel.Length > MaxNameLength)
            {
                resultLabel = resultLabel.Remove(MaxNameLength);
            }

            // add machine name here for origin (the machine from where the check is launched) and the destination (the machine that runs the check or subcheck)
            // assume that origin and destination are on the same domain, or they are both not on a domain
            // Windows OS instance name rules:
            //  max 15 characters
            //  no spaces, /, \, * , . " @
            //  case insensitive, so use upper case
            // So: use period "." as delimiter. This is stripped off at use as a key or semaphore name, but otherwise can be used as a destination name for storage.
            // Format:
            // <resultLabel>.origincomputer.destinationcomputer
            // This allows storage issues to be resolved with the question of where an item is stored, and allows distribution of storage overhead over OS instances.

            resultLabel += MachineNameDelimiterChar + originMachineName.ToUpper() + MachineNameDelimiterChar + destinationMachineName.ToUpper();

            return resultLabel;
        }

        /// <summary>
        /// The check run identifier is usually appended with the upper-case machine names for origin and destination machine of the check run request, delimited with '.'.
        /// </summary>
        /// <param name="uniqueLabelForCheckRunSegment">Identifier for the check run, optionally with machine names at the end.</param>
        /// <returns>Identifier for the check run, without the machine names at the end.</returns>
        public static string StripIdOfMachineNames(string uniqueLabelForCheckRunSegment)
        {
            if (uniqueLabelForCheckRunSegment == null) throw new CheckInfrastructureBaseException("StripIdOfMachineNames: the passed string argument is null.");
            if (uniqueLabelForCheckRunSegment.Length == 0) throw new CheckInfrastructureBaseException("StripIdOfMachineNames: the passed string argument is zero-length.");

            string result = uniqueLabelForCheckRunSegment;

            int firstDelimiterIndex = uniqueLabelForCheckRunSegment.IndexOf(MachineNameDelimiterChar);

            if (firstDelimiterIndex > 0)
            {
                result = uniqueLabelForCheckRunSegment.Remove(firstDelimiterIndex);
            }

            return result;
        }

        /// <summary>
        /// Returns the origin machine name in upper case.
        /// </summary>
        /// <param name="uniqueLabelForCheckRunSegment">identifier for the check run</param>
        /// <returns>origin machine name in upper case</returns>
        public static string GetOriginMachineName(string uniqueLabelForCheckRunSegment)
        {
            if (uniqueLabelForCheckRunSegment == null) throw new CheckInfrastructureBaseException("GetOriginMachineName: the passed string argument is null.");
            if (uniqueLabelForCheckRunSegment.Length == 0) throw new CheckInfrastructureBaseException("GetOriginMachineName: the passed string argument is zero-length.");

            int lastDelimiterIndex = uniqueLabelForCheckRunSegment.LastIndexOf(MachineNameDelimiterChar);

            if (lastDelimiterIndex == -1)
            {
                throw new CheckInfrastructureBaseException(string.Format("The delimiter character '{0}' was not found in the uniqueLabelForCheckRunSegment string '{1}'", MachineNameDelimiterChar, uniqueLabelForCheckRunSegment));
            }

            int firstDelimiterIndex = uniqueLabelForCheckRunSegment.IndexOf(MachineNameDelimiterChar);

            if (firstDelimiterIndex == lastDelimiterIndex)
            {
                throw new CheckInfrastructureBaseException(string.Format("There is only one delimiter character '{0}' was in the uniqueLabelForCheckRunSegment string '{1}'", MachineNameDelimiterChar, uniqueLabelForCheckRunSegment));
            }

            if (firstDelimiterIndex == lastDelimiterIndex - 1)
            {
                throw new CheckInfrastructureBaseException(string.Format("There is no origin machine named in the uniqueLabelForCheckRunSegment string '{0}'", uniqueLabelForCheckRunSegment));
            }

            int originMachineNameLength = lastDelimiterIndex - firstDelimiterIndex - 1;
            return uniqueLabelForCheckRunSegment.Substring(firstDelimiterIndex + 1, originMachineNameLength).ToUpper();
        }

        /// <summary>
        /// Returns the destination machine name in upper case.
        /// </summary>
        /// <param name="uniqueLabelForCheckRunSegment">identifier for the check run</param>
        /// <returns>destination machine name in upper case</returns>
        public static string GetDestinationMachineName(string uniqueLabelForCheckRunSegment)
        {
            if (uniqueLabelForCheckRunSegment == null) throw new CheckInfrastructureBaseException("GetDestinationMachineName: the passed string argument is null.");
            if (uniqueLabelForCheckRunSegment.Length == 0) throw new CheckInfrastructureBaseException("GetDestinationMachineName: the passed string argument is zero-length.");

            int lastDelimiterIndex = uniqueLabelForCheckRunSegment.LastIndexOf(MachineNameDelimiterChar);

            if (lastDelimiterIndex == -1)
            {
                throw new CheckInfrastructureBaseException(string.Format("The delimiter character '{0}' was not found in the uniqueLabelForCheckRunSegment string '{1}'", MachineNameDelimiterChar, uniqueLabelForCheckRunSegment));
            }

            int totalLength = uniqueLabelForCheckRunSegment.Length;
            return uniqueLabelForCheckRunSegment.Substring(lastDelimiterIndex + 1, totalLength - lastDelimiterIndex - 1).ToUpper();
        }
    }
}
