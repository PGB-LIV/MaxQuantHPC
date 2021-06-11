﻿using RabbitMQ.Client;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MaxQuantHPC.Agent
{
    internal class PublisherAgent : Agent
    {
        public string Command { get; internal set; }
        public string CorrelationId { get; private set; }
        public string ReplyChannelName { get; private set; }

        private byte[] ReplyKey { get; } = Guid.NewGuid().ToByteArray();

        private byte[] CorrelationKey { get; } = Guid.NewGuid().ToByteArray();

        internal void Start()
        {
            Connect();
            Logger.Instance.Write("Connected to RabbitMQ Server");
        }

        internal void SendCommand(string[] args)
        {
            Command = FormatCommand(args);

            Logger.Instance.Write("Received: " + Command);

            IBasicProperties enqueueProperties = Channel.CreateBasicProperties();

            CorrelationId = GetCorrelationId(Command);
            ReplyChannelName = CreateReplyChannel(Command);

            enqueueProperties.ReplyTo = ReplyChannelName;
            enqueueProperties.CorrelationId = CorrelationId;

            byte[] commandBytes = Encoding.UTF8.GetBytes(Command);

            Logger.Instance.Write("Sending to: " + JobQueueName + ", ID: " + CorrelationId + ", Reply-to: " + ReplyChannelName);
            Channel.BasicPublish(exchange: "", routingKey: JobQueueName, basicProperties: enqueueProperties, body: commandBytes);

            Logger.Instance.Write("Sent.");

            if (Config.ExecOnTask != null)
            {
                ExecuteTaskScript();
            }
        }

        private void ExecuteTaskScript()
        {
            string[] var = Config.ExecOnTask.Split(' ', 2);

            string command = var[0];
            string args = "";
            if (var.Length > 1)
            {
                args = var[1];
            }
            ProcessStartInfo startInfo = new ProcessStartInfo(command, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.OutputDataReceived += (s, d) =>
            {
                Logger.Instance.Write(d.Data);
            };

            // Capture error output
            process.ErrorDataReceived += (s, d) =>
            {
                Logger.Instance.Write(d.Data);
            };

            _ = process.Start();

            // start listening on the stream
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit();
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

                if (correlationId != CorrelationId)
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

                uint messagesDeleted = Channel.QueueDelete(ReplyChannelName, true, true);
                Logger.Instance.Write("Channel deleted. " + messagesDeleted + " Messages deleted.");

                if (programResult.ExitCode != 0)
                {
                    Channel.BasicPublish(exchange: "", routingKey: ErrorLogName, body: result.Body);
                }

                return programResult;
            }
        }

        private string CreateReplyChannel(string command)
        {
            string replyChannelName = Config.ChannelPrefix + GetHash(ReplyKey, command);

            _ = Channel.QueueDeclare(queue: replyChannelName, durable: false,
              exclusive: false, autoDelete: true, arguments: null);

            return replyChannelName;
        }

        private string GetCorrelationId(string command)
        {
            return GetHash(CorrelationKey, command);
        }

        private static string GetHash(byte[] key, string plainText)
        {
            using (HMAC hashAlgorithm = HMAC.Create("HMACMD5"))
            {
                hashAlgorithm.Key = key;
                byte[] inputBytes = Encoding.ASCII.GetBytes(plainText);
                byte[] hashBytes = hashAlgorithm.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    _ = sb.Append(hashBytes[i].ToString("X2", CultureInfo.InvariantCulture));
                }

                return sb.ToString();
            }
        }
    }
}