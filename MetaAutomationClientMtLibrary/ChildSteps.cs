////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    internal static class ChildSteps
    {
        /// <summary>
        /// Adds a child step, either as the only child step or as the last child step.
        /// </summary>
        /// <param name="step"></param>
        /// <param name="stepName"></param>
        /// <returns>the new step</returns>
        public static XElement AddChildStep(XElement step, string stepName)
        {
            XElement resultStep = null;
            XNode lastNode = step.LastNode;
            XElement lastChildStep = lastNode as XElement;

            if (lastChildStep == null)
            {
                step.Add(
                    new XElement(DataStringConstants.ElementNames.CheckStepInformation,
                    new XAttribute(DataStringConstants.AttributeNames.Name, stepName)));
                resultStep = (XElement)step.LastNode;
            }
            else
            {
                lastChildStep.AddAfterSelf(
                    new XElement(DataStringConstants.ElementNames.CheckStepInformation,
                    new XAttribute(DataStringConstants.AttributeNames.Name, stepName)));
                resultStep = (XElement)step.LastNode;
            }

            return resultStep;
        }

        /// <summary>
        /// Adds a child step XElement with it's own child elements, either as the only child step or as the last child step.
        /// </summary>
        /// <param name="step"></param>
        /// <param name="newChildXElement"></param>
        /// <returns>the new step</returns>
        public static XElement AddChildStep(XElement step, XElement newChildXElement)
        {
            XElement resultStep = null;
            XNode lastNode = step.LastNode;
            XElement lastChildStep = lastNode as XElement;

            if (lastChildStep == null)
            {
                step.Add(newChildXElement);
                resultStep = (XElement)step.LastNode;
            }
            else
            {
                lastChildStep.AddAfterSelf(newChildXElement);
                resultStep = (XElement)step.LastNode;
            }

            return resultStep;
        }

        public static void ReplaceChildStep(XElement target, XElement replacement)
        {
            XElement previousElement = target.PreviousNode as XElement;

            if (previousElement == null)
            {
                XElement parentElement = target.Parent;
                target.Remove();
                parentElement.Add(replacement);
            }
            else
            {
                target.Remove();
                previousElement.AddAfterSelf(replacement);
            }
        }

        /// <summary>
        /// Searches child steps for matching name, removing unexecuted steps along the way
        /// </summary>
        /// <param name="step"></param>
        /// <param name="stepName"></param>
        /// <returns>the correct child step or null</returns>
        public static XElement CleanStepsOnSearchForCorrectChildStep(XElement step, string stepName)
        {
            XElement resultStep = null;

            var iterateInElementOrderThroughUnexecutedSteps = GetIteratorForUnexecutedChildElements(step);

            // Iterate through the unexecuted steps until either a) the correct step is found, or b) have to create a 
            //  new one with the correct name
            foreach (XElement unexecutedStep in iterateInElementOrderThroughUnexecutedSteps)
            {
                if (unexecutedStep.Attribute(DataStringConstants.AttributeNames.Name).Value == stepName)
                {
                    // found the step in the xml, so continue. Don't remove any following steps, because the check 
                    //  might need those later.
                    resultStep = unexecutedStep;
                    break;
                }
                else
                {
                    // The existing step in the steps XML does not match, so remove it.
                    unexecutedStep.Remove();
                }
            }

            return resultStep;
        }

        private static IEnumerable<XElement> GetIteratorForUnexecutedChildElements(XElement rootElement)
        {
            return ((IEnumerable<XElement>)rootElement.Elements(DataStringConstants.ElementNames.CheckStepInformation)).
                Where<XElement>(p => p.Attribute(DataStringConstants.AttributeNames.Value) == null).ToList<XElement>();
        }
    }
}
