using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.Services;
using WindowsFormsApp1.UI;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1
{
    public class MovementsForm : UserControl
    {
        private readonly MovementManager _movements = new MovementManager();
        private readonly InventoryManager _inventory;
        private readonly string _performedBy;
        private readonly string _userRole;

        private PeriodFilterPanel periodPanel;
        private ComboBox cbTypeFilter;
        private TextBox txtSearch;
        private DataGridView grid;
        private Label lblSummary;
        private Button btnNew;
        private Button btnRefresh;
        private Button btnDelete;
        private Button btnExport;

        public MovementsForm(InventoryManager inventory, string performedBy, string userRole)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _performedBy = performedBy ?? "";
            _userRole = userRole ?? "";

            try
            {
                _movements.EnsureSchema();
                BuildUi();
                ApplyPermissions();
            }
            catch (Exception ex)
            {
                ExceptionHandler.ShowError("Ошибка инициализации модуля распоряжений", ex);
            }
        }

        public void RefreshPermissions() => ApplyPermissions();

        private void ApplyPermissions()
        {
            bool canCreate = AuthorizationHelper.CanRegisterDisposition(_userRole);
            bool canDelete = AuthorizationHelper.CanDeleteDisposition(_userRole);
            bool canExport = AuthorizationHelper.CanExportDisposition(_userRole);

            btnNew.Enabled = canCreate;
            btnDelete.Enabled = canDelete;
            btnExport.Enabled = canExport;

            if (!canCreate)
                btnNew.Text = "Оформление недоступно";
        }

        private void BuildUi()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.Background;
            Padding = new Padding(12);

            var header = new Label
            {
                Text = "Распоряжение имуществом",
                Dock = DockStyle.Top,
                Height = 32,
                Font = AppTheme.FontHeader,
                ForeColor = AppTheme.Primary
            };

            var subtitle = new Label
            {
                Text = "Регистрация выдачи, перемещения между кабинетами, назначения ответственных лиц и списания. " +
                       "После оформления данные в реестре инвентаря обновляются автоматически.",
                Dock = DockStyle.Top,
                MaximumSize = new Size(2000, 0),
                AutoSize = true,
                Font = AppTheme.FontUi,
                ForeColor = AppTheme.TextSecondary,
                Padding = new Padding(0, 0, 0, 8)
            };

            periodPanel = new PeriodFilterPanel();
            periodPanel.PeriodChanged += (s, e) => SafeLoadData();

            var filterBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = AppTheme.Background };
            var flowFilter = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 4, 0, 0)
            };
            flowFilter.Controls.Add(new Label { Text = "Вид:", AutoSize = true, Margin = new Padding(0, 8, 4, 0), Font = AppTheme.FontUi });
            cbTypeFilter = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 4, 12, 0) };
            AppTheme.ApplyToComboBox(cbTypeFilter);
            cbTypeFilter.SelectedIndexChanged += (s, e) => SafeLoadData();
            flowFilter.Controls.Add(cbTypeFilter);
            flowFilter.Controls.Add(new Label { Text = "Поиск:", AutoSize = true, Margin = new Padding(0, 8, 4, 0), Font = AppTheme.FontUi });
            txtSearch = new TextBox { Width = 200, Margin = new Padding(0, 4, 8, 0) };
            AppTheme.ApplyToTextBox(txtSearch);
            InputMaskHelper.ApplySearchMask(txtSearch);
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) SafeLoadData(); };
            var btnFind = new Button { Text = "Найти", Width = 80, Height = 30, Margin = new Padding(0, 4, 0, 0) };
            AppTheme.ApplyToButton(btnFind);
            btnFind.Click += (s, e) => SafeLoadData();
            flowFilter.Controls.Add(txtSearch);
            flowFilter.Controls.Add(btnFind);
            filterBar.Controls.Add(flowFilter);

            var toolbar = new ToolbarPanel();
            btnNew = new Button { Text = "Оформить распоряжение" };
            btnRefresh = new Button { Text = "Обновить" };
            btnDelete = new Button { Text = "Удалить запись" };
            btnExport = new Button { Text = "Экспорт" };
            btnNew.Click += BtnNew_Click;
            btnRefresh.Click += (s, e) => SafeLoadData();
            btnDelete.Click += BtnDelete_Click;
            btnExport.Click += BtnExport_Click;
            toolbar.AddButton(btnNew, true);
            toolbar.AddButton(btnRefresh);
            toolbar.AddButton(btnDelete);
            toolbar.AddButton(btnExport);

            lblSummary = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Font = AppTheme.FontUi,
                ForeColor = AppTheme.TextSecondary,
                Text = "Записей: 0"
            };

            grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true };
            AppTheme.ApplyToDataGridView(grid);
            grid.DataBindingComplete += (s, e) => GridHelper.HideTechnicalColumns(grid);

            Controls.Add(grid);
            Controls.Add(lblSummary);
            Controls.Add(toolbar);
            Controls.Add(filterBar);
            Controls.Add(periodPanel);
            Controls.Add(subtitle);
            Controls.Add(header);

            LoadTypeFilter();
        }

        private void SafeLoadData()
        {
            try { LoadData(); }
            catch (Exception ex) { ExceptionHandler.ShowError("Ошибка загрузки журнала", ex, FindForm()); }
        }

        private void LoadTypeFilter()
        {
            try
            {
                var types = _movements.GetMovementTypes();
                ComboBoxHelper.Bind(cbTypeFilter, types, "TypeName", "TypeID", "— Все виды —", 0);
            }
            catch (Exception ex)
            {
                ExceptionHandler.ShowError("Не удалось загрузить виды распоряжений", ex, FindForm());
            }
        }

        public void LoadData()
        {
            int? typeId = ComboBoxHelper.GetSelectedInt(cbTypeFilter);
            if (typeId.HasValue && typeId.Value <= 0) typeId = null;

            var data = _movements.GetMovements(
                periodPanel.DateFrom,
                periodPanel.DateTo,
                typeId,
                txtSearch.Text);

            grid.DataSource = data;
            GridHelper.HideTechnicalColumns(grid);
            if (grid.Columns.Contains("Дата"))
                grid.Columns["Дата"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";

            lblSummary.Text = $"Период: {periodPanel.GetPeriodDescription()}  |  Записей: {data.Rows.Count}";
            Logger.Info($"Загружен журнал распоряжений: {data.Rows.Count} записей");
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            if (!AuthorizationHelper.EnsureAuthorized(
                AuthorizationHelper.CanRegisterDisposition(_userRole), FindForm(),
                "оформление распоряжения имуществом"))
                return;

            try
            {
                using (var dlg = new DispositionOperationDialog(_inventory, _movements, _performedBy, _userRole))
                {
                    if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                        SafeLoadData();
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                ExceptionHandler.ShowWarning(ex.Message, FindForm());
            }
            catch (Exception ex)
            {
                ExceptionHandler.ShowError("Не удалось открыть форму распоряжения", ex, FindForm());
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (!AuthorizationHelper.EnsureAuthorized(
                AuthorizationHelper.CanDeleteDisposition(_userRole), FindForm(),
                "удаление записей из журнала распоряжений"))
                return;

            if (grid.CurrentRow == null)
            {
                ExceptionHandler.ShowWarning("Выберите запись в таблице.", FindForm());
                return;
            }

            if (MessageBox.Show(FindForm(),
                "Удалить выбранное распоряжение из журнала?\n(Состояние имущества в реестре не откатывается автоматически.)",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                object cell = grid.CurrentRow.Cells["MovementID"].Value;
                if (cell == null || cell == DBNull.Value)
                    throw new InvalidOperationException("Не удалось определить выбранную запись.");
                int id = Convert.ToInt32(cell);
                if (_movements.DeleteMovement(id))
                    SafeLoadData();
            }
            catch (Exception ex)
            {
                ExceptionHandler.ShowError("Ошибка удаления записи", ex, FindForm());
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (!AuthorizationHelper.EnsureAuthorized(
                AuthorizationHelper.CanExportData(_userRole), FindForm(), "экспорт журнала"))
                return;

            try
            {
                if (grid.DataSource is DataTable table && table.Rows.Count > 0)
                {
                    if (ExcelExporter.ExportToExcelWithDialog(table, $"Распоряжения_{periodPanel.DateFrom:yyyyMMdd}"))
                        MessageBox.Show(FindForm(), "Журнал экспортирован.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    ExceptionHandler.ShowWarning("Нет данных для экспорта.", FindForm());
            }
            catch (Exception ex)
            {
                ExceptionHandler.ShowError("Ошибка экспорта", ex, FindForm());
            }
        }
    }
}
