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
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Xsl;
    using System.Xml.XPath;

    public class CheckRunTransforms
    {
        private static XslCompiledTransform m_XslCompiledTransformCraToCrl = null;
        private static object m_LockObjectXslCompiledTransformCraToCrl = null;

        private static XslCompiledTransform m_XslCompiledTransformCrlToCra = null;
        private static object m_LockObjectXslCompiledTransformCrlToCra = null;

        private static XslCompiledTransform m_XslCompiledTransformCraToSubCrl = null;
        private static object m_LockObjectXslCompiledTransformCraToSubCrl = null;

        static CheckRunTransforms()
        {
            CheckRunTransforms.m_LockObjectXslCompiledTransformCraToCrl = new object();
            CheckRunTransforms.m_LockObjectXslCompiledTransformCraToSubCrl = new object();
            CheckRunTransforms.m_LockObjectXslCompiledTransformCrlToCra = new object();
        }

        public CheckRunTransforms()
        {
        }

        public XDocument ConvertCheckRunLaunchToCheckRunArtifact(XDocument crl)
        {
            DataValidation.Instance.ValidateCheckRunLaunch(crl);
            XDocument craXDocumentResult = new XDocument();

            // Transform CRL to CRA
            using (XmlWriter xmlWriter = craXDocumentResult.CreateWriter())
            {
                CheckRunTransforms checkRunTransforms = new CheckRunTransforms();
                XslCompiledTransform xslTransform = checkRunTransforms.CrlToCraTransform;
                xslTransform.Transform(crl.CreateReader(), xmlWriter);
            }

            // Remove all the extra whitespace text nodes. This works around what seems to be a bug in the XSL implementation
            var nodeIterator = from node in craXDocumentResult.Root.DescendantNodes() where (node.NodeType == XmlNodeType.Text) select node;
            List<XNode> textNodesToRemove = new List<XNode>();

            foreach (XNode textNode in nodeIterator)
            {
                textNodesToRemove.Add(textNode);
            }

            foreach (XNode textNode in textNodesToRemove)
            {
                textNode.Remove();
            }

            return craXDocumentResult;
        }

        public XDocument ConvertCheckRunArtifactToCheckRunLaunch(XDocument cra)
        {
            XDocument crlXDocumentResult = new XDocument();

            using (XmlWriter xmlWriter = crlXDocumentResult.CreateWriter())
            {
                XslCompiledTransform xslTransform = this.CraToCrlTransform;
                xslTransform.Transform(cra.CreateReader(), xmlWriter);
            }

            return crlXDocumentResult;
        }

        /// <summary>
        /// Creates a check run launch for a sub-check call, by modifying the XML for the appropriate target check steps, the 
        ///     correct check run data, and the sub-check children of the target sub-check to set up for iteration
        /// </summary>
        /// <param name="craOriginalCopy">original copy, for reference</param>
        /// <param name="originalCurrentSubCheck">original current sub-check, for reference</param>
        /// <param name="originalCurrentStep">original current check step, for reference</param>
        /// <returns></returns>
        public XDocument CreateSubCheckRunLaunch(XDocument craOriginalCopy, XElement originalCurrentSubCheck, XElement originalCurrentStep)
        {
            // Transform just like creating a CRL for a check, but with these differences:
            // 1. For step data, only include step data below the current step
            // 2. For CheckrunData:
            //          overwrite with  name/value pairs from the SubCheck
            //          move into place the only include subcheck that is child of current subject and recursive children
            // 4. transform to preserve all CheckCustomData

            XDocument craWorkingCopy = new XDocument(craOriginalCopy);

            // Fix steps
            string currentStepXPath = GetRootXPathForElementInCRA(originalCurrentStep);
            XElement workingCopyStepRootElement = craWorkingCopy.Root.Element(DataStringConstants.ElementNames.CompleteCheckStepInfo);

            // clean steps from working copy
            workingCopyStepRootElement.RemoveAll();

            // copy the relevant part of the step hierarchy from the original to the working copy
            foreach (XElement childSteps in originalCurrentStep.Elements())
            {
                workingCopyStepRootElement.Add(childSteps);
            }

            // Fix CheckRunData settings for the subcheck call...

            // use xpath from the original check run artifact (CRA) to select the target subcheck in the working copy Xdoc
            string currentSubCheckXPath = GetRootXPathForElementInCRA(originalCurrentSubCheck);
            XElement currentSubCheckWorkingCopy = craWorkingCopy.XPathSelectElement(currentSubCheckXPath);

            // Get the CheckRunData root in the working copy
            XElement checkRunDataRootWorkingCopy = craWorkingCopy.Root.Element(DataStringConstants.ElementNames.CheckRunData);


            // Copy data as needed from the subcheck into the current check of the working copy CRA
            foreach (XElement dataElement in currentSubCheckWorkingCopy.Elements(DataStringConstants.ElementNames.DataElement))
            {
                this.AddSafeDataElement(checkRunDataRootWorkingCopy,
                    dataElement.Attribute(DataStringConstants.AttributeNames.Name).Value,
                    dataElement.Attribute(DataStringConstants.AttributeNames.Value).Value);
            }

            // Remove all SubChecks from the CheckRunData XElement
            // First, make a list, to not disturb the iterator
            List<XElement> elementsToRemove = new List<XElement>();

            foreach (XElement subCheckWorkingCopyElement in checkRunDataRootWorkingCopy.Elements(DataStringConstants.ElementNames.SubCheckData))
            {
                elementsToRemove.Add(subCheckWorkingCopyElement);
            }

            // Second, remove the items
            foreach (XElement elementToRemove in elementsToRemove)
            {
                elementToRemove.Remove();
            }

            // re-add all children of the current target subcheck as children of the CheckRunData XElement, 
            //  to create the CRA as it will look to the target subecheck
            foreach (XElement currentSubCheckChildSubCheck in currentSubCheckWorkingCopy.Elements(DataStringConstants.ElementNames.SubCheckData))
            {
                checkRunDataRootWorkingCopy.Add(currentSubCheckChildSubCheck);
            }

            // Do the transform to make CRL from CRA
            XDocument subCheckLaunchXDocumentResult = new XDocument();

            using (XmlWriter xmlWriter = subCheckLaunchXDocumentResult.CreateWriter())
            {
                XslCompiledTransform xslTransform = this.CraToSubCrlTransform;
                xslTransform.Transform(craWorkingCopy.CreateReader(), xmlWriter);
            }

            string id = CheckRunDataHandles.CreateIdFromCrxXDocument(subCheckLaunchXDocumentResult);

            this.AddSafeDataElement(
                subCheckLaunchXDocumentResult.Root.Element(DataStringConstants.ElementNames.CheckRunData),
                DataStringConstants.NameAttributeValues.CheckObjectStorageKey, id);

            DataValidation.Instance.ValidateCheckRunLaunch(subCheckLaunchXDocumentResult);

            return subCheckLaunchXDocumentResult;
        }

        private string GetRootXPathForElementInCRA(XElement element)
        {
            StringBuilder result = new StringBuilder();

            XElement currentElement = element;

            while (currentElement != null)
            {
                // insert brackets with index if needed for xpath
                string elementName = currentElement.Name.ToString();
                XElement parentElement = currentElement.Parent;

                if (parentElement != null)
                {
                    if (parentElement.Elements(elementName).Count<XElement>() > 1)
                    {
                        int index = 1; // indexing in xpath is 1-based
                        var elementIterator = parentElement.Elements(elementName);

                        foreach (XElement peerElement in elementIterator)
                        {
                            if (peerElement == currentElement)
                            {
                                break;
                            }
                            else
                            {
                                index++;
                            }
                        }

                        result.Insert(0, ']');
                        result.Insert(0, index);
                        result.Insert(0, '[');
                    }
                }

                result.Insert(0, elementName);
                result.Insert(0, '/');
                currentElement = currentElement.Parent;
            }

            return result.ToString();
        }

        private XslCompiledTransform CraToCrlTransform
        {
            get
            {
                lock (m_LockObjectXslCompiledTransformCraToCrl)
                {
                    if (CheckRunTransforms.m_XslCompiledTransformCraToCrl == null)
                    {
                        CheckRunTransforms.m_XslCompiledTransformCraToCrl = new XslCompiledTransform();

                        try
                        {
                            m_XslCompiledTransformCraToCrl.Load(this.CheckRunArtifactToCheckRunLaunchStyleSheetXmlReader);
                        }
                        catch (Exception)
                        {
                            CheckRunTransforms.m_XslCompiledTransformCraToCrl = null;
                            throw;
                        }
                    }
                }

                return CheckRunTransforms.m_XslCompiledTransformCraToCrl;
            }
        }

        private XslCompiledTransform CrlToCraTransform
        {
            get
            {
                lock (m_LockObjectXslCompiledTransformCrlToCra)
                {
                    if (CheckRunTransforms.m_XslCompiledTransformCrlToCra == null)
                    {
                        CheckRunTransforms.m_XslCompiledTransformCrlToCra = new XslCompiledTransform();

                        try
                        {
                            m_XslCompiledTransformCrlToCra.Load(this.CheckRunLaunchToCheckRunArtifactStyleSheetXmlReader);
                        }
                        catch (Exception)
                        {
                            CheckRunTransforms.m_XslCompiledTransformCrlToCra = null;
                            throw;
                        }
                    }
                }

                return CheckRunTransforms.m_XslCompiledTransformCrlToCra;
            }
        }

        private XslCompiledTransform CraToSubCrlTransform
        {
            get
            {
                lock (m_LockObjectXslCompiledTransformCraToSubCrl)
                {
                    if (CheckRunTransforms.m_XslCompiledTransformCraToSubCrl == null)
                    {
                        CheckRunTransforms.m_XslCompiledTransformCraToSubCrl = new XslCompiledTransform();

                        try
                        {
                            m_XslCompiledTransformCraToSubCrl.Load(this.CheckRunArtifactToSubCheckRunLaunchStyleSheetXmlReader);
                        }
                        catch (Exception)
                        {
                            CheckRunTransforms.m_XslCompiledTransformCraToSubCrl = null;
                            throw;
                        }
                    }
                }

                return CheckRunTransforms.m_XslCompiledTransformCraToSubCrl;
            }
        }

        private void AddSafeDataElement(XElement parent, string name, string value)
        {
            XElement element = parent.XPathSelectElement(string.Format(
                "{0}[@{1}='{2}']",
                DataStringConstants.ElementNames.DataElement,
                DataStringConstants.AttributeNames.Name,
                name));

            if (element == null)
            {
                // Add a DataElement, but before any SubCheckData elements
                XElement subCheckDataElementToPrecede = parent.Element(DataStringConstants.ElementNames.SubCheckData);

                if (subCheckDataElementToPrecede == null)
                {
                    // There are no SubCheckData elements, so just add the new DataElement 
                    parent.Add(new XElement(
                        DataStringConstants.ElementNames.DataElement,
                        new XAttribute(DataStringConstants.AttributeNames.Name, name),
                        new XAttribute(DataStringConstants.AttributeNames.Value, value)));
                }
                else
                {
                    subCheckDataElementToPrecede.AddBeforeSelf(new XElement(
                        DataStringConstants.ElementNames.DataElement,
                        new XAttribute(DataStringConstants.AttributeNames.Name, name),
                        new XAttribute(DataStringConstants.AttributeNames.Value, value)));
                }
            }
            else
            {
                XAttribute valueAttribute = element.Attribute(DataStringConstants.AttributeNames.Value);
                valueAttribute.Value = value;
            }
        }

        private XmlReader CheckRunArtifactToCheckRunLaunchStyleSheetXmlReader
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(@"<?xml version='1.0' encoding='utf-8'?>");
                sb.Append(@"<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>");
                sb.Append(@"  <xsl:output method='xml' indent='no' encoding='utf-8' omit-xml-declaration='yes'/>");
                sb.Append(@"  <xsl:template match='/CheckRunArtifact'>");
                sb.Append(@"    <xsl:element name='CheckRunLaunch'>");
                sb.Append(@"      <xsl:apply-templates select='CheckRunData'/>");
                sb.Append(@"      <xsl:apply-templates select='CheckCustomData'/>");
                // Write CheckFailData as an empty element to CRL. All fail data is dropped, but the empty element is required.
                sb.Append(@"      <xsl:element name='CheckFailData'>");
                sb.Append(@"      </xsl:element>");
                sb.Append(@"      <xsl:element name='CompleteCheckStepInfo'>");
                sb.Append(@"        <xsl:apply-templates select='CompleteCheckStepInfo/CheckStep'/>");
                sb.Append(@"      </xsl:element>");
                sb.Append(@"    </xsl:element>");
                sb.Append(@"  </xsl:template>");
                sb.Append(@"  <xsl:template match='CheckStep'>");
                sb.Append(@"    <xsl:element name='CheckStep'>");
                sb.Append(@"      <xsl:attribute name='Name'>");
                sb.Append(@"        <xsl:value-of select='@Name'/>");
                sb.Append(@"      </xsl:attribute>");

                sb.Append(@"      <xsl:choose>");
                sb.Append(@"        <xsl:when test='@msTimeLimit'>");
                sb.Append(@"          <xsl:attribute name='msTimeLimit'>");
                sb.Append(@"            <xsl:value-of select='@msTimeLimit'/>");
                sb.Append(@"          </xsl:attribute>");
                sb.Append(@"        </xsl:when>");
                sb.Append(@"        <xsl:otherwise>");
                sb.Append(@"          <xsl:attribute name='msTimeLimit'>");

                sb.Append(@"            <xsl:value-of select='");
                sb.Append(DataStringConstants.NumericConstants.DefaultTimeoutMilliseconds.ToString());
                sb.Append(@"'/>");

                sb.Append(@"          </xsl:attribute>");
                sb.Append(@"        </xsl:otherwise>");
                sb.Append(@"      </xsl:choose>");

                sb.Append(@"      <xsl:if test='@CountDownToFail'>");
                sb.Append(@"        <xsl:attribute name='CountDownToFail'>");
                sb.Append(@"          <xsl:value-of select='@CountDownToFail'/>");
                sb.Append(@"        </xsl:attribute>");
                sb.Append(@"      </xsl:if>");

                sb.Append(@"      <xsl:if test='@FailCheckStep'>");
                sb.Append(@"        <xsl:attribute name='FailCheckStep'>");
                sb.Append(@"          <xsl:value-of select='@FailCheckStep'/>");
                sb.Append(@"        </xsl:attribute>");
                sb.Append(@"      </xsl:if>");

                sb.Append(@"      <xsl:apply-templates/>");
                sb.Append(@"    </xsl:element>");
                sb.Append(@"  </xsl:template>");

                // Copy all CheckCustomData, verbatim
                sb.Append(@"  <xsl:template match='CheckCustomData'>");
                sb.Append(@"    <xsl:copy-of select='.'/>");
                sb.Append(@"  </xsl:template>");

                // Copy the CheckRunData element and all child nodes, verbatim
                sb.Append(@"  <xsl:template match='CheckRunData'>");
                sb.Append(@"    <xsl:copy-of select='.'/>");
                sb.Append(@"  </xsl:template>");

                sb.Append(@"</xsl:stylesheet>");

                StringReader stringReader = new StringReader(sb.ToString());
                XmlReader xmlReader = XmlReader.Create(stringReader);
                return xmlReader;
            }
        }

        private XmlReader CheckRunLaunchToCheckRunArtifactStyleSheetXmlReader
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(@"<?xml version='1.0' encoding='utf-8'?>");
                sb.Append(@"<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>");
                sb.Append(@"  <xsl:output method='xml' indent='no' encoding='utf-8' omit-xml-declaration='yes'/>");
                sb.Append(@"  <xsl:template match='/CheckRunLaunch'>");

                // Note that the following processing instruction will cause a web browser to look for the stylesheet
                //  on opening the artifact from the check run.
                // If the specified stylesheet is not found, the browser may display blank, but view source of the page
                //  to see the data of the artifact.
                // Note: commented out for dev purposes. To display the web page presentation of an artifact,
                //  un-comment the following lines and check that the xsl stylesheet is placed so that a web
                //  browser can find it.
                //sb.Append(@"    <xsl:processing-instruction name='xml-stylesheet'>");
                //sb.Append(@"      href='CheckRunArtifact.xsl' type='text/xsl'");
                //sb.Append(@"    </xsl:processing-instruction>");

                sb.Append(@"    <xsl:element name='CheckRunArtifact'>");
                sb.Append(@"      <xsl:apply-templates select='CheckRunData'/>");
                sb.Append(@"      <xsl:element name='CheckCustomData'>");
                sb.Append(@"        <xsl:for-each select='CheckCustomData/DataElement'>");
                sb.Append(@"          <xsl:element name='DataElement'>");
                sb.Append(@"            <xsl:attribute name='Name'>");
                sb.Append(@"              <xsl:value-of select='@Name'/>");
                sb.Append(@"            </xsl:attribute>");
                sb.Append(@"            <xsl:attribute name='Value'>");
                sb.Append(@"              <xsl:value-of select='@Value'/>");
                sb.Append(@"            </xsl:attribute>");
                sb.Append(@"          </xsl:element>");
                sb.Append(@"        </xsl:for-each>");
                sb.Append(@"      </xsl:element>");
                sb.Append(@"      <xsl:element name='CheckFailData'/>");
                sb.Append(@"      <xsl:element name='CompleteCheckStepInfo'>");
                sb.Append(@"        <xsl:apply-templates select='CompleteCheckStepInfo/CheckStep'/>");
                sb.Append(@"      </xsl:element>");
                sb.Append(@"    </xsl:element>");
                sb.Append(@"  </xsl:template>");
                sb.Append(@"  <xsl:template match='CheckStep'>");
                sb.Append(@"    <xsl:element name='CheckStep'>");
                sb.Append(@"      <xsl:attribute name='Name'>");
                sb.Append(@"        <xsl:value-of select='@Name'/>");
                sb.Append(@"      </xsl:attribute>");

                sb.Append(@"      <xsl:attribute name='msTimeLimit'>");
                sb.Append(@"        <xsl:value-of select='@msTimeLimit'/>");
                sb.Append(@"      </xsl:attribute>");

                sb.Append(@"      <xsl:if test='@CountDownToFail'>");
                sb.Append(@"        <xsl:attribute name='CountDownToFail'>");
                sb.Append(@"          <xsl:value-of select='@CountDownToFail'/>");
                sb.Append(@"        </xsl:attribute>");
                sb.Append(@"      </xsl:if>");

                sb.Append(@"      <xsl:if test='@FailCheckStep'>");
                sb.Append(@"        <xsl:attribute name='FailCheckStep'>");
                sb.Append(@"          <xsl:value-of select='@FailCheckStep'/>");
                sb.Append(@"        </xsl:attribute>");
                sb.Append(@"      </xsl:if>");

                sb.Append(@"      <xsl:apply-templates/>");
                sb.Append(@"    </xsl:element>");
                sb.Append(@"  </xsl:template>");

                // Copy the CheckRunData element and all child nodes, verbatim
                sb.Append(@"  <xsl:template match='CheckRunData'>");
                sb.Append(@"    <xsl:copy-of select='.'/>");
                sb.Append(@"  </xsl:template>");

                sb.Append(@"</xsl:stylesheet>");

                StringReader stringReader = new StringReader(sb.ToString());
                XmlReader xmlReader = XmlReader.Create(stringReader);
                return xmlReader;
            }
        }

        private XmlReader CheckRunArtifactToSubCheckRunLaunchStyleSheetXmlReader
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(@"<?xml version='1.0' encoding='utf-8'?>");
                sb.Append(@"<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>");
                sb.Append(@"  <xsl:output method='xml' indent='no' encoding='utf-8' omit-xml-declaration='yes'/>");
                sb.Append(@"  <xsl:template match='/CheckRunArtifact'>");
                sb.Append(@"    <xsl:element name='CheckRunLaunch'>");
                sb.Append(@"      <xsl:apply-templates select='CheckRunData'/>");
                sb.Append(@"      <xsl:apply-templates select='CheckCustomData'/>");

                // Write CheckFailData as an empty element to CRL. All fail data is dropped, but the empty element is required.
                sb.Append(@"      <xsl:element name='CheckFailData'>");
                sb.Append(@"      </xsl:element>");
                sb.Append(@"      <xsl:element name='CompleteCheckStepInfo'>");
                sb.Append(@"        <xsl:apply-templates select='CompleteCheckStepInfo/CheckStep'/>");
                sb.Append(@"      </xsl:element>");
                sb.Append(@"    </xsl:element>");
                sb.Append(@"  </xsl:template>");
                sb.Append(@"  <xsl:template match='CheckStep'>");
                sb.Append(@"    <xsl:element name='CheckStep'>");
                sb.Append(@"      <xsl:attribute name='Name'>");
                sb.Append(@"        <xsl:value-of select='@Name'/>");
                sb.Append(@"      </xsl:attribute>");

                sb.Append(@"      <xsl:choose>");
                sb.Append(@"        <xsl:when test='@msTimeLimit'>");
                sb.Append(@"          <xsl:attribute name='msTimeLimit'>");
                sb.Append(@"            <xsl:value-of select='@msTimeLimit'/>");
                sb.Append(@"          </xsl:attribute>");
                sb.Append(@"        </xsl:when>");
                sb.Append(@"        <xsl:otherwise>");
                sb.Append(@"          <xsl:attribute name='msTimeLimit'>");

                sb.Append(@"            <xsl:value-of select='");
                sb.Append(DataStringConstants.NumericConstants.DefaultTimeoutMilliseconds.ToString());
                sb.Append(@"'/>");

                sb.Append(@"          </xsl:attribute>");
                sb.Append(@"        </xsl:otherwise>");
                sb.Append(@"      </xsl:choose>");

                sb.Append(@"      <xsl:if test='@CountDownToFail'>");
                sb.Append(@"        <xsl:attribute name='CountDownToFail'>");
                sb.Append(@"          <xsl:value-of select='@CountDownToFail'/>");
                sb.Append(@"        </xsl:attribute>");
                sb.Append(@"      </xsl:if>");

                sb.Append(@"      <xsl:if test='@FailCheckStep'>");
                sb.Append(@"        <xsl:attribute name='FailCheckStep'>");
                sb.Append(@"          <xsl:value-of select='@FailCheckStep'/>");
                sb.Append(@"        </xsl:attribute>");
                sb.Append(@"      </xsl:if>");

                sb.Append(@"      <xsl:apply-templates/>");
                sb.Append(@"    </xsl:element>");
                sb.Append(@"  </xsl:template>");

                // Copy the CheckRunData element and all child nodes, verbatim
                sb.Append(@"  <xsl:template match='CheckRunData'>");
                sb.Append(@"    <xsl:copy-of select='.'/>");
                sb.Append(@"  </xsl:template>");

                // Copy the CheckCustomData element and all child nodes, verbatim
                sb.Append(@"  <xsl:template match='CheckCustomData'>");
                sb.Append(@"    <xsl:copy-of select='.'/>");
                sb.Append(@"  </xsl:template>");

                sb.Append(@"</xsl:stylesheet>");

                StringReader stringReader = new StringReader(sb.ToString());
                XmlReader xmlReader = XmlReader.Create(stringReader);
                return xmlReader;
            }
        }
    }
}
