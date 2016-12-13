////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationBaseMtLibrary
{
    using System;

    public class CheckInfrastructureBaseException : Exception
    {
        public CheckInfrastructureBaseException() : base() { }

        public CheckInfrastructureBaseException(string message) : base(message) { }

        public CheckInfrastructureBaseException(string message, System.Exception ex) : base(message, ex) { }
    }
}
