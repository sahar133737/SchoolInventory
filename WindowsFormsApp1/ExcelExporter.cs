using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class ExcelExporter
    {
        // Экспорт данных в CSV формат (совместимый с Excel)
        public static bool ExportToExcel(DataTable dataTable, string filePath)
        {
            try
            {
                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    // Добавляем BOM для правильного отображения кириллицы в Excel
                    byte[] bom = { 0xEF, 0xBB, 0xBF };
                    sw.BaseStream.Write(bom, 0, bom.Length);

                    // Записываем заголовки
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        if (i > 0) sw.Write(",");
                        sw.Write(QuoteValue(dataTable.Columns[i].ColumnName));
                    }
                    sw.WriteLine();

                    // Записываем данные
                    foreach (DataRow row in dataTable.Rows)
                    {
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            if (i > 0) sw.Write(",");
                            string value = row[i]?.ToString() ?? string.Empty;
                            sw.Write(QuoteValue(value));
                        }
                        sw.WriteLine();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static string QuoteValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            // Если значение содержит запятую, кавычки или перевод строки, заключаем в кавычки
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                // Экранируем кавычки
                value = value.Replace("\"", "\"\"");
                return "\"" + value + "\"";
            }

            return value;
        }

        // Экспорт с диалогом выбора файла
        public static bool ExportToExcelWithDialog(DataTable dataTable, string defaultFileName = "Отчет_инвентарь")
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*";
                saveDialog.FileName = $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                saveDialog.Title = "Сохранить отчет как";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    return ExportToExcel(dataTable, saveDialog.FileName);
                }
            }

            return false;
        }
    }
}

