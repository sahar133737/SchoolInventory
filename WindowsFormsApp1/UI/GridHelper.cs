using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace WindowsFormsApp1.UI
{
    public static class GridHelper
    {
        private static readonly Dictionary<string, string> InventoryHeaders = new Dictionary<string, string>
        {
            { "ItemName", "Наименование" },
            { "Description", "Описание" },
            { "CategoryName", "Категория" },
            { "Classroom", "Кабинет" },
            { "ResponsiblePerson", "Ответственный" },
            { "InventoryNumber", "Инв. номер" },
            { "PurchaseDate", "Дата покупки" },
            { "PurchasePrice", "Стоимость, руб." },
            { "CurrentState", "Состояние" },
            { "Status", "Статус" }
        };

        public static void LocalizeInventoryGrid(DataGridView grid)
        {
            ApplyHeaders(grid, InventoryHeaders);
            HideTechnicalColumns(grid);
            if (grid.Columns.Contains("PurchaseDate"))
                grid.Columns["PurchaseDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
            if (grid.Columns.Contains("PurchasePrice"))
            {
                grid.Columns["PurchasePrice"].DefaultCellStyle.Format = "N2";
                grid.Columns["PurchasePrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        public static void LocalizeFromDataTable(DataGridView grid, DataTable table)
        {
            grid.DataSource = table;
            foreach (DataColumn col in table.Columns)
            {
                if (grid.Columns.Contains(col.ColumnName))
                    grid.Columns[col.ColumnName].HeaderText = col.ColumnName;
            }
            HideTechnicalColumns(grid);
            AppTheme.StyleSubtotalGridRows(grid);
        }

        /// <summary>
        /// Скрывает служебные колонки (ID, тип строки и т.п.).
        /// </summary>
        public static void HideTechnicalColumns(DataGridView grid)
        {
            if (grid == null) return;
            foreach (DataGridViewColumn col in grid.Columns)
            {
                string name = col.DataPropertyName;
                if (string.IsNullOrEmpty(name))
                    name = col.Name;
                if (ShouldHideColumn(name))
                    col.Visible = false;
            }
        }

        private static bool ShouldHideColumn(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            if (name.Equals("Тип строки", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Equals("Код", StringComparison.Ordinal)) return true;
            if (name.EndsWith("ID", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public static void BindGrid(DataGridView grid, DataTable table, Action<DataGridView> localize = null)
        {
            grid.DataSource = table;
            localize?.Invoke(grid);
            HideTechnicalColumns(grid);
        }

        private static void ApplyHeaders(DataGridView grid, Dictionary<string, string> map)
        {
            foreach (var kv in map)
            {
                if (grid.Columns.Contains(kv.Key))
                    grid.Columns[kv.Key].HeaderText = kv.Value;
            }
        }

        public static void LocalizeUsersGrid(DataGridView grid)
        {
            var map = new Dictionary<string, string>
            {
                { "Username", "Логин" },
                { "FullName", "ФИО" },
                { "Role", "Роль" },
                { "CreatedDate", "Дата создания" },
                { "IsActive", "Активен" }
            };
            ApplyHeaders(grid, map);
            HideTechnicalColumns(grid);
        }

        public static void LocalizeCategoriesGrid(DataGridView grid)
        {
            var map = new Dictionary<string, string>
            {
                { "CategoryName", "Наименование категории" }
            };
            ApplyHeaders(grid, map);
            HideTechnicalColumns(grid);
        }
    }
}
