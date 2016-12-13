////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using System;
    using System.Threading;

    /// <summary>
    /// Note 1 (Atomic Check aspects reflected here: Check Steps)
    /// This step encapsulates running a check step, so the flight-recorder log reflects the steps and in case of check
    ///  failure, the artifacts include information on the steps. This is much more powerful than code comments, because
    ///  the steps are recorded in the log and in the artifacts in the correct hierarchy to point to root cause in case of
    ///  check failure.
    /// </summary>
    internal class CheckStepRunner
    {
        private CheckMethodStepRecords m_CheckStepRecords = null;

        public CheckStepRunner(CheckMethodStepRecords checkStepRecords)
        {
            m_CheckStepRecords = checkStepRecords;
        }

        /// <summary>
        /// Note 2 (Atomic Check aspects reflected here: Check Steps)
        /// For doing any sort of automated analysis or triage of the check artifact, it is important to have a stable name
        ///  for the step. The name of the step is not the place to insert any data that might be specific to a given check
        ///  run. This is important for potential future implementation of patterns Smart Retry and Automated Triage. If 
        ///  there is data specific to a check run that is useful for determining root cause, e.g., some data from an 
        ///  external system, this can be added with methods SetCustomData or AddCheckFailData of class CheckRunArtifact. 
        ///  For purposes of analysis, the name for the name/value pair must be stable. If needed to help development, 
        ///  notes can be logged separately with class CheckRunLog for development time logging.
        /// </summary>
        /// <param name="stepName"></param>
        /// <param name="stepCode"></param>
        public void DoStep(string stepName, Action stepCode)
        {
            // The ‘using’ statement manages the lifecycle of the step. The reason this implementation has the using
            //  block in a separate method is due to an implementation compromise for platform independence.
            using (CheckStep step = new CheckStep(this.m_CheckStepRecords, stepName))
            {
                try
                {
#if DEBUG
                    if (step.FailTheCheckAtThisStep)
                    {
                        throw new CheckFailException("Throwing Exception due to instruction in check run artifact.");
                    }
#endif
                    stepCode();
                }
                catch (ThreadAbortException ex)
                {
                    step.ExceptionThrown = true;

                    // Stop the thread abort
                    Thread.ResetAbort();

                    // If this thread was aborted, the exception and stack will not specify why it was aborted, so this 
                    //  implementation wraps the exception with such a message and re-throws. No information is lost.
                    throw new
                        CheckFailException(
                        string.Format("Step '{0}' was aborted with message '{1}'",
                        step.Name, this.m_CheckStepRecords.CheckTimeoutAbortMessage), ex);
                }
                catch (Exception /*ex */)
                {
                    step.ExceptionThrown = true;
                    throw;
                }
            }
        }
    }
}
