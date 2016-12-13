////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationLauncherMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using MetaAutomationClientMtLibrary;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;

    /// <summary>
    /// This class is ONLY to be used to create a check run launch object for the main check, not a sub-check.
    /// </summary>
    public class CheckRunLaunchCreator
    {
        #region publicMembers

        public CheckRunLaunchCreator()
        {
        }

        public CheckRunLaunchCreator(Dictionary<string, string> dataMembers)
        {
            m_CrlDataElements = dataMembers;
        }


        public CheckRunLaunchCreator(Dictionary<string, string> dataMembers, Dictionary<string, string> customData)
        {
            m_CrlDataElements = dataMembers;
            m_CustomDataElements = customData;
        }

        public Dictionary<string, string> DataMembers
        {
            get
            {
                return m_CrlDataElements;
            }
            set
            {
                m_CrlDataElements = value;
            }
        }

        public Dictionary<string, string> CustomDataMembers
        {
            get
            {
                return m_CustomDataElements;
            }
            set
            {
                m_CustomDataElements = value;
            }
        }


        /// <summary>
        /// This method is ONLY to be used for launching checks, not sub-checks.
        /// It creates a CheckRunLaunch object from the passed CheckRunArtifact object, the data members supplied,
        /// and any custom data.
        /// All custom data from the passed CheckRunArtifact is removed.
        /// </summary>
        /// <param name="lastArtifactForThisCheckMethod"></param>
        /// <returns></returns>
        public XDocument CreateCheckRunLaunch(XDocument lastArtifactForThisCheckMethod = null)
        {
            XDocument crlXDocumentResult = null;

            if (m_CrlDataElements == null) throw new CheckInfrastructureClientException("CreateCheckRunLaunch: the DataMember Dictionary is null.");
            if (m_CrlDataElements.Count == 0) throw new CheckInfrastructureClientException("CreateCheckRunLaunch: the DataMember Dictionary has zero entries.");

            if (m_CrlDataElements[DataStringConstants.NameAttributeValues.CheckRunGuid] == null)
            {
                throw new CheckInfrastructureClientException(string.Format("CreateCheckRunLaunch: the DataMember Dictionary must have a '{0}' entry.", DataStringConstants.NameAttributeValues.CheckRunGuid));
            }

            try
            {
                XElement checkRunDataElement = null;

                if (lastArtifactForThisCheckMethod == null)
                {
                    crlXDocumentResult = XDocument.Parse(@"<?xml version='1.0' encoding='utf-8'?><CheckRunLaunch><CheckRunData/><CheckCustomData/><CheckFailData/><CompleteCheckStepInfo/></CheckRunLaunch>");
                    checkRunDataElement = crlXDocumentResult.Root.Element(DataStringConstants.ElementNames.CheckRunData);

                    foreach (KeyValuePair<string, string> nameValuePair in m_CrlDataElements)
                    {
                        // Add the name/value pairs
                        checkRunDataElement.Add(
                            new XElement(DataStringConstants.ElementNames.DataElement,
                                new XAttribute[] {
                                new XAttribute(DataStringConstants.AttributeNames.Name, nameValuePair.Key),
                                new XAttribute(DataStringConstants.AttributeNames.Value, nameValuePair.Value)
                            }));
                    }
                }
                else
                {
                    // Validate CRA
                    DataValidation.Instance.ValidateCheckRunArtifact(lastArtifactForThisCheckMethod);

                    // Transform CRA to CRL
                    CheckRunTransforms checkRunTransforms = new CheckRunTransforms();
                    crlXDocumentResult = checkRunTransforms.ConvertCheckRunArtifactToCheckRunLaunch(lastArtifactForThisCheckMethod);

                    // Populate or overwrite with the supplied data, and for DataElement elements that are not specified, give them empty-string values
                    //  to prevent spurious data passing from the previous CRA to the current CRL
                    checkRunDataElement = crlXDocumentResult.Root.Element(DataStringConstants.ElementNames.CheckRunData);

                    // Iterate through the DataElement elements in doc order
                    var dataElementIterator = checkRunDataElement.Elements(DataStringConstants.ElementNames.DataElement);

                    foreach (XElement dataElement in dataElementIterator)
                    {
                        XAttribute nameAttribute = dataElement.Attribute(DataStringConstants.AttributeNames.Name);
                        XAttribute valueAttribute = dataElement.Attribute(DataStringConstants.AttributeNames.Value);
                        string name = nameAttribute.Value;

                        if (m_CrlDataElements.ContainsKey(name))
                        {
                            valueAttribute.Value = m_CrlDataElements[name];
                        }
                        else if (
                            (nameAttribute.Value == DataStringConstants.NameAttributeValues.CheckBeginTime)
                            || (nameAttribute.Value == DataStringConstants.NameAttributeValues.CheckEndTime)
#if DEBUG
                            || (nameAttribute.Value == DataStringConstants.NameAttributeValues.CheckObjectStorageKey)
#endif
                            )
                        {
                            // Begin and end times are special cases, to be cleaned out here
                            valueAttribute.Value = string.Empty;
                        }
                    }
                }

                // Initialize Reserved_SubCheckMap in the CRL, for root check and all subchecks
                this.SetCheckCounters(checkRunDataElement);
#if DEBUG
                this.DecrementCountDownToFail(crlXDocumentResult);
#endif
#if DEBUG
                DataValidation.Instance.ValidateCheckRunLaunch(crlXDocumentResult);
#endif

                if ((this.m_CustomDataElements != null) && (this.m_CustomDataElements.Count > 0))
                {
                    XElement customDataElement = crlXDocumentResult.Root.Element(DataStringConstants.ElementNames.CheckCustomData);

                    if (customDataElement == null)
                    {
                        checkRunDataElement.AddAfterSelf(new XElement(DataStringConstants.ElementNames.CheckCustomData));
                        customDataElement = crlXDocumentResult.Root.Element(DataStringConstants.ElementNames.CheckCustomData);
                    }

                    foreach (KeyValuePair<string, string> nameValuePair in m_CustomDataElements)
                    {
                        XElement existingElementForKey = customDataElement.XPathSelectElement(
                            string.Format("{0}[@{1}]",
                            DataStringConstants.ElementNames.DataElement,
                            nameValuePair.Key));

                        if (existingElementForKey == null)
                        {
                            // Add the name/value pairs
                            customDataElement.Add(
                            new XElement(DataStringConstants.ElementNames.DataElement,
                                new XAttribute[] {
                                new XAttribute(DataStringConstants.AttributeNames.Name, nameValuePair.Key),
                                new XAttribute(DataStringConstants.AttributeNames.Value, nameValuePair.Value)
                            }));
                        }
                        else
                        {
                            // update the value
                            XAttribute valueAttribute = existingElementForKey.Attribute(DataStringConstants.AttributeNames.Value);

                            if (valueAttribute == null)
                            {
                                existingElementForKey.Add(new XAttribute(DataStringConstants.AttributeNames.Value, nameValuePair.Value));
                            }
                            else
                            {
                                valueAttribute.Value = nameValuePair.Value;
                            }
                        }
                    }
                }
#if DEBUG
                string id = CheckRunDataHandles.CreateIdFromCrxXDocument(crlXDocumentResult);

                this.AddSafeDataElement(
                    crlXDocumentResult.Root.Element(DataStringConstants.ElementNames.CheckRunData),
                    DataStringConstants.NameAttributeValues.CheckObjectStorageKey,
                    id);
#endif
#if DEBUG
                DataValidation.Instance.ValidateCheckRunLaunch(crlXDocumentResult);
#endif
            }
            catch (Exception)
            {
                throw;
            }

            return crlXDocumentResult;
        }

        #endregion //publicMembers

        #region privateMethods

        // Sets or re-sets the Reserved_SubCheckMap DataElement(s) to reflect the subcheck hierarchy.
        // This is needed to prevent collisions in the check run objects and the semaphores, and helps simplify debugging.
        // Note that the labels are 1-based, like XPath
        private void SetCheckCounters(XElement checkRunDataElement)
        {
            SetCheckCounters(checkRunDataElement, string.Empty, 1);
        }

        private void SetCheckCounters(XElement checkRunBaseElement, string rootLabel, int oneBasedCounter)
        {
            string label = rootLabel + oneBasedCounter.ToString();
            SetOneCheckCounter(checkRunBaseElement, label);

            var baseElements = checkRunBaseElement.Elements(DataStringConstants.ElementNames.SubCheckData);
            int counter = 1;

            foreach (XElement subCheckBaseElement in baseElements)
            {
                // recurse
                SetCheckCounters(subCheckBaseElement, label, counter++);
            }
        }

        private void SetOneCheckCounter(XElement checkRunBaseElement, string label)
        {
            this.AddSafeDataElement(checkRunBaseElement, DataStringConstants.NameAttributeValues.Reserved_SubCheckMap, label);
        }

#if DEBUG
        /// <summary>
        /// Decrements the numbers in the optional CountDownToFail attributes in the steps.
        /// When the numbers get to zero, the check or subcheck fails at that step.
        /// </summary>
        /// <param name="crl"></param>
        private void DecrementCountDownToFail(XDocument crl)
        {
            string xPathSpec = string.Format("//{0}[@{1}]", DataStringConstants.ElementNames.CheckStepInformation, DataStringConstants.AttributeNames.CountDownToFail);
            var elementsWithCountDownToFailAttributes = from XElement element in crl.Root.XPathSelectElements(xPathSpec) select element;

            foreach (XElement element in elementsWithCountDownToFailAttributes)
            {
                XAttribute theAttribute = element.Attribute(DataStringConstants.AttributeNames.CountDownToFail);
                string countDownString = theAttribute.Value;
                int countDownCurrent = 0;
                Int32.TryParse(countDownString, out countDownCurrent);
                countDownCurrent--;

                if (countDownCurrent < 0)
                {
                    // remove the attribute; there's no need for it anymore
                    theAttribute.Remove();
                }
                else
                {
                    // write the new value
                    theAttribute.Value = countDownCurrent.ToString();
                }
            }
        }
#endif

        private void AddSafeDataElement(XElement parent, string name, string value)
        {
            XElement element = parent.XPathSelectElement(string.Format(
                "{0}[@{1}='{2}']",
                DataStringConstants.ElementNames.DataElement,
                DataStringConstants.AttributeNames.Name,
                name));

            if (element == null)
            {
                // Add a DataElement, but before any SubCheckData elements
                XElement subCheckDataElementToPrecede = parent.Element(DataStringConstants.ElementNames.SubCheckData);

                if (subCheckDataElementToPrecede == null)
                {
                    // There are no SubCheckData elements, so just add the new DataElement 
                    parent.Add(new XElement(
                        DataStringConstants.ElementNames.DataElement,
                        new XAttribute(DataStringConstants.AttributeNames.Name, name),
                        new XAttribute(DataStringConstants.AttributeNames.Value, value)));
                }
                else
                {
                    subCheckDataElementToPrecede.AddBeforeSelf(new XElement(
                        DataStringConstants.ElementNames.DataElement,
                        new XAttribute(DataStringConstants.AttributeNames.Name, name),
                        new XAttribute(DataStringConstants.AttributeNames.Value, value)));
                }
            }
            else
            {
                XAttribute valueAttribute = element.Attribute(DataStringConstants.AttributeNames.Value);
                valueAttribute.Value = value;
            }
        }
        #endregion // privateMethods

        #region privateMembers

        private Dictionary<string, string> m_CrlDataElements = null;
        private Dictionary<string, string> m_CustomDataElements = null;

        #endregion //privateMembers
    }
}
