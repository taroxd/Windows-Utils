using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            RunSystemProcess("cleanmgr.exe", $"/D " + system32[0]);
            RunSystemProcess("dism.exe", "/Online /Cleanup-Image /StartComponentCleanup /ResetBase");

            if (Directory.Exists(updateCache))
            {
                Directory.Delete(updateCache, true);
            }
        }

        static void RunSystemProcess(string fileName, string arguments)
        {
            Process.Start(Path.Combine(system32, fileName), arguments).WaitForExit();
        }
    }
}
