using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1.Services
{
    public class PermissionService
    {
        private static PermissionService _instance;
        private readonly DbController _db = new DbController();
        private Dictionary<string, HashSet<string>> _rolePermissions = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        public static PermissionService Instance => _instance ?? (_instance = new PermissionService());

        public void EnsureSchema()
        {
            string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissions]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[RolePermissions] (
        [RoleCode] NVARCHAR(50) NOT NULL,
        [PermissionCode] NVARCHAR(80) NOT NULL,
        [IsGranted] BIT NOT NULL CONSTRAINT DF_RolePerm_Granted DEFAULT (1),
        CONSTRAINT PK_RolePermissions PRIMARY KEY ([RoleCode], [PermissionCode])
    );
END";
            _db.ExecuteNonQuery(sql);
            SeedDefaultsIfEmpty();
        }

        private void SeedDefaultsIfEmpty()
        {
            object count = _db.ExecuteScalar("SELECT COUNT(*) FROM RolePermissions");
            if (Convert.ToInt32(count) > 0) return;

            GrantRole(AuthorizationHelper.RoleAdmin, AppPermission.AllCodes);
            GrantRole(AuthorizationHelper.RoleUser, new[]
            {
                AppPermission.InventoryView, AppPermission.InventoryEdit,
                AppPermission.ReportsView, AppPermission.ReportsGenerate,
                AppPermission.ReportsExportExcel, AppPermission.ReportsExportPdf,
                AppPermission.StatisticsView, AppPermission.StatisticsExport,
                AppPermission.DispositionView, AppPermission.DispositionCreate, AppPermission.DispositionExport
            });
            Logger.Info("Загружены права доступа по умолчанию для ролей Admin и User");
        }

        private void GrantRole(string roleCode, IEnumerable<string> permissions)
        {
            foreach (string p in permissions)
            {
                _db.ExecuteNonQuery(
                    @"IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleCode=@r AND PermissionCode=@p)
                      INSERT INTO RolePermissions (RoleCode, PermissionCode, IsGranted) VALUES (@r, @p, 1)",
                    new[]
                    {
                        new SqlParameter("@r", roleCode),
                        new SqlParameter("@p", p)
                    });
            }
        }

        public void Reload()
        {
            _rolePermissions.Clear();
            var dt = _db.GetData("SELECT RoleCode, PermissionCode FROM RolePermissions WHERE IsGranted = 1");
            foreach (DataRow row in dt.Rows)
            {
                string role = row["RoleCode"].ToString();
                string perm = row["PermissionCode"].ToString();
                if (!_rolePermissions.ContainsKey(role))
                    _rolePermissions[role] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _rolePermissions[role].Add(perm);
            }
        }

        public bool HasPermission(string roleCode, string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(roleCode) || string.IsNullOrWhiteSpace(permissionCode))
                return false;
            if (_rolePermissions.Count == 0)
                Reload();
            if (!_rolePermissions.TryGetValue(roleCode.Trim(), out var set))
                return false;
            return set.Contains(permissionCode);
        }

        public DataTable GetPermissionsForRole(string roleCode)
        {
            var table = new DataTable();
            table.Columns.Add("PermissionCode", typeof(string));
            table.Columns.Add("Наименование", typeof(string));
            table.Columns.Add("Разрешено", typeof(bool));

            var granted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var dt = _db.GetData(
                "SELECT PermissionCode, IsGranted FROM RolePermissions WHERE RoleCode = @role",
                new[] { new SqlParameter("@role", roleCode) });
            foreach (DataRow r in dt.Rows)
            {
                if (Convert.ToBoolean(r["IsGranted"]))
                    granted.Add(r["PermissionCode"].ToString());
            }

            foreach (string code in AppPermission.AllCodes)
            {
                table.Rows.Add(code, AppPermission.GetDisplayName(code), granted.Contains(code));
            }
            return table;
        }

        public void SaveRolePermissions(string roleCode, IEnumerable<string> grantedPermissionCodes)
        {
            if (string.Equals(roleCode, AuthorizationHelper.RoleAdmin, StringComparison.OrdinalIgnoreCase))
            {
                var all = new HashSet<string>(AppPermission.AllCodes, StringComparer.OrdinalIgnoreCase);
                if (!grantedPermissionCodes.All(all.Contains) || grantedPermissionCodes.Count() < AppPermission.AllCodes.Length)
                    throw new InvalidOperationException("Для роли «Администратор» нельзя отключить базовые права. Разрешены только полные права.");
            }

            _db.ExecuteNonQuery("DELETE FROM RolePermissions WHERE RoleCode = @role",
                new[] { new SqlParameter("@role", roleCode) });

            foreach (string code in grantedPermissionCodes.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                _db.ExecuteNonQuery(
                    "INSERT INTO RolePermissions (RoleCode, PermissionCode, IsGranted) VALUES (@role, @perm, 1)",
                    new[]
                    {
                        new SqlParameter("@role", roleCode),
                        new SqlParameter("@perm", code)
                    });
            }
            Reload();
            Logger.Info($"Обновлены права роли {roleCode}: {grantedPermissionCodes.Count()} разрешений");
        }

        public static string[] GetKnownRoles() => new[] { AuthorizationHelper.RoleAdmin, AuthorizationHelper.RoleUser };
    }
}
