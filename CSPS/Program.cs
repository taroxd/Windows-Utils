using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSPS
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

            if (argLength == 0)
            {
                DisplayHelpAndExit();
            }

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
                    case "/help":
                    case "/h":
                    case "/?":
                        DisplayHelpAndExit();
                        break;
                    case "/c":
                        procInfo.FileName = TryFetchNextArgument();

                        if (argIndex + 1 < argLength)
                        {
                            #region Set Remaining Arguments

                            currentArg = Environment.CommandLine;

                            // use argIndex as string index
                            argIndex = currentArg.IndexOf("/c ", StringComparison.Ordinal) + 2;

                            while (currentArg[++argIndex] == ' ') ;

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
                        break;
                    case "/wd":
                        procInfo.WorkingDirectory = TryFetchNextArgument();
                        cdToProgram = false;
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
#if CONSOLE
                            Console.Error.WriteLine("Invalid argument for /j");
#endif
                            Environment.Exit(1);
                        }
                        break;
                    case "/j1":
                        processorAffinity = 1;
                        break;
                    case "/pa":
                        if (!int.TryParse(TryFetchNextArgument(), out processorAffinity))
                        {
#if CONSOLE
                            Console.Error.WriteLine("Invalid argument for /pa");
#endif
                            Environment.Exit(1);
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
#if CONSOLE
                            Console.Error.WriteLine(currentArg + ": Invalid option");
#endif
                            Environment.Exit(1);
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
#if CONSOLE
                Console.Error.WriteLine("No file given");
#endif
                Environment.Exit(1);
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
#if CONSOLE
                Console.Error.WriteLine(e.Message);
#endif
                Environment.Exit(1);
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
#if CONSOLE
                Console.Error.WriteLine("Missing arguments");
#endif
                Environment.Exit(1);
                return null;
            }
        }

        private static void DisplayHelpAndExit()
        {
#if CONSOLE
            Console.Write(
                "Usage: cspsc [OPTIONS] FILE [ARGS]\r\n" +
                "       cspsc [OPTIONS] /c FILE [ARG1 ARG2 ...]\r\n" +
                "\r\n" +
                "Start a process with various options\r\n" +
                "\r\n" +
                "/?, /h, /help\r\n" +
                "    display this help and exit\r\n" +
                "\r\n" +
                "/c FILE [ARG1 ARG2 ...]\r\n" +
                "    execute specfied command\r\n" +
                "    /c must be after all options\r\n" +
                "    Anything after /c will be command\r\n" +
                "\r\n" +
                "/e, /env KEY VALUE\r\n" +
                "    set environment variable\r\n" +
                "    This option can be passed for multiple times\r\n" +
                "\r\n" +
                "/na, /noadmin\r\n" +
                "    try to suppress UAC prompt\r\n" +
                "    Alias of /env __COMPAT_LAYER RUNASINVOKER\r\n" +
                "\r\n" +
                "/wd DIR\r\n" +
                "    set working directory\r\n" +
                "\r\n" +
                "/cd\r\n" +
                "    set working directory to the same as FILE\r\n" +
                "\r\n" +
                "/nse\r\n" +
                "    no ShellExecute [CLI default]\r\n" +
                "\r\n" +
                "/se\r\n" +
                "    use ShellExecute [win32 default]\r\n" +
                "\r\n" +
                "/v, /verb VERB\r\n" +
                "    use ShellExecute with specified verb\r\n" +
                "\r\n" +
                "/admin\r\n" +
                "    alias of /verb runas\r\n" +
                "\r\n" +
                "/w, /wait\r\n" +
                "    wait for the process to exit [CLI default]\r\n" +
                "\r\n" +
                "/nw, /nowait\r\n" +
                "    no wait for the process to exit [win32 default]\r\n" +
                "\r\n" +
                "/pa N\r\n" +
                "    set processor affinity of the process\r\n" +
                "\r\n" +
                "/j N\r\n" +
                "    use only N processors\r\n" +
                "    Alias of /pa ${2**n - 1}\r\n" +
                "\r\n" +
                "/j1\r\n" +
                "    alias of /pa 1 or /j 1\r\n" +
                "\r\n" +
                "/pr, /priority N\r\n" +
                "    set process priority (0-5)  [default: 2]\r\n"
            );
#endif
            Environment.Exit(1);
        }
    }
}
