using RabbitMQ.Client;
using System;
using System.IO;

namespace MaxQuantTaskCore
{
    internal class Config
    {
        private static Config instance = null;

        internal string ChannelPrefix { get; private set; } = "MQMP-";
        internal string JobQueueName { get; private set; } = "JobQueue";
        internal string ErrorLogName { get; private set; } = "ErrorLog";
        internal string HostName { get; private set; } = "localhost";

        internal int PrepareSearchOverride { get; private set; } = 0;

        internal int Port { get; private set; } = 5672;

        internal string DotNetPath { get; private set; } = "dotnet";

        internal string MaxQuantTaskCorePath { get; private set; } = "MaxQuantTaskCore.Original.dll";

        internal IConnection Connection { get; private set; }

        internal IModel Channel { get; private set; }

        internal int ThrottleMin { get; private set; } = 0;

        internal int ThrottleMax { get; private set; } = 0;

        internal string LogDir { get; private set; }

        private Config()
        {
            string configPath = Environment.GetEnvironmentVariable("MQG_CONFIG_PATH");

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
                        Port = int.Parse(value);
                        break;

                    case "rmq_prefix":
                        ChannelPrefix = value;
                        break;

                    case "prepare_search_threads":
                        PrepareSearchOverride = int.Parse(value);
                        break;

                    case "throttle_min":
                        ThrottleMin = int.Parse(value);
                        break;

                    case "throttle_max":
                        ThrottleMax = int.Parse(value);
                        break;

                    case "log_dir":
                        LogDir = value;
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
            Console.WriteLine("MQG_CONFIG_PATH\t" + Environment.GetEnvironmentVariable("MQG_CONFIG_PATH"));

            Console.WriteLine("==Config Variables==");
            Console.WriteLine("runtimepath\t" + DotNetPath);
            Console.WriteLine("taskpath\t" + MaxQuantTaskCorePath);
            Console.WriteLine("rmq_host\t" + HostName);
            Console.WriteLine("rmq_port\t" + Port);
            Console.WriteLine("rmq_prefix\t" + ChannelPrefix);
            Console.WriteLine("prepare_search_threads\t" + PrepareSearchOverride);
            Console.WriteLine("throttle_min\t" + ThrottleMin);
            Console.WriteLine("throttle_max\t" + ThrottleMax);
            Console.WriteLine("log_dir\t" + LogDir);
        }
    }
}