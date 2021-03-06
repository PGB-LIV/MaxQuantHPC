using System;
using System.Globalization;
using System.IO;

namespace MaxQuantHPC.Utilities
{
    internal class Logger
    {
        private static Logger instance;

        private string Hostname { get; set; }

        private int ProcessId { get; set; }

        private TextWriter LogFile { get; set; }

        private Logger()
        {
            Hostname = System.Net.Dns.GetHostName();

            ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;

            if (Config.Instance.LogDir != null)
            {
                LogFile = new StreamWriter(Config.Instance.LogDir + Path.DirectorySeparatorChar + Hostname + "." + ProcessId + ".log");
            }
        }

        internal void LogToStdOut()
        {
            LogFile = new StreamWriter(Console.OpenStandardOutput());
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
            if (LogFile == null)
            {
                return;
            }

            LogFile.WriteLine("[" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) + "] " + text);
            LogFile.Flush();
        }

        ~Logger()
        {
            if (LogFile != null)
            {
                LogFile.Close();
                LogFile = null;
            }
        }
    }
}