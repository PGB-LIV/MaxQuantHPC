using RabbitMQ.Client;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace MaxQuantHPC.Agent
{
    internal class ConsumerAgent : Agent
    {
        private bool IsShutdown { get; set; }

        internal void Start()
        {
            Connect();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Out.WriteLine("Connected to RabbitMQ Server. Requesting work...");

            WaitForJobs();
        }

        private void WaitForJobs()
        {
            int idleTime = 0;
            while (true)
            {
                BasicGetResult job = Channel.BasicGet(JobQueueName, false);

                Random rand = new Random();
                int sleepMs = rand.Next(Config.ThrottleMin * 1000, Config.ThrottleMax * 1000);

                Thread.Sleep(sleepMs);
                idleTime += sleepMs / 1000;

                if (job != null)
                {
                    ProcessJob(job);
                    idleTime = 0;
                }

                if (Config.MaxIdleTime > 0 && idleTime > Config.MaxIdleTime)
                {
                    IsShutdown = true;
                }

                if (IsShutdown)
                {
                    break;
                }
            }
        }

        private void ProcessJob(BasicGetResult job)
        {
            string response = null;

            byte[] body = job.Body.ToArray();
            IBasicProperties properties = job.BasicProperties;
            IBasicProperties replyProperties = Channel.CreateBasicProperties();
            replyProperties.CorrelationId = properties.CorrelationId;

            try
            {
                string command = Encoding.UTF8.GetString(body);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Out.WriteLine("Command received:");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine(command);

                if (command == "SHUTDOWN")
                {
                    IsShutdown = true;
                    return;
                }

                ProgramResult result = Execute(command);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Out.WriteLine("Done.");

                response = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            }
            catch (Exception e)
            {
                ProgramResult result = new ProgramResult
                {
                    StdErr = e.Message,
                    ExitCode = 1
                };

                response = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            }
            finally
            {
                if (!IsShutdown)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Out.WriteLine("Returning Result:");

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Out.WriteLine(response);

                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    Channel.BasicPublish(exchange: "", routingKey: properties.ReplyTo,
                      basicProperties: replyProperties, body: responseBytes);
                }

                Channel.BasicAck(deliveryTag: job.DeliveryTag,
                  multiple: false);
            }
        }

        internal ProgramResult Execute(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(
                Config.DotNetPath, command
                )
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Out.WriteLine("Executing:");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Out.WriteLine(Config.DotNetPath + " " + command);

            Process process = new Process
            {
                StartInfo = startInfo
            };

            StringBuilder errors = new StringBuilder();
            StringBuilder output = new StringBuilder();
            bool hadErrors = false;
            process.OutputDataReceived += (s, d) =>
            {
                _ = output.Append(d.Data);
            };

            // Capture error output
            process.ErrorDataReceived += (s, d) =>
            {
                if (!hadErrors)
                {
                    hadErrors = !string.IsNullOrEmpty(d.Data);
                }

                _ = errors.Append(d.Data);
            };

            _ = process.Start();

            // start listening on the stream
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit();

            int exitCode = process.ExitCode;

            ProgramResult result = new ProgramResult
            {
                StdOut = output.ToString(),
                StdErr = errors.ToString(),
                ExitCode = exitCode,
                Command = Config.DotNetPath + " " + command
            };

            return result;
        }
    }
}