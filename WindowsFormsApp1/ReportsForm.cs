using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class ReportsForm : UserControl
    {
        private InventoryManager inventoryManager;
        private DataGridView gridReports;
        private Button btnGenerateReport;
        private Button btnExportToExcel;
        private ComboBox cbReportType;
        private Label lblReportType;
        private Panel panelReport;
        private DataTable reportData;

        public ReportsForm(InventoryManager inventoryManager)
        {
            this.inventoryManager = inventoryManager;
            InitializeComponent();
            ApplyNeomorphicStyle();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Panel для отчета
            panelReport = new Panel();
            panelReport.Dock = DockStyle.Fill;
            panelReport.Padding = new Padding(20);
            panelReport.BackColor = NeomorphicStyle.BackgroundColor;

            // Label для типа отчета
            lblReportType = new Label();
            lblReportType.Text = "Тип отчета:";
            lblReportType.Location = new Point(20, 20);
            lblReportType.Size = new Size(120, 25);
            lblReportType.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            lblReportType.ForeColor = NeomorphicStyle.TextColor;

            // ComboBox для выбора типа отчета
            cbReportType = new ComboBox();
            cbReportType.Location = new Point(150, 18);
            cbReportType.Size = new Size(300, 30);
            cbReportType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbReportType.Items.AddRange(new string[] 
            {
                "Полный отчет по инвентарю",
                "Отчет по категориям",
                "Отчет по состоянию",
                "Отчет по кабинетам",
                "Отчет по ответственным"
            });
            cbReportType.SelectedIndex = 0;
            cbReportType.Font = new Font("Segoe UI", 10F);

            // Кнопка генерации отчета
            btnGenerateReport = new Button();
            btnGenerateReport.Text = "Сформировать отчет";
            btnGenerateReport.Location = new Point(470, 15);
            btnGenerateReport.Size = new Size(180, 35);
            btnGenerateReport.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnGenerateReport.Click += BtnGenerateReport_Click;

            // Кнопка экспорта
            btnExportToExcel = new Button();
            btnExportToExcel.Text = "Экспорт в Excel";
            btnExportToExcel.Location = new Point(670, 15);
            btnExportToExcel.Size = new Size(150, 35);
            btnExportToExcel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExportToExcel.Enabled = false;
            btnExportToExcel.Click += BtnExportToExcel_Click;

            // DataGridView для отображения отчета
            gridReports = new DataGridView();
            gridReports.Location = new Point(20, 70);
            gridReports.Size = new Size(800, 450);
            gridReports.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridReports.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridReports.AllowUserToAddRows = false;
            gridReports.AllowUserToDeleteRows = false;
            gridReports.ReadOnly = true;
            gridReports.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridReports.BackgroundColor = NeomorphicStyle.SurfaceColor;
            gridReports.GridColor = NeomorphicStyle.DarkShadow;
            gridReports.DefaultCellStyle.BackColor = NeomorphicStyle.SurfaceColor;
            gridReports.DefaultCellStyle.ForeColor = NeomorphicStyle.TextColor;
            gridReports.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            gridReports.ColumnHeadersDefaultCellStyle.BackColor = NeomorphicStyle.SurfaceColor;
            gridReports.ColumnHeadersDefaultCellStyle.ForeColor = NeomorphicStyle.TextColor;
            gridReports.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            gridReports.EnableHeadersVisualStyles = false;
            gridReports.RowHeadersVisible = false;

            // Добавляем элементы на панель
            panelReport.Controls.Add(lblReportType);
            panelReport.Controls.Add(cbReportType);
            panelReport.Controls.Add(btnGenerateReport);
            panelReport.Controls.Add(btnExportToExcel);
            panelReport.Controls.Add(gridReports);

            // Настройка UserControl
            this.Controls.Add(panelReport);
            this.Size = new Size(840, 540);
            this.BackColor = NeomorphicStyle.BackgroundColor;

            this.ResumeLayout(false);
        }

        private void ApplyNeomorphicStyle()
        {
            // Применяем неоморфный стиль к кнопкам
            ApplyNeomorphicButtonStyle(btnGenerateReport);
            ApplyNeomorphicButtonStyle(btnExportToExcel);
            ApplyNeomorphicComboBoxStyle(cbReportType);
        }

        private void ApplyNeomorphicButtonStyle(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = NeomorphicStyle.SurfaceColor;
            button.ForeColor = NeomorphicStyle.TextColor;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.Paint += (s, e) => DrawNeomorphicButton(e.Graphics, button, button.ClientRectangle);
        }

        private void ApplyNeomorphicComboBoxStyle(ComboBox comboBox)
        {
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.BackColor = NeomorphicStyle.SurfaceColor;
            comboBox.ForeColor = NeomorphicStyle.TextColor;
        }

        private void DrawNeomorphicButton(Graphics g, Button button, Rectangle rect)
        {
            bool isPressed = button.ClientRectangle.Contains(button.PointToClient(Control.MousePosition)) &&
                            Control.MouseButtons == MouseButtons.Left && button.Enabled;

            if (!button.Enabled)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(NeomorphicStyle.SurfaceColor.R - 20, 
                                                                        NeomorphicStyle.SurfaceColor.G - 20, 
                                                                        NeomorphicStyle.SurfaceColor.B - 20)))
                {
                    g.FillRoundedRectangle(brush, rect, 12);
                }
                return;
            }

            // Основной фон
            using (SolidBrush brush = new SolidBrush(NeomorphicStyle.SurfaceColor))
            {
                g.FillRoundedRectangle(brush, rect, 12);
            }

            if (isPressed)
            {
                // Внутренняя тень
                Rectangle innerRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
                using (Pen darkPen = new Pen(NeomorphicStyle.DarkShadow, 3))
                {
                    g.DrawRoundedRectangle(darkPen, innerRect, 10);
                }
            }
            else
            {
                // Внешние тени
                Rectangle lightRect = new Rectangle(rect.X - 3, rect.Y - 3, rect.Width, rect.Height);
                Rectangle darkRect = new Rectangle(rect.X + 3, rect.Y + 3, rect.Width, rect.Height);

                using (GraphicsPath lightPath = NeomorphicStyle.CreateRoundedRectangle(lightRect, 12))
                using (GraphicsPath darkPath = NeomorphicStyle.CreateRoundedRectangle(darkRect, 12))
                {
                    using (Pen lightPen = new Pen(NeomorphicStyle.LightShadow, 5))
                    using (Pen darkPen = new Pen(NeomorphicStyle.DarkShadow, 5))
                    {
                        lightPen.LineJoin = LineJoin.Round;
                        darkPen.LineJoin = LineJoin.Round;
                        g.DrawPath(lightPen, lightPath);
                        g.DrawPath(darkPen, darkPath);
                    }
                }
            }

            // Текст
            using (SolidBrush textBrush = new SolidBrush(button.Enabled ? NeomorphicStyle.TextColor : Color.Gray))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(button.Text, button.Font, textBrush, rect, sf);
            }
        }

        private void FillRoundedRectangle(Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using (GraphicsPath path = NeomorphicStyle.CreateRoundedRectangle(rect, radius))
            {
                g.FillPath(brush, path);
            }
        }

        private void DrawRoundedRectangle(Graphics g, Pen pen, Rectangle rect, int radius)
        {
            using (GraphicsPath path = NeomorphicStyle.CreateRoundedRectangle(rect, radius))
            {
                g.DrawPath(pen, path);
            }
        }

        private void BtnGenerateReport_Click(object sender, EventArgs e)
        {
            try
            {
                string reportType = cbReportType.SelectedItem?.ToString() ?? "";
                reportData = GenerateReport(reportType);
                
                if (reportData != null && reportData.Rows.Count > 0)
                {
                    gridReports.DataSource = reportData;
                    btnExportToExcel.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Нет данных для отображения.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnExportToExcel.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportToExcel_Click(object sender, EventArgs e)
        {
            try
            {
                if (reportData == null || reportData.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string reportType = cbReportType.SelectedItem?.ToString() ?? "Отчет";
                if (ExcelExporter.ExportToExcelWithDialog(reportData, reportType.Replace(" ", "_")))
                {
                    MessageBox.Show("Отчет успешно экспортирован в Excel.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable GenerateReport(string reportType)
        {
            DataTable data = inventoryManager.GetAllInventory();

            switch (reportType)
            {
                case "Полный отчет по инвентарю":
                    return data;

                case "Отчет по категориям":
                    DataTable categoryReport = data.Clone();
                    var categoryGroups = data.AsEnumerable()
                        .GroupBy(r => r.Field<string>("CategoryName") ?? "Без категории");
                    foreach (var group in categoryGroups)
                    {
                        foreach (DataRow row in group)
                        {
                            categoryReport.ImportRow(row);
                        }
                    }
                    return categoryReport;

                case "Отчет по состоянию":
                    DataTable stateReport = data.Clone();
                    var stateGroups = data.AsEnumerable()
                        .GroupBy(r => r.Field<string>("CurrentState") ?? "Не указано");
                    foreach (var group in stateGroups.OrderBy(g => g.Key))
                    {
                        foreach (DataRow row in group)
                        {
                            stateReport.ImportRow(row);
                        }
                    }
                    return stateReport;

                case "Отчет по кабинетам":
                    DataTable classroomReport = data.Clone();
                    var classroomGroups = data.AsEnumerable()
                        .GroupBy(r => r.Field<string>("Classroom") ?? "Не указано");
                    foreach (var group in classroomGroups.OrderBy(g => g.Key))
                    {
                        foreach (DataRow row in group)
                        {
                            classroomReport.ImportRow(row);
                        }
                    }
                    return classroomReport;

                case "Отчет по ответственным":
                    DataTable personReport = data.Clone();
                    var personGroups = data.AsEnumerable()
                        .GroupBy(r => r.Field<string>("ResponsiblePerson") ?? "Не указано");
                    foreach (var group in personGroups.OrderBy(g => g.Key))
                    {
                        foreach (DataRow row in group)
                        {
                            personReport.ImportRow(row);
                        }
                    }
                    return personReport;

                default:
                    return data;
            }
        }
    }
}

