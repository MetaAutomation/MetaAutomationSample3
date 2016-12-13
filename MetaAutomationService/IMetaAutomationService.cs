////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationService
{
    using System.ServiceModel;

    [ServiceContract]
    public interface IMetaAutomationService
    {
        [OperationContract]
        string StartCheckRun(string checkRunLaunchXml);

        [OperationContract]
        string CompleteCheckRun(string checkRunArtifactXml);

        [OperationContract]
        string GetCheckRunArtifact(string uniqueLabelForCheckRunSegment);

        [OperationContract]
        string GetCheckRunLaunch(string uniqueLabelForCheckRunSegment);

        [OperationContract]
        string AbortCheckRun(string uniqueLabelForCheckRunSegment, string errorMessage);

        [OperationContract]
        string GetAbortMessage(string uniqueLabelForCheckRunSegment);
    }
}
