////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System.Xml.Linq;

    /// <summary>
    /// This is a place to declare, define and organize constants, to reduce the risk of typos
    /// and concern about hard-coded strings in the code with minor differences, which could cause
    /// hard-to-find check failures. These could also go into a configuration file, if there is a
    /// need to change configuration after compile.
    /// </summary>
    static public class CheckConstants
    {


        public enum StepResults
        {
            Uninitialized = 0,
            Pass = 1,
            Fail = 2,
            Blocked = 3
        }

        static public class AttributeValues
        {
            public const string CheckClientUser = "CheckClientUser";
            public const string CheckFailureExceptionInformation = "ExceptionInformation";
            public const string CheckRetryRunGuid = "CheckRetryRunGuid";
            public const string CheckStepAtFailure = "Recursive check steps at failure";
            public const string CheckStepInnerStep = "InnerStep";
            public const string ExceptionInnerExceptionName = "InnerException";
            public const string ExceptionMessage = "ExceptionMessage";
            public const string ExceptionStackTraceName = "StackTrace";
            public const string ExceptionTypeName = "ExceptionType";
            public const string SubCheckExceptions = "SubCheckExceptions";
        }

        public delegate XDocument RunSubCheckDelegate(XDocument checkRunLaunch);
    }
}
