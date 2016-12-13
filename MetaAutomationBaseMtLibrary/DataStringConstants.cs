////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationBaseMtLibrary
{
    /// <summary>
    /// This class contains all of the strings used for the XML data.
    /// </summary>
    public static class DataStringConstants
    {
        public static class ElementNames
        {
            // base elements to define a check run launch (CRL) or check run artifact (CRA)
            public const string CheckRunLaunch = "CheckRunLaunch";
            public const string CheckRunArtifact = "CheckRunArtifact";

            // required at least as empty elements, in this document order
            public const string CheckRunData = "CheckRunData";
            public const string CheckCustomData = "CheckCustomData";
            public const string CheckFailData = "CheckFailData";
            public const string CompleteCheckStepInfo = "CompleteCheckStepInfo";

            // other elements
            public const string SubCheckData = "SubCheckData";
            public const string DataElement = "DataElement";
            public const string CheckStepInformation = "CheckStep"; 
        }

        public static class AttributeNames
        {
            public const string Name = "Name";
            public const string Value = "Value";
            public const string TimeElapsed = "msTimeElapsed";
            public const string TimeLimit = "msTimeLimit";
            public const string MachineName = "MachineName";
            public const string CountDownToFail = "CountDownToFail";
            public const string FailCheckStep = "FailCheckStep";
        }

        public static class NameAttributeValues
        {
            public const string CheckObjectStorageKey = "CheckObjectStorageKey";
            public const string PathAndFileToRunner = "PathAndFileToRunner";
            public const string CheckLibraryAssembly = "CheckLibraryAssembly";

            public const string DestinationMachine = "DestinationMachine";
            public const string OriginMachine = "OriginMachine";

            public const string CheckJobSpecGuid = "CheckJobSpecGuid";
            public const string CheckJobRunGuid = "CheckJobRunGuid";
            public const string CheckRunGuid = "CheckRunGuid";
            public const string CheckMethodName = "CheckMethodName";

            public const string CheckMethodGuid = "CheckMethodGuid";

            public const string CheckClientUser = "CheckClientUser";
            public const string CheckBeginTime = "CheckBeginTime";
            public const string CheckEndTime = "CheckEndTime";

            public const string CheckUserName = "CheckUserName";

            // The MetaAutomationService and the MetaAutomationServiceMtLibrary run in services 
            //  as a user determined by system configuration. This name is used to define the user
            //  (and the domain, as needed) as whom the service runs, probably as the thread pool
            //  used for the service. The value is used to define access to the named semaphore used for synchronization.
            public const string ThreadPoolUserName = "ThreadPoolUserName";

            public const string SemaphoreTimeOutMilliseconds = "SemaphoreTimeOutMilliseconds";

            public const string Reserved_SubCheckMap = "Reserved_SubCheckMap";

        }

        public static class StatusString
        {
            public static string DefaultServiceSuccessMessage = "Success.";
        }

        public static class NumericConstants
        {
            public static uint DefaultTimeoutMilliseconds = 30000; // 30 seconds
        }
    }
}
