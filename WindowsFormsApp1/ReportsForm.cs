using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.Export;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.Services;
using WindowsFormsApp1.UI;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1
{
    public partial class ReportsForm : UserControl
    {
        private readonly InventoryManager _inventory;
        private readonly ReportService _reportService = new ReportService();
        private readonly string _preparedByName;
        private readonly string _userRole;

        private ListBox lstReports;
        private Label lblDescription;
        private Label lblTitle;
        private DataGridView gridReports;
        private Button btnGenerate;
        private Button btnExcel;
        private Button btnPdf;
        private InventoryFilterPanel filterPanel;
        private PeriodFilterPanel periodPanel;
        private Panel rightPanel;
        private Panel leftPanel;
        private ReportResult _currentReport;

        private const int LeftPanelWidth = 300;

        public ReportsForm(InventoryManager inventoryManager, string preparedByName, string userRole)
        {
            _inventory = inventoryManager;
            _preparedByName = preparedByName ?? "";
            _userRole = userRole ?? "";
            InitializeComponent();
            ApplyPermissions();
        }

        public void RefreshPermissions() => ApplyPermissions();

        public void EnsureLayout() => EnsureLeftPanelLayout();

        private void ApplyPermissions()
        {
            btnGenerate.Enabled = AuthorizationHelper.CanGenerateReports(_userRole);
            btnExcel.Enabled = false;
            btnPdf.Enabled = false;

            if (!AuthorizationHelper.CanGenerateReports(_userRole))
                btnGenerate.Text = "Нет прав";
            else
                btnGenerate.Text = "Сформировать отчёт";

            btnExcel.Enabled = AuthorizationHelper.CanExportReportsExcel(_userRole) && _currentReport?.Data != null;
            btnPdf.Enabled = AuthorizationHelper.CanExportReportsPdf(_userRole) && _currentReport != null;
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Dock = DockStyle.Fill;
            BackColor = AppTheme.Background;
            Padding = new Padding(8);

            // Левая панель фиксированной ширины — сразу корректный размер без «схлопывания» SplitContainer
            leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = LeftPanelWidth,
                MinimumSize = new Size(260, 0),
                BackColor = AppTheme.Surface,
                Padding = new Padding(0)
            };

            var leftLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = AppTheme.Surface,
                Padding = new Padding(8)
            };
            leftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));

            var lblList = new Label
            {
                Text = "Виды отчётов",
                Dock = DockStyle.Fill,
                Height = 26,
                Font = AppTheme.FontHeader,
                ForeColor = AppTheme.Primary,
                Margin = new Padding(0, 0, 0, 4)
            };

            lstReports = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = AppTheme.FontUi,
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false,
                ItemHeight = 26,
                HorizontalScrollbar = true
            };
            lstReports.Items.AddRange(new object[]
            {
                "1. Сводный финансовый",
                "2. По кабинетам и МОЛ",
                "3. Состояние × категория",
                "4. Реестр с группировкой"
            });
            lstReports.SelectedIndex = 0;
            lstReports.SelectedIndexChanged += LstReports_SelectedIndexChanged;

            var lblActions = new Label
            {
                Text = "Управление отчётом",
                Dock = DockStyle.Fill,
                Height = 24,
                Font = AppTheme.FontUiBold,
                ForeColor = AppTheme.Primary,
                Margin = new Padding(0, 8, 0, 4)
            };

            btnGenerate = CreateSideButton("Сформировать отчёт", true);
            btnExcel = CreateSideButton("Экспорт Excel", false);
            btnPdf = CreateSideButton("Экспорт PDF", false);
            btnGenerate.Click += BtnGenerate_Click;
            btnExcel.Click += BtnExcel_Click;
            btnPdf.Click += BtnPdf_Click;

            leftLayout.Controls.Add(lblList, 0, 0);
            leftLayout.Controls.Add(lstReports, 0, 1);
            leftLayout.Controls.Add(lblActions, 0, 2);
            leftLayout.Controls.Add(btnGenerate, 0, 3);
            leftLayout.Controls.Add(btnExcel, 0, 4);
            leftLayout.Controls.Add(btnPdf, 0, 5);

            // ——— Правая панель: параметры + таблица (без кнопок) ———
            rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.Background, Padding = new Padding(4, 0, 0, 0) };

            lblTitle = new Label
            {
                Text = "Отчёты",
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = AppTheme.FontHeader,
                ForeColor = AppTheme.Primary,
                Padding = new Padding(0, 0, 0, 2)
            };

            lblDescription = new Label
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = AppTheme.FontUi,
                ForeColor = AppTheme.TextSecondary,
                MaximumSize = new Size(700, 0),
                Text = "Выберите вид отчёта слева и нажмите «Сформировать отчёт».",
                Padding = new Padding(0, 0, 0, 8)
            };

            periodPanel = new PeriodFilterPanel();

            filterPanel = new InventoryFilterPanel(_inventory, FilterPanelLayout.Compact)
            {
                Dock = DockStyle.Top,
                Height = 132
            };

            gridReports = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.ApplyToDataGridView(gridReports);

            rightPanel.Controls.Add(gridReports);
            rightPanel.Controls.Add(filterPanel);
            rightPanel.Controls.Add(periodPanel);
            rightPanel.Controls.Add(lblDescription);
            rightPanel.Controls.Add(lblTitle);
            rightPanel.Resize += RightPanel_Resize;

            leftPanel.Controls.Add(leftLayout);
            leftLayout.Dock = DockStyle.Fill;

            // Порядок важен: сначала Fill, затем Left
            Controls.Add(rightPanel);
            Controls.Add(leftPanel);

            Load += ReportsForm_Load;
            VisibleChanged += ReportsForm_VisibleChanged;

            LstReports_SelectedIndexChanged(null, EventArgs.Empty);
            ResumeLayout(true);
            PerformLayout();
        }

        private void ReportsForm_Load(object sender, EventArgs e) => EnsureLeftPanelLayout();

        private void ReportsForm_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
                EnsureLeftPanelLayout();
        }

        private void EnsureLeftPanelLayout()
        {
            if (leftPanel == null) return;
            leftPanel.Width = LeftPanelWidth;
            RightPanel_Resize(null, EventArgs.Empty);
            leftPanel.PerformLayout();
            PerformLayout();
        }

        private static Button CreateSideButton(string text, bool primary)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 6),
                MinimumSize = new Size(200, 36)
            };
            AppTheme.ApplyToButton(btn, primary);
            return btn;
        }

        private void RightPanel_Resize(object sender, EventArgs e)
        {
            int w = Math.Max(200, rightPanel.ClientSize.Width - 12);
            lblDescription.MaximumSize = new Size(w, 0);
        }

        private void LstReports_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstReports.SelectedIndex < 0) return;
            string[] fullTitles =
            {
                "1. Сводный финансовый отчёт по категориям",
                "2. Отчёт по кабинетам и МОЛ",
                "3. Анализ по состоянию и категориям",
                "4. Реестр с группировкой по категориям"
            };
            lblTitle.Text = fullTitles[lstReports.SelectedIndex];
            lblDescription.Text = GetReportDescription(lstReports.SelectedIndex);
            RightPanel_Resize(null, EventArgs.Empty);
        }

        private static string GetReportDescription(int index)
        {
            switch (index)
            {
                case 0: return "Группировка по категориям: количество, сумма, средняя цена, доля %, общий итог.";
                case 1: return "Группировка по кабинетам и ответственным лицам с подытогами по кабинету.";
                case 2: return "Матрица «состояние × категория» с суммированием и итогами.";
                case 3: return "Детальный реестр позиций с промежуточными итогами по категориям.";
                default: return "";
            }
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (!AuthorizationHelper.EnsureAuthorized(
                AuthorizationHelper.CanGenerateReports(_userRole), FindForm(), "формирование отчётов"))
                return;

            try
            {
                var kind = (ReportKind)(lstReports.SelectedIndex + 1);
                var filter = filterPanel.GetCriteria();
                periodPanel.ApplyToCriteria(filter);

                _currentReport = _reportService.BuildReport(kind, filter);

                if (_currentReport?.Data == null || _currentReport.Data.Rows.Count == 0)
                {
                    gridReports.DataSource = null;
                    btnExcel.Enabled = false;
                    btnPdf.Enabled = false;
                    MessageBox.Show("Нет данных за выбранный период.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                gridReports.DataSource = _currentReport.Data;
                GridHelper.HideTechnicalColumns(gridReports);
                AppTheme.StyleSubtotalGridRows(gridReports);

                btnExcel.Enabled = AuthorizationHelper.CanExportReportsExcel(_userRole);
                btnPdf.Enabled = AuthorizationHelper.CanExportReportsPdf(_userRole);

                lblDescription.Text = _currentReport.Description +
                    Environment.NewLine + $"Период: {periodPanel.GetPeriodDescription()}  |  Строк: {_currentReport.Data.Rows.Count}";
                RightPanel_Resize(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ExceptionHandler.ShowError("Ошибка формирования отчёта", ex, FindForm());
            }
        }

        private void BtnExcel_Click(object sender, EventArgs e)
        {
            if (!AuthorizationHelper.EnsureAuthorized(
                AuthorizationHelper.CanExportReportsExcel(_userRole), FindForm(), "экспорт отчёта в Excel"))
                return;
            if (_currentReport?.Data == null) return;
            try
            {
                if (ExcelExporter.ExportToExcelWithDialog(_currentReport.Data, Sanitize(_currentReport.Title)))
                    MessageBox.Show("Отчёт экспортирован в Excel (CSV).", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ExceptionHandler.ShowError("Ошибка экспорта", ex, FindForm());
            }
        }

        private void BtnPdf_Click(object sender, EventArgs e)
        {
            if (!AuthorizationHelper.EnsureAuthorized(
                AuthorizationHelper.CanExportReportsPdf(_userRole), FindForm(), "экспорт отчёта в PDF"))
                return;
            if (_currentReport == null) return;
            try
            {
                PdfReportExporter.ExportWithDialog(_currentReport, _preparedByName);
            }
            catch (Exception ex)
            {
                ExceptionHandler.ShowError("Ошибка экспорта PDF", ex, FindForm());
            }
        }

        private static string Sanitize(string s)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            return s;
        }
    }
}
