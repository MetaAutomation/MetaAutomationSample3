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

    public class Example_2_CheckOfWebPageBuiltToFailRandomly
    {
        [CheckMethod(CheckMethodName = "FlakyPageCheck", CheckMethodGuid = "DE7F55D5-EA38-4821-9F7F-CF75F179C4AF")]
        public void FlakyPageCheck()
        {
            try
            {
                string basicPageText = null;
                const string DefaultPagePath = "http://metaautomation.net/FlakyPage.aspx";

                Check.Step("Get the basic web page", delegate
                {
                    HttpWebRequest webRequest = null;
                    HttpWebResponse response = null;

                    Check.Step("Make a HttpWebRequest", delegate
                    {
                        webRequest = (HttpWebRequest)HttpWebRequest.Create(DefaultPagePath);
                    });

                    Check.Step("Get the HttpWebResponse", delegate
                    {
                        webRequest.Timeout = (int)Check.StepTimeout; // gets the timeout defined in the check artifact for this step
                        string httpStatus = "No response."; // default

                        try
                        {
                            response = (HttpWebResponse)webRequest.GetResponse();
                        }
                        catch (WebException webEx)
                        {
                            response = (HttpWebResponse)webEx.Response;
                            throw;
                        }
                        finally
                        {
                            if (response != null)
                            {
                                httpStatus = response.StatusCode.ToString();
                            }

                            Check.SetCustomDataCheckStep("HttpStatusCode", httpStatus);
                        }
                    });

                    Check.Step("Read the text of the page", delegate
                    {
                        Stream receiveStream = response.GetResponseStream();

                        if (receiveStream == null)
                        {
                            throw new CheckFailException("The response stream of the HttpWebResponse is null.");
                        }

                        StreamReader readResponse = new StreamReader(receiveStream, Encoding.UTF8);

                        try
                        {
                            basicPageText = readResponse.ReadToEnd();
                        }
                        catch (IOException ex)
                        {
                            throw new CheckFailException("Reading the response failed.", ex);
                        }

                        if (basicPageText == null)
                        {
                            throw new CheckFailException("Failed to read the stream of the HttpWebResponse is null.");
                        }
                        else if (basicPageText.Length == 0)
                        {
                            throw new CheckFailException("The response stream of the HttpWebResponse yielded zero characters.");
                        }

                        response.Close();
                        readResponse.Close();
                    });
                });

                Check.Step("Target verification step", delegate
                {
                    const string TargetString = "Pattern Language";

                    if (!basicPageText.Contains(TargetString))
                    {
                        Check.AddCheckFailData("page source", basicPageText);

                        throw new CheckFailException(string.Format("The search string '{0}' was not found on the page. Check for a screenshot and the 'page source' data element.", TargetString));
                    }
                });
            }
            catch (Exception ex)
            {
                Check.ReportFailureData(ex);
            }
        }
    }
}
