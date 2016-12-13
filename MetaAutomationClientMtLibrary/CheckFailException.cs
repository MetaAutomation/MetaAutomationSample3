////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using System;
    using MetaAutomationBaseMtLibrary;

    public class CheckFailException : Exception
    {
        public CheckFailException()
            : base()
        {
        }

        public CheckFailException(string message)
            : base(message)
        {
        }


        public CheckFailException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}
