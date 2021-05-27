using RabbitMQ.Client;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MaxQuantTaskCore
{
    internal abstract class Daemon
    {
        protected Config Config = Config.Instance;

        protected string JobQueueName { get; private set; } = "JobQueue";
        protected string ErrorLogName { get; private set; } = "ErrorLog";

        public IConnection Connection { get; private set; }
        public IModel Channel { get; private set; }

        protected IConnection EstablishConnection(int retry)
        {
            int attempts = 0;
            Exception rethrow;
            do
            {
                try
                {
                    ConnectionFactory factory = new ConnectionFactory() { HostName = Config.HostName, Port = Config.Port };
                    return factory.CreateConnection();
                }
                catch (Exception e)
                {
                    attempts++;
                    int sleepTime = (int)Math.Pow(2, attempts) * 1000;

                    Thread.Sleep(sleepTime);
                    rethrow = e;
                }
            } while (attempts < retry);

            throw rethrow;
        }

        protected void Connect()
        {
            Connection = EstablishConnection(6);

            Channel = Connection.CreateModel();

            Channel.BasicQos(0, 1, false);

            Channel.QueueDeclare(queue: JobQueueName, durable: false,
              exclusive: false, autoDelete: false, arguments: null);

            Channel.QueueDeclare(queue: ErrorLogName, durable: false,
              exclusive: false, autoDelete: false, arguments: null);
        }

        internal string FormatCommand(string[] args)
        {
            StringBuilder sbCmd = new StringBuilder();

            sbCmd.Append('"');
            sbCmd.Append(Config.MaxQuantTaskCorePath);
            sbCmd.Append('"');

            foreach (string arg in args)
            {
                sbCmd.Append(' ');
                sbCmd.Append('"');
                sbCmd.Append(arg);
                sbCmd.Append('"');
            }

            return sbCmd.ToString();
        }
    }
}