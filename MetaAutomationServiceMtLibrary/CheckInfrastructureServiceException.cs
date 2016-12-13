////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationServiceMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System;

    /// <summary>
    /// This exception is specific to the check run infrastructure. If this exception is thrown, the SUT is not
    ///  touched by the check in any way, and no artifact is created.
    /// </summary>
    [Serializable]
    public class CheckInfrastructureServiceException : CheckInfrastructureBaseException
    {
        public CheckInfrastructureServiceException() : base() { }

        public CheckInfrastructureServiceException(string message) : base(message) { }

        public CheckInfrastructureServiceException(string message, System.Exception ex) : base(message, ex) { }
    }
}
