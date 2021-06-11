using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;

namespace MaxQuantHPC.Agent
{
    internal abstract class Agent
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
                    ConnectionFactory factory = new ConnectionFactory() { HostName = Config.HostName, Port = Config.Port, AutomaticRecoveryEnabled = true };
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

            _ = Channel.QueueDeclare(queue: JobQueueName, durable: false,
              exclusive: false, autoDelete: false, arguments: null);

            _ = Channel.QueueDeclare(queue: ErrorLogName, durable: false,
              exclusive: false, autoDelete: false, arguments: null);
        }

        internal void Stop()
        {
            Disconnect();
        }

        protected void Disconnect()
        {
            Connection.Close();
        }

        internal string FormatCommand(string[] args)
        {
            StringBuilder sbCmd = new StringBuilder();

            _ = sbCmd.Append('"');
            _ = sbCmd.Append(Config.MaxQuantTaskCorePath);
            _ = sbCmd.Append('"');

            foreach (string arg in args)
            {
                _ = sbCmd.Append(' ');
                _ = sbCmd.Append('"');
                _ = sbCmd.Append(arg);
                _ = sbCmd.Append('"');
            }

            return sbCmd.ToString();
        }

        ~Agent()
        {
            if (Connection.IsOpen)
            {
                Disconnect();
            }
        }
    }
}