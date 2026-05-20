using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1.Export
{
    public static class PdfReportExporter
    {
        private const double MarginLeft = 40;
        private const double MarginTop = 40;
        private const double RowHeight = 18;
        private const double HeaderRowHeight = 22;

        public static bool ExportWithDialog(ReportResult report, string preparedByName)
        {
            if (report?.Data == null || report.Data.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта в PDF.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "PDF документ (*.pdf)|*.pdf";
                dlg.FileName = SanitizeFileName(report.Title) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".pdf";
                if (dlg.ShowDialog() != DialogResult.OK)
                    return false;

                try
                {
                    ExportToFile(report, dlg.FileName, preparedByName);
                    if (MessageBox.Show("PDF успешно сохранён. Открыть файл?", "Экспорт PDF",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка экспорта PDF", ex);
                    MessageBox.Show($"Ошибка экспорта PDF:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        public static void ExportToFile(ReportResult report, string filePath, string preparedByName)
        {
            var doc = new PdfDocument();
            doc.Info.Title = report.Title;
            doc.Info.Author = AppConfig.OrganizationName;

            var page = doc.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);
            var fontTitle = new XFont("Arial", 14, XFontStyle.Bold);
            var fontHeader = new XFont("Arial", 10, XFontStyle.Bold);
            var fontNormal = new XFont("Arial", 9, XFontStyle.Regular);
            var fontSmall = new XFont("Arial", 8, XFontStyle.Regular);
            var fontBold = new XFont("Arial", 9, XFontStyle.Bold);

            double y = MarginTop;
            double pageWidth = page.Width - MarginLeft * 2;

            // Шапка организации
            DrawCentered(gfx, AppConfig.OrganizationName.ToUpper(), fontHeader, page, ref y);
            y += 4;
            DrawCentered(gfx, AppConfig.OrganizationAddress, fontSmall, page, ref y);
            y += 2;
            DrawCentered(gfx, AppConfig.OrganizationDepartment, fontSmall, page, ref y);
            y += 14;
            DrawCentered(gfx, "УТВЕРЖДАЮ", fontSmall, page, ref y);
            y += 12;
            gfx.DrawString(AppConfig.ReportApprovedByTitle, fontSmall, XBrushes.Black,
                new XRect(MarginLeft + pageWidth * 0.55, y, pageWidth * 0.4, 14), XStringFormats.TopLeft);
            y += 14;
            gfx.DrawString("_______________ / _______________", fontSmall, XBrushes.Black,
                new XRect(MarginLeft + pageWidth * 0.55, y, pageWidth * 0.4, 14), XStringFormats.TopLeft);
            y += 8;
            gfx.DrawString($"«___» _____________ {DateTime.Now.Year} г.", fontSmall, XBrushes.Black,
                new XRect(MarginLeft + pageWidth * 0.55, y, pageWidth * 0.4, 14), XStringFormats.TopLeft);
            y += 24;

            DrawCentered(gfx, report.Title, fontTitle, page, ref y);
            y += 6;
            if (!string.IsNullOrEmpty(report.Description))
            {
                DrawWrapped(gfx, report.Description, fontSmall, MarginLeft, ref y, pageWidth);
                y += 8;
            }
            gfx.DrawString($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}", fontSmall, XBrushes.Black,
                new XRect(MarginLeft, y, pageWidth, 14), XStringFormats.TopLeft);
            y += 18;

            var visibleCols = report.Data.Columns.Cast<DataColumn>()
                .Where(c => c.ColumnName != "Тип строки")
                .ToList();
            if (visibleCols.Count == 0) visibleCols = report.Data.Columns.Cast<DataColumn>().ToList();

            double colWidth = pageWidth / visibleCols.Count;

            // Заголовок таблицы
            double x = MarginLeft;
            foreach (var col in visibleCols)
            {
                gfx.DrawRectangle(XPens.Black, x, y, colWidth, HeaderRowHeight);
                gfx.DrawString(col.ColumnName, fontBold, XBrushes.Black,
                    new XRect(x + 2, y + 2, colWidth - 4, HeaderRowHeight - 4), XStringFormats.TopLeft);
                x += colWidth;
            }
            y += HeaderRowHeight;

            foreach (DataRow dataRow in report.Data.Rows)
            {
                if (y > page.Height - 120)
                {
                    DrawSignatureBlock(gfx, fontSmall, page, preparedByName, page.Height - 100);
                    page = doc.AddPage();
                    page.Size = PdfSharp.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    y = MarginTop;
                }

                bool isSubtotal = report.HasRowTypeColumn && report.Data.Columns.Contains("Тип строки") &&
                    (dataRow["Тип строки"].ToString() == "Итого по группе" || dataRow["Тип строки"].ToString() == "ИТОГО");
                var rowFont = isSubtotal ? fontBold : fontNormal;
                var brush = isSubtotal ? new XSolidBrush(XColor.FromArgb(230, 240, 250)) : XBrushes.White;

                x = MarginLeft;
                foreach (var col in visibleCols)
                {
                    gfx.DrawRectangle(XPens.Gray, x, y, colWidth, RowHeight);
                    gfx.DrawRectangle(brush, x, y, colWidth, RowHeight);
                    string text = FormatCellValue(dataRow[col.ColumnName], col.DataType);
                    gfx.DrawString(Truncate(text, 42), rowFont, XBrushes.Black,
                        new XRect(x + 2, y + 2, colWidth - 4, RowHeight - 4), XStringFormats.TopLeft);
                    x += colWidth;
                }
                y += RowHeight;
            }

            y += 20;
            if (y > page.Height - 100)
            {
                page = doc.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                y = MarginTop;
            }
            DrawSignatureBlock(gfx, fontSmall, page, preparedByName, y);

            doc.Save(filePath);
        }

        private static void DrawSignatureBlock(XGraphics gfx, XFont font, PdfPage page, string preparedBy, double y)
        {
            double w = page.Width - MarginLeft * 2;
            gfx.DrawString("Составил:", font, XBrushes.Black, new XRect(MarginLeft, y, w, 14), XStringFormats.TopLeft);
            gfx.DrawString($"{AppConfig.ReportPreparedByTitle}  _______________ / {preparedBy ?? "_______________"}", font, XBrushes.Black,
                new XRect(MarginLeft + 70, y, w - 70, 14), XStringFormats.TopLeft);
            y += 22;
            gfx.DrawString("Проверил:", font, XBrushes.Black, new XRect(MarginLeft, y, w, 14), XStringFormats.TopLeft);
            gfx.DrawString("Главный бухгалтер  _______________ / _______________", font, XBrushes.Black,
                new XRect(MarginLeft + 70, y, w - 70, 14), XStringFormats.TopLeft);
            y += 22;
            gfx.DrawString("Утвердил:", font, XBrushes.Black, new XRect(MarginLeft, y, w, 14), XStringFormats.TopLeft);
            gfx.DrawString($"{AppConfig.ReportApprovedByTitle}  _______________ / _______________", font, XBrushes.Black,
                new XRect(MarginLeft + 70, y, w - 70, 14), XStringFormats.TopLeft);
        }

        private static void DrawCentered(XGraphics gfx, string text, XFont font, PdfPage page, ref double y)
        {
            gfx.DrawString(text, font, XBrushes.Black,
                new XRect(MarginLeft, y, page.Width - MarginLeft * 2, 20), XStringFormats.TopCenter);
            y += 16;
        }

        private static void DrawWrapped(XGraphics gfx, string text, XFont font, double x, ref double y, double width)
        {
            gfx.DrawString(text, font, XBrushes.Black, new XRect(x, y, width, 40), XStringFormats.TopLeft);
            y += 28;
        }

        private static string FormatCellValue(object value, Type type)
        {
            if (value == null || value == DBNull.Value) return "—";
            if (type == typeof(DateTime) || value is DateTime)
                return Convert.ToDateTime(value).ToString("dd.MM.yyyy");
            if (value is decimal d) return d.ToString("N2");
            if (value is double db) return db.ToString("N2");
            return value.ToString();
        }

        private static string Truncate(string s, int max) =>
            string.IsNullOrEmpty(s) ? "" : (s.Length <= max ? s : s.Substring(0, max - 1) + "…");

        private static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Length > 80 ? name.Substring(0, 80) : name;
        }
    }
}
