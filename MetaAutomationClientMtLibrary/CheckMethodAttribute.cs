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
    /// An instance of this attribute class has a name and a GUID (unique identifier) to label 
    ///  the check as implemented. In the Visual Studio UnitTesting namespace, the corresponding 
    ///  attribute is class DescriptionAttribute, but that namespace is not used here to maintain
    ///  platform independence for the example implementation of Atomic Check.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CheckMethodAttribute : Attribute
    {
        public string CheckMethodName { get; set; }
        public string CheckMethodGuid { get; set; }
    }
}
