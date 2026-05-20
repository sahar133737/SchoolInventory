namespace WindowsFormsApp1.Models
{
    /// <summary>
    /// Коды прав доступа в системе.
    /// </summary>
    public static class AppPermission
    {
        public const string InventoryView = "Inventory.View";
        public const string InventoryEdit = "Inventory.Edit";
        public const string InventoryDelete = "Inventory.Delete";

        public const string ReportsView = "Reports.View";
        public const string ReportsGenerate = "Reports.Generate";
        public const string ReportsExportExcel = "Reports.ExportExcel";
        public const string ReportsExportPdf = "Reports.ExportPdf";

        public const string StatisticsView = "Statistics.View";
        public const string StatisticsExport = "Statistics.Export";

        public const string DispositionView = "Disposition.View";
        public const string DispositionCreate = "Disposition.Create";
        public const string DispositionDelete = "Disposition.Delete";
        public const string DispositionExport = "Disposition.Export";

        public const string UsersManage = "Users.Manage";
        public const string CategoriesManage = "Categories.Manage";
        public const string ClassroomsManage = "Classrooms.Manage";
        public const string ResponsibleManage = "Responsible.Manage";
        public const string SystemLogView = "SystemLog.View";
        public const string SystemTestData = "System.TestData";
        public const string PermissionsManage = "Permissions.Manage";

        public static readonly string[] AllCodes =
        {
            InventoryView, InventoryEdit, InventoryDelete,
            ReportsView, ReportsGenerate, ReportsExportExcel, ReportsExportPdf,
            StatisticsView, StatisticsExport,
            DispositionView, DispositionCreate, DispositionDelete, DispositionExport,
            UsersManage, CategoriesManage, ClassroomsManage, ResponsibleManage,
            SystemLogView, SystemTestData, PermissionsManage
        };

        public static string GetDisplayName(string code)
        {
            switch (code)
            {
                case InventoryView: return "Просмотр инвентаря";
                case InventoryEdit: return "Добавление и изменение инвентаря";
                case InventoryDelete: return "Удаление записей инвентаря";
                case ReportsView: return "Просмотр модуля отчётов";
                case ReportsGenerate: return "Формирование отчётов";
                case ReportsExportExcel: return "Экспорт отчётов в Excel";
                case ReportsExportPdf: return "Экспорт отчётов в PDF";
                case StatisticsView: return "Просмотр статистики";
                case StatisticsExport: return "Экспорт статистики";
                case DispositionView: return "Просмотр распоряжений";
                case DispositionCreate: return "Оформление распоряжений";
                case DispositionDelete: return "Удаление записей распоряжений";
                case DispositionExport: return "Экспорт журнала распоряжений";
                case UsersManage: return "Управление пользователями";
                case CategoriesManage: return "Управление категориями";
                case ClassroomsManage: return "Управление кабинетами";
                case ResponsibleManage: return "Управление ответственными";
                case SystemLogView: return "Просмотр журнала системы";
                case SystemTestData: return "Генерация тестовых данных";
                case PermissionsManage: return "Управление правами ролей";
                default: return code;
            }
        }
    }
}
