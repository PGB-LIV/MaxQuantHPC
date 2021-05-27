using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MaxQuantTaskCore
{
    internal class Logger
    {
        private static Logger instance = null;

        private string Hostname { get; set; }

        private string LogDir { get; set; }

        private int ProcessId { get; set; }

        private TextWriter LogFile { get; set; }

        private Logger()
        {
            Hostname = System.Net.Dns.GetHostName();
            LogDir = Config.Instance.LogDir;
            ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;

            LogFile = new StreamWriter(LogDir + Path.DirectorySeparatorChar + Hostname + "." + ProcessId + ".log");
        }

        internal static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logger();
                }

                return instance;
            }
        }

        internal void Write(string text)
        {
            LogFile.WriteLine("[" + DateTime.UtcNow.ToString() + "] " + text);
            LogFile.Flush();
        }
    }
}