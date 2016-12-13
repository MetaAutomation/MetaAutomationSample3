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
    /// Note 1 (Atomic Check aspects reflected here: Distinct Artifacts, Check Steps)
    /// A check step is useful due to the procedural nature of a user story (first the user does this, then that, etc.) and
    ///  this class supports nested steps so the steps can add detail to the artifact that helps identify root cause. The 
    ///  steps’ begin and end stages are also reported to the log, to help at check development or maintenance time.
    /// </summary>
    internal sealed class CheckStep : IDisposable
    {
        private static object m_lockObjectForSteps = new object();
        private CheckMethodStepRecords m_CheckStepRecords = null;
        private static int m_StepDepth = 0;
        private Timer m_timeoutTimer = null;
        private uint m_timeoutMS = 0;
        private DateTime m_StepBegin = DateTime.Now;
        private bool m_FailBeforeCheckStepCode = false;

        string m_CheckStepName = null;
        Thread m_StepThread = null;

        public CheckStep(CheckMethodStepRecords checkStepRecords, string checkStepName)
        {
            this.m_StepBegin = DateTime.Now;

            lock (m_lockObjectForSteps)
            {
                m_CheckStepRecords = checkStepRecords;
                m_CheckStepName = checkStepName;
                m_StepThread = Thread.CurrentThread;
                CheckStep.m_StepDepth++;

                // Note 2 (Atomic Check aspects reflected here: Check Steps)
                // The step is tracked here for step begin.
#if DEBUG
        this.m_FailBeforeCheckStepCode =
#endif
                m_CheckStepRecords.BeginStep(checkStepName, out this.m_timeoutMS);

                // Only enable timeouts if the thread is not STA. Timeouts for steps won't work for STA
                if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
                {
                    TimerCallback m_TimoutCallback = this.TimeOut;
                    this.m_timeoutTimer = new Timer(m_TimoutCallback, null, this.m_timeoutMS, Timeout.Infinite /*Send only one timeout event*/);
                }

                AbortMessage = string.Empty;
            }
        }

#if DEBUG
        public bool FailTheCheckAtThisStep
        {
            get
            {
                return this.m_FailBeforeCheckStepCode;
            }
        }
#endif
        public string Name
        {
            get
            {
                return m_CheckStepName;
            }
        }

        /// <summary>
        /// The TimeOut method is a precaution to time the check step out, if something else does not time out first. This
        ///  is to prevent the check from hanging longer than desired, or to have more control over the timeout interval 
        ///  for any check step, in case of an unexpectedly long operation. This only works if the aborted thread is 
        ///  actively doing something, though, so, e.g., if a thread is waiting on a network response the effective 
        ///  timeout will still effectively be as set for the network request.
        /// </summary>
        /// <param name="o"></param>
        void TimeOut(object o)
        {
            // Only throw a timeout if an exception is not already being handled.
            if (!ExceptionThrown)
            {
                if (m_StepThread.IsAlive)
                {
                    string abortMessage = string.Format("Aborting the check step '{0}' due to CheckStep timeout of {1}ms.",
                       this.m_CheckStepName,
                       this.m_timeoutMS);
                    this.m_CheckStepRecords.CheckTimeoutAbortMessage = abortMessage;
                    this.AttemptAbortOfStep(abortMessage);
                }
            }
        }

        public bool ExceptionThrown
        {
            get
            {
                return this.m_CheckStepRecords.FailInitiatedInCheck;
            }
            set
            {
                this.m_CheckStepRecords.FailInitiatedInCheck = value;
            }
        }

        private void AttemptAbortOfStep(string abortMessage)
        {
            AbortMessage = abortMessage;
            this.ExceptionThrown = true;
            // This is how the step timeouts are implemented.
            // Comment the following line out to disable timeouts set by the step infrastructure.
            m_StepThread.Abort();
        }

        public string AbortMessage { get; private set; }

        /// <summary>
        /// Note 3 (Atomic Check aspects reflected here: Check Steps)
        /// Dispose() happens if the step completes successfully, or if an exception is thrown.
        /// The two scenarios are represented below, depending on the value of the Boolean ExceptionThrown.
        /// </summary>
        void IDisposable.Dispose()
        {
            lock (m_lockObjectForSteps)
            {
                CheckStep.m_StepDepth--;

                // Dispose() the timer to cancel it.
                if (this.m_timeoutTimer != null)
                {
                    this.m_timeoutTimer.Dispose();
                    this.m_timeoutTimer = null;
                }

                // Get elapsed time for this step
                int msCount = (int)(DateTime.Now - this.m_StepBegin).TotalMilliseconds;

                if (ExceptionThrown)
                {
                    // Fail the current step, and set the value attributes for the correct steps to “fail” or “blocked.”
                    m_CheckStepRecords.SetCurrentStepResult(CheckConstants.StepResults.Fail, msCount);
                    m_CheckStepRecords.EndStep(false);
                }
                else
                {
                    // Pass the current step.
                    m_CheckStepRecords.SetCurrentStepResult(CheckConstants.StepResults.Pass, msCount);


                    // Note 4 (Atomic Check aspects reflected here: Check Steps)
                    // Given that there is not an exception being thrown out of the step, the check step has completed 
                    //  normally for this code block.
                    m_CheckStepRecords.EndStep(true);
                }
            }
        }
    }
}
