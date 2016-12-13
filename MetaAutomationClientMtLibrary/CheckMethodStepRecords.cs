////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using MetaAutomationBaseMtLibrary;

    /// <summary>
    /// This class maintains a hierarchy of steps for the check, starting with the steps from the last run of the same check
    /// keeps track of the current step and any changes to the steps, the check failure stack at check failure, and the
    /// status of check steps in the artifact as "Pass," "Fail," or "Blocked."
    /// At check fail, this class also creates a stack of steps at fail, in analog to a stack trace.
    /// </summary>
    internal class CheckMethodStepRecords : CheckData
    {
        private XElement m_Root_CompleteCheckStepInfo = null;

        // The XElement represents the current step of the check
        private XElement m_currentStep = null;
        private bool attributesForFailedAndBlockedStepsHaveBeenSet = false;
        private object m_lockObjectForStepRecords = new object();
        private CheckStepRunner m_CheckStepRunner = null;
        private bool m_FailInitiatedInCheck = false;
        private string m_CheckAbortMessage = string.Empty;

        public CheckMethodStepRecords(XElement completeCheckStepInfo)
        {
            if (completeCheckStepInfo == null)
            {
                throw new CheckInfrastructureClientException("The parameter 'checkRunDataElement' is null.");
            }

            string elementName = completeCheckStepInfo.Name.ToString();

            if (elementName != DataStringConstants.ElementNames.CompleteCheckStepInfo)
            {
                throw new CheckInfrastructureClientException(string.Format("The initializing element has name '{0}'. Expected name='{1}'", elementName, DataStringConstants.ElementNames.CompleteCheckStepInfo));
            }

            m_Root_CompleteCheckStepInfo = completeCheckStepInfo;
            m_currentStep = m_Root_CompleteCheckStepInfo;

            m_CheckStepRunner = new CheckStepRunner(this);
        }

        public void DoStep(string stepName, Action stepCode)
        {
            m_CheckStepRunner.DoStep(stepName, stepCode);
        }

        public uint StepTimeout
        {
            get
            {
                uint timeout = DataStringConstants.NumericConstants.DefaultTimeoutMilliseconds;

                if (m_currentStep != null)
                {
                    XAttribute timeoutAttribute = m_currentStep.Attribute(DataStringConstants.AttributeNames.TimeLimit);

                    if (timeoutAttribute != null)
                    {
                        // XSD schema enforces that the value of the attribute supports uint
                        timeout = uint.Parse(timeoutAttribute.Value);
                    }
                }

                return timeout;
            }
        }

        /// <summary>
        ///  Moves the step recorder currency to the new location on the XML hiearchy
        ///    that traces the hierarchical steps for execution record of this check, and creates a new
        ///    XElement for this if needed.
        /// </summary>
        /// <param name="stepName"></param>
        /// <param name="msTimeLimit">out parameter of timeout read from XML, or default of 30000ms</param>
        /// <returns>debug fail is required before step begin</returns>
        public bool BeginStep(string stepName, out uint msTimeLimit)
        {
            msTimeLimit = DataStringConstants.NumericConstants.DefaultTimeoutMilliseconds;
            bool debugFailRequired = false;

            // Check for a pre-existing step that maps to this one. If there isn't an existing step with the correct name, 
            //  then create one.
            lock (m_lockObjectForStepRecords)
            {
                bool correctStepFound = false;

                if ((m_currentStep != null) && (m_currentStep.Attribute(DataStringConstants.AttributeNames.Name) != null) && (m_currentStep.Attribute(DataStringConstants.AttributeNames.Name).Value == stepName))
                {
                    // easy case: the current step is the correct step.
                    correctStepFound = true;
                }
                else
                {
                    XElement tempStep = ChildSteps.CleanStepsOnSearchForCorrectChildStep(m_currentStep, stepName);

                    if (tempStep != null)
                    {
                        m_currentStep = tempStep;
                        correctStepFound = true;
                    }
                }

                // if available, read the timeout from the step
                if (correctStepFound)
                {
                    XAttribute timeLimitAttribute = m_currentStep.Attribute(DataStringConstants.AttributeNames.TimeLimit);

                    if (timeLimitAttribute != null)
                    {
                        // XML scheme requires int here, so will fit
                        msTimeLimit = uint.Parse(timeLimitAttribute.Value);
                    }
                }

                // if the step wasn't found, add it
                if (!correctStepFound)
                {
                    // Add the correct step
                    m_currentStep = ChildSteps.AddChildStep(m_currentStep, stepName);

                    // set time limit in ms
                    SetTimeLimitAttribute(m_currentStep, msTimeLimit);
                }

                SetMachineNameAttribute(m_currentStep);
            }

#if DEBUG
            XAttribute countDownToFailAttribute = m_currentStep.Attribute(DataStringConstants.AttributeNames.CountDownToFail);
            XAttribute failCheckStepAttribute = m_currentStep.Attribute(DataStringConstants.AttributeNames.FailCheckStep);

            if (((countDownToFailAttribute != null) && (countDownToFailAttribute.Value == "0")) || ((failCheckStepAttribute != null) && (failCheckStepAttribute.Value == "true")))
            {
                debugFailRequired = true;
            }
#endif

            return debugFailRequired;
        }

        /// <summary>
        /// This method is called at the end of the step, e.g. from the dispose method of the step object.
        /// </summary>
        /// <param name="checkRunIsPassing"></param>
        public void EndStep(bool checkRunIsPassing)
        {
            lock (m_lockObjectForStepRecords)
            {
                // The XML that records the steps and the execution record is only available during the check run. In case
                //  of check failure, a step stack is created for use with the failure record in the artifact. Here,  
                //  the code has to check if the recursive steps are available or not.
                if (checkRunIsPassing)
                {
                    // Check for unexecuted check steps as children of this element, non-recursive, and clean these steps 
                    //  out.
                    CleanUnexecutedChildElements(m_currentStep);
                }

                // Go up to step parent
                if (m_currentStep.Parent is XElement)
                {
                    m_currentStep = m_currentStep.Parent;
                }
            }
        }

        /// <summary>
        /// The name of the current check step
        /// </summary>
        public string CurrentStepName
        {
            get
            {
                string returnValue;

                lock (m_lockObjectForStepRecords)
                {
                    returnValue = m_currentStep.Attribute(DataStringConstants.AttributeNames.Name).Value;
                }

                return returnValue;
            }
        }

        /// <summary>
        /// The XElement that is the current step
        /// </summary>
        public XElement CurrentStep
        {
            get
            {
                return m_currentStep;
            }
        }
        
        /// <summary>
        /// Place a DataElement as a child element to the CheckStep
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetDataElementInCheckStep(string name, string value)
        {
            base.AddOrUpdateNameValuePairDataElement(m_currentStep, name, value);
        }

        public void SetCurrentStepResult(CheckConstants.StepResults result, int msTimeResult)
        {
            // Due to exceptions not propagating past the invocation of the check method,
            //  use this property to prevent the step from looking like a "Pass."
            // This would normally apply at the check step that is the root method call.
            if ((result == CheckConstants.StepResults.Pass) && (this.FailInitiatedInCheck))
            {
                // override the "Pass" with a "Fail" because the check is failing
                result = CheckConstants.StepResults.Fail;
            }

            lock (m_lockObjectForStepRecords)
            {
                SetTimeElapsedAttribute(m_currentStep, msTimeResult);

                // Set the nodes unless the case of "Fail." In that case, set nodes to "Fail recursively, so do the Fail in
                //  the below block and just once.
                if (result != CheckConstants.StepResults.Fail)
                {
                    SetValueAttribute(m_currentStep, result);
                }
                else
                {
                    // The current step is set to "Fail" with this code block, and 
                    // the following code sets all the blocked steps to "Blocked" if needed, i.e. if the check is failing
                    if ((!attributesForFailedAndBlockedStepsHaveBeenSet) && (result == CheckConstants.StepResults.Fail))
                    {
                        // Given the locking, this is the first "Fail" step
                        attributesForFailedAndBlockedStepsHaveBeenSet = true;
                        Stack<string> tempReversedStack = new Stack<string>();

                        XElement checkStepRootFinder = m_currentStep;

                        // find the node that is ancestor to all of the steps
                        while (checkStepRootFinder.Name == DataStringConstants.ElementNames.CheckStepInformation)
                        {
                            SetValueAttribute(checkStepRootFinder, result);
                            tempReversedStack.Push(
                                checkStepRootFinder.Attribute(DataStringConstants.AttributeNames.Name).Value);

                            if (checkStepRootFinder.Parent == null)
                            {
                                // Break out of the loop. This can happen if this context is a subcheck
                                break;
                            }
                            else
                            {
                                checkStepRootFinder = checkStepRootFinder.Parent;
                            }
                        }

                        var allElementsInDocumentOrder = checkStepRootFinder.Descendants();
                        HashSet<XElement> followingElements = new HashSet<XElement>();

                        // find all steps that follow the current check step
                        foreach (XElement currentElement in allElementsInDocumentOrder)
                        {
                            if (currentElement.IsAfter((XNode)m_currentStep))
                            {
                                followingElements.Add(currentElement);
                            }
                        }

                        // Set all these steps as "Blocked" unless they already have a status set with the "Value" attribute
                        foreach (XElement element in followingElements)
                        {
                            if (element.Attribute(DataStringConstants.AttributeNames.Value) == null)
                            {
                                element.SetAttributeValue(
                                    DataStringConstants.AttributeNames.Value,
                                    CheckConstants.StepResults.Blocked);
                            }
                        }
                    }
                }
            }
        }

        public bool FailInitiatedInCheck
        {
            set
            {
                m_FailInitiatedInCheck = value;
            }
            get
            {
                return m_FailInitiatedInCheck;
            }
        }

        public string CheckTimeoutAbortMessage
        {
            set
            {
                m_CheckAbortMessage = value;
            }
            get
            {
                return m_CheckAbortMessage;
            }
        }

        /// <summary>
        /// Add results from subcheck:
        ///  *steps
        ///  *exception data
        ///  *recursive steps at failure
        /// This method throws CheckFailException if the subcheck failed.
        /// </summary>
        /// <param name="subCheckResults">Streamed data from subcheck</param>
        public void SetResultsFromSubCheck(string subCheckResults)
        {
            if (subCheckResults == null) throw new ArgumentNullException("subCheckResults");

            XDocument subcheckDoc = XDocument.Parse(subCheckResults);

            // Set check steps
            this.SetResultsForDescendantSteps(subcheckDoc.Descendants(DataStringConstants.ElementNames.CompleteCheckStepInfo).First());

            // Get subcheck exception informaiton and steps-at-failure, IFF they exist
            XElement checkFailDataFromSubCheck = subcheckDoc.Descendants(DataStringConstants.ElementNames.CheckFailData).FirstOrDefault();

            if (checkFailDataFromSubCheck != null)
            {
                // the subcheck failed, so insert the element before CompleteCheckStepInfo = CheckConstants.Strings.CheckStepRootInfo.
                // At this time, this is the only failure info in the current process context
                XElement completeCheckStepInfo = subcheckDoc.Descendants(DataStringConstants.ElementNames.CompleteCheckStepInfo).FirstOrDefault();


                completeCheckStepInfo.AddBeforeSelf(checkFailDataFromSubCheck);

                throw new CheckFailException("The subcheck failed, so rethrowing here to fail the check.");
            }
            // else, the subcheck passed
        }


        /// <summary>
        /// Merges steps and step results for subcheck into current check
        /// </summary>
        /// <param name="subCheckResultElement">Assumes that the first XElement with name 'StepInfo' has no siblings</param>
        private void SetResultsForDescendantSteps(XElement firstStep)
        {
            // Ensure that the step added is the correct type, or null
            while ((firstStep != null) && (firstStep.Name != DataStringConstants.ElementNames.CheckStepInformation))
            {
                firstStep = firstStep.Elements().FirstOrDefault();
            }

            if (firstStep != null)
            {
                m_currentStep.AddFirst(firstStep);
            }
        }

        /// <summary>
        /// Gets an iterator for all child elements that 
        ///     a) have no 'Value' attribute
        ///     b) have no descendant elements with a 'Value' attribute
        ///  i.e. they have not been executed during this check method run.
        /// </summary>
        /// <param name="rootElement">The element to search</param>
        /// <returns>iterator over all children with no 'Value' attribute</returns>
        private void CleanUnexecutedChildElements(XElement rootElement)
        {
            List<XElement> noResultElementsInDocumentOrder = ((IEnumerable<XElement>)
                rootElement.Elements(DataStringConstants.ElementNames.CheckStepInformation)).
                Where<XElement>(p => p.Attribute(DataStringConstants.AttributeNames.Value) == null).ToList<XElement>();

            // iterate
            //  iterate through list by index
            //      if no child elements, remove element from XML, remove from list, restart iterator
            //      if child elements..
            //          count child elements with 'Value' attribute. If >zero, remove from list, restart iterator
            bool thereMightBeMoreToRemove = false;

            do
            {
                thereMightBeMoreToRemove = false;

                foreach (XElement candidateToRemoveFromXml in noResultElementsInDocumentOrder)
                {
                    // there are more elements to potentially be removed from XML
                    thereMightBeMoreToRemove = true;

                    if (candidateToRemoveFromXml.Elements().Count() == 0)
                    {
                        // This element has no 'Value' attribute to show a result, and it has no children, so remove from XML *and* remove from list, then restart
                        candidateToRemoveFromXml.Remove();
                        noResultElementsInDocumentOrder.Remove(candidateToRemoveFromXml);
                        break;
                    }
                    else
                    {
                        // check for children with 'Value' attribute
                        if ((candidateToRemoveFromXml.Elements(DataStringConstants.ElementNames.CheckStepInformation).
                            Where<XElement>(p => p.Attribute(DataStringConstants.AttributeNames.Value) != null).Count()) > 0)
                        {
                            // there is at least one child with a Value, so don't remove this element. Remove it from list instead, and restart iterator
                            noResultElementsInDocumentOrder.Remove(candidateToRemoveFromXml);
                            break;
                        }
                        else
                        {
                            // there are no 'Value' results in any children of this element, so remove it
                            candidateToRemoveFromXml.Remove();
                            noResultElementsInDocumentOrder.Remove(candidateToRemoveFromXml);
                            break;
                        }
                    }
                }
            } while (thereMightBeMoreToRemove);
        }

        /// <summary>
        /// Sets the "Value" attribute to the specified value, whether or not the attribute already exists
        /// </summary>
        /// <param name="element"></param>
        /// <param name="result"></param>
        private void SetValueAttribute(XElement element, CheckConstants.StepResults result)
        {
            XAttribute checkStepStateAttribute = element.Attribute(DataStringConstants.AttributeNames.Value);

            if (null == checkStepStateAttribute)
            {
                element.Add(new XAttribute(DataStringConstants.AttributeNames.Value, result.ToString()));
            }
            else
            {
                checkStepStateAttribute.Value = result.ToString();
            }
        }

        /// <summary>
        /// Sets the "msTimeLimit" attribute to the specified value, whether or not the attribute already exists
        /// </summary>
        /// <param name="element"></param>
        /// <param name="result"></param>
        private void SetTimeLimitAttribute(XElement element, uint result)
        {
            XAttribute checkStepStateAttribute = element.Attribute(DataStringConstants.AttributeNames.TimeLimit);

            if (null == checkStepStateAttribute)
            {
                element.Add(new XAttribute(DataStringConstants.AttributeNames.TimeLimit, result.ToString()));
            }
            else
            {
                checkStepStateAttribute.Value = result.ToString();
            }
        }

        /// <summary>
        /// Sets the "msTimeElapsed" attribute to the specified value, whether or not the attribute already exists
        /// </summary>
        /// <param name="element"></param>
        /// <param name="result"></param>
        private void SetTimeElapsedAttribute(XElement element, int result)
        {
            XAttribute checkStepStateAttribute = element.Attribute(DataStringConstants.AttributeNames.TimeElapsed);

            if (null == checkStepStateAttribute)
            {
                element.Add(new XAttribute(DataStringConstants.AttributeNames.TimeElapsed, result.ToString()));
            }
            else
            {
                checkStepStateAttribute.Value = result.ToString();
            }
        }

        private void SetMachineNameAttribute(XElement element)
        {
            XAttribute machineNameAttribute = element.Attribute(DataStringConstants.AttributeNames.MachineName);

            if (null == machineNameAttribute)
            {
                element.Add(new XAttribute(DataStringConstants.AttributeNames.MachineName, Environment.MachineName));
            }
            else
            {
                machineNameAttribute.Value = Environment.MachineName;
            }
        }
    }
}
