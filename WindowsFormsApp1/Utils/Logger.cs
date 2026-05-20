using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1.Utils
{
    /// <summary>
    /// Модуль логирования в файлы Logs/log_yyyyMMdd.txt
    /// </summary>
    public static class Logger
    {
        private static readonly object Sync = new object();
        private static readonly string LogDirectory = Path.Combine(Application.StartupPath, "Logs");

        static Logger()
        {
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);
        }

        private static string CurrentLogFile => Path.Combine(LogDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");

        public static void Info(string message) => WriteLog("INFO", message);

        public static void Warning(string message) => WriteLog("WARNING", message);

        public static void Error(string message, Exception ex = null)
        {
            var sb = new StringBuilder(message);
            if (ex != null)
            {
                sb.AppendLine();
                sb.Append($"Исключение: {ex.GetType().Name}. {ex.Message}");
                if (!string.IsNullOrEmpty(ex.StackTrace))
                    sb.AppendLine().Append(ex.StackTrace);
            }
            WriteLog("ERROR", sb.ToString());
        }

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
                string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message.Replace("\r\n", " | ")}";
                lock (Sync)
                {
                    File.AppendAllText(CurrentLogFile, entry + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // не прерываем работу приложения
            }
        }

        /// <summary>
        /// Последние строки журнала (сегодняшний файл, при отсутствии — последний log_*.txt).
        /// </summary>
        public static string ReadRecentEntries(int maxLines = 500, string levelFilter = null)
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    return "Папка журналов не найдена.";

                string file = File.Exists(CurrentLogFile)
                    ? CurrentLogFile
                    : Directory.GetFiles(LogDirectory, "log_*.txt").OrderBy(f => f).LastOrDefault();

                if (string.IsNullOrEmpty(file) || !File.Exists(file))
                    return "Журнал пуст. Выполните действия в приложении — записи появятся здесь.";

                string[] lines;
                lock (Sync)
                    lines = File.ReadAllLines(file, Encoding.UTF8);

                if (!string.IsNullOrEmpty(levelFilter))
                    lines = lines.Where(l => l.IndexOf($"[{levelFilter}]", StringComparison.OrdinalIgnoreCase) >= 0).ToArray();

                if (lines.Length == 0)
                    return "Нет записей для выбранного уровня.";

                int start = Math.Max(0, lines.Length - maxLines);
                var sb = new StringBuilder();
                sb.AppendLine($"Файл: {Path.GetFileName(file)}");
                sb.AppendLine(new string('─', 60));
                for (int i = start; i < lines.Length; i++)
                    sb.AppendLine(lines[i]);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return "Ошибка чтения журнала: " + ex.Message;
            }
        }

        public static void OpenLogFolder()
        {
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);
            Process.Start("explorer.exe", LogDirectory);
        }

        public static void CleanOldLogs(int daysToKeep = 30)
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    return;

                DateTime cutoff = DateTime.Now.AddDays(-daysToKeep);
                foreach (string file in Directory.GetFiles(LogDirectory, "log_*.txt"))
                {
                    if (File.GetLastWriteTime(file) < cutoff)
                        File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                WriteLog("WARNING", "Не удалось очистить старые журналы: " + ex.Message);
            }
        }
    }
}
