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
            var procInfo = new ProcessStartInfo()
            {
#if CONSOLE
                UseShellExecute = false
#endif
            };

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
                    case "/c":
                        procInfo.FileName = TryFetchNextArgument();

                        if (argIndex + 1 < argLength)
                        {
                            #region Set Remaining Arguments
                            // use argIndex as string index
                            argIndex = 0;
                            currentArg = Environment.CommandLine;
                            currentArg = currentArg.Substring(currentArg.IndexOf("/c ") + 3);

                            // trim FILE
                            if (currentArg[argIndex] == '"')
                            {
                                argIndex = currentArg.IndexOf("\" ", argIndex + 1, StringComparison.Ordinal);
                            }
                            else
                            {
                                argIndex = currentArg.IndexOf(' ', argIndex);
                            }

                            while (currentArg[++argIndex] == ' ') ;

                            procInfo.Arguments = currentArg.Substring(argIndex);
                            #endregion
                        }
                        argIndex = argLength; // break outer loop
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
                    case "/nse":
                        procInfo.UseShellExecute = false;
                        break;
                    case "/j":
                        int.TryParse(TryFetchNextArgument(), out processorAffinity);
                        if (processorAffinity > 0 && processorAffinity < 32)
                        {
                            processorAffinity = (1 << processorAffinity) - 1;
                        }
                        else
                        {
                            WriteErrorAndExit("Invalid argument for /j");
                        }
                        break;
                    case "/j1":
                        processorAffinity = 1;
                        break;
                    case "/pa":
                        if (!int.TryParse(TryFetchNextArgument(), out processorAffinity))
                        {
                            WriteErrorAndExit("Invalid argument for /pa");
                        }
                        break;
                    case "/pr":
                    case "/priority":
                        #region Process Priority
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
                        #endregion
                        break;
                    default:
                        if (currentArg.StartsWith("/"))
                        {
                            WriteErrorAndExit(currentArg + ": Invalid option");
                        }
                        procInfo.FileName = currentArg;

                        // argIndex now points at filename, increase 1 to point at arguments
                        ++argIndex;

                        if (argIndex > argLength)            // no arguments
                        {

                        }
                        else if (argIndex == argLength - 1)  // only one argument
                        {
                            procInfo.Arguments = args[argIndex];
                        }
                        else
                        {
                            // Not reliable. For more reliable arguments, use /c
                            procInfo.Arguments = String.Join(" ", args.Skip(argIndex));
                        }

                        argIndex = argLength; // break outer loop
                        break;
                }
            }

            if (String.IsNullOrEmpty(procInfo.FileName))
            {
                WriteErrorAndExit("No file given");
            }
#if !DEBUG
            try
            {
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
            catch (Exception e)
            {
                WriteErrorAndExit(e.Message);
            }
#endif
        }

        private static string TryFetchNextArgument()
        {
            if (argLength > ++argIndex)
            {
                return Args[argIndex];
            }
            else
            {
                WriteErrorAndExit("Missing arguments");
                return null;
            }
        }


        private static void WriteErrorAndExit(string errorMessage)
        {
#if CONSOLE
            Console.Error.WriteLine(errorMessage);
#endif
            Environment.Exit(1);

        }
#if CONSOLE
        private static void DisplayHelpAndExit()
        {
            Console.Write(
                "Usage: ProcessLauncherC [OPTIONS] FILE [ARGS]\r\n" +
                "       ProcessLauncherC [OPTIONS] /c FILE [ARG1 ARG2 ...]\r\n" +
                "Start a command with various options.\r\n" +
                "\r\n" +
                "/?, /h, /help    display this help and exit\r\n" +
                "/c FILE [ARG1 ARG2 ...]\r\n" +
                "                 execute specfied command.\r\n" +
                "                   /c must be after all options.\r\n" +
                "                   Anything after /c will be command." +
                "/e, /env KEY VALUE\r\n" +
                "                 set environment variable\r\n" +
                "                   This option can be passed for multiple times\r\n" +
                "/na, /noadmin    try to suppress UAC prompt\r\n" +
                "                   Alias of /env __COMPAT_LAYER RUNASINVOKER\r\n" +
                "/wd DIR          set working directory\r\n" +
                "/cd              set working directory to the same as FILE\r\n" +
                "/nse             no ShellExecute [CLI default]\r\n" +
                "/se              use ShellExecute [win32 default]\r\n" +
                "/v, /verb VERB   use ShellExecute with specified verb\r\n" +
                "/admin           alias of /verb runas\r\n" +
                "/w, /wait        wait for the process to exit [CLI default]\r\n" +
                "/nw, /nowait     no wait for the process to exit [win32 default]\r\n" +
                "/pa N            set processor affinity of the process\r\n" +
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
