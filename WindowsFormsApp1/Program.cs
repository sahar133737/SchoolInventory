using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Logger.Info("=== Запуск приложения Школьная инвентаризация ===");
                
                // Очистка старых логов
                Logger.CleanOldLogs(30);
                
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                // Показываем форму авторизации
                using (LoginForm loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        Logger.Info($"Успешная авторизация пользователя: {loginForm.LoggedInFullName} ({loginForm.LoggedInRole})");
                        // Если авторизация успешна, показываем главную форму
                        Application.Run(new Form1(loginForm.LoggedInUsername, loginForm.LoggedInFullName, loginForm.LoggedInRole));
                    }
                    else
                    {
                        Logger.Info("Авторизация отменена пользователем");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Критическая ошибка при запуске приложения", ex);
                MessageBox.Show($"Критическая ошибка при запуске приложения:\n{ex.Message}\n\nПодробности в лог-файле.", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Logger.Info("=== Завершение работы приложения ===");
            }
        }
    }
}
