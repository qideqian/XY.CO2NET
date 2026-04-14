using System;
using System.IO;

namespace XY.SMS
{
    public static class SmsLogHelper
    {
        public static void WriteLog(string content)
        {
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            var logFile = Path.Combine(logDir, $"sms_{DateTime.Now:yyyyMMdd}.txt");
            var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {content}\r\n";
            File.AppendAllText(logFile, logLine);
        }
    }
}