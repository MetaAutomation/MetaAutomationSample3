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

    public class Example_3_TwoTierCheckOfFileSystem
    {
        [CheckMethod(CheckMethodName = "ExampleOuterCheck", CheckMethodGuid = "C12DDDFF-B4FA-43C5-B664-5060849E5193")]
        public void ExampleOuterCheck()
        {
            try
            {
                Check.Step("Create and write to the file", delegate
                {
                    StreamWriter streamWriter = null;

                    Check.Step("Create the file.", delegate
                    {
                        string fileNameAndPath = null;

                        Check.Step("Get the filename.", delegate
                        {
                            // Commented out, but kept for future reference
                            //FaultGenerator.Instance.ThrowOnProbability(0.0, typeof(ApplicationException));
                            fileNameAndPath = Check.GetCustomDataCheckGlobal("TestFileNameAndPath");
                            if (fileNameAndPath == null) throw new CheckFailException("The file name 'TestFileNameAndPath' was not found in the test data");
                        });

                        Check.Step("Report the current user identity", delegate
                        {
                            Check.SetCustomDataCheckStep("Current user", Environment.UserName);
                        });

                        Check.Step("Get the stream to write to the file.", delegate
                        {
                            FileStream fileStream = File.Create(fileNameAndPath);
                            if (fileStream == null) throw new CheckFailException(string.Format("Creating the file '{0}' failed.", fileNameAndPath));
                            streamWriter = new StreamWriter(fileStream);
                        });
                    });

                    Check.Step("Create and record data for the file", delegate
                    {
                        string randomString = SimpleFuzzer.RandomString(10);
                        Check.SetCustomDataCheckGlobal("RandomString", randomString);
                    });

                    Check.Step("Write and close the file", delegate
                    {
                        string fileData = Check.GetCustomDataCheckGlobal("RandomString");
                        Check.SetCustomDataCheckStep("Data written to file", fileData);
                        streamWriter.Write(fileData);
                        streamWriter.Close();
                    });

                    Check.Step("Verify file contents from a different machine and/or process", delegate
                    {
                        Check.CallSubCheck(1);
                    });
                });
            }
            catch (Exception ex)
            {
                Check.ReportFailureData(ex);
            }
        }

        [CheckMethod(CheckMethodName = "ExampleInnerCheck", CheckMethodGuid = "88180CD8-445C-4F26-8949-B3B731F610B5")]
        public void ExampleInnerCheck()
        {
            try
            {
                Check.Step("Read the file and verify the contents.", delegate
                {
                    StreamReader streamReader = null;

                    Check.Step("Report the current user identity", delegate
                    {
                        Check.SetCustomDataCheckStep("Current user", Environment.UserName);
                    });

                    Check.Step("Open the file for reading.", delegate
                    {
                        string fileNameAndPath = Check.GetCustomDataCheckGlobal("TestFileNameAndPath");
                        if (fileNameAndPath == null) throw new CheckFailException("The file name 'TestFileNameAndPath' was not found in the test data");
                        FileStream fileStream = File.OpenRead(fileNameAndPath);
                        streamReader = new StreamReader(fileStream);
                    });

                    string recoveredString = null;

                    Check.Step("Read the file.", delegate
                    {
                        recoveredString = streamReader.ReadToEnd();
                        streamReader.Close();
                    });

                    Check.Step("Verify file contents.", delegate
                    {
                        string originalData = Check.GetCustomDataCheckGlobal("RandomString");
                        Check.SetCustomDataCheckStep("Expected data in file", originalData);
                        Check.SetCustomDataCheckStep("Actual data read from file", recoveredString);

                        if (recoveredString != originalData)
                        {
                            throw new CheckFailException(string.Format("Data comparison failed. Original string '{0}' does not match the recovered string '{1}'.", originalData, recoveredString));
                        }
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
