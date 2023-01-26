using System;
using System.IO;

namespace ThreatDetectionModule
{
    internal class Logger 
    {
        private readonly string _logFilePath;

        public Logger(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public void WriteLine(string message)
        {
            using (var file = new StreamWriter(_logFilePath, true))
            {
                file.WriteLine($"[{DateTime.Now}] {message}");
            }
        }
    }
}
