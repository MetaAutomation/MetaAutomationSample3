////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationService
{
    using MetaAutomationServiceMtLibrary;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml.Linq;

    public class MetaAutomationService : IMetaAutomationService
    {
        public MetaAutomationService() : base()
        {
        }

        /// <summary>
        /// Validates inputs using XSD and checking for required DataElement elements.
        /// If the destination machine is local, uses the file path in the XML to launch the target check process.
        /// If the destination machine is not local, the request is forwarded.
        /// </summary>
        /// <param name="checkRunLaunchXml">The StartCheckRun or 'CRL' XML for the check segment</param>
        /// <returns>Success result text or error information</returns>
        public string StartCheckRun(string checkRunLaunchXml)
        {
            string resultMessage = MetaAutomationBaseMtLibrary.DataStringConstants.StatusString.DefaultServiceSuccessMessage;

            try
            {
                if (checkRunLaunchXml == null) throw new CheckInfrastructureServiceException("StartCheckRun: the passed string argument is null.");
                if (checkRunLaunchXml.Length == 0) throw new CheckInfrastructureServiceException("StartCheckRun: the passed string argument is zero-length.");

#if DEBUG
                PostEventLogMessage(string.Format("StartCheckRun '{0}'.", checkRunLaunchXml));
#endif

                XDocument crlXDocument = CheckRunValidation.Instance.ValidateCheckRunLaunchIntoXDocument(checkRunLaunchXml);

                // Forward the storage request, or not?
                if (OSInstanceInfo.Instance.IsDestinationMachineLocalInstance(crlXDocument))
                {
                    CheckDestinationServices.Instance.StartCheckRun(crlXDocument);
                }
                else
                {
                    // Forward request to the destination machine
                    string destinationMachineName = OSInstanceInfo.Instance.GetDestinationMachineName(crlXDocument);
                    MetaAutomationServiceClient metaAutomationServiceClientOnDestinationMachine = new MetaAutomationServiceClient(destinationMachineName);
                    resultMessage = metaAutomationServiceClientOnDestinationMachine.StartCheckRun(checkRunLaunchXml);
                }
            }
            catch (Exception ex)
            {
                resultMessage = MetaAutomationService.DefaultServiceErrorMessage + " StartCheckRun failed." + ex.ToString();
                PostEventLogMessage(resultMessage);
            }

            return resultMessage;
        }

        /// <summary>
        /// Temporarily stores the CheckRunResult or 'CRL' in a dictionary on the service on the origin machine for retrieval by the process that launched the check run segment, and signals the named semaphore so the dependent check can continue.
        /// </summary>
        /// <param name="checkRunArtifactXml">CheckRunResult or 'CRL' XML</param>
        /// <returns>Success result text or error information</returns>
        public string CompleteCheckRun(string checkRunArtifactXml)
        {
            string resultMessage = MetaAutomationBaseMtLibrary.DataStringConstants.StatusString.DefaultServiceSuccessMessage;

            try
            {
                if (checkRunArtifactXml == null) throw new CheckInfrastructureServiceException("CompleteCheckRun: the passed string argument is null.");
                if (checkRunArtifactXml.Length == 0) throw new CheckInfrastructureServiceException("CompleteCheckRun: the passed string argument is zero-length.");

#if DEBUG
                PostEventLogMessage(string.Format("CompleteCheckRun '{0}'.", checkRunArtifactXml));
#endif

                XDocument craXDocument = CheckRunValidation.Instance.ValidateCheckRunArtifactIntoXDocument(checkRunArtifactXml);

                if (OSInstanceInfo.Instance.IsOriginMachineLocalInstance(craXDocument))
                {
                    resultMessage = CheckOriginServices.Instance.CompleteCheckRun(craXDocument);
                }
                else
                {
                    // Forward the request to store the CRA on the client or origin machine
                    string originMachineName = OSInstanceInfo.Instance.GetOriginMachineName(craXDocument);
                    MetaAutomationServiceClient metaAutomationServiceClientOnOriginMachine = new MetaAutomationServiceClient(originMachineName);
                    resultMessage = metaAutomationServiceClientOnOriginMachine.CompleteCheckRun(checkRunArtifactXml);
                }
            }
            catch (Exception ex)
            {
                resultMessage = MetaAutomationService.DefaultServiceErrorMessage + " CompleteCheckRun failed." + ex.ToString();
                PostEventLogMessage(resultMessage);
            }

            return resultMessage;
        }

        /// <summary>
        /// Retrieves the CheckRunLaunch (CRL) by the identifier string
        /// </summary>
        /// <param name="uniqueLabelForCheckRunSegment">unique label string for the check run segment</param>
        /// <returns>The CheckRunLaunch (CRL) XML string</returns>
        public string GetCheckRunLaunch(string uniqueLabelForCheckRunSegment)
        {
            string resultXml = null;

            try
            {
                if (uniqueLabelForCheckRunSegment == null) throw new CheckInfrastructureServiceException("GetCheckRunLaunch: the passed string argument is null.");
                if (uniqueLabelForCheckRunSegment.Length == 0) throw new CheckInfrastructureServiceException("GetCheckRunLaunch: the passed string argument is zero-length.");

#if DEBUG
                PostEventLogMessage(string.Format("GetCheckRunLaunch '{0}'.", uniqueLabelForCheckRunSegment));
#endif

                // Use the lib for local check process launches only, otherwise forward the request to the same service on another machine.
                if (OSInstanceInfo.Instance.IsDestinationMachineLocalInstance(uniqueLabelForCheckRunSegment))
                {
                    resultXml = CheckDestinationServices.Instance.GetCheckRunLaunch(uniqueLabelForCheckRunSegment);
                }
                else
                {
                    // The destination machine is NOT the local machine, so forward the request to the destination machine
                    string destinationMachineName = OSInstanceInfo.Instance.GetDestinationMachineName(uniqueLabelForCheckRunSegment);
                    MetaAutomationServiceClient metaAutomationServiceOnDestinationMachine = new MetaAutomationServiceClient(destinationMachineName);
                    resultXml = metaAutomationServiceOnDestinationMachine.GetCheckRunLaunch(uniqueLabelForCheckRunSegment);
                }
            }
            catch (KeyNotFoundException ex)
            {
                resultXml = string.Format("{0} GetCheckRunLaunch failed at key='{1}'. {2}",
                    MetaAutomationService.DefaultServiceErrorMessage,
                    uniqueLabelForCheckRunSegment,
                    ex.ToString());

                PostEventLogMessage(resultXml);
            }
            catch (Exception ex)
            {
                resultXml = MetaAutomationService.DefaultServiceErrorMessage + " GetCheckRunLaunch failed." + ex.ToString();
                PostEventLogMessage(resultXml);
            }

            return resultXml;
        }

        /// <summary>
        /// Retrieves the CheckRunArtifact (CRA) by the identifier string
        /// </summary>
        /// <param name="uniqueLabelForCheckRunSegment">unique label string for the check run segment</param>
        /// <returns>The CheckRunArtifact (CRA) XML string</returns>
        public string GetCheckRunArtifact(string uniqueLabelForCheckRunSegment)
        {
            string resultXml = null;

            try
            {
                if (uniqueLabelForCheckRunSegment == null) throw new CheckInfrastructureServiceException("GetCheckRunArtifact: the passed string argument is null.");
                if (uniqueLabelForCheckRunSegment.Length == 0) throw new CheckInfrastructureServiceException("GetCheckRunArtifact: the passed string argument is zero-length.");

#if DEBUG
                PostEventLogMessage(string.Format("GetCheckRunArtifact '{0}'.", uniqueLabelForCheckRunSegment));
#endif

                if (OSInstanceInfo.Instance.IsOriginMachineLocalInstance(uniqueLabelForCheckRunSegment))
                {
                    resultXml = CheckOriginServices.Instance.GetCheckRunArtifact(uniqueLabelForCheckRunSegment);
                }
                else
                {
                    // The origin machine is NOT the local machine, so forward the request to the origin (client) machine
                    string originMachineName = OSInstanceInfo.Instance.GetOriginMachineName(uniqueLabelForCheckRunSegment);
                    MetaAutomationServiceClient metaAutomationServiceOnOriginMachine = new MetaAutomationServiceClient(originMachineName);
                    resultXml = metaAutomationServiceOnOriginMachine.GetCheckRunArtifact(uniqueLabelForCheckRunSegment);
                }
            }
            catch (KeyNotFoundException ex)
            {
                resultXml = string.Format("{0} GetCheckRunArtifact failed at key='{1}'. {2}",
                    MetaAutomationService.DefaultServiceErrorMessage,
                    uniqueLabelForCheckRunSegment,
                    ex.ToString());

                PostEventLogMessage(resultXml);
            }
            catch (Exception ex)
            {
                resultXml = MetaAutomationService.DefaultServiceErrorMessage + " GetCheckRunArtifact failed." + ex.ToString();
                PostEventLogMessage(resultXml);
            }

            return resultXml;
        }

        /// <summary>
        /// This method signals an unrecoverable error in the infrastructure
        /// </summary>
        /// <param name="uniqueLabelForCheckRunSegment">unique identifier for the semaphore, the CheckRunLaunch (CRL) and the CheckRunArtifact(CRA)</param>
        /// <param name="errorMessage">The error to be stored with the identifier</param>
        /// <returns></returns>
        public string AbortCheckRun(string uniqueLabelForCheckRunSegment, string errorMessage)
        {
            string resultMessage = MetaAutomationService.DefaultServiceAbortMessage;

            try
            {
                if (uniqueLabelForCheckRunSegment == null) throw new CheckInfrastructureServiceException("AbortCheckRun: the passed string argument 'uniqueLabelForCheckRunSegment' is null.");
                if (uniqueLabelForCheckRunSegment.Length == 0) throw new CheckInfrastructureServiceException("AbortCheckRun: the passed string argument 'uniqueLabelForCheckRunSegment' is zero-length.");
                if (errorMessage == null) throw new CheckInfrastructureServiceException("AbortCheckRun: the passed string argument 'errorMessage' is null.");
                if (errorMessage.Length == 0) throw new CheckInfrastructureServiceException("AbortCheckRun: the passed string argument 'errorMessage' is zero-length.");

#if DEBUG
                PostEventLogMessage(string.Format("AbortCheckRun '{0}', error='{1}'.", uniqueLabelForCheckRunSegment, errorMessage));
#endif

                if (OSInstanceInfo.Instance.IsDestinationMachineLocalInstance(uniqueLabelForCheckRunSegment))
                {
                    resultMessage += CheckDestinationServices.Instance.AbortCheckRun(uniqueLabelForCheckRunSegment, errorMessage);
                }

                if (OSInstanceInfo.Instance.IsOriginMachineLocalInstance(uniqueLabelForCheckRunSegment))
                {
                    resultMessage += CheckOriginServices.Instance.AbortCheckRun(uniqueLabelForCheckRunSegment, errorMessage);
                }
                else
                {
                    // forward the abort request to the origin machine, where it is completed
                    string originMachineName = OSInstanceInfo.Instance.GetOriginMachineName(uniqueLabelForCheckRunSegment);
                    MetaAutomationServiceClient metaAutomationServiceOnOriginMachine = new MetaAutomationServiceClient(originMachineName);
                    resultMessage = metaAutomationServiceOnOriginMachine.AbortCheckRun(uniqueLabelForCheckRunSegment, resultMessage);
                }

                return resultMessage;
            }
            catch (Exception ex)
            {
                resultMessage = MetaAutomationService.DefaultServiceErrorMessage + string.Format(" AbortCheckRun failed with exception '{0}'.", ex);
                PostEventLogMessage(resultMessage);
            }

            return resultMessage;
        }

        /// <summary>
        /// Get the error message stored by the identifier
        /// </summary>
        /// <param name="uniqueLabelForCheckRunSegment"></param>
        /// <returns></returns>
        public string GetAbortMessage(string uniqueLabelForCheckRunSegment)
        {
            string resultMessage = MetaAutomationService.DefaultServiceAbortMessage;

            try
            {
                if (uniqueLabelForCheckRunSegment == null) throw new CheckInfrastructureServiceException("GetAbortMessage: the passed string argument 'uniqueLabelForCheckRunSegment' is null.");
                if (uniqueLabelForCheckRunSegment.Length == 0) throw new CheckInfrastructureServiceException("GetAbortMessage: the passed string argument 'uniqueLabelForCheckRunSegment' is zero-length.");

#if DEBUG
                PostEventLogMessage(string.Format("GetAbortMessage '{0}'.", uniqueLabelForCheckRunSegment));
#endif

                if (OSInstanceInfo.Instance.IsOriginMachineLocalInstance(uniqueLabelForCheckRunSegment))
                {
                    resultMessage = CheckOriginServices.Instance.GetAbortMessage(uniqueLabelForCheckRunSegment);
                }
                else
                {
                    // forward the abort request to the origin machine, where it is completed
                    string originMachineName = OSInstanceInfo.Instance.GetOriginMachineName(uniqueLabelForCheckRunSegment);
                    MetaAutomationServiceClient metaAutomationServiceOnOriginMachine = new MetaAutomationServiceClient(originMachineName);
                    resultMessage = metaAutomationServiceOnOriginMachine.GetAbortMessage(uniqueLabelForCheckRunSegment);
                }
            }
            catch (KeyNotFoundException ex)
            {
                resultMessage = string.Format("{0} GetCheckRunArtifact failed at key='{1}'. {2}",
                    MetaAutomationService.DefaultServiceErrorMessage,
                    uniqueLabelForCheckRunSegment,
                    ex.ToString());

                PostEventLogMessage(resultMessage);
            }
            catch (Exception ex)
            {
                resultMessage = string.Format("GetAbortMessage failed with exception '{0}'.", ex);
                PostEventLogMessage(resultMessage);
            }

            return resultMessage;
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

                message = string.Format("Process='{0}' eventcount='{1}' {2}{3}", Process.GetCurrentProcess().Id, MetaAutomationService.m_eventCounter++, Environment.NewLine, message);
                m_MetaAutomationServiceEventLog.WriteEntry(message);
            }
            catch (ArgumentException)
            {
                throw;
            }
        }

        private static string DefaultServiceAbortMessage = "MetaAutomationService Abort default message.";
        private static string DefaultServiceErrorMessage = "MetaAutomationService Error.";
        private static int m_eventCounter = 0;

        private EventLog m_MetaAutomationServiceEventLog = null;
    }
}
