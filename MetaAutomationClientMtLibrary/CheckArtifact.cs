////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System.Xml.Linq;

    public class CheckArtifact
    {
        #region publicMembers

        public CheckArtifact()
        {
        }

        /// <summary>
        /// Accepts and validates CheckRunLaunch XDocument
        /// </summary>
        /// <param name="checkRunLaunchXDocument"></param>
        /// <returns></returns>
        public void InitializeCheckRunFromCheckRunLaunch(XDocument checkRunLaunchXDocument, CheckConstants.RunSubCheckDelegate runSubCheckDelegate = null)
        {
            DataValidation.Instance.ValidateCheckRunLaunch(checkRunLaunchXDocument);
            CheckRunTransforms checkRunTransforms = new CheckRunTransforms();
            XDocument checkRunArtifact = checkRunTransforms.ConvertCheckRunLaunchToCheckRunArtifact(checkRunLaunchXDocument);

            this.m_CheckRunArtifact = new CheckRunArtifact(checkRunArtifact, runSubCheckDelegate);
        }

        public void DoStep(string stepName, System.Action stepCode)
        {
            this.m_SubCheckCalledFromStep = false;
            this.m_CheckRunArtifact.DoStep(stepName, stepCode);
        }

        public uint StepTimeout
        {
            get
            {
                return this.m_CheckRunArtifact.StepTimeout;
            }
        }

        public void AddCheckFailData(string name, string value)
        {
            this.m_CheckRunArtifact.AddCheckFailData(name, value);
        }

        public void AddCheckExceptionInformation(System.Exception ex)
        {
            this.m_CheckRunArtifact.AddCheckFailInformation(ex);
        }

        /// <summary>
        /// Gets a value from the custom data for the check run artifact
        /// </summary>
        /// <param name="name">name of the value</param>
        /// <returns>the value string</returns>
        public string GetCustomData(string name)
        {
            return this.m_CheckRunArtifact.GetCustomData(name);
        }

        /// <summary>
        /// Sets a value for the custom data for the check run artifact
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetCustomData(string name, string value)
        {
            this.m_CheckRunArtifact.SetCustomData(name, value);
        }

        /// <summary>
        /// Sets a value for the custom data as a DataElement in the check step
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetCustomDataCheckStep(string name, string value)
        {
            this.m_CheckRunArtifact.SetCustomDataCheckStep(name, value);
        }

        /// <summary>
        /// clears the name/value pair in the custom data for the check run artiface
        /// </summary>
        /// <param name="name"></param>
        public void ClearCustomData(string name)
        {
            this.m_CheckRunArtifact.ClearCustomData(name);
        }

        /// <summary>
        /// clears all custom data
        /// </summary>
        public void ClearAllCustomData()
        {
            this.m_CheckRunArtifact.ClearAllCustomData();
        }

        public XDocument CompleteCheckRun()
        {
            return m_CheckRunArtifact.ArtifactDocument;
        }

        /// <summary>
        /// Calls a sub-check. NOTE that this uses a 1-based index, just as XPath does.
        /// </summary>
        /// <param name="oneBasedIndex">1-based index into the subcheck specification in the check run launch (CRL)</param>
        public void CallSubCheck(int oneBasedIndex)
        {
            if (this.m_SubCheckCalledFromStep)
            {
                throw new CheckInfrastructureClientException("No more than 1 call to a sub check is allowed inside a check step definition.");
            }

            this.m_SubCheckCalledFromStep = true;
            m_CheckRunArtifact.CallSubCheck(oneBasedIndex);
        }

        #endregion //publicMembers
        #region privateMembers

        private CheckRunArtifact m_CheckRunArtifact = null;
        private bool m_SubCheckCalledFromStep = false;
        #endregion //privateMembers
    }
}
