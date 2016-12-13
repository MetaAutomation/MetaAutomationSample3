////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using System.Collections.Generic;
    using System.Xml.Linq;
    using MetaAutomationBaseMtLibrary;
    using System;

    /// <summary>
    /// Note 1 (Atomic Check aspects reflected here: Actionable Artifact, Failure Data)
    /// This class maintains data about the check run in case of failure. It is not used in case of check pass.
    /// </summary>
    internal class CheckFailData : CheckData
    {
        public CheckFailData(XElement checkFailData)
        {
            if (checkFailData == null)
            {
                throw new CheckInfrastructureClientException("The parameter 'checkFailData' is null.");
            }

            string elementName = checkFailData.Name.ToString();

            if (elementName != DataStringConstants.ElementNames.CheckFailData)
            {
                throw new CheckInfrastructureClientException(string.Format("The initializing element has name '{0}'. Expected name='{1}'", elementName, DataStringConstants.ElementNames.CheckFailData));
            }

            base.m_BaseElementForSection = checkFailData;
        }

        private CheckFailData(): base()
        {
        }

        /// <summary>
        /// Note 3a (Atomic Check aspects reflected here: Actionable Artifact, Failure Data)
        /// Unlike class CheckRunData, class CheckFailData has two overloaded methods for adding named objects, to support 
        ///  data hierarchies for reporting, e.g., check steps or exception stacks.
        /// This overload is for a simple name/value pair.
        /// See also Note 3b below.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, string value)
        {
            base.AddDataElement(name, value);
        }

        /// <summary>
        /// Note 3b (Atomic Check aspects reflected here: Actionable Artifact, Failure Data)
        /// Unlike class CheckRunData, class CheckFailData has two overloaded methods for adding named objects, to support 
        ///  data hierarchies for reporting, e.g., check steps or exception stacks.
        /// This overload is for a name and a collection of child data nodes, to support hierarchy.
        /// See also Note 3a above
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, XElement childElement)
        {
            base.AddDataElement(name, childElement);
        }

        /// <summary>
        /// Note 4 (Atomic Check aspects reflected here: Failure Data)
        /// The data serializes to XML at check completion for artifact storage as a file, but it could just as easily be 
        ///  stored to a relational database. The artifact data is strongly typed and without presentation so it can 
        ///  display in different ways and it is queryable for the Smart Retry and Automated Triage patterns.
        /// </summary>
        public XElement XElementFailData
        {
            get
            {
                return base.m_BaseElementForSection;
            }
        }

        public void AddExceptionInformation(Exception ex, XElement exceptionElement = null)
        {
            if (ex == null)
            {
                throw new CheckInfrastructureClientException("The exception parameter must not be null.");
            }

            if (exceptionElement == null)
            {
                // Default to exception information base
                exceptionElement  = base.m_BaseElementForSection;
            }

            // add element for the exception name
            exceptionElement.Add(new XElement(DataStringConstants.ElementNames.DataElement,
                new XAttribute(DataStringConstants.AttributeNames.Name, CheckConstants.AttributeValues.ExceptionTypeName),
                new XAttribute(DataStringConstants.AttributeNames.Value, ex.GetType().ToString())));

            // add element for the exception message
            exceptionElement.Add(new XElement(DataStringConstants.ElementNames.DataElement,
                new XAttribute(DataStringConstants.AttributeNames.Name, CheckConstants.AttributeValues.ExceptionMessage),
                new XAttribute(DataStringConstants.AttributeNames.Value, ex.Message)));

            // add stack trace
            XElement stackTraceBaseElement = new XElement(DataStringConstants.ElementNames.DataElement,
                new XAttribute(DataStringConstants.AttributeNames.Name, CheckConstants.AttributeValues.ExceptionStackTraceName));

            exceptionElement.Add(stackTraceBaseElement);
            this.AddStackTraceCollection(stackTraceBaseElement, ex.StackTrace);

            if (ex.InnerException != null)
            {
                // Recursion happens here in case of an inner exception.
                // Create XElement first
                XElement innerExceptionBaseElement = new XElement(DataStringConstants.ElementNames.DataElement,
                    new XAttribute(DataStringConstants.AttributeNames.Name, CheckConstants.AttributeValues.ExceptionInnerExceptionName));

                exceptionElement.Add(innerExceptionBaseElement);
                this.AddExceptionInformation(ex.InnerException, innerExceptionBaseElement);
            }
        }

        /// <summary>
        /// This method gets the stack trace and breaks up the string into stack frames by index. By convention, index 0 is
        ///  the current frame (where the exception was caught, and not re-thrown), and the highest index is the origin of 
        ///  the exception.
        /// </summary>
        /// <param name="stackTrace"></param>
        /// <returns></returns>
        private void AddStackTraceCollection(XElement el, string stackTrace)
        {
            if (el == null)
            {
                throw new CheckInfrastructureClientException("the XElement to contain the stack trace must not be null.");
            }

            if (string.IsNullOrEmpty(stackTrace))
            {
                throw new CheckInfrastructureClientException("The stackTrace string must not be null or empty.");
            }

            string[] frames = stackTrace.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < frames.Length; i++)
            {
                // The format specifier is to help sorting of the stack frames, and assumes that the number of stack frames is never > 99
                //  i.e. this assumes that the stack frame index is always a one-digit or two-digit number
                el.Add(new XElement(DataStringConstants.ElementNames.DataElement,
                    new XAttribute(DataStringConstants.AttributeNames.Name, i.ToString("D2")),
                    new XAttribute(DataStringConstants.AttributeNames.Value, frames[i].Trim())));
            }
        }
    }
}
