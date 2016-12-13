////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationServiceMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System.Xml.Linq;

    public class CheckRunValidation : ICheckRunValidation
    {
        public XDocument ValidateCheckRunLaunchIntoXDocument(string checkRunLaunchXML)
        {
            return DataValidation.Instance.ValidateCheckRunLaunchIntoXDocument(checkRunLaunchXML);
        }

        public XDocument ValidateCheckRunArtifactIntoXDocument(string checkRunArtifactXML)
        {
            return DataValidation.Instance.ValidateCheckRunArtifactIntoXDocument(checkRunArtifactXML);
        }

        public static CheckRunValidation Instance
        {
            get
            {
                lock (m_InstanceObject)
                {
                    if (CheckRunValidation.m_TheInstance == null)
                    {
                        CheckRunValidation.m_TheInstance = new CheckRunValidation();
                    }
                }

                return CheckRunValidation.m_TheInstance;
            }
        }

        private static CheckRunValidation m_TheInstance = null;
        private static object m_InstanceObject = new object();
    }
}
