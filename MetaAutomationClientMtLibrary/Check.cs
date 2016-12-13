////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using System;

    /// <summary>
    /// class Check presents methods to be used from check implementation code.
    /// This class is mostly static, but privately maintains a single instance of CheckArtifact for the check run.
    /// There is only one check run in a given process.
    /// </summary>
    public static class Check
    {
        private static CheckArtifact m_CheckArtifactInstance = null;

        public static CheckArtifact CheckArtifactInstance
        {
            get
            {
                if (Check.m_CheckArtifactInstance == null)
                {
                    Check.m_CheckArtifactInstance = new CheckArtifact();
                }

                return Check.m_CheckArtifactInstance;
            }
        }

        public static void Step(string stepName, System.Action stepCode)
        {
            Check.CheckArtifactInstance.DoStep(stepName, stepCode);
        }

        public static uint StepTimeout
        {
            get
            {
                return Check.CheckArtifactInstance.StepTimeout;
            }
        }

        public static void CallSubCheck(int oneBasedIndex)
        {
            Check.CheckArtifactInstance.CallSubCheck(oneBasedIndex);
        }

        public static void ReportFailureData(Exception ex)
        {
            Check.CheckArtifactInstance.AddCheckExceptionInformation(ex);
        }

        public static void AddCheckFailData(string name, string value)
        {
            Check.CheckArtifactInstance.AddCheckFailData(name, value);
        }

        public static string GetCustomDataCheckGlobal(string name)
        {
            return Check.CheckArtifactInstance.GetCustomData(name);
        }

        public static void SetCustomDataCheckGlobal(string name, string value)
        {
            Check.CheckArtifactInstance.SetCustomData(name, value);
        }

        public static void SetCustomDataCheckStep(string name, string value)
        {
            Check.CheckArtifactInstance.SetCustomDataCheckStep(name, value);
        }
    }
}
