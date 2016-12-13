////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace CheckProcess
{
    using MetaAutomationClientMt;
    using System;
    using System.Diagnostics;
    using System.Threading;

    class CheckProcessMain
    {
        static void Main(string[] args)
        {
            // Uncomment the next four lines, just for debugging...    
            //while (!Debugger.IsAttached)
            //{
            //    Thread.Sleep(200);
            //}

            try
            {
                CheckRunner checkRunner = new CheckRunner();
                checkRunner.Run(args);
            }
            catch (Exception)
            {
                // Do nothing; the error(s) should be reported through MetaAutomation service.
            }
        }
    }
}
