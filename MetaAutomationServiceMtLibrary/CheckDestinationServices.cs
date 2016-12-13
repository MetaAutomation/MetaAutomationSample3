////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationServiceMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System;
    using System.IO;
    using System.Xml.Linq;

    public class CheckDestinationServices : ICheckDestinationServices
    {
        public string StartCheckRun(XDocument checkRunLaunch)
        {
            string pathAndFileNameForExe = DataAccessors.GetCheckRunValue(checkRunLaunch, DataStringConstants.NameAttributeValues.PathAndFileToRunner);

            if (!Path.IsPathRooted(pathAndFileNameForExe))
            {
                // path is relative path. Add environment current directory
                pathAndFileNameForExe = Path.Combine(Environment.CurrentDirectory, pathAndFileNameForExe);
            }

            // Store the CRL temporarily on the local machine, because this is the server machine
            string uniqueLabelForCheckRunSegment = CrxDepot.Instance.SetCrlXDocument(checkRunLaunch);

            // STart the check run or sub-check run
            CheckRunLocal checkRunLocal = new CheckRunLocal();
            return checkRunLocal.Start(uniqueLabelForCheckRunSegment, pathAndFileNameForExe);
        }

        public string GetCheckRunLaunch(string uniqueLabelForCheckRunSegment)
        {
            return CrxDepot.Instance.GetCrlXDocument(uniqueLabelForCheckRunSegment).ToString();
        }

        public string AbortCheckRun(string uniqueLabelForCheckRunSegment, string errorMessage)
        {
            string result = errorMessage;

            // If the CRL if present, clear it and note that it was there
            if (CrxDepot.Instance.ClearCrlXDocumentById(uniqueLabelForCheckRunSegment))
            {
                // append more information for error condition
                result += "CheckRunLaunch document was cleared from destination server. ";
            }

            return result;
        }

        public static CheckDestinationServices Instance
        {
            get
            {
                lock (m_InstanceObject)
                {
                    if (CheckDestinationServices.m_TheInstance == null)
                    {
                        CheckDestinationServices.m_TheInstance = new CheckDestinationServices();
                    }
                }

                return CheckDestinationServices.m_TheInstance;
            }
        }

        private static CheckDestinationServices m_TheInstance = null;
        private static object m_InstanceObject = new object();
    }
}
