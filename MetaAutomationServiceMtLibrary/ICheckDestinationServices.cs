////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationServiceMtLibrary
{
    using System.Xml.Linq;

    public interface ICheckDestinationServices
    {
        string StartCheckRun(XDocument checkRunLaunch);
		string GetCheckRunLaunch(string uniqueLabelForCheckRunSegment);
        string AbortCheckRun(string uniqueLabelForCheckRunSegment, string errorMessage);
    }
}
