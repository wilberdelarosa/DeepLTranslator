using System;
using System.IO;
using System.Text;

namespace DeepLTranslator.Logging
{
    public static class ErrorLogger
    {
        private static readonly string LogFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DeepLTranslator",
            "error_log.txt"
        );

        static ErrorLogger()
        {
            try
            {
                var directory = Path.GetDirectoryName(LogFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch
            {
                // Si no se puede crear el directorio, los logs se perderán
            }
        }

        public static void LogError(Exception ex, string context = "")
        {
            try
            {
                var logEntry = new StringBuilder();
                logEntry.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR");
                logEntry.AppendLine($"Context: {context}");
                logEntry.AppendLine($"Exception Type: {ex.GetType().Name}");
                logEntry.AppendLine($"Message: {ex.Message}");
                logEntry.AppendLine($"Stack Trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    logEntry.AppendLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                logEntry.AppendLine(new string('-', 80));

                File.AppendAllText(LogFilePath, logEntry.ToString());
            }
            catch
            {
                // Si no se puede escribir al log, no hacer nada para evitar excepciones en cascada
            }
        }

        public static void LogInfo(string message, string context = "")
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO - {context}: {message}\n";
                File.AppendAllText(LogFilePath, logEntry);
            }
            catch
            {
                // Si no se puede escribir al log, no hacer nada
            }
        }

        public static string GetLogFilePath() => LogFilePath;
    }
}
