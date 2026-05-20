using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1.UI
{
    /// <summary>
    /// Выбор периода для отчётов и статистики.
    /// </summary>
    public class PeriodFilterPanel : UserControl
    {
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;

        public event EventHandler PeriodChanged;

        public PeriodFilterPanel()
        {
            Dock = DockStyle.Top;
            Height = 52;
            MinimumSize = new Size(400, 52);
            BackColor = AppTheme.Surface;
            Padding = new Padding(10, 8, 10, 8);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true
            };

            var lbl = new Label
            {
                Text = "Период:",
                AutoSize = true,
                Margin = new Padding(0, 8, 8, 0),
                Font = AppTheme.FontUiBold,
                ForeColor = AppTheme.Primary
            };

            dtpFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 110,
                Margin = new Padding(0, 4, 4, 0),
                MinDate = new DateTime(1990, 1, 1)
            };
            var lblTo = new Label { Text = "—", AutoSize = true, Margin = new Padding(0, 8, 4, 0) };
            dtpTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 110,
                Margin = new Padding(0, 4, 12, 0),
                MinDate = new DateTime(1990, 1, 1)
            };

            SetCurrentMonth();

            dtpFrom.ValueChanged += OnPeriodControlChanged;
            dtpTo.ValueChanged += OnPeriodControlChanged;

            flow.Controls.Add(lbl);
            flow.Controls.Add(dtpFrom);
            flow.Controls.Add(lblTo);
            flow.Controls.Add(dtpTo);
            flow.Controls.Add(MakePresetButton("Месяц", () => SetCurrentMonth()));
            flow.Controls.Add(MakePresetButton("Квартал", () => SetCurrentQuarter()));
            flow.Controls.Add(MakePresetButton("Год", () => SetCurrentYear()));

            Controls.Add(flow);
        }

        public DateTime DateFrom => dtpFrom.Value.Date;

        public DateTime DateTo => dtpTo.Value.Date;

        public void ApplyToCriteria(Models.InventoryFilterCriteria criteria)
        {
            if (criteria == null) return;
            criteria.DateFrom = DateFrom;
            criteria.DateTo = DateTo;
        }

        public string GetPeriodDescription()
        {
            return $"{DateFrom:dd.MM.yyyy} — {DateTo:dd.MM.yyyy}";
        }

        private void OnPeriodControlChanged(object sender, EventArgs e)
        {
            if (dtpFrom.Value.Date > dtpTo.Value.Date)
                dtpTo.Value = dtpFrom.Value;
            PeriodChanged?.Invoke(this, EventArgs.Empty);
        }

        private Button MakePresetButton(string text, Action apply)
        {
            var btn = new Button { Text = text, AutoSize = true, Margin = new Padding(0, 4, 6, 0) };
            AppTheme.ApplyToButton(btn);
            btn.Click += (s, e) =>
            {
                apply();
                PeriodChanged?.Invoke(this, EventArgs.Empty);
            };
            return btn;
        }

        public void SetCurrentMonth()
        {
            var today = DateTime.Today;
            dtpFrom.Value = new DateTime(today.Year, today.Month, 1);
            dtpTo.Value = today;
        }

        public void SetCurrentQuarter()
        {
            var today = DateTime.Today;
            int q = (today.Month - 1) / 3;
            dtpFrom.Value = new DateTime(today.Year, q * 3 + 1, 1);
            dtpTo.Value = today;
        }

        public void SetCurrentYear()
        {
            var today = DateTime.Today;
            dtpFrom.Value = new DateTime(today.Year, 1, 1);
            dtpTo.Value = today;
        }
    }
}
