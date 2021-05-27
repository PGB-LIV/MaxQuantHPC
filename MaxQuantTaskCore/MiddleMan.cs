using RabbitMQ.Client;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MaxQuantTaskCore
{
    internal class MiddleMan : Daemon
    {
        public string Command { get; internal set; }
        public string CorrelationId { get; private set; }
        public string ReplyChannelName { get; private set; }

        private readonly byte[] secretKey = Guid.NewGuid().ToByteArray();

        internal void Start()
        {
            this.Connect();
            Logger.Instance.Write("Connected to RabbitMQ Server");
        }

        internal void SendCommand(string[] args)
        {
            Command = FormatCommand(args);

            Logger.Instance.Write("Received: " + Command);

            IBasicProperties enqueueProperties = Channel.CreateBasicProperties();

            this.CorrelationId = this.GetCorrelationId(Command);
            this.ReplyChannelName = this.CreateReplyChannel(Command);

            enqueueProperties.ReplyTo = ReplyChannelName;
            enqueueProperties.CorrelationId = CorrelationId;

            byte[] commandBytes = Encoding.UTF8.GetBytes(Command);

            Logger.Instance.Write("Sending to: " + JobQueueName + ", ID: " + CorrelationId + ", Reply-to: " + ReplyChannelName);
            Channel.BasicPublish(exchange: "", routingKey: JobQueueName, basicProperties: enqueueProperties, body: commandBytes);
            Logger.Instance.Write("Sent.");
        }

        internal ProgramResult WaitResult()
        {
            Logger.Instance.Write("Awaiting response from: " + ReplyChannelName);
            while (true)
            {
                BasicGetResult result = Channel.BasicGet(ReplyChannelName, false);

                if (result == null)
                {
                    Thread.Sleep(10000);
                    continue;
                }

                string correlationId = result.BasicProperties.CorrelationId;

                if (correlationId != this.CorrelationId)
                {
                    Channel.BasicNack(result.DeliveryTag, false, true);
                    Logger.Instance.Write("Result Rejected. ID: " + correlationId);
                    continue;
                }

                Logger.Instance.Write("Result Accepted. ID: " + correlationId);

                string jsonResponse = Encoding.UTF8.GetString(result.Body.ToArray());

                ProgramResult programResult = Newtonsoft.Json.JsonConvert.DeserializeObject<ProgramResult>(jsonResponse);

                Logger.Instance.Write("Result: " + jsonResponse);

                Channel.BasicAck(result.DeliveryTag, false);
                Logger.Instance.Write("Acked.");
                Channel.QueueDelete(ReplyChannelName);
                Logger.Instance.Write("Channel deleted.");

                if (programResult.ExitCode != 0)
                {
                    // TODO: Set expires

                    Channel.BasicPublish(exchange: "", routingKey: ErrorLogName, body: result.Body);
                }

                return programResult;
            }
        }

        private string CreateReplyChannel(string command)
        {
            string replyChannelName = Config.ChannelPrefix + GetHash(secretKey, command);

            Channel.QueueDeclare(queue: replyChannelName, durable: false,
              exclusive: false, autoDelete: true, arguments: null);

            return replyChannelName;
        }

        private string GetCorrelationId(string command)
        {
            return GetHash(secretKey, command);
        }

        private static string GetHash(byte[] secretKey, string plainText)
        {
            using (HMAC hashAlgorithm = HMAC.Create("HMACMD5"))
            {
                hashAlgorithm.Key = secretKey;
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(plainText);
                byte[] hashBytes = hashAlgorithm.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}