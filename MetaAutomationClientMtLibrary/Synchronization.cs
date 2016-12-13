////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System;
    using System.Security.AccessControl;
    using System.Threading;

    public static class Synchronization
    {
        public static Semaphore CreateAndGetNamedSemaphore(string accessUserName, string semaphoreName)
        {
            Semaphore resultSemaphore = null;
            string truncatedCreateSemaphoreName = string.Empty;

            try
            {
                bool createdNew = false;
                truncatedCreateSemaphoreName = CheckRunDataHandles.StripIdOfMachineNames(semaphoreName);

                // Create rights on the semaphore so the test user can signal it
                SemaphoreSecurity semaphoreSecurity = new SemaphoreSecurity();

                // Allow the user specified in the parameter to signal the semaphore. This user may be the identify specified with the distributed solution.
                SemaphoreAccessRule rule = new SemaphoreAccessRule(accessUserName, SemaphoreRights.Synchronize | SemaphoreRights.Modify, AccessControlType.Allow);
                semaphoreSecurity.AddAccessRule(rule);

                // Allow the current user to signal the semaphore. Note that a different format is required for a local user vs. a domain user
                string currentUserName = Environment.UserName;

                // Check if local user or domain user
                if (!string.IsNullOrEmpty(Environment.UserDomainName))
                {
                    currentUserName = Environment.UserDomainName + "\\" + currentUserName;
                }

                rule = new SemaphoreAccessRule(currentUserName, SemaphoreRights.Synchronize | SemaphoreRights.Modify, AccessControlType.Allow);
                semaphoreSecurity.AddAccessRule(rule);

                // Create the semaphore
                resultSemaphore = new Semaphore(0, 1, truncatedCreateSemaphoreName, out createdNew, semaphoreSecurity);

                if (!createdNew)
                {
                    throw new CheckInfrastructureClientException(string.Format("Semaphore with name='{0}' for user='{1}' was not created.", truncatedCreateSemaphoreName, accessUserName));
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                if (ex.Message.Contains("Access to the port"))
                {
                    throw new CheckInfrastructureClientException(string.Format("Semaphore with name='{0}' for user='{1}' was not created. Is there a duplicate named semaphore?", truncatedCreateSemaphoreName, accessUserName));
                }
                else
                {
                    throw;
                }
            }

            return resultSemaphore;
        }
    }
}
