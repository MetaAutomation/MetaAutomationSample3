////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationClientMtLibrary
{
    using System;
    using MetaAutomationBaseMtLibrary;

    /// <summary>
    /// Note 1 (Atomic Check aspects reflected here: Actionable Artifact, Fail Fast, Failure Data)
    /// This class represents the custom exception type or types used by the team for doing quality work on a project. 
    /// Using a custom exception gives advantages:
    ///     •	Exceptions can be handled differently according to their source
    ///     •	Custom exceptions can also form a hierarchy, for further handling customization owned by the team or 
    ///             separating out different exception sources.
    ///     •	Custom exceptions can have custom data or serializations
    ///     •	Handling an exception by placing it as inner exception to a custom exception allows adding custom data 
    ///             without losing any information. For example, this can be done for a method context for a stack-based 
    ///             language.
    /// </summary>
    [Serializable]
    public class CheckRunException : System.Exception
    {
        public CheckRunException() : base() { }

        public CheckRunException(string message) : base(message) { }

        public CheckRunException(string message, System.Exception ex) : base(message, ex) { }
    }
}
