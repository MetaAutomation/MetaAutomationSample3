////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationServiceMtLibrary
{
    using System.Xml.Linq;

    public interface IOSInstanceInfo
    {
        bool IsOriginMachineLocalInstance(string uniqueLabelForCheckRunSegment);
        bool IsOriginMachineLocalInstance(XDocument checkRun);

        bool IsDestinationMachineLocalInstance(string uniqueLabelForCheckRunSegment);
        bool IsDestinationMachineLocalInstance(XDocument checkRun);

        string GetOriginMachineName(string uniqueLabelForCheckRunSegment);
        string GetOriginMachineName(XDocument checkRun);

        string GetDestinationMachineName(string uniqueLabelForCheckRunSegment);
        string GetDestinationMachineName(XDocument checkRun);
    }
}
