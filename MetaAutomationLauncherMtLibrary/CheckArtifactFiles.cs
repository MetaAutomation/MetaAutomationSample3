////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace MetaAutomationLauncherMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using MetaAutomationClientMt;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;

    public class CheckArtifactFiles
    {
        private const string ArtifactFileNameExtension = "xml";
        private const string ArtifactFileNameRoot = "CheckRunArtifact";

        /// <summary>
        /// This method takes the relative path and filename of a check run artifact, and returns the relative path and filename of the check run artifact for the new result.
        /// This method is intended to run asynchronously as part of the Task-based Asynchronous Pattern (TAP)
        /// </summary>
        /// <param name="fullPathNameToCheckRunArtifact"></param>
        /// <returns></returns>
        public static string RunCheck(string fullPathNameToCheckRunArtifact)
        {
            // read file to CRA xdoc
            XDocument lastCheckRunArtifact = XDocument.Load(fullPathNameToCheckRunArtifact);
            string pathToCheckArtifacts = Path.GetDirectoryName(fullPathNameToCheckRunArtifact);

            // Make changes as needed to determine the new check run
            XDocument checkRunLaunch = AssembleCheckRunLaunch(lastCheckRunArtifact);

            // run check
            CheckRunner checkRunner = new CheckRunner();
            XDocument checkRunArtifact = checkRunner.Run(checkRunLaunch);

            // write destination file
            string fileName = SaveCheckRunArtifact(pathToCheckArtifacts, checkRunArtifact);

            // return new CRA file name
            return fileName;
        }

        private static XDocument AssembleCheckRunLaunch(XDocument lastCheck)
        {
            Dictionary<string, string> checkDataMembers = new Dictionary<string, string>();

            // The destination machine, also called the server machine, is where the check or subcheck will run.
            //checkDataMembers.Add(DataStringConstants.NameAttributeValues.DestinationMachine, System.Net.Dns.GetHostName().ToUpper());

            // The client user. Here, just use the user name of the process that is running this code.
            //checkDataMembers.Add(DataStringConstants.NameAttributeValues.CheckClientUser, Environment.UserName);

            // The CheckRunGuid must be unique every time.
            checkDataMembers.Add(DataStringConstants.NameAttributeValues.CheckRunGuid, Guid.NewGuid().ToString("D"));

            // The machine name for the origin. In this case, it's just the machine that this is running on.
            checkDataMembers.Add(DataStringConstants.NameAttributeValues.OriginMachine, Environment.MachineName);


            // Leave the job spec GUID empty here, as a placeholder for when the job is being managed
            //checkDataMembers.Add(DataStringConstants.NameAttributeValues.CheckJobSpecGuid, Guid.Empty.ToString("D"));

            // Leave the job run GUID empty here, as a placeholder for when the job is being managed
            //checkDataMembers.Add(DataStringConstants.NameAttributeValues.CheckJobRunGuid, Guid.Empty.ToString("D"));

            CheckRunLaunchCreator checkRunLaunchCreator = new CheckRunLaunchCreator(checkDataMembers);

            return checkRunLaunchCreator.CreateCheckRunLaunch(lastCheck);
        }

        private static string SaveCheckRunArtifact(string fullPath, XDocument cra)
        {
            const string ArtifactFileNameExtension = "xml";
            const string ArtifactFileNameRoot = "CheckRunArtifact";
            string checkMethodRunGuid = DataAccessors.GetCheckRunValue(cra, DataStringConstants.NameAttributeValues.CheckRunGuid);

            string fileName = string.Format(
                "{0}_{1}.{2}",
                ArtifactFileNameRoot,
                checkMethodRunGuid,
                ArtifactFileNameExtension);

            cra.Save(Path.Combine(fullPath, fileName));

            /// BEGIN DEBUG temporary debugging code follows
            /// save extra copy of the same file in 
            //const string relativePathToTempCopy = @"..\..\tempcopy";
            //string tempPath = Path.Combine(fullPath, relativePathToTempCopy);
            //string[] pathComponents = fullPath.Split(Path.DirectorySeparatorChar);
            //int pathComponentCount = pathComponents.Length;
            //string tempFileName = string.Format("{0}_{1}_{3}.{2}", pathComponents[pathComponentCount - 2], pathComponents[pathComponentCount - 1], ArtifactFileNameExtension, fileCounter++);
            //cra.Save(Path.Combine(@"c:\t2", tempFileName), SaveOptions.None);
            /// END DEBUG
            
            return fileName;
        }
    }
}
