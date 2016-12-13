////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace CheckMethods
{
    using MetaAutomationClientMtLibrary;
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    public class Example_Shared
    {
        [CheckMethod(CheckMethodName = "MultipleTier000", CheckMethodGuid = "64941B8E-A19C-48D4-BA0A-F51E7D668B4E")]
        public void MultipleTier000()
        {
            try
            {
                Check.SetCustomDataCheckGlobal("Custom run data MultipleTier000.outer scope", "custom run value MultipleTier000.outer scope");

                Check.Step("MultipleTier000, Step 1.", delegate
                {
                    Check.SetCustomDataCheckGlobal("SetCustomData in MultipleTier000, Step 1. NAME", "AddCheckRunData in MultipleTier000, Step 1. VALUE");
                    Check.CallSubCheck(1); // MultipleTier100
                });

                Check.Step("MultipleTier000, Step 2.", delegate
                {
                    Check.Step("MultipleTier000, Step 2.1", delegate
                    {
                        Check.CallSubCheck(2); // MultipleTier200
                    });
                    Check.Step("MultipleTier000, Step 2.2", delegate
                    {
                        Check.CallSubCheck(3); // MultipleTier300
                    });
                });
            }
            catch (Exception ex)
            {
                Check.ReportFailureData(ex);
            }
        }

        [CheckMethod(CheckMethodName = "MultipleTier100", CheckMethodGuid = "C550FF73-1420-4CBF-8C17-229888195C03")]
        public void MultipleTier100()
        {
            try
            {
                Check.Step("MultipleTier100, Step 1.", delegate
                {
                    Check.CallSubCheck(1); // MultipleTier110
                });

                Check.Step("MultipleTier100, Step 2.", delegate
                {
                    Check.CallSubCheck(2); // MultipleTier120
                });
            }
            catch (Exception ex)
            {
                Check.ReportFailureData(ex);
            }
        }

        [CheckMethod(CheckMethodName = "MultipleTier200", CheckMethodGuid = "0A5D02EF-1151-41D0-9880-E950D2154EE3")]
        public void MultipleTier200()
        {
            try
            {
                Check.Step("MultipleTier200, Step 1.", delegate
                {
                    System.Threading.Thread.Sleep(50);
                });
            }
            catch (Exception ex)
            {
                Check.ReportFailureData(ex);
            }
        }

        [CheckMethod(CheckMethodName = "MultipleTier300", CheckMethodGuid = "86DC9DF2-86E7-4DA6-8FED-89CE2B051CA3")]
        public void MultipleTier300()
        {
            try
            {
                Check.Step("MultipleTier300, Step 1.", delegate
                {
                    System.Threading.Thread.Sleep(50);
                });
            }
            catch (Exception ex)
            {
                Check.ReportFailureData(ex);
            }
        }

        [CheckMethod(CheckMethodName = "MultipleTier110", CheckMethodGuid = "13ADE059-1D28-45EE-8194-C5C514AC6CBA")]
        public void MultipleTier110()
        {
            try
            {
                Check.Step("MultipleTier110, Step 1.", delegate
                {
                    System.Threading.Thread.Sleep(50);
                });
            }
            catch (Exception ex)
            {
                Check.ReportFailureData(ex);
            }
        }

        [CheckMethod(CheckMethodName = "MultipleTier120", CheckMethodGuid = "40EA37B7-CBBD-4AAE-9904-AE5D790E10B6")]
        public void MultipleTier120()
        {
            try
            {
                Check.Step("MultipleTier120, Step 1.", delegate
                {
                    Check.CallSubCheck(1); // MultipleTier121
                });
            }
            catch (Exception ex)
            {
                Check.ReportFailureData(ex);
            }
        }

        [CheckMethod(CheckMethodName = "MultipleTier121", CheckMethodGuid = "C35A168F-BF65-497C-9128-9AD1F9FA6C88")]
        public void MultipleTier121()
        {
            try
            {
                Check.Step("MultipleTier121, Step 1.", delegate
                {
                    System.Threading.Thread.Sleep(50);
                });
            }
            catch (Exception ex)
            {
                Check.ReportFailureData(ex);
            }
        }
    }
}
