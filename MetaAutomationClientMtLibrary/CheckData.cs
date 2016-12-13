////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System;
    using System.Globalization;
    using System.Xml.Linq;
    using System.Xml.XPath;

    /// <summary>
    /// Note 1 (Atomic Check aspects reflected here: Actionable Artifact, Distinct Artifacts)
    /// This abstract class holds base functionality for managing and generating XML for check run and check failure data. 
    ///  If the harness uses a relational database rather than XML for storing artifacts, the implementation of the 
    ///  Composite pattern would be adapted to that serialization.
    /// </summary>
    internal abstract class CheckData
    {
        protected XElement m_BaseElementForSection = null;

        protected CheckData()
        {
        }

        /// <summary>
        /// Note 4 (Atomic Check aspects reflected here: Actionable Artifact, Artifact Data, Separate Presentation)
        /// This method accepts simple name/value string pairs or name/collection pairs to make the basic unit of artifact 
        ///  data, so the artifacts support hierarchical structure as needed.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected void AddDataElement(string name, object value)
        {
            if (value is string)
            {
                this.m_BaseElementForSection.Add(
                    new XElement(
                    DataStringConstants.ElementNames.DataElement,
                    new XAttribute(DataStringConstants.AttributeNames.Name, name),
                    new XAttribute(DataStringConstants.AttributeNames.Value, value)));
            }
            else
            {
                this.m_BaseElementForSection.Add(
                    new XElement(
                    DataStringConstants.ElementNames.DataElement,
                    new XAttribute(DataStringConstants.AttributeNames.Name, name),
                    value));
            }
        }

        public void AddOrUpdateNameValuePairDataElement(string name, string value)
        {
            AddOrUpdateNameValuePairDataElement(this.m_BaseElementForSection, name, value);
        }

        protected void AddOrUpdateNameValuePairDataElement(XElement parentElement, string name, string value)
        {
            XElement element = parentElement.XPathSelectElement(string.Format(
                "{0}[@{1}='{2}']",
                DataStringConstants.ElementNames.DataElement,
                DataStringConstants.AttributeNames.Name,
                name));

            if (element == null)
            {
                // Add a DataElement, but before any SubCheckData or step custom data elements.
                // The code assumes that (according to the XSD) SubCheckData elements and DataElement children of CheckStep elements
                //  are never children of the same parent.
                XElement elementToPrecede = parentElement.Element(DataStringConstants.ElementNames.SubCheckData);

                if (elementToPrecede == null)
                {
                    elementToPrecede = parentElement.Element(DataStringConstants.ElementNames.DataElement);
                }

                if (elementToPrecede == null)
                {
                    // There are no SubCheckData elements, so just add the new DataElement 
                    parentElement.Add(new XElement(
                        DataStringConstants.ElementNames.DataElement,
                        new XAttribute(DataStringConstants.AttributeNames.Name, name),
                        new XAttribute(DataStringConstants.AttributeNames.Value, value)));
                }
                else
                {
                    elementToPrecede.AddBeforeSelf(new XElement(
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

        protected string GetValueFromNameValuePairDataElement(string name)
        {
            string result = null;

            XElement element = this.m_BaseElementForSection.XPathSelectElement(string.Format(
                "{0}[@{1}='{2}']",
                DataStringConstants.ElementNames.DataElement,
                DataStringConstants.AttributeNames.Name,
                name));

            if (element != null)
            {
                XAttribute valueAttribute = element.Attribute(DataStringConstants.AttributeNames.Value);

                // as the schema requires, assume the result is not null
                result = valueAttribute.Value;
            }

            return result;
        }

        protected void RemoveDataElement(string name)
        {
            XElement element = this.m_BaseElementForSection.XPathSelectElement(string.Format(
                "{0}[@{1}='{2}']",
                DataStringConstants.ElementNames.DataElement,
                DataStringConstants.AttributeNames.Name,
                name));

            if (element != null)
            {
                element.Remove();
            }
        }
    }
}
