using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.Utils;
using WindowsFormsApp1;

namespace WindowsFormsApp1.Services
{
    public class ReportService
    {
        private readonly DbController _db = new DbController();

        public ReportResult BuildReport(ReportKind kind, InventoryFilterCriteria filter = null)
        {
            switch (kind)
            {
                case ReportKind.CategoryFinancialSummary:
                    return BuildCategoryFinancialSummary(filter);
                case ReportKind.ClassroomResponsibleSummary:
                    return BuildClassroomResponsibleSummary(filter);
                case ReportKind.StateCategoryMatrix:
                    return BuildStateCategoryMatrix(filter);
                case ReportKind.DetailedRegistryWithSubtotals:
                    return BuildDetailedRegistry(filter);
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        /// <summary>
        /// Отчёт 1: свод по категориям с суммированием и долей от общей стоимости.
        /// </summary>
        public ReportResult BuildCategoryFinancialSummary(InventoryFilterCriteria filter)
        {
            string where = BuildWhereClause(filter, "i");
            string sql = $@"
SELECT 
    ISNULL(c.CategoryName, N'Без категории') AS [Категория],
    COUNT(*) AS [Количество позиций],
    SUM(ISNULL(i.PurchasePrice, 0)) AS [Сумма стоимости, руб.],
    AVG(ISNULL(i.PurchasePrice, 0)) AS [Средняя цена, руб.],
    CAST(0 AS DECIMAL(8,2)) AS [Доля от общей суммы, %],
  N'Деталь' AS [Тип строки]
FROM Inventory i
LEFT JOIN Categories c ON i.CategoryID = c.CategoryID
{where}
GROUP BY c.CategoryName
ORDER BY [Сумма стоимости, руб.] DESC";

            var table = _db.GetData(sql, BuildParameters(filter));
            decimal grandTotal = table.AsEnumerable().Sum(r => r.Field<decimal?>("Сумма стоимости, руб.") ?? 0);

            foreach (DataRow row in table.Rows)
            {
                decimal sum = row.Field<decimal?>("Сумма стоимости, руб.") ?? 0;
                row["Доля от общей суммы, %"] = grandTotal > 0 ? Math.Round(sum / grandTotal * 100, 2) : 0;
            }

            AddGrandTotalRow(table,
                groupColumns: new[] { "Категория" },
                sumColumns: new[] { "Количество позиций", "Сумма стоимости, руб." },
                avgColumns: new[] { "Средняя цена, руб." },
                percentColumn: "Доля от общей суммы, %");

            return new ReportResult
            {
                Title = "Сводный финансовый отчёт по категориям имущества",
                Description = "Группировка по категориям с подсчётом количества, суммы, средней цены и доли в общей стоимости.",
                Data = table,
                HasRowTypeColumn = true
            };
        }

        /// <summary>
        /// Отчёт 2: распределение имущества по кабинетам и ответственным лицам.
        /// </summary>
        public ReportResult BuildClassroomResponsibleSummary(InventoryFilterCriteria filter)
        {
            string where = BuildWhereClause(filter, "i");
            string sql = $@"
SELECT 
    ISNULL(cl.RoomNumber + N' — ' + cl.RoomName, N'Кабинет не указан') AS [Кабинет],
    ISNULL(p.LastName + N' ' + p.FirstName, N'Не назначен') AS [Ответственное лицо],
    COUNT(*) AS [Количество единиц],
    SUM(ISNULL(i.PurchasePrice, 0)) AS [Сумма стоимости, руб.],
    MIN(i.PurchaseDate) AS [Первая дата поступления],
    MAX(i.PurchaseDate) AS [Последняя дата поступления],
    N'Деталь' AS [Тип строки]
FROM Inventory i
LEFT JOIN Classrooms cl ON i.ClassroomID = cl.ClassroomID
LEFT JOIN ResponsiblePersons p ON i.ResponsiblePersonID = p.PersonID
{where}
GROUP BY cl.RoomNumber, cl.RoomName, p.LastName, p.FirstName
ORDER BY [Кабинет], [Ответственное лицо]";

            var table = _db.GetData(sql, BuildParameters(filter));
            AddSubtotalsByColumn(table, "Кабинет",
                sumColumns: new[] { "Количество единиц", "Сумма стоимости, руб." });
            AddGrandTotalRow(table,
                groupColumns: new[] { "Кабинет", "Ответственное лицо" },
                sumColumns: new[] { "Количество единиц", "Сумма стоимости, руб." },
                avgColumns: null,
                percentColumn: null);

            return new ReportResult
            {
                Title = "Отчёт по кабинетам и материально ответственным лицам",
                Description = "Группировка по кабинетам и МОЛ с промежуточными итогами по каждому кабинету.",
                Data = table,
                HasRowTypeColumn = true
            };
        }

        /// <summary>
        /// Отчёт 3: матрица «состояние × категория» с суммированием.
        /// </summary>
        public ReportResult BuildStateCategoryMatrix(InventoryFilterCriteria filter)
        {
            string where = BuildWhereClause(filter, "i");
            string sql = $@"
SELECT 
    ISNULL(i.CurrentState, N'Не указано') AS [Состояние],
    ISNULL(c.CategoryName, N'Без категории') AS [Категория],
    COUNT(*) AS [Количество],
    SUM(ISNULL(i.PurchasePrice, 0)) AS [Сумма, руб.],
    AVG(ISNULL(i.PurchasePrice, 0)) AS [Средняя стоимость, руб.],
    N'Деталь' AS [Тип строки]
FROM Inventory i
LEFT JOIN Categories c ON i.CategoryID = c.CategoryID
{where}
GROUP BY i.CurrentState, c.CategoryName
ORDER BY [Состояние], [Категория]";

            var table = _db.GetData(sql, BuildParameters(filter));
            AddSubtotalsByColumn(table, "Состояние",
                sumColumns: new[] { "Количество", "Сумма, руб." },
                avgColumns: new[] { "Средняя стоимость, руб." });
            AddGrandTotalRow(table,
                groupColumns: new[] { "Состояние", "Категория" },
                sumColumns: new[] { "Количество", "Сумма, руб." },
                avgColumns: new[] { "Средняя стоимость, руб." },
                percentColumn: null);

            return new ReportResult
            {
                Title = "Аналитический отчёт по состоянию и категориям имущества",
                Description = "Двухуровневая группировка: состояние объекта и категория с итогами по состоянию.",
                Data = table,
                HasRowTypeColumn = true
            };
        }

        /// <summary>
        /// Отчёт 4: реестр с детализацией и подытогами по категориям.
        /// </summary>
        public ReportResult BuildDetailedRegistry(InventoryFilterCriteria filter)
        {
            string where = BuildWhereClause(filter, "i");
            string sql = $@"
SELECT 
    ISNULL(c.CategoryName, N'Без категории') AS [Категория],
    i.InventoryNumber AS [Инв. номер],
    i.ItemName AS [Наименование],
    ISNULL(cl.RoomNumber + N' — ' + cl.RoomName, N'—') AS [Кабинет],
    ISNULL(p.LastName + N' ' + p.FirstName, N'—') AS [Ответственный],
    i.PurchaseDate AS [Дата поступления],
    ISNULL(i.PurchasePrice, 0) AS [Стоимость, руб.],
    ISNULL(i.CurrentState, N'—') AS [Состояние],
    ISNULL(i.Status, N'—') AS [Статус],
    N'Позиция' AS [Тип строки]
FROM Inventory i
LEFT JOIN Categories c ON i.CategoryID = c.CategoryID
LEFT JOIN Classrooms cl ON i.ClassroomID = cl.ClassroomID
LEFT JOIN ResponsiblePersons p ON i.ResponsiblePersonID = p.PersonID
{where}
ORDER BY [Категория], [Наименование]";

            var source = _db.GetData(sql, BuildParameters(filter));
            var result = source.Clone();

            string currentCategory = null;
            int groupCount = 0;
            decimal groupSum = 0;

            Action flushGroup = () =>
            {
                if (currentCategory == null) return;
                var sub = result.NewRow();
                sub["Категория"] = currentCategory;
                sub["Наименование"] = $"Итого по категории «{currentCategory}»";
                sub["Инв. номер"] = DBNull.Value;
                sub["Кабинет"] = DBNull.Value;
                sub["Ответственный"] = DBNull.Value;
                sub["Дата поступления"] = DBNull.Value;
                sub["Стоимость, руб."] = groupSum;
                sub["Состояние"] = DBNull.Value;
                sub["Статус"] = $"Позиций: {groupCount}";
                sub["Тип строки"] = "Итого по группе";
                result.Rows.Add(sub);
            };

            foreach (DataRow row in source.Rows)
            {
                string cat = row["Категория"].ToString();
                if (currentCategory != null && cat != currentCategory)
                    flushGroup();

                if (currentCategory != cat)
                {
                    currentCategory = cat;
                    groupCount = 0;
                    groupSum = 0;
                }

                result.ImportRow(row);
                groupCount++;
                groupSum += Convert.ToDecimal(row["Стоимость, руб."]);
            }
            flushGroup();

            int totalCount = source.Rows.Count;
            decimal totalSum = source.AsEnumerable().Sum(r => Convert.ToDecimal(r["Стоимость, руб."]));
            var grand = result.NewRow();
            grand["Категория"] = "ВСЕГО";
            grand["Наименование"] = "Общий итог по реестру";
            grand["Стоимость, руб."] = totalSum;
            grand["Статус"] = $"Всего позиций: {totalCount}";
            grand["Тип строки"] = "ИТОГО";
            result.Rows.Add(grand);

            return new ReportResult
            {
                Title = "Реестр материальных ценностей с группировкой по категориям",
                Description = "Полный перечень объектов с промежуточными итогами по каждой категории и общим итогом.",
                Data = result,
                HasRowTypeColumn = true
            };
        }

        private static string BuildWhereClause(InventoryFilterCriteria filter, string alias)
        {
            if (filter == null || !filter.HasActiveFilters)
                return string.Empty;

            var parts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                parts.Add($"({alias}.ItemName LIKE @Search OR {alias}.Description LIKE @Search OR {alias}.InventoryNumber LIKE @Search)");
            if (filter.CategoryId.HasValue)
                parts.Add($"{alias}.CategoryID = @CategoryId");
            if (filter.ClassroomId.HasValue)
                parts.Add($"{alias}.ClassroomID = @ClassroomId");
            if (filter.ResponsiblePersonId.HasValue)
                parts.Add($"{alias}.ResponsiblePersonID = @ResponsiblePersonId");
            if (!string.IsNullOrWhiteSpace(filter.CurrentState))
                parts.Add($"{alias}.CurrentState = @CurrentState");
            if (!string.IsNullOrWhiteSpace(filter.Status))
                parts.Add($"{alias}.Status = @Status");
            if (filter.DateFrom.HasValue)
                parts.Add($"{alias}.PurchaseDate >= @DateFrom");
            if (filter.DateTo.HasValue)
                parts.Add($"{alias}.PurchaseDate <= @DateTo");
            if (filter.PriceFrom.HasValue)
                parts.Add($"{alias}.PurchasePrice >= @PriceFrom");
            if (filter.PriceTo.HasValue)
                parts.Add($"{alias}.PurchasePrice <= @PriceTo");

            return parts.Count > 0 ? "WHERE " + string.Join(" AND ", parts) : string.Empty;
        }

        private static SqlParameter[] BuildParameters(InventoryFilterCriteria filter)
        {
            if (filter == null || !filter.HasActiveFilters) return null;
            var list = new System.Collections.Generic.List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                list.Add(new SqlParameter("@Search", "%" + filter.SearchText.Trim() + "%"));
            if (filter.CategoryId.HasValue)
                list.Add(new SqlParameter("@CategoryId", filter.CategoryId.Value));
            if (filter.ClassroomId.HasValue)
                list.Add(new SqlParameter("@ClassroomId", filter.ClassroomId.Value));
            if (filter.ResponsiblePersonId.HasValue)
                list.Add(new SqlParameter("@ResponsiblePersonId", filter.ResponsiblePersonId.Value));
            if (!string.IsNullOrWhiteSpace(filter.CurrentState))
                list.Add(new SqlParameter("@CurrentState", filter.CurrentState));
            if (!string.IsNullOrWhiteSpace(filter.Status))
                list.Add(new SqlParameter("@Status", filter.Status));
            if (filter.DateFrom.HasValue)
                list.Add(new SqlParameter("@DateFrom", filter.DateFrom.Value.Date));
            if (filter.DateTo.HasValue)
                list.Add(new SqlParameter("@DateTo", filter.DateTo.Value.Date));
            if (filter.PriceFrom.HasValue)
                list.Add(new SqlParameter("@PriceFrom", filter.PriceFrom.Value));
            if (filter.PriceTo.HasValue)
                list.Add(new SqlParameter("@PriceTo", filter.PriceTo.Value));
            return list.Count > 0 ? list.ToArray() : null;
        }

        private static void AddGrandTotalRow(DataTable table, string[] groupColumns, string[] sumColumns,
            string[] avgColumns, string percentColumn)
        {
            var row = table.NewRow();
            foreach (var col in groupColumns)
                if (table.Columns.Contains(col))
                    row[col] = col == groupColumns[0] ? "ИТОГО" : (object)DBNull.Value;
            foreach (var col in sumColumns)
                if (table.Columns.Contains(col))
                    row[col] = table.AsEnumerable()
                        .Where(r => r["Тип строки"].ToString() == "Деталь")
                        .Sum(r => Convert.ToDecimal(r[col]));
            if (avgColumns != null)
            {
                var details = table.AsEnumerable().Where(r => r["Тип строки"].ToString() == "Деталь").ToList();
                foreach (var col in avgColumns)
                    if (table.Columns.Contains(col) && details.Count > 0)
                        row[col] = Math.Round(details.Average(r => Convert.ToDecimal(r[col])), 2);
            }
            if (!string.IsNullOrEmpty(percentColumn) && table.Columns.Contains(percentColumn))
                row[percentColumn] = 100m;
            row["Тип строки"] = "ИТОГО";
            table.Rows.Add(row);
        }

        private static void AddSubtotalsByColumn(DataTable table, string groupColumn, string[] sumColumns, string[] avgColumns = null)
        {
            var groups = table.AsEnumerable()
                .Where(r => r["Тип строки"].ToString() == "Деталь")
                .GroupBy(r => r[groupColumn].ToString())
                .ToList();

            var rebuilt = table.Clone();
            foreach (var g in groups)
            {
                foreach (var r in g)
                    rebuilt.ImportRow(r);
                var sub = rebuilt.NewRow();
                sub[groupColumn] = g.Key;
                sub["Тип строки"] = "Итого по группе";
                foreach (var col in table.Columns.Cast<DataColumn>().Select(c => c.ColumnName))
                {
                    if (col == groupColumn || col == "Тип строки") continue;
                    if (sumColumns != null && sumColumns.Contains(col))
                        sub[col] = g.Sum(r => r[col] == DBNull.Value ? 0 : Convert.ToDecimal(r[col]));
                    else if (avgColumns != null && avgColumns.Contains(col) && g.Any())
                        sub[col] = Math.Round(g.Average(r => r[col] == DBNull.Value ? 0 : Convert.ToDecimal(r[col])), 2);
                    else if (col.Contains("дата") || col.Contains("Дата"))
                        sub[col] = DBNull.Value;
                    else if (rebuilt.Columns[col].DataType == typeof(string))
                        sub[col] = "—";
                }
                var labelCol = rebuilt.Columns.Cast<DataColumn>()
                    .FirstOrDefault(c => c.ColumnName != groupColumn && c.ColumnName != "Тип строки" && c.DataType == typeof(string));
                if (labelCol != null)
                    sub[labelCol.ColumnName] = $"Подытог: {g.Key}";
                rebuilt.Rows.Add(sub);
            }
            table.Clear();
            foreach (DataRow r in rebuilt.Rows)
                table.ImportRow(r);
        }
    }
}
