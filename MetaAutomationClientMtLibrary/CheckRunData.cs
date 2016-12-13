////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using MetaAutomationBaseMtLibrary;
    using System.Globalization;

    /// <summary>
    /// Note 1 (Atomic Check aspects reflected here: Actionable Artifact, Artifact Data, Run Data)
    /// This class holds data about the check run that applies whether or not the check passes. Every artifact from a check
    ///  includes data serialized from an instance of this class.
    /// </summary>
    internal class CheckRunData : CheckData
    {
        #region publicMethods
        public CheckRunData(XElement checkRunDataElement, CheckConstants.RunSubCheckDelegate runSubCheckDelegate = null)
        {
            if (checkRunDataElement == null)
            {
                throw new CheckInfrastructureClientException("The parameter 'checkRunDataElement' is null.");
            }

            this.m_RunSubCheckDelegate = runSubCheckDelegate;

            string elementName = checkRunDataElement.Name.ToString();

            if (elementName != DataStringConstants.ElementNames.CheckRunData)
            {
                throw new CheckInfrastructureClientException(string.Format("The initializing element has name '{0}'. Expected name='{1}'", elementName, DataStringConstants.ElementNames.CheckRunData));
            }

            base.m_BaseElementForSection = checkRunDataElement;
        }



        public void AddCheckBeginTimeStamp(XDocument cra)
        {
            base.AddOrUpdateNameValuePairDataElement(
                DataStringConstants.NameAttributeValues.CheckBeginTime,
                DateTime.Now.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture));
        }

        public void AddCheckEndTimeStamp(XDocument cra)
        {
            base.AddOrUpdateNameValuePairDataElement(
                DataStringConstants.NameAttributeValues.CheckEndTime,
                DateTime.Now.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Note 3 (Atomic Check aspects reflected here: Actionable Artifact, Artifact Data)
        /// Unlike class CheckFailData, there is no need of a data hierarchy, so this class only exposes one Add method 
        ///  with value of type string. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, string value)
        {
            base.AddDataElement(name, value);
        }

        /// <summary>
        /// invokes the sub check
        /// </summary>
        /// <param name="oneBasedIndex">1-based index into the check run object</param>
        /// <param name="currentStep">current step</param>
        public void CallSubCheck(int oneBasedIndex, XElement currentStep)
        {
            int subCheckCount = this.SubCheckCount;

            if (oneBasedIndex > SubCheckCount)
            {
                throw new CheckInfrastructureClientException(string.Format("CallSubCheck invoked with 1-based index='{0}', but the total number of subChecks is '{1}'. This might be due to a mismatch between the XML driving the check and the check method call of a sub-check.", oneBasedIndex, subCheckCount));
            }

            CheckRunTransforms checkRunTransforms = new CheckRunTransforms();
            XDocument subCheckCrl = checkRunTransforms.CreateSubCheckRunLaunch(this.m_BaseElementForSection.Document, this.GetSubCheck(oneBasedIndex), currentStep);
            XDocument artifactFromSubCheck = null;

            if (this.m_RunSubCheckDelegate != null)
            {
                artifactFromSubCheck = this.m_RunSubCheckDelegate(subCheckCrl);
            }
            else
            {
                throw new CheckInfrastructureClientException("The subcheck delegate is not set, so cannot be invoked for subcheck.");
            }

            XElement rootOfSubsteps = artifactFromSubCheck.Root.Element(DataStringConstants.ElementNames.CompleteCheckStepInfo);

            // remove child steps from current step as needed
            XElement rootStepFromSubCheck = rootOfSubsteps.Elements(DataStringConstants.ElementNames.CheckStepInformation).First<XElement>();

            if (rootStepFromSubCheck != null)
            {
                string subCheckRootStepName = rootStepFromSubCheck.Attribute(DataStringConstants.AttributeNames.Name).Value;
                XElement tempCurrentStep = ChildSteps.CleanStepsOnSearchForCorrectChildStep(currentStep, subCheckRootStepName);

                if (tempCurrentStep == null)
                {
                    ChildSteps.AddChildStep(currentStep, rootStepFromSubCheck);
                }
                else
                {
                    ChildSteps.ReplaceChildStep(tempCurrentStep, rootStepFromSubCheck);
                }
            }

            // Copy over the subcheck exception informaiton, and throw if needed
            XElement checkFailDataFromSubCheck = artifactFromSubCheck.Root.Element(DataStringConstants.ElementNames.CheckFailData);

            if (checkFailDataFromSubCheck.HasElements)
            {
                XElement failDataRoot = currentStep.Document.Root.Element(DataStringConstants.ElementNames.CheckFailData);
                XElement subCheckRoot = new XElement(DataStringConstants.ElementNames.DataElement,
                    new XAttribute(DataStringConstants.AttributeNames.Name, CheckConstants.AttributeValues.SubCheckExceptions));

                failDataRoot.Add(subCheckRoot);

                // the subcheck failed, so insert the element before CompleteCheckStepInfo = CheckConstants.Strings.CheckStepRootInfo.
                // At this time, this is the only failure info in the current process context
                foreach (XElement el in checkFailDataFromSubCheck.Elements())
                {
                    subCheckRoot.Add(el);
                }

                throw new CheckFailException("The subcheck failed, so rethrowing here to fail the check.");
            }
        }

        /// <summary>
        /// Gets the sub check XElement
        /// </summary>
        /// <param name="oneBasedIndex">1-based index</param>
        /// <returns></returns>
        public XElement GetSubCheck(int oneBasedIndex)
        {
            if (oneBasedIndex < 1)
            {
                throw new CheckInfrastructureClientException(string.Format("1-based index '{0}' is out of range.", oneBasedIndex));
            }

            return base.m_BaseElementForSection.Elements(DataStringConstants.ElementNames.SubCheckData).ToList<XElement>()[oneBasedIndex - 1];
        }

        #endregion // publicMethods

        #region privateMethods

        /// <summary>
        /// This is the count of subchecks that can be invoked directly from the current check or subcheck context.
        /// </summary>
        private int SubCheckCount
        {
            get
            {
                return base.m_BaseElementForSection.Elements(DataStringConstants.ElementNames.SubCheckData).Count<XElement>();
            }
        }

        private CheckRunData()
        {
        }
        #endregion // privateMethods

        #region privateData

        private CheckConstants.RunSubCheckDelegate m_RunSubCheckDelegate = null;

        #endregion // privateData
    }
}
