////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationServiceMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System.Threading;

    internal static class Synchronization
    {
        /// <summary>
        /// This method releases the client call that started the check or subcheck. If this method completes before the client calls WaitOne(), the client won't block at all.
        /// </summary>
        /// <param name="semaphoreName"></param>
        public static void ReleaseOne(string semaphoreName)
        {
            // signal the named semaphore
            Semaphore namedSemaphoreWaitingOnCheckRunResult = null;
            string truncatedReleaseSemaphoreName = CheckRunDataHandles.StripIdOfMachineNames(semaphoreName);

            bool semaphoreFindResult = Semaphore.TryOpenExisting(truncatedReleaseSemaphoreName, out namedSemaphoreWaitingOnCheckRunResult);

            if (!semaphoreFindResult)
            {
                throw new CheckInfrastructureServiceException(string.Format("The semaphore with name='{0}' was not found.", semaphoreName));
            }

            // Releases the thread in the process that created the named semaphore, so that thread can continue
            //  to call GetCheckRunArtifact
            namedSemaphoreWaitingOnCheckRunResult.Release(1);
        }
    }
}
