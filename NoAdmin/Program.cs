using System;
using System.Diagnostics;

namespace NoAdmin
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Environment.Exit(1);
            }

            Environment.SetEnvironmentVariable("__COMPAT_LAYER", "RUNASINVOKER");

            var proc = new ProcessStartInfo(args[0])
            {
                WorkingDirectory = Environment.CurrentDirectory,
            };

            if (args.Length > 1)
            {
                #region Set Remaining Arguments
                var commandLine = Environment.CommandLine;

                var argIndex = 0;

                // trim off args[0] and args[1]
                for (var argc = 0; argc < 2; ++argc)
                {
                    if (commandLine[argIndex] == '"')
                    {
                        // Since the first two arguments must be filenames containing no double-quotes, 
                        // a double-quote followed by a space is the closing double-quote.
                        argIndex = commandLine.IndexOf("\" ", argIndex + 1, StringComparison.Ordinal);
                    }
                    else
                    {
                        // No double-quotes, so args begin after first whitespace.
                        argIndex = commandLine.IndexOf(' ', argIndex);
                    }

                    // We already know that args.Length > 1, so we are sure that argIndex is not -1 now.

                    // Trim off all white spaces.
                    // Use ++argIndex instead of argIndex++ here because the first char is an unwanted double-quote or space.
                    while (commandLine[++argIndex] == ' ') ;
                }

                proc.Arguments = commandLine.Substring(argIndex);
                #endregion
            }

            try
            {
                Process.Start(proc);
            }
            catch (Exception)
            {
                // Console.Error.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }
    }
}
