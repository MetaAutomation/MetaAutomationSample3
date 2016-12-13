////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System;

    public static class CheckDataValidation
    {
        /// <summary>
        /// Determines whether a string represents a valid CheckRunArtifact.
        /// This method will not throw an exception.
        /// </summary>
        /// <param name="craCandidate">CRA string</param>
        /// <returns>True if this is a valid CheckRunArtifact</returns>
        public static bool IsValidCheckRunArtifact(string craCandidate)
        {
            bool result = false;

            try
            {
                DataValidation.Instance.ValidateCheckRunArtifactIntoXDocument(craCandidate);
                result = true;
            }
            catch (Exception)
            {
                // do nothing in this case
            }

            return result;
        }
    }
}
