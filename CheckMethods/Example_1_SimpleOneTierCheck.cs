////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace CheckMethods
{
    using MetaAutomationClientMtLibrary;
    using System;

    public class Example_1_SimpleOneTierCheck
    {
        [CheckMethod(CheckMethodName = "OneTierExampleCheck", CheckMethodGuid = "B650890A-20D1-4D84-B067-7535F1005195")]
        public void OneTierExampleCheck()
        {
            try
            {
                Check.Step("Step 1.", delegate
                {
                    System.Threading.Thread.Sleep(50);
                });

                Check.Step("Step 2.", delegate
                {
                    Check.Step("Step 3.", delegate
                    {
                        System.Threading.Thread.Sleep(55);
                    });
                });
            }
            catch (Exception ex)
            {
                Check.ReportFailureData(ex);
            }
        }
    }
}
