using MaxQuantTaskCore.Agent;
using System;
using System.Reflection;

namespace MaxQuantTaskCore
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
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
            if (Environment.GetEnvironmentVariable("MQG_CONFIG_PATH") == null)
            {
                Console.Error.WriteLine("Error! You must set the environment MQG_CONFIG_PATH to the location of your settings.conf");
                return 2;
            }

            if (args.Length < 3)
            {
                return ParseLocalArg(args);
            }

            return StartPublisherAgent(args);
        }

        private static int ParseLocalArg(string[] args)
        {
            if (args[0] == "--errorLog")
            {
                LogAgent logs = new LogAgent();
                logs.DumpLogs();

                return 0;
            }
            else if (args[0] == "--agent")
            {
                Console.Out.WriteLine("Launching agent...");
                ConsumerAgent daemon = new ConsumerAgent();
                daemon.Start();

                return 0;
            }
            else if (args[0] == "--queue")
            {
                QueueAgent queue = new QueueAgent();
                queue.DumpQueue();

                return 0;
            }
            else if (args[0] == "--config")
            {
                Config config = Config.Instance;
                config.DumpConfig();

                return 0;
            }

            Console.Out.WriteLine("MaxQuant-GRID v" + Assembly.GetEntryAssembly().GetName().Version);
            Console.Out.WriteLine("--agent to start worker node agent");
            Console.Out.WriteLine("--errorLog to dump error log");
            Console.Out.WriteLine("--queue to dump error log");
            Console.Out.WriteLine("--config to dump config");
            return 0;
        }

        private static int StartPublisherAgent(string[] args)
        {
            ProgramResult programResult;
            PublisherAgent agent = new PublisherAgent();

            agent.Start();

            agent.SendCommand(args);
            programResult = agent.WaitResult();
            agent.Stop();

            Console.Error.Write(programResult.StdErr);
            Console.Out.Write(programResult.StdOut);

            return programResult.ExitCode;
        }
    }
}