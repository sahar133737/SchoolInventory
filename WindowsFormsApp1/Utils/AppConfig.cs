using System;
using System.Configuration;

namespace WindowsFormsApp1.Utils
{
    /// <summary>
    /// Модуль конфигурации приложения
    /// </summary>
    public static class AppConfig
    {
        /// <summary>
        /// Получить строку подключения к базе данных
        /// </summary>
        public static string GetConnectionString()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // Значение по умолчанию
                return @"Server=SAHAR\SQLSERVER;Database=SchoolInventoryDB;Integrated Security=true;";
            }
            
            return connectionString;
        }

        /// <summary>
        /// Получить настройку из конфигурации
        /// </summary>
        public static string GetSetting(string key, string defaultValue = "")
        {
            string value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        /// <summary>
        /// Получить настройку как булево значение
        /// </summary>
        public static bool GetBoolSetting(string key, bool defaultValue = false)
        {
            string value = GetSetting(key);
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Получить настройку как целое число
        /// </summary>
        public static int GetIntSetting(string key, int defaultValue = 0)
        {
            string value = GetSetting(key);
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }
    }
}

