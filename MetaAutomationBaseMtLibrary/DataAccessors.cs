////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationBaseMtLibrary
{
    using System.Xml.Linq;
    using System.Xml.XPath;

    public static class DataAccessors
    {
        public static string GetCheckRunValue(XDocument crx, string name)
        {
            XElement elementWithDataChildren = crx.Root.Element(DataStringConstants.ElementNames.CheckRunData);
            XElement targetDataElement = elementWithDataChildren.XPathSelectElement(string.Format(
                "{0}[@{1}='{2}']",
                DataStringConstants.ElementNames.DataElement,
                DataStringConstants.AttributeNames.Name,
                name));

            if (targetDataElement == null)
            {
                throw new CheckInfrastructureBaseException(string.Format("GetCheckRunValue failed because the element with attribute name='{0}' was not found.", name));
            }

            XAttribute valueAttribute = targetDataElement.Attribute(DataStringConstants.AttributeNames.Value);
            return valueAttribute.Value;
        }

        public static bool CheckRunValueIsPresent(XDocument crx, string name)
        {
            bool result = false;
            XElement elementWithDataChildren = crx.Root.Element(DataStringConstants.ElementNames.CheckRunData);
            XElement targetDataElement = elementWithDataChildren.XPathSelectElement(string.Format(
                "{0}[@{1}='{2}']",
                DataStringConstants.ElementNames.DataElement,
                DataStringConstants.AttributeNames.Name,
                name));

            if (targetDataElement != null)
            {
                result = true;
            }

            return result;
        }
    }
}
