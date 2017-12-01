using System;
using System.Diagnostics;
using System.IO;

namespace WUClean
{
    class Program
    {
        readonly static string system32 = Environment.SystemDirectory;
        readonly static string updateCache =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.Windows,
                    Environment.SpecialFolderOption.DoNotVerify),
                @"SoftwareDistribution\Download");

        static void Main(string[] args)
        {
            RunSystemProcess("cleanmgr.exe", "/D " + system32[0]);
            RunSystemProcess("dism.exe", "/Online /Cleanup-Image /StartComponentCleanup /ResetBase");

            try
            {
                Directory.Delete(updateCache, true);
            }
            catch (Exception)
            {

            }
        }

        static void RunSystemProcess(string fileName, string arguments)
        {
            Process.Start(Path.Combine(system32, fileName), arguments).WaitForExit();
        }
    }
}
