using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.UI
{
    public enum FilterPanelLayout
    {
        Full,
        Compact
    }

    /// <summary>
    /// Панель фильтрации с корректной вёрсткой (TableLayoutPanel).
    /// </summary>
    public class InventoryFilterPanel : UserControl
    {
        private readonly InventoryManager _inventory;
        private readonly FilterPanelLayout _layout;
        private TextBox txtSearch;
        private ComboBox cbCategory;
        private ComboBox cbClassroom;
        private ComboBox cbResponsible;
        private ComboBox cbState;
        private ComboBox cbStatus;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private NumericUpDown numPriceFrom;
        private NumericUpDown numPriceTo;
        private CheckBox chkDateFrom;
        private CheckBox chkDateTo;
        private Button btnApply;
        private Button btnReset;

        public event EventHandler FilterApplied;

        public InventoryFilterPanel(InventoryManager inventory, FilterPanelLayout layout = FilterPanelLayout.Full)
        {
            _inventory = inventory;
            _layout = layout;
            BuildUi();
            LoadLookups();
        }

        public InventoryFilterCriteria GetCriteria()
        {
            var c = new InventoryFilterCriteria { SearchText = txtSearch.Text.Trim() };
            if (cbCategory.SelectedIndex > 0 && cbCategory.SelectedValue is int catId && catId > 0)
                c.CategoryId = catId;
            if (cbClassroom.SelectedIndex > 0 && cbClassroom.SelectedValue is int roomId && roomId > 0)
                c.ClassroomId = roomId;
            if (cbResponsible.SelectedIndex > 0 && cbResponsible.SelectedValue is int personId && personId > 0)
                c.ResponsiblePersonId = personId;
            if (cbState.SelectedIndex > 0)
                c.CurrentState = cbState.SelectedItem?.ToString();
            if (cbStatus.SelectedIndex > 0)
                c.Status = cbStatus.SelectedItem?.ToString();
            if (chkDateFrom.Checked)
                c.DateFrom = dtpFrom.Value.Date;
            if (chkDateTo.Checked)
                c.DateTo = dtpTo.Value.Date;
            if (numPriceFrom.Value > 0)
                c.PriceFrom = numPriceFrom.Value;
            if (numPriceTo.Value > 0)
                c.PriceTo = numPriceTo.Value;
            return c;
        }

        private void BuildUi()
        {
            Dock = DockStyle.Top;
            AutoSize = false;
            Height = _layout == FilterPanelLayout.Compact ? 132 : 178;
            MinimumSize = new Size(640, Height);
            BackColor = AppTheme.Surface;
            Padding = new Padding(10, 8, 10, 8);

            var border = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.Surface,
                Padding = new Padding(8)
            };
            border.Paint += (s, e) =>
            {
                var r = border.ClientRectangle;
                r.Width -= 1; r.Height -= 1;
                using (var pen = new Pen(AppTheme.Border))
                    e.Graphics.DrawRectangle(pen, r);
            };

            int cols = _layout == FilterPanelLayout.Compact ? 4 : 6;
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = cols,
                RowCount = _layout == FilterPanelLayout.Compact ? 2 : 3,
                AutoSize = false
            };

            for (int i = 0; i < cols; i++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));

            int rowH = 54;
            int btnRowH = 40;
            for (int r = 0; r < grid.RowCount; r++)
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, r < grid.RowCount - 1 ? rowH : btnRowH));

            txtSearch = CreateTextBox();
            cbCategory = CreateCombo();
            cbClassroom = CreateCombo();
            cbResponsible = CreateCombo();
            cbState = CreateCombo();
            cbStatus = CreateCombo();
            dtpFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Dock = DockStyle.Fill, MinDate = new DateTime(1990, 1, 1) };
            dtpTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Dock = DockStyle.Fill, MinDate = new DateTime(1990, 1, 1) };
            chkDateFrom = new CheckBox { Text = "Дата с", AutoSize = false, Dock = DockStyle.Left, Width = 72, Font = AppTheme.FontUi };
            chkDateTo = new CheckBox { Text = "Дата по", AutoSize = false, Dock = DockStyle.Left, Width = 72, Font = AppTheme.FontUi };
            numPriceFrom = new NumericUpDown { Maximum = 99999999, Dock = DockStyle.Fill, Height = 28 };
            numPriceTo = new NumericUpDown { Maximum = 99999999, Dock = DockStyle.Fill, Height = 28 };

            btnApply = new Button { Text = "Применить", Width = 120, Height = 32, Margin = new Padding(0, 0, 8, 0) };
            btnReset = new Button { Text = "Сбросить", Width = 100, Height = 32 };
            AppTheme.ApplyToButton(btnApply, true);
            AppTheme.ApplyToButton(btnReset);
            btnApply.Click += (s, e) => FilterApplied?.Invoke(this, EventArgs.Empty);
            btnReset.Click += (s, e) => ResetFilters();

            if (_layout == FilterPanelLayout.Compact)
            {
                grid.Controls.Add(MakeField("Поиск", txtSearch), 0, 0);
                grid.SetColumnSpan(grid.GetControlFromPosition(0, 0), 2);
                grid.Controls.Add(MakeField("Категория", cbCategory), 2, 0);
                grid.Controls.Add(MakeField("Кабинет", cbClassroom), 3, 0);

                var btnFlow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding = new Padding(0, 4, 0, 0)
                };
                btnApply.Margin = new Padding(0, 0, 6, 4);
                btnReset.Margin = new Padding(0, 0, 6, 4);
                btnFlow.Controls.Add(btnApply);
                btnFlow.Controls.Add(btnReset);
                var linkFull = new LinkLabel
                {
                    Text = "Все фильтры…",
                    AutoSize = true,
                    Margin = new Padding(0, 10, 0, 4),
                    LinkColor = AppTheme.Accent
                };
                linkFull.LinkClicked += (s, e) => ShowExtendedFilterDialog();
                btnFlow.Controls.Add(linkFull);
                grid.Controls.Add(btnFlow, 0, 1);
                grid.SetColumnSpan(btnFlow, 4);
                grid.RowStyles[1] = new RowStyle(SizeType.AutoSize);
            }
            else
            {
                grid.Controls.Add(MakeField("Поиск", txtSearch), 0, 0);
                grid.SetColumnSpan(grid.GetControlFromPosition(0, 0), 2);
                grid.Controls.Add(MakeField("Категория", cbCategory), 2, 0);
                grid.Controls.Add(MakeField("Кабинет", cbClassroom), 3, 0);
                grid.Controls.Add(MakeField("Ответственный", cbResponsible), 4, 0);
                grid.SetColumnSpan(grid.GetControlFromPosition(4, 0), 2);

                grid.Controls.Add(MakeField("Состояние", cbState), 0, 1);
                grid.Controls.Add(MakeField("Статус", cbStatus), 1, 1);
                grid.Controls.Add(MakeField("Дата с", MakeDateRow(chkDateFrom, dtpFrom)), 2, 1);
                grid.Controls.Add(MakeField("Дата по", MakeDateRow(chkDateTo, dtpTo)), 3, 1);
                grid.Controls.Add(MakeField("Цена от, ₽", numPriceFrom), 4, 1);
                grid.Controls.Add(MakeField("Цена до, ₽", numPriceTo), 5, 1);

                var btnFlow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    Padding = new Padding(0, 4, 0, 0)
                };
                btnFlow.Controls.Add(btnApply);
                btnFlow.Controls.Add(btnReset);
                grid.Controls.Add(btnFlow, 0, 2);
                grid.SetColumnSpan(btnFlow, cols);
            }

            border.Controls.Add(grid);
            Controls.Add(border);
        }

        public void LoadFromCriteria(InventoryFilterCriteria c)
        {
            if (c == null) return;
            txtSearch.Text = c.SearchText ?? "";
            if (c.CategoryId.HasValue) TrySelectCombo(cbCategory, c.CategoryId.Value);
            if (c.ClassroomId.HasValue) TrySelectCombo(cbClassroom, c.ClassroomId.Value);
            if (c.ResponsiblePersonId.HasValue) TrySelectCombo(cbResponsible, c.ResponsiblePersonId.Value);
            if (!string.IsNullOrEmpty(c.CurrentState))
                for (int i = 0; i < cbState.Items.Count; i++)
                    if (cbState.Items[i].ToString() == c.CurrentState) { cbState.SelectedIndex = i; break; }
            if (!string.IsNullOrEmpty(c.Status))
                for (int i = 0; i < cbStatus.Items.Count; i++)
                    if (cbStatus.Items[i].ToString() == c.Status) { cbStatus.SelectedIndex = i; break; }
            chkDateFrom.Checked = c.DateFrom.HasValue;
            if (c.DateFrom.HasValue) dtpFrom.Value = c.DateFrom.Value;
            chkDateTo.Checked = c.DateTo.HasValue;
            if (c.DateTo.HasValue) dtpTo.Value = c.DateTo.Value;
            numPriceFrom.Value = c.PriceFrom ?? 0;
            numPriceTo.Value = c.PriceTo ?? 0;
        }

        private static void TrySelectCombo(ComboBox cb, int id)
        {
            for (int i = 0; i < cb.Items.Count; i++)
            {
                cb.SelectedIndex = i;
                if (cb.SelectedValue is int v && v == id) return;
            }
        }

        private void ShowExtendedFilterDialog()
        {
            using (var dlg = new Form())
            {
                dlg.Text = "Расширенные параметры отчёта";
                dlg.Size = new Size(900, 260);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = dlg.MinimizeBox = false;
                var panel = new InventoryFilterPanel(_inventory, FilterPanelLayout.Full) { Dock = DockStyle.Fill };
                panel.LoadFromCriteria(GetCriteria());
                var ok = new Button { Text = "Применить", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom, Height = 40 };
                AppTheme.ApplyToButton(ok, true);
                dlg.Controls.Add(panel);
                dlg.Controls.Add(ok);
                dlg.AcceptButton = ok;
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    LoadFromCriteria(panel.GetCriteria());
                    FilterApplied?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private static Control MakeField(string caption, Control input)
        {
            var host = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(4, 0, 4, 0)
            };
            host.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            host.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            var lbl = new Label
            {
                Text = caption,
                Dock = DockStyle.Fill,
                Font = AppTheme.FontUi,
                ForeColor = AppTheme.TextSecondary,
                TextAlign = ContentAlignment.BottomLeft
            };
            input.Dock = DockStyle.Fill;
            input.MinimumSize = new Size(60, 26);
            host.Controls.Add(lbl, 0, 0);
            host.Controls.Add(input, 0, 1);
            return host;
        }

        private static Control MakeDateRow(CheckBox chk, DateTimePicker dtp)
        {
            var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            chk.Dock = DockStyle.Fill;
            dtp.Dock = DockStyle.Fill;
            p.Controls.Add(chk, 0, 0);
            p.Controls.Add(dtp, 1, 0);
            return p;
        }

        private static TextBox CreateTextBox()
        {
            var tb = new TextBox { Height = 28 };
            AppTheme.ApplyToTextBox(tb);
            return tb;
        }

        private static ComboBox CreateCombo()
        {
            var cb = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Height = 28,
                IntegralHeight = false,
                MaxDropDownItems = 12
            };
            AppTheme.ApplyToComboBox(cb);
            return cb;
        }

        private void LoadLookups()
        {
            try
            {
                BindCombo(cbCategory, _inventory.GetCategories(), "CategoryName", "CategoryID", "— Все категории —");
                BindCombo(cbClassroom, _inventory.GetClassrooms(), "Name", "ClassroomID", "— Все кабинеты —");
                BindCombo(cbResponsible, _inventory.GetResponsiblePersons(), "Name", "PersonID", "— Все ответственные —");

                cbState.Items.Clear();
                cbState.Items.Add("— Все состояния —");
                foreach (var s in _inventory.GetDistinctStates()) cbState.Items.Add(s);
                cbState.SelectedIndex = 0;

                cbStatus.Items.Clear();
                cbStatus.Items.Add("— Все статусы —");
                foreach (var s in _inventory.GetDistinctStatuses()) cbStatus.Items.Add(s);
                cbStatus.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static void BindCombo(ComboBox cb, DataTable data, string display, string value, string allText)
        {
            var dt = data.Copy();
            var row = dt.NewRow();
            row[display] = allText;
            row[value] = 0;
            dt.Rows.InsertAt(row, 0);
            cb.DataSource = dt;
            cb.DisplayMember = display;
            cb.ValueMember = value;
        }

        public void ResetFilters()
        {
            txtSearch.Clear();
            if (cbCategory.Items.Count > 0) cbCategory.SelectedIndex = 0;
            if (cbClassroom.Items.Count > 0) cbClassroom.SelectedIndex = 0;
            if (cbResponsible.Items.Count > 0) cbResponsible.SelectedIndex = 0;
            cbState.SelectedIndex = 0;
            cbStatus.SelectedIndex = 0;
            chkDateFrom.Checked = chkDateTo.Checked = false;
            numPriceFrom.Value = 0;
            numPriceTo.Value = 0;
            FilterApplied?.Invoke(this, EventArgs.Empty);
        }
    }
}
