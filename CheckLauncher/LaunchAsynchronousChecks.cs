////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace CheckLauncher
{
    using MetaAutomationBaseMtLibrary;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Xml.Linq;
    using System.Xml.XPath;

    class LaunchAsynchronousChecks
    {
        private const string CheckMapPath = @"..\..\Artifacts";
        private const string CheckMapFile = "CheckMap.xml";
        private const string CheckElementName = "Check";
        private const string DirectoryName = "DirectoryName";
        private const string CurrentCheckRunArtifact = "CurrentCheckRunArtifact";
        private const string KeyDisambiguationIndex = "KeyDisambiguationIndex";

        static void Main(string[] args)
        {
            try
            {
                const int IterateCount = 1;

                for (int i = 0; i < IterateCount; i++)
                {
                    try
                    {
                        Console.WriteLine(string.Format("**** Starting run #{0} of {1}...", i, IterateCount));

                        LaunchAsynchronousChecks.RunChecks();
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(5000);
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine();
                Console.WriteLine("Hit Enter to continue...");
                Console.Read();
            }

        }

        static void RunChecks()
        {
            try
            {
                XDocument checkMap = XDocument.Load(Path.Combine(CheckMapPath, CheckMapFile));

                var checkElementsIterator = checkMap.Descendants(CheckElementName);
                Dictionary<string, IAsyncResult> checkRunMap = new Dictionary<string, IAsyncResult>();
                Func<string, string> targetMethod = new Func<string, string>(MetaAutomationLauncherMtLibrary.CheckArtifactFiles.RunCheck);

                // launch all target checks
                foreach (XElement checkElement in checkElementsIterator)
                {
                    string directoryName = null;
                    string fileName = null;
                    int index = 0;
                    bool useIndex = false;

                    foreach (XElement dataElement in checkElement.Elements(DataStringConstants.ElementNames.DataElement))
                    {
                        XAttribute nameAttribute = dataElement.Attribute(DataStringConstants.AttributeNames.Name);
                        string name = nameAttribute.Value;
                        XAttribute valueAttribute = dataElement.Attribute(DataStringConstants.AttributeNames.Value);
                        string value = valueAttribute.Value;

                        if (name == DirectoryName)
                        {
                            directoryName = value;
                        }
                        else if (name == CurrentCheckRunArtifact)
                        {
                            fileName = value;
                        }
                        else if (name == KeyDisambiguationIndex)
                        {
                            useIndex = Int32.TryParse(value, out index);
                        }
                    }

                    if (directoryName == null)
                    {
                        throw new ApplicationException("'directoryName' is missing.");
                    }

                    if (fileName == null)
                    {
                        throw new ApplicationException("'fileName' is missing.");
                    }

                    string pathAndFileName = Path.Combine(CheckMapPath, directoryName, fileName);
                    //Console.WriteLine(string.Format("Original pathAndFileName:'{0}'", pathAndFileName));
                    pathAndFileName = (Path.GetFullPath(pathAndFileName));
                    //Console.WriteLine(string.Format("Normalized pathAndFileName:'{0}'", pathAndFileName));
                    IAsyncResult result = targetMethod.BeginInvoke(pathAndFileName, null, null);

                    if (useIndex)
                    {
                        // Note the space delimeter, which allows the actual file path directory to be parsed out later
                        directoryName += " " + index;
                    }

                    // to save trouble with collections
                    //directoryName.Replace(Path.PathSeparator, '_');

                    checkRunMap.Add(directoryName, result);
                }

                // poll in same thread until they're all finished
                bool waitingOnCompletion = false;
                int pollCounter = 0;

                do
                {
                    Console.WriteLine();
                    Console.WriteLine(string.Format("poll count {0}", pollCounter++));
                    waitingOnCompletion = false;
                    Dictionary<string, string> checkCompletions = new Dictionary<string, string>();

                    foreach (KeyValuePair<string, IAsyncResult> pair in checkRunMap)
                    {
                        waitingOnCompletion = true;
                        Console.WriteLine("Pair found:");
                        Console.WriteLine("   " + pair.Key);

                        IAsyncResult asyncResult = pair.Value;

                        if (asyncResult.IsCompleted)
                        {
                            string result = targetMethod.EndInvoke(asyncResult);
                            Console.WriteLine(string.Format("      completed '{0}'", result));

                            // Note completions for later entry
                            checkCompletions.Add(pair.Key, result);
                        }
                        else
                        {
                            Console.WriteLine("      not completed.");
                        }
                    }

                    foreach (KeyValuePair<string, string> completedCheck in checkCompletions)
                    {
                        XElement checkElement = null;
                        string keyName = completedCheck.Key;
                        string directoryName = keyName;
                        int index = -1;
                        bool useIndex = false;

                        // Factor out the collection index if needed, because it's not part of the file directory
                        int spaceIndex = directoryName.IndexOf(' ');

                        if (spaceIndex > -1)
                        {
                            useIndex = Int32.TryParse(directoryName.Substring(spaceIndex + 1), out index);
                            directoryName = directoryName.Substring(0, spaceIndex);
                        }

                        foreach (XElement candidateCheck in checkMap.Descendants(CheckElementName))
                        {
                            string xpathToCheckDirectory = string.Format(
                                    "{0}[@{1}='{2}']",
                                    DataStringConstants.ElementNames.DataElement,
                                    DataStringConstants.AttributeNames.Name,
                                    DirectoryName);

                            if (candidateCheck.XPathSelectElement(xpathToCheckDirectory).Attribute(DataStringConstants.AttributeNames.Value).Value == directoryName)
                            {
                                if (useIndex)
                                {
                                    // must match the index element as well
                                    string xpathToCheckIndex = string.Format(
                                        "{0}[@{1}='{2}']",
                                        DataStringConstants.ElementNames.DataElement,
                                        DataStringConstants.AttributeNames.Name,
                                        KeyDisambiguationIndex);

                                    if (candidateCheck.XPathSelectElement(xpathToCheckIndex).Attribute(DataStringConstants.AttributeNames.Value).Value == index.ToString())
                                    {
                                        checkElement = candidateCheck;
                                        break;
                                    }
                                }
                                else
                                {
                                    checkElement = candidateCheck;
                                    break;
                                }
                            }
                        }

                        if (checkElement == null)
                        {
                            throw new ApplicationException("The correct element 'Check' was not found in the check map.");

                        }

                        // correct Check element found
                        string xpathToFindFile = string.Format(
                            "{0}[@{1}='{2}']",
                            DataStringConstants.ElementNames.DataElement,
                            DataStringConstants.AttributeNames.Name,
                            CurrentCheckRunArtifact);

                        XElement artifactVectorElement = checkElement.XPathSelectElement(xpathToFindFile);

                        XAttribute valueAttribute = artifactVectorElement.Attribute(DataStringConstants.AttributeNames.Value);
                        valueAttribute.Value = completedCheck.Value;

                        // Remove the key from the list of asynchronous operations, because it's complete
                        checkRunMap.Remove(keyName);
                    }

                    Thread.Sleep(1000);
                } while (waitingOnCompletion);

                checkMap.Save(Path.Combine(CheckMapPath, CheckMapFile));

                Console.WriteLine("Completed all.");
                Thread.Sleep(500); // TODO DEBUG this sleep is just for visibility on the console
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                Console.WriteLine("finally...");
            }
        }
    }
}
