using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.UI;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1
{
    public partial class StatisticsForm : UserControl
    {
        private readonly InventoryManager _inventory;
        private PeriodFilterPanel periodPanel;
        private Label lblTotalItems;
        private Label lblTotalValue;
        private Label lblAvgValue;
        private Label lblCategoryCount;
        private Label lblGoodCondition;
        private Label lblNeedsRepair;
        private DataGridView gridCategoryStats;
        private DataGridView gridStateStats;
        private Button btnRefresh;
        private Button btnExport;
        private readonly string _userRole;

        public StatisticsForm(InventoryManager inventoryManager, string userRole = "")
        {
            _inventory = inventoryManager;
            _userRole = userRole ?? "";
            InitializeComponent();
            btnExport.Enabled = AuthorizationHelper.CanExportStatistics(_userRole);
            if (!AuthorizationHelper.CanExportStatistics(_userRole))
                btnExport.Visible = false;
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Dock = DockStyle.Fill;
            BackColor = AppTheme.Background;
            Padding = new Padding(12);

            var header = new Label
            {
                Text = "Статистика инвентаря",
                Dock = DockStyle.Top,
                Height = 32,
                Font = AppTheme.FontHeader,
                ForeColor = AppTheme.Primary
            };

            periodPanel = new PeriodFilterPanel();

            var toolbar = new ActionButtonRow();
            btnRefresh = toolbar.AddButton("Обновить", true);
            btnExport = toolbar.AddButton("Экспорт");
            btnRefresh.Click += BtnRefresh_Click;
            btnExport.Click += BtnExport_Click;

            var summaryPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 140,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppTheme.Background,
                Padding = new Padding(0, 8, 0, 8)
            };
            summaryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            summaryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            var panelSummary = CreateStatPanel("Общие показатели");
            var flowSummary = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            lblTotalItems = CreateStatLabel("Всего предметов: 0");
            lblTotalValue = CreateStatLabel("Общая стоимость: 0 ₽");
            lblAvgValue = CreateStatLabel("Средняя стоимость: 0 ₽");
            lblCategoryCount = CreateStatLabel("Категорий: 0");
            flowSummary.Controls.AddRange(new Control[] { lblTotalItems, lblTotalValue, lblAvgValue, lblCategoryCount });
            panelSummary.Controls.Add(flowSummary);

            var panelCondition = CreateStatPanel("По состоянию");
            var flowCondition = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            lblGoodCondition = CreateStatLabel("В хорошем состоянии: 0");
            lblNeedsRepair = CreateStatLabel("Требуют ремонта: 0");
            flowCondition.Controls.AddRange(new Control[] { lblGoodCondition, lblNeedsRepair });
            panelCondition.Controls.Add(flowCondition);

            summaryPanel.Controls.Add(panelSummary, 0, 0);
            summaryPanel.Controls.Add(panelCondition, 1, 0);

            var gridsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(0, 8, 0, 0)
            };
            gridsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            gridsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            gridsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
            gridsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var lblCat = new Label { Text = "По категориям", Dock = DockStyle.Fill, Font = AppTheme.FontUiBold, ForeColor = AppTheme.Primary };
            var lblState = new Label { Text = "По состояниям", Dock = DockStyle.Fill, Font = AppTheme.FontUiBold, ForeColor = AppTheme.Primary };
            gridCategoryStats = new DataGridView { Dock = DockStyle.Fill };
            gridStateStats = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.ApplyToDataGridView(gridCategoryStats);
            AppTheme.ApplyToDataGridView(gridStateStats);

            gridsPanel.Controls.Add(lblCat, 0, 0);
            gridsPanel.Controls.Add(lblState, 1, 0);
            gridsPanel.Controls.Add(gridCategoryStats, 0, 1);
            gridsPanel.Controls.Add(gridStateStats, 1, 1);

            Controls.Add(gridsPanel);
            Controls.Add(summaryPanel);
            Controls.Add(toolbar);
            Controls.Add(periodPanel);
            Controls.Add(header);
            ResumeLayout(false);
        }

        private static Panel CreateStatPanel(string title)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Surface,
                Padding = new Padding(12)
            };
            var lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 24,
                Font = AppTheme.FontUiBold,
                ForeColor = AppTheme.Primary
            };
            panel.Controls.Add(lblTitle);
            return panel;
        }

        private static Label CreateStatLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = AppTheme.FontUi,
                ForeColor = AppTheme.TextPrimary,
                Margin = new Padding(0, 4, 0, 0)
            };
        }

        public void LoadStatistics()
        {
            try
            {
                Logger.Info("Загрузка статистики");
                var criteria = new InventoryFilterCriteria();
                periodPanel.ApplyToCriteria(criteria);
                DataTable data = _inventory.GetFilteredInventory(criteria);

                int totalItems = data.Rows.Count;
                decimal totalValue = 0;
                int goodCondition = 0;
                int needsRepair = 0;

                foreach (DataRow row in data.Rows)
                {
                    if (row["PurchasePrice"] != DBNull.Value)
                        totalValue += Convert.ToDecimal(row["PurchasePrice"]);

                    string state = row["CurrentState"]?.ToString() ?? "";
                    if (state == "Отличное" || state == "Хорошее")
                        goodCondition++;
                    else if (state == "Требует ремонта" || state == "Неисправное")
                        needsRepair++;
                }

                decimal avgValue = totalItems > 0 ? totalValue / totalItems : 0;

                lblTotalItems.Text = $"Всего предметов: {totalItems}";
                lblTotalValue.Text = $"Общая стоимость: {totalValue:N2} ₽";
                lblAvgValue.Text = $"Средняя стоимость: {avgValue:N2} ₽";
                lblGoodCondition.Text = $"В хорошем состоянии: {goodCondition}";
                lblNeedsRepair.Text = $"Требуют ремонта: {needsRepair}";

                var categoryStats = new DataTable();
                categoryStats.Columns.Add("Категория", typeof(string));
                categoryStats.Columns.Add("Количество", typeof(int));
                categoryStats.Columns.Add("Сумма, руб.", typeof(decimal));

                foreach (var g in data.AsEnumerable()
                    .GroupBy(r => r.Field<string>("CategoryName") ?? "Без категории"))
                {
                    categoryStats.Rows.Add(g.Key, g.Count(), g.Sum(r => r.Field<decimal?>("PurchasePrice") ?? 0));
                }

                gridCategoryStats.DataSource = categoryStats;
                lblCategoryCount.Text = $"Категорий: {categoryStats.Rows.Count}";

                var stateStats = new DataTable();
                stateStats.Columns.Add("Состояние", typeof(string));
                stateStats.Columns.Add("Количество", typeof(int));

                foreach (var g in data.AsEnumerable()
                    .GroupBy(r => r.Field<string>("CurrentState") ?? "Не указано"))
                {
                    stateStats.Rows.Add(g.Key, g.Count());
                }

                gridStateStats.DataSource = stateStats;
                GridHelper.HideTechnicalColumns(gridCategoryStats);
                GridHelper.HideTechnicalColumns(gridStateStats);

                Logger.Info($"Статистика за период {periodPanel.GetPeriodDescription()}: {totalItems} предметов");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки статистики", ex);
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e) => LoadStatistics();

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (!AuthorizationHelper.EnsureAuthorized(
                AuthorizationHelper.CanExportStatistics(_userRole), FindForm(), "экспорт статистики"))
                return;
            try
            {
                var criteria = new InventoryFilterCriteria();
                periodPanel.ApplyToCriteria(criteria);
                DataTable data = _inventory.GetFilteredInventory(criteria);
                string name = $"Статистика_{periodPanel.DateFrom:yyyyMMdd}_{periodPanel.DateTo:yyyyMMdd}";
                if (ExcelExporter.ExportToExcelWithDialog(data, name))
                    MessageBox.Show("Статистика экспортирована.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка экспорта статистики", ex);
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
