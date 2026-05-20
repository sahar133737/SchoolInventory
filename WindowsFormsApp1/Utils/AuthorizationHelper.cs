using System.Windows.Forms;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.Services;

namespace WindowsFormsApp1.Utils
{
    /// <summary>
    /// Проверка прав текущей роли (данные из таблицы RolePermissions).
    /// </summary>
    public static class AuthorizationHelper
    {
        public const string RoleAdmin = "Admin";
        public const string RoleUser = "User";

        public static bool HasPermission(string role, string permissionCode) =>
            PermissionService.Instance.HasPermission(role, permissionCode);

        public static bool IsAdmin(string role) =>
            string.Equals(role?.Trim(), RoleAdmin, System.StringComparison.OrdinalIgnoreCase);

        public static bool CanViewInventory(string role) => HasPermission(role, AppPermission.InventoryView);
        public static bool CanEditInventory(string role) => HasPermission(role, AppPermission.InventoryEdit);
        public static bool CanDeleteInventory(string role) => HasPermission(role, AppPermission.InventoryDelete);

        public static bool CanViewReports(string role) => HasPermission(role, AppPermission.ReportsView);
        public static bool CanGenerateReports(string role) => HasPermission(role, AppPermission.ReportsGenerate);
        public static bool CanExportReportsExcel(string role) => HasPermission(role, AppPermission.ReportsExportExcel);
        public static bool CanExportReportsPdf(string role) => HasPermission(role, AppPermission.ReportsExportPdf);

        public static bool CanViewStatistics(string role) => HasPermission(role, AppPermission.StatisticsView);
        public static bool CanExportStatistics(string role) => HasPermission(role, AppPermission.StatisticsExport);

        public static bool CanViewDisposition(string role) => HasPermission(role, AppPermission.DispositionView);
        public static bool CanRegisterDisposition(string role) => HasPermission(role, AppPermission.DispositionCreate);
        public static bool CanDeleteDisposition(string role) => HasPermission(role, AppPermission.DispositionDelete);
        public static bool CanExportDisposition(string role) => HasPermission(role, AppPermission.DispositionExport);

        public static bool CanManageUsers(string role) => HasPermission(role, AppPermission.UsersManage);
        public static bool CanManageCategories(string role) => HasPermission(role, AppPermission.CategoriesManage);
        public static bool CanManageClassrooms(string role) => HasPermission(role, AppPermission.ClassroomsManage);
        public static bool CanManageResponsible(string role) => HasPermission(role, AppPermission.ResponsibleManage);
        public static bool CanViewSystemLog(string role) => HasPermission(role, AppPermission.SystemLogView);
        public static bool CanGenerateTestData(string role) => HasPermission(role, AppPermission.SystemTestData);
        public static bool CanManagePermissions(string role) => HasPermission(role, AppPermission.PermissionsManage);

        // Совместимость со старыми вызовами
        public static bool CanExportData(string role) =>
            CanExportReportsExcel(role) || CanExportDisposition(role) || CanExportStatistics(role);

        public static bool CanManageReferences(string role) =>
            CanManageClassrooms(role) || CanManageResponsible(role);

        public static bool EnsureAuthorized(bool allowed, IWin32Window owner, string actionDescription)
        {
            if (allowed) return true;
            MessageBox.Show(owner,
                $"Недостаточно прав для выполнения действия:\n{actionDescription}\n\n" +
                "Обратитесь к администратору или измените права роли в разделе «Права доступа».",
                "Доступ запрещён",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            Logger.Warning($"Отказ в доступе: {actionDescription}");
            return false;
        }
    }
}
