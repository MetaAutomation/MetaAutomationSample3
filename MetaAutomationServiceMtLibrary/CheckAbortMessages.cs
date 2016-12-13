////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationServiceMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System.Collections.Generic;

    internal class CheckAbortMessages
    {
        private static CheckAbortMessages m_TheInstance = null;
        private static object m_InstanceLock = new object();
        private Dictionary<string, string> m_Errors = null;
        private object m_CheckErrorsLockObject = null;

        public static CheckAbortMessages Instance
        {
            get
            {
                lock (m_InstanceLock)
                {
                    if (CheckAbortMessages.m_TheInstance == null)
                    {
                        CheckAbortMessages.m_TheInstance = new CheckAbortMessages();
                    }
                }

                return CheckAbortMessages.m_TheInstance;
            }
        }

        private CheckAbortMessages()
        {
            m_Errors = new Dictionary<string, string>();
            m_CheckErrorsLockObject = new object();
        }

        public void SetMessage(string uniqueLabelForCheckRunSegment, string message)
        {
            string cleanedID = CheckRunDataHandles.StripIdOfMachineNames(uniqueLabelForCheckRunSegment);

            lock (m_CheckErrorsLockObject)
            {
                m_Errors.Add(cleanedID, message);
            }
        }

        public void ClearMessages()
        {
            lock (m_CheckErrorsLockObject)
            {
                m_Errors.Clear();
            }
        }

        public string GetMessage(string uniqueLabelForCheckRunSegment)
        {
            string result = null;
            string cleanedID = CheckRunDataHandles.StripIdOfMachineNames(uniqueLabelForCheckRunSegment);

            lock (m_CheckErrorsLockObject)
            {
                result = m_Errors[cleanedID];
            }

            return result;
        }
    }
}
