using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1
{
    public partial class StatisticsForm : UserControl
    {
        private InventoryManager inventoryManager;
        private Panel panelStats;
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

        public StatisticsForm(InventoryManager inventoryManager)
        {
            this.inventoryManager = inventoryManager;
            InitializeComponent();
            ApplyNeomorphicStyle();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            panelStats = new Panel();
            panelStats.Dock = DockStyle.Fill;
            panelStats.Padding = new Padding(20);
            panelStats.BackColor = NeomorphicStyle.BackgroundColor;
            panelStats.AutoScroll = true;

            // Заголовок
            var lblTitle = new Label();
            lblTitle.Text = "📈 Статистика инвентаря";
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = NeomorphicStyle.AccentColor;
            lblTitle.Location = new Point(20, 10);
            lblTitle.AutoSize = true;

            // Кнопки
            btnRefresh = new Button();
            btnRefresh.Text = "🔄 Обновить";
            btnRefresh.Location = new Point(20, 50);
            btnRefresh.Size = new Size(150, 40);
            btnRefresh.Click += BtnRefresh_Click;

            btnExport = new Button();
            btnExport.Text = "📥 Экспорт";
            btnExport.Location = new Point(180, 50);
            btnExport.Size = new Size(150, 40);
            btnExport.Click += BtnExport_Click;

            // Панели статистики
            var panelSummary = CreateStatPanel("📊 Общая статистика", 20, 100, 400, 180);
            
            lblTotalItems = CreateStatLabel("Всего предметов: 0", 20, 40);
            lblTotalValue = CreateStatLabel("Общая стоимость: 0 ₽", 20, 70);
            lblAvgValue = CreateStatLabel("Средняя стоимость: 0 ₽", 20, 100);
            lblCategoryCount = CreateStatLabel("Категорий: 0", 20, 130);

            panelSummary.Controls.Add(lblTotalItems);
            panelSummary.Controls.Add(lblTotalValue);
            panelSummary.Controls.Add(lblAvgValue);
            panelSummary.Controls.Add(lblCategoryCount);

            var panelCondition = CreateStatPanel("🔧 По состоянию", 440, 100, 400, 180);
            
            lblGoodCondition = CreateStatLabel("В хорошем состоянии: 0", 20, 40);
            lblNeedsRepair = CreateStatLabel("Требуют ремонта: 0", 20, 70);

            panelCondition.Controls.Add(lblGoodCondition);
            panelCondition.Controls.Add(lblNeedsRepair);

            // Таблица по категориям
            var lblCatStats = new Label();
            lblCatStats.Text = "📁 Статистика по категориям";
            lblCatStats.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblCatStats.ForeColor = NeomorphicStyle.TextColor;
            lblCatStats.Location = new Point(20, 300);
            lblCatStats.AutoSize = true;

            gridCategoryStats = CreateStatsGrid(20, 330, 400, 200);

            // Таблица по состояниям
            var lblStateStats = new Label();
            lblStateStats.Text = "🔧 Статистика по состояниям";
            lblStateStats.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblStateStats.ForeColor = NeomorphicStyle.TextColor;
            lblStateStats.Location = new Point(440, 300);
            lblStateStats.AutoSize = true;

            gridStateStats = CreateStatsGrid(440, 330, 400, 200);

            panelStats.Controls.Add(lblTitle);
            panelStats.Controls.Add(btnRefresh);
            panelStats.Controls.Add(btnExport);
            panelStats.Controls.Add(panelSummary);
            panelStats.Controls.Add(panelCondition);
            panelStats.Controls.Add(lblCatStats);
            panelStats.Controls.Add(gridCategoryStats);
            panelStats.Controls.Add(lblStateStats);
            panelStats.Controls.Add(gridStateStats);

            this.Controls.Add(panelStats);
            this.Size = new Size(860, 560);
            this.BackColor = NeomorphicStyle.BackgroundColor;

            this.ResumeLayout(false);
        }

        private Panel CreateStatPanel(string title, int x, int y, int width, int height)
        {
            var panel = new Panel();
            panel.Location = new Point(x, y);
            panel.Size = new Size(width, height);
            panel.BackColor = NeomorphicStyle.SurfaceColor;
            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(NeomorphicStyle.SurfaceColor))
                {
                    e.Graphics.FillRoundedRectangle(brush, panel.ClientRectangle, 12);
                }
            };

            var lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTitle.ForeColor = NeomorphicStyle.AccentColor;
            lblTitle.Location = new Point(15, 10);
            lblTitle.AutoSize = true;
            panel.Controls.Add(lblTitle);

            return panel;
        }

        private Label CreateStatLabel(string text, int x, int y)
        {
            var label = new Label();
            label.Text = text;
            label.Font = new Font("Segoe UI", 10F);
            label.ForeColor = NeomorphicStyle.TextColor;
            label.Location = new Point(x, y);
            label.AutoSize = true;
            return label;
        }

        private DataGridView CreateStatsGrid(int x, int y, int width, int height)
        {
            var grid = new DataGridView();
            grid.Location = new Point(x, y);
            grid.Size = new Size(width, height);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.BackgroundColor = NeomorphicStyle.SurfaceColor;
            grid.GridColor = NeomorphicStyle.DarkShadow;
            grid.DefaultCellStyle.BackColor = NeomorphicStyle.SurfaceColor;
            grid.DefaultCellStyle.ForeColor = NeomorphicStyle.TextColor;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.ColumnHeadersDefaultCellStyle.BackColor = NeomorphicStyle.SurfaceColor;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = NeomorphicStyle.TextColor;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.BorderStyle = BorderStyle.None;
            return grid;
        }

        private void ApplyNeomorphicStyle()
        {
            ApplyButtonStyle(btnRefresh);
            ApplyButtonStyle(btnExport);
        }

        private void ApplyButtonStyle(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = NeomorphicStyle.SurfaceColor;
            button.ForeColor = NeomorphicStyle.TextColor;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        public void LoadStatistics()
        {
            try
            {
                Logger.Info("Загрузка статистики");
                DataTable data = inventoryManager.GetAllInventory();

                // Общая статистика
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

                // Статистика по категориям
                var categoryStats = new DataTable();
                categoryStats.Columns.Add("Категория", typeof(string));
                categoryStats.Columns.Add("Количество", typeof(int));
                categoryStats.Columns.Add("Стоимость", typeof(decimal));

                var categories = data.AsEnumerable()
                    .GroupBy(r => r.Field<string>("CategoryName") ?? "Без категории")
                    .Select(g => new
                    {
                        Category = g.Key,
                        Count = g.Count(),
                        Value = g.Sum(r => r.Field<decimal?>("PurchasePrice") ?? 0)
                    });

                foreach (var cat in categories)
                {
                    categoryStats.Rows.Add(cat.Category, cat.Count, cat.Value);
                }

                gridCategoryStats.DataSource = categoryStats;
                lblCategoryCount.Text = $"Категорий: {categoryStats.Rows.Count}";

                // Статистика по состояниям
                var stateStats = new DataTable();
                stateStats.Columns.Add("Состояние", typeof(string));
                stateStats.Columns.Add("Количество", typeof(int));

                var states = data.AsEnumerable()
                    .GroupBy(r => r.Field<string>("CurrentState") ?? "Не указано")
                    .Select(g => new { State = g.Key, Count = g.Count() });

                foreach (var state in states)
                {
                    stateStats.Rows.Add(state.State, state.Count);
                }

                gridStateStats.DataSource = stateStats;

                Logger.Info($"Статистика загружена: {totalItems} предметов");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки статистики", ex);
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadStatistics();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable data = inventoryManager.GetAllInventory();
                if (ExcelExporter.ExportToExcelWithDialog(data, "Статистика_инвентаря"))
                {
                    MessageBox.Show("Статистика экспортирована.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка экспорта статистики", ex);
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

