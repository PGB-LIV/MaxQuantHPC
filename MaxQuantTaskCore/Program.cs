using System;
using System.IO;
using System.Reflection;

namespace MaxQuantTaskCore
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (Environment.GetEnvironmentVariable("MQG_CONFIG_PATH") == null)
            {
                System.Console.Error.WriteLine("Error! You must set the environment MQG_CONFIG_PATH to the location of your config.txt");
                return 2;
            }

            try
            {
                return HandleArgs(args);
            }
            catch (Exception e)
            {
                string text = "----------\n";
                text += e.Message + "\n";
                text += e.StackTrace + "\n";
                text += "----------\n";

                Logger.Instance.Write(text);
            }

            return 1;
        }

        private static int HandleArgs(string[] args)
        {
            if (args.Length == 0 || args[0] == "--help")
            {
                Console.Out.WriteLine("MaxQuant-GRID v" + Assembly.GetEntryAssembly().GetName().Version);
                Console.Out.WriteLine("--agent to start worker node agent");
                Console.Out.WriteLine("--errorLog to dump error log");
                Console.Out.WriteLine("--queue to dump error log");
                Console.Out.WriteLine("--config to dump config");
                return 0;
            }
            else if (args[0] == "--errorLog")
            {
                LogDaemon logs = new LogDaemon();
                logs.DumpLogs();

                return 0;
            }
            else if (args[0] == "--agent")
            {
                Console.Out.WriteLine("Launching agent...");
                ListenDaemon daemon = new ListenDaemon();
                daemon.Start();

                return 0;
            }
            else if (args[0] == "--queue")
            {
                QueueDaemon queue = new QueueDaemon();
                queue.DumpQueue();

                return 0;
            }
            else if (args[0] == "--config")
            {
                Config config = Config.Instance;
                config.DumpConfig();

                return 0;
            }
            else if (args.Length < 3)
            {
                Console.Out.WriteLine("Argument not found. See --help");
            }

            ProgramResult programResult;
            MiddleMan middleMan = new MiddleMan();

            middleMan.Start();

            if (args[6] == "70" && Config.Instance.PrepareSearchOverride != 0)
            {
                args[9] = Config.Instance.PrepareSearchOverride.ToString();
            }

            middleMan.SendCommand(args);
            programResult = middleMan.WaitResult();

            Console.Error.Write(programResult.StdErr);
            Console.Out.Write(programResult.StdOut);

            return programResult.ExitCode;
        }
    }
}