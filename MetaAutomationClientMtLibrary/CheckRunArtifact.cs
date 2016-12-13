////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using System;
    using System.Xml.Linq;
    using MetaAutomationBaseMtLibrary;
    using System.IO;
    using System.Xml;
    using System.Xml.Xsl;
    using System.Globalization;

    /// <summary>
    /// Note 1 (Atomic Check aspects reflected here: Actionable Artifact, Artifact Data, Separate Presentation, Check Steps, Failure Data)
    /// This class manages the check result data and check run artifacts, including class CheckRunData, CheckFailData, and 
    ///  CheckMethodStepRecords.
    /// </summary>
    internal class CheckRunArtifact
    {
        #region publicMembers

        public CheckRunArtifact(XDocument checkRunArtifact, CheckConstants.RunSubCheckDelegate runSubCheckDelegate = null)
        {
            m_ArtifactLockObject = new object();
            m_CheckRunArtifact_XDocument = checkRunArtifact;

            m_CheckRunData = new CheckRunData(m_CheckRunArtifact_XDocument.Root.Element(DataStringConstants.ElementNames.CheckRunData), runSubCheckDelegate);
            this.AddCheckBeginTimeStamp(m_CheckRunArtifact_XDocument);

            m_CheckCustomData = new CheckCustomData(m_CheckRunArtifact_XDocument.Root.Element(DataStringConstants.ElementNames.CheckCustomData));
            m_CheckFailData = new CheckFailData(m_CheckRunArtifact_XDocument.Root.Element(DataStringConstants.ElementNames.CheckFailData));
            m_CheckMethodStepRecords = new CheckMethodStepRecords(m_CheckRunArtifact_XDocument.Root.Element(DataStringConstants.ElementNames.CompleteCheckStepInfo));
        }

        // Note 3 (Atomic Check aspects reflected here: Actionable Artifact, Artifact Data, Separate Presentation, Failure Data)
        // Check fail data is added to a flat collection with this method overload.
        public void AddCheckFailData(string name, string value)
        {
            lock (m_ArtifactLockObject)
            {
                m_CheckFailData.Add(name, value);
            }
        }

        /// <summary>
        /// Note 4 (Atomic Check aspects reflected here: Actionable Artifact, Artifact Data, Separate Presentation, Failure Data)
        /// This method overload supports a hierarchical structure to the artifact data; instead of a name/value pair, this 
        ///  takes a name/node collection pair.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddCheckFailData(string name, XElement childElement)
        {
            lock (m_ArtifactLockObject)
            {
                m_CheckFailData.Add(name, childElement);
            }
        }

        /// <summary>
        /// Note 5 (Atomic Check aspects reflected here: Failure Data)
        /// This method is for adding exception and check step failure information to the artifact XML.
        /// </summary>
        /// <param name="ex"></param>
        public void AddCheckFailInformation(Exception ex)
        {
            lock (m_ArtifactLockObject)
            {
                m_CheckFailData.AddExceptionInformation(ex);
            }
        }

        public XDocument ArtifactDocument
        {
            get
            {
                return this.CreateArtifactDocument();
            }
        }

        public void DoStep(string stepName, Action stepCode)
        {
            this.m_CheckMethodStepRecords.DoStep(stepName, stepCode);
        }

        public uint StepTimeout
        {
            get
            {
                return this.m_CheckMethodStepRecords.StepTimeout;
            }
        }

        /// <summary>
        /// Gets a value from the custom data for the check run artifact
        /// </summary>
        /// <param name="name">name of the value</param>
        /// <returns>the value string</returns>
        public string GetCustomData(string name)
        {
            return m_CheckCustomData.GetCustomData(name);
        }

        /// <summary>
        /// Sets a value for the custom data for the check run artifact
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetCustomData(string name, string value)
        {
            m_CheckCustomData.SetCustomData(name, value);
        }

        /// <summary>
        /// Sets a value for the custom data for the check run artifact
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetCustomDataCheckStep(string name, string value)
        {
            m_CheckMethodStepRecords.SetDataElementInCheckStep(name, value);
        }

        /// <summary>
        /// clears the name/value pair in the custom data for the check run artiface
        /// </summary>
        /// <param name="name"></param>
        public void ClearCustomData(string name)
        {
            m_CheckCustomData.ClearCustomData(name);
        }

        /// <summary>
        /// clears all custom data
        /// </summary>
        public void ClearAllCustomData()
        {
            m_CheckCustomData.ClearAllCustomData();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oneBasedIndex">1-based index</param>
        public void CallSubCheck(int oneBasedIndex)
        {
            XElement currentStep = this.m_CheckMethodStepRecords.CurrentStep;
            m_CheckRunData.CallSubCheck(oneBasedIndex, currentStep);
        }

        #endregion //publicMembers
        #region privateMethods
        private XDocument CreateArtifactDocument()
        {
            this.AddCheckEndTimeStamp(m_CheckRunArtifact_XDocument);
            return m_CheckRunArtifact_XDocument;
        }

        private void AddCheckBeginTimeStamp(XDocument cra)
        {
            m_CheckRunData.AddCheckBeginTimeStamp(cra);
        }

        private void AddCheckEndTimeStamp(XDocument cra)
        {
            m_CheckRunData.AddCheckEndTimeStamp(cra);
        }

        #endregion //privateMethods
        #region privateMembers
        private object m_ArtifactLockObject = null;
        private XDocument m_CheckRunArtifact_XDocument = null;

        private CheckRunData m_CheckRunData = null;
        private CheckCustomData m_CheckCustomData = null;
        private CheckFailData m_CheckFailData = null;
        private CheckMethodStepRecords m_CheckMethodStepRecords = null;

        private string m_checkMethodRunGuid = string.Empty;
        #endregion //privateMembers
    }
}
