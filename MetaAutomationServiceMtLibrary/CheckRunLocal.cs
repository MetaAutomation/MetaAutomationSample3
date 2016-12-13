using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaAutomationServiceMtLibrary
{
    using MetaAutomationBaseMtLibrary;
    using System;
    using System.Diagnostics;
    using System.IO;

    public class CheckRunLocal
    {
        public string Start(string uniqueLabelForCheckRunSegment, string pathAndFileNameForExe)
        {
            string result = string.Empty;

            try
            {
                // Validate path
                if (!Path.IsPathRooted(pathAndFileNameForExe))
                {
                    throw new CheckInfrastructureServiceException(string.Format("The given file path '{0}' needs a root.", pathAndFileNameForExe));
                }

                if (!Path.HasExtension(pathAndFileNameForExe))
                {
                    throw new CheckInfrastructureServiceException(string.Format("The given file path '{0}' needs a file name and extension.", pathAndFileNameForExe));
                }

                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.Arguments = uniqueLabelForCheckRunSegment; // don't strip the machine name, this is needed later by the service
                processStartInfo.FileName = pathAndFileNameForExe;
                processStartInfo.WorkingDirectory = Path.GetDirectoryName(pathAndFileNameForExe);

                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardInput = false;
                processStartInfo.RedirectStandardOutput = false;
                processStartInfo.RedirectStandardError = false;

                Process process = Process.Start(processStartInfo);

                if (process == null)
                {
                    result = string.Format("The process with given file path '{0}' was not started.", pathAndFileNameForExe);
                }
                else
                {
                    result = string.Format("The process with given file path '{0}' was successfully started.", pathAndFileNameForExe);
                }

                result = string.Format("{0}. {1} ", DataStringConstants.StatusString.DefaultServiceSuccessMessage, result);
            }
            catch (Exception ex)
            {
                result += " StartCheckRun failed." + ex.ToString();
            }

            return result;
        }
    }
}
