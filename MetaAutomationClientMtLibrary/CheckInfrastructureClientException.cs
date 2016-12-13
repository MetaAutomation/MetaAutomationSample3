////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System;

    /// <summary>
    /// This exception is specific to the check run infrastructure. If this exception is thrown, the SUT is not
    ///  touched by the check in any way, and no artifact is created.
    /// </summary>
    [Serializable]
    public class CheckInfrastructureClientException : CheckInfrastructureBaseException
    {
        public CheckInfrastructureClientException() : base() { }

        public CheckInfrastructureClientException(string message) : base(message) { }

        public CheckInfrastructureClientException(string message, System.Exception ex) : base(message, ex) { }
    }
}
