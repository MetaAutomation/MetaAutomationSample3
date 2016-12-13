////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationServiceMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Xml.Linq;

    /// <summary>
    /// This class manages short-term storage and validation of the XML for the CheckRunLaunch (CRL) objects and the CheckRunArtifact (CRA) objects.
    /// This class implements the Singleton pattern.
    /// </summary>
    internal class CrxDepot
    {
        #region publicMethods
        public static CrxDepot Instance
        {
            get
            {
                lock(m_TypeLockObject)
                {
                    if (CrxDepot.m_TheInstance == null)
                    {
                        CrxDepot.m_TheInstance = new CrxDepot();
                    }
                }

                return CrxDepot.m_TheInstance;
            }
        }

        public string SetCrlXDocument(XDocument crlXDoc)
        {
            string uniqueLabelForCheckRunSegment = CheckRunDataHandles.CreateIdFromCrxXDocument(crlXDoc);
            string truncatedId = CheckRunDataHandles.StripIdOfMachineNames(uniqueLabelForCheckRunSegment);


            lock (m_CheckRunLaunchLockObject)
            {
                m_CRL_XDocuments.Add(truncatedId, crlXDoc);
            }


            return uniqueLabelForCheckRunSegment;
        }

        public string SetCraXDocument(XDocument xDocCra)
        {
            string uniqueLabelForCheckRunSegment = CheckRunDataHandles.CreateIdFromCrxXDocument(xDocCra);
            string truncatedId = CheckRunDataHandles.StripIdOfMachineNames(uniqueLabelForCheckRunSegment);


            lock (m_CheckRunArtifactLockObject)
            {
                m_CRA_XDocuments.Add(truncatedId, xDocCra);
            }

            return uniqueLabelForCheckRunSegment;
        }

        public XDocument GetCraXDocument(string uniqueLabelForCheckRunSegment)
        {
            XDocument result = null;
            string truncatedId = CheckRunDataHandles.StripIdOfMachineNames(uniqueLabelForCheckRunSegment);


            try
            {
                lock (m_CheckRunArtifactLockObject)
                {
                    result = m_CRA_XDocuments[truncatedId];
                    m_CRA_XDocuments.Remove(truncatedId);
                }
            }
            catch (KeyNotFoundException ex)
            {
                string existingKeys = null;

                lock (m_CheckRunArtifactLockObject)
                {
                    existingKeys = this.ListCraKeys();
                }

                string message = string.Format("GetCraXDocument Failed to find a key. ID='{0}{1}{2}', truncated='{3}{4}{5}', existingkeys='{6}'", Environment.NewLine, uniqueLabelForCheckRunSegment, Environment.NewLine, Environment.NewLine, truncatedId, Environment.NewLine, existingKeys);
                throw new CheckInfrastructureServiceException(message, ex);
            }

            return result;
        }

        public XDocument GetCrlXDocument(string uniqueLabelForCheckRunSegment)
        {
            XDocument result = null;
            string truncatedId = CheckRunDataHandles.StripIdOfMachineNames(uniqueLabelForCheckRunSegment);

            try
            {
                lock (m_CheckRunLaunchLockObject)
                {
                    result = m_CRL_XDocuments[truncatedId];
                    m_CRL_XDocuments.Remove(truncatedId);
                }
            }
            catch (KeyNotFoundException ex)
            {
                string existingKeys = null;

                lock (m_CheckRunArtifactLockObject)
                {
                    existingKeys = this.ListCrlKeys();
                }

                string message = string.Format("GetCrlXDocument Failed to find a key. ID='{0}{1}{2}', truncated='{3}{4}{5}', existingkeys='{6}'", Environment.NewLine, uniqueLabelForCheckRunSegment, Environment.NewLine, Environment.NewLine, truncatedId, Environment.NewLine, this.ListCrlKeys());
                throw new CheckInfrastructureServiceException(message, ex);
            }

            return result;
        }

        public bool ClearCrlXDocumentById(string uniqueLabelForCheckRunSegment)
        {
            bool documentRemoved = false;

            try
            {
                string truncatedId = CheckRunDataHandles.StripIdOfMachineNames(uniqueLabelForCheckRunSegment);

                lock (m_CheckRunLaunchLockObject)
                {
                    documentRemoved = m_CRL_XDocuments.Remove(truncatedId);
                }
            }
            catch (KeyNotFoundException)
            {
                // do nothing
            }

            return documentRemoved;
        }

        public bool ClearCraXDocumentById(string uniqueLabelForCheckRunSegment)
        {
            bool documentRemoved = false;

            try
            {
                string truncatedId = CheckRunDataHandles.StripIdOfMachineNames(uniqueLabelForCheckRunSegment);

                lock (m_CheckRunArtifactLockObject)
                {
                    documentRemoved = m_CRA_XDocuments.Remove(truncatedId);
                }
            }
            catch (KeyNotFoundException)
            {
                // do nothing
            }

            return documentRemoved;
        }

#endregion //publicMethods
#region privateMembers
        private static object m_TypeLockObject = new Object();
        private static CrxDepot m_TheInstance = null;

        private Dictionary<string, XDocument> m_CRA_XDocuments = null;
        private object m_CheckRunArtifactLockObject = null;

        private Dictionary<string, XDocument> m_CRL_XDocuments = null;
        private object m_CheckRunLaunchLockObject = null;

        private EventLog m_MetaAutomationServiceEventLog = null;

        private static int m_eventCounter = 0;
        private static int m_instanceCounter = 0;
#endregion
#region privateMethods

        private CrxDepot()
        {
            m_CRA_XDocuments = new Dictionary<string, XDocument>();
            m_CheckRunArtifactLockObject = new Object();
            m_CRL_XDocuments = new Dictionary<string, XDocument>();
            m_CheckRunLaunchLockObject = new Object();
            m_instanceCounter++;
        }

        private string ListCraKeys()
        {
            int count = m_CRA_XDocuments.Count;
            StringBuilder keys = new StringBuilder(string.Format("total count:{0}", count));

            foreach (KeyValuePair<string, XDocument> kvp in m_CRA_XDocuments)
            {
                keys.Append(Environment.NewLine);
                keys.Append(kvp.Key);
            }

            keys.Append(Environment.NewLine);

            return keys.ToString();
        }

        private string ListCrlKeys()
        {
            int count = m_CRL_XDocuments.Count;
            StringBuilder keys = new StringBuilder(string.Format("total count:{0}", count));

            foreach (KeyValuePair<string, XDocument> kvp in m_CRL_XDocuments)
            {
                keys.Append(Environment.NewLine);
                keys.Append(kvp.Key);
            }

            keys.Append(Environment.NewLine);

            return keys.ToString();
        }

        private void PostEventLogMessage(string message)
        {
            const string EventLogTitle = "MetaAutomationService";
            const string SourceTitle = "MetaAutomationService";

            try
            {
                if ((m_MetaAutomationServiceEventLog == null) || (!EventLog.SourceExists(EventLogTitle)))
                {
                    if (!EventLog.SourceExists(EventLogTitle))
                    {
                        EventSourceCreationData eventSourceCreationData = new EventSourceCreationData(SourceTitle, EventLogTitle);
                        EventLog.CreateEventSource(eventSourceCreationData);
                    }

                    m_MetaAutomationServiceEventLog = new EventLog(EventLogTitle, Environment.MachineName, SourceTitle);
                }

                message = string.Format("Process='{0}' instanceCounter='{1}' hash='{3}' eventcount='{2}' {4}{5}", Process.GetCurrentProcess().Id, CrxDepot.m_instanceCounter, CrxDepot.m_eventCounter++, this.GetHashCode(), Environment.NewLine, message); // TODO DEBUG
                m_MetaAutomationServiceEventLog.WriteEntry(message);
            }
            catch (ArgumentException)
            {
                throw;
            }
        }

#endregion //privateMethods

    }
}
