////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationServiceMtLibrary
{
    using MetaAutomationBaseMtLibrary;

    public class OSInstanceInfo : IOSInstanceInfo
    {
        private static OSInstanceInfo m_TheInstance = null;

        public static OSInstanceInfo Instance
        {
            get
            {
                if (OSInstanceInfo.m_TheInstance == null)
                {
                    OSInstanceInfo.m_TheInstance = new OSInstanceInfo();
                }

                return OSInstanceInfo.m_TheInstance;
            }
        }

        public bool IsOriginMachineLocalInstance(string uniqueLabelForCheckRunSegment)
        {
            return (CheckRunDataHandles.GetOriginMachineName(uniqueLabelForCheckRunSegment) == System.Net.Dns.GetHostName().ToUpper());
        }

        public bool IsOriginMachineLocalInstance(System.Xml.Linq.XDocument crl)
        {
            string originMachine = DataAccessors.GetCheckRunValue(crl, DataStringConstants.NameAttributeValues.OriginMachine);
            return (originMachine.ToUpper() == System.Net.Dns.GetHostName().ToUpper());
        }

        public bool IsDestinationMachineLocalInstance(string uniqueLabelForCheckRunSegment)
        {
            return (CheckRunDataHandles.GetDestinationMachineName(uniqueLabelForCheckRunSegment) == System.Net.Dns.GetHostName().ToUpper());
        }

        public bool IsDestinationMachineLocalInstance(System.Xml.Linq.XDocument checkRun)
        {
            string destinationMachine = DataAccessors.GetCheckRunValue(checkRun, DataStringConstants.NameAttributeValues.DestinationMachine);
            return (destinationMachine.ToUpper() == System.Net.Dns.GetHostName().ToUpper());
        }

        public string GetOriginMachineName(string uniqueLabelForCheckRunSegment)
        {
            return CheckRunDataHandles.GetOriginMachineName(uniqueLabelForCheckRunSegment);
        }

        public string GetOriginMachineName(System.Xml.Linq.XDocument checkRun)
        {
            return DataAccessors.GetCheckRunValue(checkRun, DataStringConstants.NameAttributeValues.OriginMachine);
        }

        public string GetDestinationMachineName(string uniqueLabelForCheckRunSegment)
        {
            return CheckRunDataHandles.GetDestinationMachineName(uniqueLabelForCheckRunSegment);
        }

        public string GetDestinationMachineName(System.Xml.Linq.XDocument checkRun)
        {
            return DataAccessors.GetCheckRunValue(checkRun, DataStringConstants.NameAttributeValues.DestinationMachine);
        }
    }
}
