using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProcessLauncher
{
    class Program
    {

        private static string[] Args = null;
        private static string currentArg = null;
        private static int argIndex = 0;
        private static int argLength = 0;
        private static bool wait =
#if CONSOLE
                                    true;
#else
                                    false;
#endif
        private static int processorAffinity = 0;
        private static bool cdToProgram = false;
        private static ProcessPriorityClass priority = ProcessPriorityClass.Normal;

        static void Main(string[] args)
        {
            Args = args;
            argLength = args.Length;
#if CONSOLE
            if (argLength == 0)
            {
                DisplayHelpAndExit();
            }
#endif
            var procInfo = new ProcessStartInfo();
#if !DEBUG
            try
            {
#endif
            for (; argIndex < argLength; ++argIndex)
            {
                currentArg = args[argIndex];
                switch (currentArg)
                {
#if CONSOLE
                    case "/help":
                    case "/h":
                    case "/?":
                        DisplayHelpAndExit();
                        break;
#endif
                    case "/arg":
                    case "/args":
                        procInfo.Arguments = TryFetchNextArgument();
                        break;
                    case "/env":
                    case "/e":
                        Environment.SetEnvironmentVariable(TryFetchNextArgument(), TryFetchNextArgument());
                        cdToProgram = false;
                        break;
                    case "/wd":
                        procInfo.WorkingDirectory = TryFetchNextArgument();
                        break;
                    case "/cd":
                        cdToProgram = true;
                        break;
                    case "/verb":
                    case "/v":
                        procInfo.Verb = TryFetchNextArgument();
                        procInfo.UseShellExecute = true;
                        break;
                    case "/admin":
                        procInfo.Verb = "runas";
                        procInfo.UseShellExecute = true;
                        break;
                    case "/noadmin":
                    case "/na":
                        Environment.SetEnvironmentVariable("__COMPAT_LAYER", "RUNASINVOKER");
                        break;
                    case "/wait":
                    case "/w":
                        wait = true;
                        break;
                    case "/nowait":
                    case "/nw":
                        wait = false;
                        break;
                    case "/se":
                        procInfo.UseShellExecute = true;
                        break;
                    case "/j":
                        processorAffinity = int.Parse(TryFetchNextArgument());
                        if (processorAffinity > 0 && processorAffinity < 32)
                        {
                            processorAffinity = (1 << processorAffinity) - 1;
                        }
                        break;
                    case "/j1":
                        processorAffinity = 1;
                        break;
                    case "/pa":
                        processorAffinity = int.Parse(TryFetchNextArgument());
                        break;
                    case "/pr":
                    case "/priority":
                        switch (TryFetchNextArgument())
                        {
                            case "0":
                            case "idle":
                                priority = ProcessPriorityClass.Idle;
                                break;
                            case "1":
                            case "below":
                                priority = ProcessPriorityClass.BelowNormal;
                                break;
                            case "2":
                            case "normal":
                                priority = ProcessPriorityClass.Normal;
                                break;
                            case "3":
                            case "above":
                                priority = ProcessPriorityClass.AboveNormal;
                                break;
                            case "4":
                            case "high":
                                priority = ProcessPriorityClass.High;
                                break;
                            case "5":
                            case "realtime":
                                priority = ProcessPriorityClass.RealTime;
                                break;
                            default:
#if CONSOLE
                                Console.Error.WriteLine("unknown priority");
#endif
                                Environment.Exit(1);
                                break;
                        }
                        break;
                    default:
                        if (currentArg.StartsWith("/"))
                        {
#if CONSOLE
                            Console.Error.WriteLine("unknown option: " + currentArg);
#endif
                            Environment.Exit(1);
                        }
                        procInfo.FileName = currentArg;
                        break;
                }
            }

#if CONSOLE
            if (wait && !procInfo.UseShellExecute)
            {
                procInfo.RedirectStandardOutput = true;
                procInfo.RedirectStandardInput = true;
                procInfo.RedirectStandardError = true;
            }
#endif

            if (cdToProgram)
            {
                procInfo.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(procInfo.FileName));
            }

            var process = Process.Start(procInfo);

            if (processorAffinity != 0)
            {
                process.ProcessorAffinity = (IntPtr)processorAffinity;
            }

            if (priority != ProcessPriorityClass.Normal)
            {
                process.PriorityClass = priority;
            }

            if (wait)
            {
                process.WaitForExit();
                Environment.Exit(process.ExitCode);
            }
#if !DEBUG
            }
#pragma warning disable CS0168 // unused variable e without CONSOLE
            catch (Exception e)
#pragma warning restore CS0168 // unused variable e
            {
#if CONSOLE
                Console.Error.WriteLine(e.Message);
#endif
                Environment.Exit(1);
            }
#endif
        }

        private static string TryFetchNextArgument()
        {
            if (argLength > argIndex)
            {
                return Args[++argIndex];
            }
            else
            {
#if CONSOLE
                Console.Error.WriteLine("Missing arguments");
#endif
                Environment.Exit(1);
                return null;
            }
        }

#if CONSOLE
        private static void DisplayHelpAndExit()
        {
            Console.Write(
                "Usage: ProcessLauncherC [OPTIONS] FILE\r\n" +
                "Start a process with various options.\r\n" +
                "\r\n" +
                "/?, /h, /help    display this help and exit\r\n" +
                "/arg, /args      arguments passed to the executable as a single string\r\n" +
                "/e, /env KEY VALUE\r\n" +
                "                 set environment variable\r\n" +
                "                   This option can be passed for multiple times\r\n" +
                "/na, /noadmin    try to suppress UAC prompt\r\n" +
                "                   Alias of /env __COMPAT_LAYER RUNASINVOKER\r\n" +
                "/wd DIR          set working directory\r\n" +
                "/cd              set working directory to the same as FILE\r\n" +
                "/se              use ShellExecute\r\n" +
                "/v, /verb        use ShellExecute with specified verb\r\n" +
                "/admin           alias of /verb runas\r\n" +
                "/w, /wait        wait for the process to exit\r\n" +
                "                   Default for command line version.\r\n" +
                "/nw, /nowait     does not wait for the process to exit\r\n" +
                "                   Default for win32 version.\r\n" +
                "/pa              set processor affinity of the process\r\n" +
                "/j N             use only N processors\r\n" +
                "                   Alias of /pa ${2**n - 1}\r\n" +
                "/j1              alias of /pa 1 or /j 1\r\n" +
                "/pr, /priority N\r\n" +
                "                 set process priority (0-5)  [default: 2]\r\n"
            );
            Environment.Exit(0);
        }
#endif
    }
}
