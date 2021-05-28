using RabbitMQ.Client;
using System;
using System.Text;

namespace MaxQuantTaskCore.Agent
{
    internal class QueueAgent : Agent
    {
        internal void DumpQueue()
        {
            this.Connect();

            BasicGetResult logEntry = Channel.BasicGet(JobQueueName, false);

            if (logEntry == null)
            {
                Console.Out.WriteLine("Queue Empty");
                return;
            }

            ProcessQueue(logEntry);
        }

        private static void ProcessQueue(BasicGetResult logEntry)
        {
            string jsonResponse = Encoding.UTF8.GetString(logEntry.Body.ToArray());

            Console.Out.WriteLine("----");
            Console.Out.WriteLine(jsonResponse);
            Console.Out.WriteLine("----");
        }
    }
}