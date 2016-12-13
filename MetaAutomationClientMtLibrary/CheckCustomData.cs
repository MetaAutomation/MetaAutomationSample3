////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    internal class CheckCustomData : CheckData
    {
        // Don't use no-argument constructor
        private CheckCustomData() { }

        public CheckCustomData(XElement checkCustomDataElement)
        {
            if (checkCustomDataElement == null)
            {
                throw new CheckInfrastructureClientException("The parameter 'checkCustomDataElement' is null.");
            }

            string elementName = checkCustomDataElement.Name.ToString();

            if (elementName != DataStringConstants.ElementNames.CheckCustomData)
            {
                throw new CheckInfrastructureClientException(string.Format("The initializing element has name '{0}'. Expected name='{1}'", elementName, DataStringConstants.ElementNames.CheckCustomData));
            }

            base.m_BaseElementForSection = checkCustomDataElement;
        }

        /// <summary>
        /// Gets a value from the custom data for the check run artifact
        /// </summary>
        /// <param name="name">name of the value</param>
        /// <returns>the value string</returns>
        public string GetCustomData(string name)
        {
            return base.GetValueFromNameValuePairDataElement(name);
        }

        /// <summary>
        /// Sets a value for the custom data for the check run artifact
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetCustomData(string name, string value)
        {
            base.AddOrUpdateNameValuePairDataElement(name, value);
        }

        /// <summary>
        /// clears the name/value pair in the custom data for the check run artiface
        /// </summary>
        /// <param name="name"></param>
        public void ClearCustomData(string name)
        {
            base.RemoveDataElement(name);
        }

        /// <summary>
        /// clears all custom data
        /// </summary>
        public void ClearAllCustomData()
        {
            base.m_BaseElementForSection.RemoveAll();
        }
    }
}
