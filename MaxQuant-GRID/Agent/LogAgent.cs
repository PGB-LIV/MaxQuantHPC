using RabbitMQ.Client;
using System;
using System.Text;

namespace MaxQuantHPC.Agent
{
    internal class LogAgent : Agent
    {
        internal void DumpLogs()
        {
            Connect();

            while (true)
            {
                BasicGetResult logEntry = Channel.BasicGet(ErrorLogName, true);

                if (logEntry == null)
                {
                    break;
                }

                ProcessLog(logEntry);
            }

            Console.Out.WriteLine("Done");
        }

        private static void ProcessLog(BasicGetResult logEntry)
        {
            string jsonResponse = Encoding.UTF8.GetString(logEntry.Body.ToArray());

            Console.Out.WriteLine("----");
            Console.Out.WriteLine(jsonResponse);
            Console.Out.WriteLine("----");
        }
    }
}