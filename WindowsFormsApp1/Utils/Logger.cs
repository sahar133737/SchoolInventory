using System;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp1.Utils
{
    /// <summary>
    /// Модуль логирования
    /// </summary>
    public static class Logger
    {
        private static readonly string LogDirectory = Path.Combine(Application.StartupPath, "Logs");
        private static readonly string LogFile = Path.Combine(LogDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");

        static Logger()
        {
            // Создаем директорию для логов, если её нет
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        /// <summary>
        /// Записать информационное сообщение
        /// </summary>
        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        /// <summary>
        /// Записать предупреждение
        /// </summary>
        public static void Warning(string message)
        {
            WriteLog("WARNING", message);
        }

        /// <summary>
        /// Записать ошибку
        /// </summary>
        public static void Error(string message, Exception ex = null)
        {
            string errorMessage = message;
            if (ex != null)
            {
                errorMessage += $"\nИсключение: {ex.GetType().Name}\nСообщение: {ex.Message}\nСтек вызовов: {ex.StackTrace}";
            }
            WriteLog("ERROR", errorMessage);
        }

        /// <summary>
        /// Записать отладочное сообщение
        /// </summary>
        public static void Debug(string message)
        {
#if DEBUG
            WriteLog("DEBUG", message);
#endif
        }

        private static void WriteLog(string level, string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                
                using (StreamWriter writer = new StreamWriter(LogFile, true, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine(logEntry);
                }
            }
            catch
            {
                // Игнорируем ошибки записи в лог, чтобы не нарушать работу приложения
            }
        }

        /// <summary>
        /// Очистить старые логи (старше указанного количества дней)
        /// </summary>
        public static void CleanOldLogs(int daysToKeep = 30)
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    return;

                DateTime cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                
                foreach (string file in Directory.GetFiles(LogDirectory, "log_*.txt"))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки очистки
            }
        }
    }
}

