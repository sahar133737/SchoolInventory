using System;
using System.Text.RegularExpressions;

namespace WindowsFormsApp1.Utils
{
    /// <summary>
    /// Модуль валидации данных
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Проверка на пустую строку
        /// </summary>
        public static bool IsNullOrEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Проверка корректности email
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка корректности инвентарного номера
        /// </summary>
        public static bool IsValidInventoryNumber(string inventoryNumber)
        {
            if (string.IsNullOrWhiteSpace(inventoryNumber))
                return false;

            // Инвентарный номер должен содержать только буквы, цифры и дефисы
            string pattern = @"^[A-Za-z0-9\-]+$";
            return Regex.IsMatch(inventoryNumber, pattern);
        }

        /// <summary>
        /// Проверка корректности цены
        /// </summary>
        public static bool IsValidPrice(decimal price)
        {
            return price >= 0 && price <= 100000000; // Максимальная цена 100 миллионов
        }

        /// <summary>
        /// Проверка корректности даты
        /// </summary>
        public static bool IsValidDate(DateTime date)
        {
            return date >= new DateTime(1900, 1, 1) && date <= DateTime.Now.AddYears(10);
        }

        /// <summary>
        /// Очистка строки от лишних пробелов
        /// </summary>
        public static string CleanString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Trim();
        }
    }
}

