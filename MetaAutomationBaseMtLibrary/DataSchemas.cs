////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationBaseMtLibrary
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;

    public class DataSchemas
    {
        private static DataSchemas m_Instance = null;
        private static XmlSchemaSet m_CheckRunArtifactSchemaSet = null;
        private static object m_LockObjectCheckRunArtifactSchemaSet = null;

        private static XmlSchemaSet m_CheckRunLaunchSchemaSet = null;
        private static object m_LockObjectCheckRunLaunchSchemaSet = null;

        private DataSchemas()
        {
            m_LockObjectCheckRunArtifactSchemaSet = new object();
            m_LockObjectCheckRunLaunchSchemaSet = new object();
        }

        public static DataSchemas Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new DataSchemas();
                }

                return m_Instance;
            }
        }

        
        public XmlSchemaSet CheckRunArtifactSchemaSet
        {
            get
            {
                lock (m_LockObjectCheckRunArtifactSchemaSet)
                {
                    if (m_CheckRunArtifactSchemaSet == null)
                    {
                        try
                        {
                            StringBuilder checkRunArtifactSchema = new StringBuilder();

                            checkRunArtifactSchema.AppendLine(@"<?xml version='1.0' encoding='utf-8'?>");
                            checkRunArtifactSchema.AppendLine(@"<xs:schema attributeFormDefault='unqualified' elementFormDefault='qualified' xmlns:xs='http://www.w3.org/2001/XMLSchema'>");

                            checkRunArtifactSchema.AppendLine(@"  <xs:complexType name='SubCheckDataElementElementType'>");
                            checkRunArtifactSchema.AppendLine(@"    <xs:sequence>");

                            // DataElements must occur before SubCheckData elements in document order
                            checkRunArtifactSchema.AppendLine(@"      <xs:element name='DataElement' type='CheckRunDataElementElementType' minOccurs='0' maxOccurs='unbounded' />");
                            checkRunArtifactSchema.AppendLine(@"      <xs:element name='SubCheckData' type='SubCheckDataElementElementType' minOccurs='0' maxOccurs='unbounded' />");
                            checkRunArtifactSchema.AppendLine(@"    </xs:sequence>");
                            checkRunArtifactSchema.AppendLine(@"  </xs:complexType>");

                            checkRunArtifactSchema.AppendLine(@"  <xs:complexType name='CheckRunDataElementElementType'>");
                            checkRunArtifactSchema.AppendLine(@"    <xs:sequence>");
                            checkRunArtifactSchema.AppendLine(@"      <xs:element name='DataElement' type='CheckRunDataElementElementType' minOccurs='0' maxOccurs='unbounded' />");
                            checkRunArtifactSchema.AppendLine(@"      <xs:element name='SubCheckData' type='SubCheckDataElementElementType' minOccurs='0' maxOccurs='unbounded'/>");
                            checkRunArtifactSchema.AppendLine(@"    </xs:sequence>");
                            checkRunArtifactSchema.AppendLine(@"    <xs:attribute name='Name' use='required'>");
                            checkRunArtifactSchema.AppendLine(@"      <xs:simpleType>");
                            checkRunArtifactSchema.AppendLine(@"        <xs:restriction base='xs:string'>");
                            checkRunArtifactSchema.AppendLine(@"          <xs:pattern value='[\S]+'/>");
                            checkRunArtifactSchema.AppendLine(@"        </xs:restriction>");
                            checkRunArtifactSchema.AppendLine(@"      </xs:simpleType>");
                            checkRunArtifactSchema.AppendLine(@"    </xs:attribute>");
                            checkRunArtifactSchema.AppendLine(@"    <xs:attribute name='Value' use='required'/>");
                            checkRunArtifactSchema.AppendLine(@"  </xs:complexType>");

                            checkRunArtifactSchema.AppendLine(@"  <xs:complexType name='CheckFailDataElementType'>");
                            checkRunArtifactSchema.AppendLine(@"    <xs:choice>");
                            checkRunArtifactSchema.AppendLine(@"      <xs:element name='DataElement' type='CheckFailDataElementType' minOccurs='0' maxOccurs='unbounded' />");
                            checkRunArtifactSchema.AppendLine(@"    </xs:choice>");
                            checkRunArtifactSchema.AppendLine(@"    <xs:attribute name='Name' type='xs:string' use='required' />");
                            checkRunArtifactSchema.AppendLine(@"    <xs:attribute name='Value' type='xs:string' use='optional' />");
                            checkRunArtifactSchema.AppendLine(@"  </xs:complexType>");

                            checkRunArtifactSchema.AppendLine(@"  <xs:complexType name='CheckStepType'>");
                            checkRunArtifactSchema.AppendLine(@"    <xs:sequence>");
                            checkRunArtifactSchema.AppendLine(@"      <xs:element name='CheckStep' type='CheckStepType' minOccurs='0' maxOccurs='unbounded' />");
                            checkRunArtifactSchema.AppendLine(@"      <xs:element minOccurs='0' maxOccurs='unbounded' name='DataElement'>"); // for custom data in steps
                            checkRunArtifactSchema.AppendLine(@"        <xs:complexType>");
                            checkRunArtifactSchema.AppendLine(@"          <xs:attribute name='Name' use='required'/>");
                            checkRunArtifactSchema.AppendLine(@"          <xs:attribute name='Value' use='required'/>");
                            checkRunArtifactSchema.AppendLine(@"        </xs:complexType>");
                            checkRunArtifactSchema.AppendLine(@"      </xs:element>");
                            checkRunArtifactSchema.AppendLine(@"    </xs:sequence>");
                            checkRunArtifactSchema.AppendLine(@"    <xs:attribute name='Name' type='xs:string' use='required' />");
                            checkRunArtifactSchema.AppendLine(@"    <xs:attribute name='Value' type='xs:string' use='optional' />");
                            checkRunArtifactSchema.AppendLine(@"    <xs:attribute name='msTimeLimit' type='xs:unsignedInt' use='optional' />");
                            checkRunArtifactSchema.AppendLine(@"    <xs:attribute name='msTimeElapsed' type='xs:unsignedInt' use='optional' />");
                            checkRunArtifactSchema.AppendLine(@"    <xs:attribute name='MachineName' type='xs:string' use='optional' />");
                            checkRunArtifactSchema.AppendLine(@"    <xs:attribute name='CountDownToFail' type='xs:int' use='optional' />");
                            checkRunArtifactSchema.AppendLine(@"    <xs:attribute name='FailCheckStep' type='xs:boolean' use='optional' />");
                            checkRunArtifactSchema.AppendLine(@"  </xs:complexType>");
                            checkRunArtifactSchema.AppendLine(@"  <xs:element name='CheckRunArtifact'>");
                            checkRunArtifactSchema.AppendLine(@"    <xs:complexType>");
                            checkRunArtifactSchema.AppendLine(@"      <xs:sequence>");

                            checkRunArtifactSchema.AppendLine(@"        <xs:element name='CheckRunData'>");
                            checkRunArtifactSchema.AppendLine(@"          <xs:complexType>");
                            checkRunArtifactSchema.AppendLine(@"            <xs:sequence>");
                            checkRunArtifactSchema.AppendLine(@"              <xs:element minOccurs='1' maxOccurs='unbounded' name='DataElement' type='CheckRunDataElementElementType'/>");
                            checkRunArtifactSchema.AppendLine(@"              <xs:element minOccurs='0' maxOccurs='unbounded' name='SubCheckData' type='SubCheckDataElementElementType'/>");
                            checkRunArtifactSchema.AppendLine(@"            </xs:sequence>");
                            checkRunArtifactSchema.AppendLine(@"          </xs:complexType>");
                            checkRunArtifactSchema.AppendLine(@"          <xs:unique name='NoMoreThanOneValueForGivenName'>");
                            checkRunArtifactSchema.AppendLine(@"            <xs:selector xpath='DataElement'/>");
                            checkRunArtifactSchema.AppendLine(@"            <xs:field xpath='@Name'/>");
                            checkRunArtifactSchema.AppendLine(@"          </xs:unique>");
                            checkRunArtifactSchema.AppendLine(@"        </xs:element>");

                            checkRunArtifactSchema.AppendLine(@"        <xs:element name='CheckCustomData' minOccurs='1' maxOccurs='1'>");
                            checkRunArtifactSchema.AppendLine(@"          <xs:complexType>");
                            checkRunArtifactSchema.AppendLine(@"            <xs:sequence>");
                            checkRunArtifactSchema.AppendLine(@"              <xs:element minOccurs='0' maxOccurs='unbounded' name='DataElement'>");
                            checkRunArtifactSchema.AppendLine(@"                <xs:complexType>");
                            checkRunArtifactSchema.AppendLine(@"                  <xs:attribute name='Name' use='required'/>");
                            checkRunArtifactSchema.AppendLine(@"                  <xs:attribute name='Value' use='required'/>");
                            checkRunArtifactSchema.AppendLine(@"                </xs:complexType>");
                            checkRunArtifactSchema.AppendLine(@"              </xs:element>");
                            checkRunArtifactSchema.AppendLine(@"            </xs:sequence>");
                            checkRunArtifactSchema.AppendLine(@"          </xs:complexType>");
                            checkRunArtifactSchema.AppendLine(@"        </xs:element>");

                            checkRunArtifactSchema.AppendLine(@"        <xs:element name='CheckFailData' minOccurs='1' maxOccurs='1'>");
                            checkRunArtifactSchema.AppendLine(@"          <xs:complexType>");
                            checkRunArtifactSchema.AppendLine(@"            <xs:sequence>");
                            checkRunArtifactSchema.AppendLine(@"              <xs:element name='DataElement' type='CheckFailDataElementType' minOccurs='0' maxOccurs='unbounded'/>");
                            checkRunArtifactSchema.AppendLine(@"            </xs:sequence>");
                            checkRunArtifactSchema.AppendLine(@"          </xs:complexType>");
                            checkRunArtifactSchema.AppendLine(@"        </xs:element>");
                            checkRunArtifactSchema.AppendLine(@"        <xs:element name='CompleteCheckStepInfo' minOccurs='1' maxOccurs='1'>");
                            checkRunArtifactSchema.AppendLine(@"          <xs:complexType>");
                            checkRunArtifactSchema.AppendLine(@"            <xs:sequence>");
                            checkRunArtifactSchema.AppendLine(@"              <xs:element name='CheckStep' type='CheckStepType' minOccurs='0' maxOccurs='unbounded'/>");
                            checkRunArtifactSchema.AppendLine(@"            </xs:sequence>");
                            checkRunArtifactSchema.AppendLine(@"          </xs:complexType>");
                            checkRunArtifactSchema.AppendLine(@"        </xs:element>");
                            checkRunArtifactSchema.AppendLine(@"      </xs:sequence>");
                            checkRunArtifactSchema.AppendLine(@"    </xs:complexType>");
                            checkRunArtifactSchema.AppendLine(@"  </xs:element>");
                            checkRunArtifactSchema.AppendLine(@"</xs:schema>");

                            DataSchemas.m_CheckRunArtifactSchemaSet = new XmlSchemaSet();
                            XmlReader schemaReader = XmlReader.Create(new StringReader(checkRunArtifactSchema.ToString()));
                            DataSchemas.m_CheckRunArtifactSchemaSet.Add("", schemaReader);
                            DataSchemas.m_CheckRunArtifactSchemaSet.Compile();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }

                return DataSchemas.m_CheckRunArtifactSchemaSet;
            }
        }

        public XmlSchemaSet CheckRunLaunchSchemaSet
        {
            get
            {
                lock (m_LockObjectCheckRunLaunchSchemaSet)
                {
                    if (m_CheckRunLaunchSchemaSet == null)
                    {
                        try
                        {
                            StringBuilder checkRunLaunchSchema = new StringBuilder();

                            checkRunLaunchSchema.AppendLine(@"<?xml version='1.0' encoding='utf-8'?>"); // line 0
                            checkRunLaunchSchema.AppendLine(@"<xs:schema attributeFormDefault='unqualified' elementFormDefault='qualified' xmlns:xs='http://www.w3.org/2001/XMLSchema'>");
                            checkRunLaunchSchema.AppendLine(@"  <xs:simpleType name='CheckRunDataElementNameType'>");
                            checkRunLaunchSchema.AppendLine(@"    <xs:restriction base='xs:string'>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='CheckObjectStorageKey'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='PathAndFileToRunner'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='DestinationMachine'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='CheckClientUser'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='CheckJobSpecGuid'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='CheckJobRunGuid'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='CheckRunGuid'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='CheckMethodName'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='OriginMachine'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='CheckMethodGuid'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='ThreadPoolUserName'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='SemaphoreTimeOutMilliseconds'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='CheckLibraryAssembly'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='CheckBeginTime'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='CheckEndTime'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='Reserved_SubCheckMap'/>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:enumeration value='CheckObjectStorageKey'/>");
                            checkRunLaunchSchema.AppendLine(@"    </xs:restriction>");
                            checkRunLaunchSchema.AppendLine(@"  </xs:simpleType>");

                            checkRunLaunchSchema.AppendLine(@"  <xs:complexType name='SubCheckDataElementElementType'>"); // line 17
                            checkRunLaunchSchema.AppendLine(@"    <xs:sequence>");

                            // DataElements must occur before SubCheckData elements in document order
                            checkRunLaunchSchema.AppendLine(@"      <xs:element name='DataElement' type='CheckRunDataElementElementType' minOccurs='0' maxOccurs='unbounded' />");
                            checkRunLaunchSchema.AppendLine(@"      <xs:element name='SubCheckData' type='SubCheckDataElementElementType' minOccurs='0' maxOccurs='unbounded' />");
                            checkRunLaunchSchema.AppendLine(@"    </xs:sequence>");
                            checkRunLaunchSchema.AppendLine(@"  </xs:complexType>");

                            checkRunLaunchSchema.AppendLine(@"  <xs:complexType name='CheckRunDataElementElementType'>"); // line 23
                            checkRunLaunchSchema.AppendLine(@"    <xs:sequence>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:element name='DataElement' type='CheckRunDataElementElementType' minOccurs='0' maxOccurs='unbounded' />");
                            checkRunLaunchSchema.AppendLine(@"      <xs:element name='SubCheckData' type='SubCheckDataElementElementType' minOccurs='0' maxOccurs='unbounded'/>");
                            checkRunLaunchSchema.AppendLine(@"    </xs:sequence>");
                            checkRunLaunchSchema.AppendLine(@"    <xs:attribute name='Name' type='CheckRunDataElementNameType' use='required' form='qualified'/>");
                            checkRunLaunchSchema.AppendLine(@"    <xs:attribute name='Value' use='required'/>");
                            checkRunLaunchSchema.AppendLine(@"  </xs:complexType>");

                            checkRunLaunchSchema.AppendLine(@"  <xs:complexType name='CheckStepType'>"); // line 30
                            checkRunLaunchSchema.AppendLine(@"    <xs:sequence>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:element name='CheckStep' minOccurs='0' maxOccurs='unbounded' />");
                            checkRunLaunchSchema.AppendLine(@"    </xs:sequence>");
                            checkRunLaunchSchema.AppendLine(@"    <xs:attribute name='Name' type='xs:string' use='required' />");
                            checkRunLaunchSchema.AppendLine(@"    <xs:attribute name='msTimeLimit' type='xs:unsignedInt' use='required' />");
                            checkRunLaunchSchema.AppendLine(@"    <xs:attribute name='CountDownToFail' type='xs:int' use='optional' />");
                            checkRunLaunchSchema.AppendLine(@"    <xs:attribute name='FailCheckStep' type='xs:boolean' use='optional' />");
                            checkRunLaunchSchema.AppendLine(@"  </xs:complexType>");

                            checkRunLaunchSchema.AppendLine(@"  <xs:element name='CheckRunLaunch'>");
                            checkRunLaunchSchema.AppendLine(@"    <xs:complexType>");
                            checkRunLaunchSchema.AppendLine(@"      <xs:sequence>");

                            checkRunLaunchSchema.AppendLine(@"        <xs:element name='CheckRunData' minOccurs='1' maxOccurs='1'>");
                            checkRunLaunchSchema.AppendLine(@"          <xs:complexType>");
                            checkRunLaunchSchema.AppendLine(@"            <xs:sequence>");
                            checkRunLaunchSchema.AppendLine(@"              <xs:element minOccurs='1' maxOccurs='32' name='DataElement' type='CheckRunDataElementElementType'/>");
                            checkRunLaunchSchema.AppendLine(@"              <xs:element minOccurs='0' maxOccurs='unbounded' name='SubCheckData' type='SubCheckDataElementElementType'/>");
                            checkRunLaunchSchema.AppendLine(@"            </xs:sequence>");
                            checkRunLaunchSchema.AppendLine(@"          </xs:complexType>");
                            checkRunLaunchSchema.AppendLine(@"          <xs:unique name='NoMoreThanOneValueForGivenName'>");
                            checkRunLaunchSchema.AppendLine(@"            <xs:selector xpath='DataElement'/>");
                            checkRunLaunchSchema.AppendLine(@"            <xs:field xpath='@Name'/>");
                            checkRunLaunchSchema.AppendLine(@"          </xs:unique>");
                            checkRunLaunchSchema.AppendLine(@"        </xs:element>");

                            checkRunLaunchSchema.AppendLine(@"        <xs:element name='CheckCustomData' minOccurs='1' maxOccurs='1'>");
                            checkRunLaunchSchema.AppendLine(@"          <xs:complexType>");
                            checkRunLaunchSchema.AppendLine(@"            <xs:sequence>");
                            checkRunLaunchSchema.AppendLine(@"              <xs:element  minOccurs='0' maxOccurs='unbounded' name='DataElement'>");
                            checkRunLaunchSchema.AppendLine(@"                <xs:complexType>");
                            checkRunLaunchSchema.AppendLine(@"                  <xs:attribute name='Name' use='required'/>");
                            checkRunLaunchSchema.AppendLine(@"                  <xs:attribute name='Value' use='required'/>");
                            checkRunLaunchSchema.AppendLine(@"                </xs:complexType>");
                            checkRunLaunchSchema.AppendLine(@"              </xs:element>");
                            checkRunLaunchSchema.AppendLine(@"            </xs:sequence>");
                            checkRunLaunchSchema.AppendLine(@"          </xs:complexType>");
                            checkRunLaunchSchema.AppendLine(@"        </xs:element>");

                            checkRunLaunchSchema.AppendLine(@"        <xs:element name='CheckFailData' minOccurs='1' maxOccurs='1'>");
                            checkRunLaunchSchema.AppendLine(@"        </xs:element>");

                            checkRunLaunchSchema.AppendLine(@"        <xs:element name='CompleteCheckStepInfo' minOccurs='1' maxOccurs='1'>");
                            checkRunLaunchSchema.AppendLine(@"          <xs:complexType>");
                            checkRunLaunchSchema.AppendLine(@"            <xs:sequence>");
                            checkRunLaunchSchema.AppendLine(@"              <xs:element name='CheckStep' type='CheckStepType' minOccurs='0' maxOccurs='unbounded'/>");
                            checkRunLaunchSchema.AppendLine(@"            </xs:sequence>");
                            checkRunLaunchSchema.AppendLine(@"          </xs:complexType>");
                            checkRunLaunchSchema.AppendLine(@"        </xs:element>");
                            checkRunLaunchSchema.AppendLine(@"      </xs:sequence>");
                            checkRunLaunchSchema.AppendLine(@"    </xs:complexType>");
                            checkRunLaunchSchema.AppendLine(@"  </xs:element>");
                            checkRunLaunchSchema.AppendLine(@"</xs:schema>");

                            DataSchemas.m_CheckRunLaunchSchemaSet = new XmlSchemaSet();
                            XmlReader schemaReader = XmlReader.Create(new StringReader(checkRunLaunchSchema.ToString()));
                            DataSchemas.m_CheckRunLaunchSchemaSet.Add("", schemaReader);
                            DataSchemas.m_CheckRunLaunchSchemaSet.Compile();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }

                return DataSchemas.m_CheckRunLaunchSchemaSet;
            }
        }
    }
}
