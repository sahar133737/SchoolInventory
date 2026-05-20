using System.Configuration;

namespace WindowsFormsApp1.Utils
{
    public static class AppConfig
    {
        public static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString
            ?? @"Server=.\SQLEXPRESS;Database=SchoolInventoryDB;Integrated Security=true;";

        public static string GetSetting(string key, string defaultValue = "") =>
            string.IsNullOrEmpty(ConfigurationManager.AppSettings[key])
                ? defaultValue
                : ConfigurationManager.AppSettings[key];

        public static bool GetBoolSetting(string key, bool defaultValue = false) =>
            bool.TryParse(GetSetting(key), out bool r) ? r : defaultValue;

        public static int GetIntSetting(string key, int defaultValue = 0) =>
            int.TryParse(GetSetting(key), out int r) ? r : defaultValue;

        public static string OrganizationName => GetSetting("OrganizationName", "МБОУ «Средняя общеобразовательная школа»");
        public static string OrganizationAddress => GetSetting("OrganizationAddress", "г. Брянск");
        public static string OrganizationDepartment => GetSetting("OrganizationDepartment", "Администрация / Хозяйственная часть");
        public static string ReportPreparedByTitle => GetSetting("ReportPreparedByTitle", "Заведующий хозяйством");
        public static string ReportApprovedByTitle => GetSetting("ReportApprovedByTitle", "Директор");
    }
}
