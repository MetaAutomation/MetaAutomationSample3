////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationBaseMtLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;

    /// <summary>
    /// This class encapsulates all data validations.
    /// </summary>
    public class DataValidation
    {
        #region publicMethods
        /// <summary>
        /// Singleton pattern to manage initialization
        /// </summary>
        public static DataValidation Instance
        {
            get
            {
                lock (m_LockObject)
                {
                    if (DataValidation.m_Instance == null)
                    {
                        DataValidation.m_Instance = new DataValidation();
                    }
                }

                return m_Instance;
            }
        }

        /// <summary>
        /// Creates an XDocument from the passed string, and validates it by XSD schema and code validations according to required properties of a CheckRunLaunch object.
        /// </summary>
        /// <param name="checkRunLaunch"></param>
        /// <returns>XDocument that is the CheckRunLaunch object</returns>
        public XDocument ValidateCheckRunLaunchIntoXDocument(string checkRunLaunch)
        {
            return this.LoadAndValidateXml(checkRunLaunch, CheckRunDocumentType.CheckRunLaunch, "Validation of CheckRunLaunch XML failed.");
        }

        /// <summary>
        /// Creates an XDocument from the passed string, and validates it by XSD schema and code validations according to required properties of a CheckRunArtifact object.
        /// </summary>
        /// <param name="checkRunLaunch"></param>
        /// <returns>XDocument that is the CheckRunArtifact object</returns>
        public XDocument ValidateCheckRunArtifactIntoXDocument(string checkRunArtifact)
        {
            return this.LoadAndValidateXml(checkRunArtifact, CheckRunDocumentType.CheckRunArtifact, "Validation of CheckRunArtifact XML failed.");
        }

        /// <summary>
        /// Validates the check run launch (CRL) XDocument, and throws an exception on failure with advisory information
        /// </summary>
        /// <param name="crl">The check run launch (CRL) XDocument</param>
        public void ValidateCheckRunLaunch(XDocument crl)
        {
            this.ValidateBySchema(crl, CheckRunDocumentType.CheckRunLaunch, "ValidateCheckRunLaunch failed on xsd schema validation");
            this.CheckRequiredDataElements(crl, CheckRunDocumentType.CheckRunLaunch, "ValidateCheckRunLaunch failed on checking for required DataElement elements");
        }

        /// <summary>
        /// Validates the check run launch (CRA) XDocument, and throws an exception on failure with advisory information
        /// </summary>
        /// <param name="cra">The check run artifact (CRA) XDocument</param>
        public void ValidateCheckRunArtifact(XDocument cra)
        {
            this.ValidateBySchema(cra, CheckRunDocumentType.CheckRunArtifact, "ValidateCheckRunArtifact failed on xsd schema validation");
            this.CheckRequiredDataElements(cra, CheckRunDocumentType.CheckRunArtifact, "ValidateCheckRunArtifact failed on checking for required DataElement elements");
        }

        #endregion //publicMethods
        #region privateStuff
        private static object m_LockObject = new object();
        private static DataValidation m_Instance = null;
        private ReadOnlyCollection<string> m_CraChecklistReadOnly = null;
        private ReadOnlyCollection<string> m_CrlChecklistReadOnly = null;

        private DataValidation()
        {
            List<string> checkRunLaunchCheckList = new List<string>();
            List<string> checkRunArtifactCheckList = new List<string>();

            // The existence of these names for the name-value pairs in the CheckRunData element is required for both the CheckRunLaunch and the CheckRunArtifact
            checkRunLaunchCheckList.Add(DataStringConstants.NameAttributeValues.PathAndFileToRunner);
            checkRunLaunchCheckList.Add(DataStringConstants.NameAttributeValues.DestinationMachine);
            checkRunLaunchCheckList.Add(DataStringConstants.NameAttributeValues.CheckRunGuid);
            checkRunLaunchCheckList.Add(DataStringConstants.NameAttributeValues.CheckJobSpecGuid);
            checkRunLaunchCheckList.Add(DataStringConstants.NameAttributeValues.CheckJobRunGuid);
            checkRunLaunchCheckList.Add(DataStringConstants.NameAttributeValues.CheckMethodName);
            checkRunLaunchCheckList.Add(DataStringConstants.NameAttributeValues.OriginMachine);
            checkRunLaunchCheckList.Add(DataStringConstants.NameAttributeValues.CheckMethodGuid);
            checkRunLaunchCheckList.Add(DataStringConstants.NameAttributeValues.CheckClientUser);

            checkRunArtifactCheckList.Add(DataStringConstants.NameAttributeValues.PathAndFileToRunner);
            checkRunArtifactCheckList.Add(DataStringConstants.NameAttributeValues.DestinationMachine);
            checkRunArtifactCheckList.Add(DataStringConstants.NameAttributeValues.CheckRunGuid);
            checkRunArtifactCheckList.Add(DataStringConstants.NameAttributeValues.CheckJobSpecGuid);
            checkRunArtifactCheckList.Add(DataStringConstants.NameAttributeValues.CheckJobRunGuid);
            checkRunArtifactCheckList.Add(DataStringConstants.NameAttributeValues.CheckMethodName);
            checkRunArtifactCheckList.Add(DataStringConstants.NameAttributeValues.OriginMachine);
            checkRunArtifactCheckList.Add(DataStringConstants.NameAttributeValues.CheckMethodGuid);
            checkRunArtifactCheckList.Add(DataStringConstants.NameAttributeValues.CheckClientUser);

            // These names are requied for the CheckRunArtifact
            checkRunArtifactCheckList.Add(DataStringConstants.NameAttributeValues.CheckBeginTime);
            checkRunArtifactCheckList.Add(DataStringConstants.NameAttributeValues.CheckEndTime);

            checkRunArtifactCheckList.Sort();
            checkRunLaunchCheckList.Sort();

            this.m_CraChecklistReadOnly = checkRunArtifactCheckList.AsReadOnly();
            this.m_CrlChecklistReadOnly = checkRunLaunchCheckList.AsReadOnly();
        }

        private enum CheckRunDocumentType
        {
            Unitialized = 0,
            CheckRunLaunch = 1,
            CheckRunArtifact = 2,
        }

        private XDocument LoadAndValidateXml(string xml, CheckRunDocumentType documentType, string errorMessage)
        {
            XDocument result = null;
            int marker = -1;

            try
            {
                marker = 0;
                result = XDocFromString(xml);
                marker = 1;
                this.ValidateBySchema(result, documentType, errorMessage);
                marker = 2;
                this.CheckRequiredDataElements(result, documentType, errorMessage);
            }
            catch (Exception ex)
            {
                if (marker >= 0)
                {
                    string message = string.Empty;

                    switch (marker)
                    {
                        case 0:
                            {
                                message = "loading";
                                break;
                            }

                        case 1:
                            {
                                message = "validating";
                                break;
                            }

                        case 2:
                            {
                                message = "checking for specific DataElement elements";
                                break;
                            }

                        default:
                            {
                                break;
                            }
                    }

                    errorMessage += string.Format("'{0}' Failed on {1}. '{3}'. Input XML '{2}'", errorMessage, message, xml, ex);
                    throw new CheckInfrastructureBaseException(errorMessage, ex);
                }
                else
                {
                    errorMessage += string.Format("LoadAndValidateXml failed. '{0}.'", ex);
                    throw new CheckInfrastructureBaseException(errorMessage, ex);
                }
            }

            return result;
        }

        private XDocument XDocFromString(string xml)
        {
            XmlReader xmlDataReader = XmlReader.Create(new StringReader(xml));
            return XDocument.Load(xmlDataReader, LoadOptions.None);
        }

        private void ValidateBySchema(XDocument xDoc, CheckRunDocumentType documentType, string errorMessage)
        {
            XmlSchemaSet schemas = null;

            try
            {
                switch (documentType)
                {
                    case CheckRunDocumentType.CheckRunLaunch:
                        {
                            schemas = DataSchemas.Instance.CheckRunLaunchSchemaSet;
                            break;
                        }

                    case CheckRunDocumentType.CheckRunArtifact:
                        {
                            schemas = DataSchemas.Instance.CheckRunArtifactSchemaSet;
                            break;
                        }

                    default:
                        {
                            throw new CheckInfrastructureBaseException(string.Format("{0} LoadAndValidateXml failed. The received document type enum of '{1}' is not a valid value.", errorMessage, documentType));
                        }
                }

                xDoc.Validate(schemas, null);
            }
            catch (XmlSchemaException ex)
            {
                throw new CheckInfrastructureBaseException(string.Format("Validation failed with schema for '{0}'", documentType), ex);
            }
        }

        private void CheckRequiredDataElements(XDocument xDoc, CheckRunDocumentType documentType, string errorMessage)
        {
            XElement elementWithDataChildren = xDoc.Root.Element(DataStringConstants.ElementNames.CheckRunData);
            ReadOnlyCollection<string> listToCheck = null;

            switch (documentType)
            {
                case CheckRunDocumentType.CheckRunLaunch:
                    {
                        listToCheck = this.m_CrlChecklistReadOnly;
                        break;
                    }

                case CheckRunDocumentType.CheckRunArtifact:
                    {
                        listToCheck = this.m_CraChecklistReadOnly;
                        break;
                    }
            }

            this.VerifyDataElementsExist(elementWithDataChildren, listToCheck);
        }

        private void VerifyDataElementsExist(XElement elementWithDataChildren, ReadOnlyCollection<string> requiredNameAttributeNames)
        {
            IEnumerable<XElement> dataElements = elementWithDataChildren.Elements(DataStringConstants.ElementNames.DataElement);

            List<string> elementsFound = new List<string>();

            foreach (XElement element in dataElements)
            {
                elementsFound.Add(element.Attribute(DataStringConstants.AttributeNames.Name).Value);
            }

            elementsFound.Sort();

            List<string> elementNamesRequiredButNotFound = new List<string>();
            int requiredIndex = 0, foundIndex = 0;
            int requiredCount = requiredNameAttributeNames.Count;
            int foundCount = elementsFound.Count;

            while ((requiredIndex < requiredCount) && (foundIndex < foundCount))
            {
                int compareResult = string.Compare(requiredNameAttributeNames[requiredIndex], elementsFound[foundIndex]);

                if (compareResult == 0)
                {
                    requiredIndex++;
                    foundIndex++;
                }
                else if (compareResult < 0)
                {
                    elementNamesRequiredButNotFound.Add(requiredNameAttributeNames[requiredIndex]);
                    requiredIndex++;
                }
                else if (compareResult > 0)
                {
                    foundIndex++;
                }
            };

            for (; requiredIndex < requiredCount; requiredIndex++)
            {
                elementNamesRequiredButNotFound.Add(requiredNameAttributeNames[requiredIndex]);
            }

            string missingRequiredStrings = string.Empty;

            foreach (string missingString in elementNamesRequiredButNotFound)
            {
                missingRequiredStrings += " " + missingString;
            }

            if (missingRequiredStrings.Length > 0)
            {
                throw new CheckInfrastructureBaseException(string.Format("The XML is missing DataElements with names '{0}.'", missingRequiredStrings));
            }
        }

        #endregion //privateStuff
    }
}
