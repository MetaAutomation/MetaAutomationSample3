////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationServiceMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    public class CheckOriginServices : ICheckOriginServices
    {
        public string CompleteCheckRun(XDocument checkRunArtifact)
        {
            string uniqueLabelForCheckRunSegment = CrxDepot.Instance.SetCraXDocument(checkRunArtifact);
            Synchronization.ReleaseOne(CheckRunDataHandles.StripIdOfMachineNames(uniqueLabelForCheckRunSegment));
            return uniqueLabelForCheckRunSegment;
        }

        public string GetCheckRunArtifact(string uniqueLabelForCheckRunSegment)
        {
            return CrxDepot.Instance.GetCraXDocument(uniqueLabelForCheckRunSegment).ToString();
        }

        public string AbortCheckRun(string uniqueLabelForCheckRunSegment, string errorMessage)
        {
            string result = errorMessage;

            // If the CRA if present, clear it and note that it was there
            if (CrxDepot.Instance.ClearCraXDocumentById(uniqueLabelForCheckRunSegment))
            {
                // append more information for error condition
                result += "CheckRunArtifact document was cleared from origin server. ";
            }

            CheckAbortMessages.Instance.SetMessage(uniqueLabelForCheckRunSegment, errorMessage);
            Synchronization.ReleaseOne(uniqueLabelForCheckRunSegment);

            return result;
        }

        public string GetAbortMessage(string uniqueLabelForCheckRunSegment)
        {
            string resultMessage = "GetAbortMessage failed. ";

            try
            {
                resultMessage = CheckAbortMessages.Instance.GetMessage(uniqueLabelForCheckRunSegment);
            }
            catch (KeyNotFoundException)
            {
                resultMessage += "The abort message was not found on the origin machine.";
            }

            return resultMessage;
        }

        public static CheckOriginServices Instance
        {
            get
            {
                lock (m_InstanceObject)
                {
                    if (CheckOriginServices.m_TheInstance == null)
                    {
                        CheckOriginServices.m_TheInstance = new CheckOriginServices();
                    }
                }

                return CheckOriginServices.m_TheInstance;
            }
        }

        private static CheckOriginServices m_TheInstance = null;
        private static object m_InstanceObject = new object();
    }
}
