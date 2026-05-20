using System;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1.Utils
{
    public static class ExceptionHandler
    {
        public static void AttachGlobalHandlers()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ShowError("Необработанная ошибка", e.Exception, null);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    Logger.Error("Критическая ошибка домена приложения", ex);
            };
        }

        public static void ShowError(string context, Exception ex, IWin32Window owner = null)
        {
            Logger.Error(context, ex);
            MessageBox.Show(owner, BuildUserMessage(context, ex), "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowWarning(string message, IWin32Window owner = null)
        {
            MessageBox.Show(owner, message, "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static string BuildUserMessage(string context, Exception ex)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(context))
                sb.AppendLine(context);
            sb.AppendLine(TranslateException(ex));
            return sb.ToString().Trim();
        }

        public static string TranslateException(Exception ex)
        {
            while (ex != null)
            {
                if (ex is SqlException sql)
                    return TranslateSqlException(sql);

                if (ex is InvalidOperationException)
                    return ex.Message;

                if (ex is UnauthorizedAccessException)
                    return "Недостаточно прав для выполнения операции.";

                ex = ex.InnerException;
            }
            return "Произошла непредвиденная ошибка. Подробности записаны в журнал системы.";
        }

        private static string TranslateSqlException(SqlException sql)
        {
            switch (sql.Number)
            {
                case 547:
                    return "Операция невозможна: связанные данные в базе (проверьте кабинет, ответственного или объект имущества).";
                case 2627:
                case 2601:
                    return "Такая запись уже существует.";
                case 2:
                case 53:
                case -1:
                    return "Не удалось подключиться к базе данных. Проверьте SQL Server и строку подключения.";
                case 4060:
                    return "База данных не найдена. Выполните скрипт создания БД.";
                default:
                    return string.IsNullOrWhiteSpace(sql.Message)
                        ? "Ошибка базы данных."
                        : sql.Message;
            }
        }
    }
}
