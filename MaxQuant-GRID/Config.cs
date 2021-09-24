using RabbitMQ.Client;
using System;
using System.Globalization;
using System.IO;

namespace MaxQuantHPC
{
    internal class Config
    {
        private static Config instance;

        internal string ConfigPathVarName { get; } = "MQHPC_CONFIG_PATH";

        internal string ChannelPrefix { get; private set; } = "MQMP-";
        internal string JobQueueName { get; private set; } = "JobQueue";
        internal string ErrorLogName { get; private set; } = "ErrorLog";
        internal string HostName { get; private set; } = "localhost";

        internal int Port { get; private set; } = 5672;

        internal string DotNetPath { get; private set; } = "dotnet";

        internal string MaxQuantTaskCorePath { get; private set; } = "MaxQuantTaskCore.Original.dll";

        internal IConnection Connection { get; private set; }

        internal IModel Channel { get; private set; }

        internal int ThrottleMin { get; private set; }

        internal int ThrottleMax { get; private set; }

        internal string LogDir { get; private set; }

        internal int MaxIdleTime { get; private set; }

        internal string ExecOnTask { get; private set; }

        private Config()
        {
            string configPath = Environment.GetEnvironmentVariable(ConfigPathVarName);

            string[] config = File.ReadAllLines(configPath);

            foreach (string line in config)
            {
                string[] pair = line.Trim().Split('=');
                string key = pair[0].Trim();
                string value = pair[1].Trim();

                switch (key)
                {
                    case "runtimepath":
                        DotNetPath = value;
                        break;

                    case "taskpath":
                        MaxQuantTaskCorePath = value;
                        break;

                    case "rmq_host":
                        HostName = value;
                        break;

                    case "rmq_port":
                        Port = int.Parse(value, CultureInfo.InvariantCulture);
                        break;

                    case "rmq_prefix":
                        ChannelPrefix = value;
                        break;

                    case "throttle_min":
                        ThrottleMin = int.Parse(value, CultureInfo.InvariantCulture);
                        break;

                    case "throttle_max":
                        ThrottleMax = int.Parse(value, CultureInfo.InvariantCulture);
                        break;

                    case "log_dir":
                        if (value.Length == 0)
                        {
                            break;
                        }

                        LogDir = value;
                        break;

                    case "max_idle_time":
                        MaxIdleTime = int.Parse(value, CultureInfo.InvariantCulture);
                        break;

                    case "exec_on_task":
                        ExecOnTask = value;
                        break;

                    default:
                        // Do nothing. For now.
                        break;
                }
            }

            JobQueueName = ChannelPrefix + "JobQueue";
            ErrorLogName = ChannelPrefix + "ErrorLog";
        }

        internal static Config Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Config();
                }

                return instance;
            }
        }

        internal void DumpConfig()
        {
            Console.WriteLine("==Environment Variables==");
            Console.WriteLine(ConfigPathVarName + "\t" + Environment.GetEnvironmentVariable(ConfigPathVarName));

            Console.WriteLine("==Config Variables==");
            Console.WriteLine("runtimepath\t" + DotNetPath);
            Console.WriteLine("taskpath\t" + MaxQuantTaskCorePath);
            Console.WriteLine("rmq_host\t" + HostName);
            Console.WriteLine("rmq_port\t" + Port);
            Console.WriteLine("rmq_prefix\t" + ChannelPrefix);
            Console.WriteLine("throttle_min\t" + ThrottleMin);
            Console.WriteLine("throttle_max\t" + ThrottleMax);
            Console.WriteLine("log_dir\t" + LogDir);
            Console.WriteLine("max_idle_time\t" + MaxIdleTime);
            Console.WriteLine("exec_on_task\t" + ExecOnTask);
        }
    }
}