using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1.UI
{
    /// <summary>
    /// Единая тема интерфейса приложения.
    /// </summary>
    public static class AppTheme
    {
        public static readonly Color Background = Color.FromArgb(240, 243, 248);
        public static readonly Color Surface = Color.White;
        public static readonly Color Primary = Color.FromArgb(30, 58, 95);
        public static readonly Color PrimaryLight = Color.FromArgb(45, 85, 135);
        public static readonly Color Accent = Color.FromArgb(25, 118, 210);
        public static readonly Color AccentHover = Color.FromArgb(21, 101, 192);
        public static readonly Color TextPrimary = Color.FromArgb(33, 37, 41);
        public static readonly Color TextSecondary = Color.FromArgb(108, 117, 125);
        public static readonly Color Border = Color.FromArgb(206, 212, 218);
        public static readonly Color GridAltRow = Color.FromArgb(248, 249, 252);
        public static readonly Color Success = Color.FromArgb(40, 167, 69);
        public static readonly Color Warning = Color.FromArgb(255, 193, 7);
        public static readonly Color Danger = Color.FromArgb(220, 53, 69);
        public static readonly Color SubtotalRow = Color.FromArgb(227, 242, 253);
        public static readonly Color GrandTotalRow = Color.FromArgb(187, 222, 251);

        public static readonly Font FontUi = new Font("Segoe UI", 9.5F);
        public static readonly Font FontUiBold = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        public static readonly Font FontHeader = new Font("Segoe UI", 11F, FontStyle.Bold);
        public static readonly Font FontTitle = new Font("Segoe UI", 14F, FontStyle.Bold);

        public static void ApplyToForm(Form form)
        {
            form.BackColor = Background;
            form.Font = FontUi;
            form.ForeColor = TextPrimary;
        }

        public static void ApplyToPanel(Panel panel)
        {
            panel.BackColor = Surface;
            panel.Padding = new Padding(12);
        }

        public static void ApplyToButton(Button btn, bool primary = false)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = FontUiBold;
            btn.Cursor = Cursors.Hand;
            btn.Height = Math.Max(btn.Height, 34);
            btn.Padding = new Padding(8, 4, 8, 4);
            if (primary)
            {
                btn.BackColor = Accent;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.MouseOverBackColor = AccentHover;
            }
            else
            {
                btn.BackColor = Surface;
                btn.ForeColor = TextPrimary;
                btn.FlatAppearance.BorderColor = Border;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.MouseOverBackColor = GridAltRow;
            }
        }

        public static void ApplyToTabControl(TabControl tabs)
        {
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.SizeMode = TabSizeMode.Normal;
            tabs.ItemSize = new Size(0, 36);
            tabs.Padding = new Point(16, 6);
            tabs.Font = FontUiBold;
            tabs.DrawItem -= TabControl_DrawItem;
            tabs.DrawItem += TabControl_DrawItem;
        }

        private static void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tabs = (TabControl)sender;
            bool selected = e.Index == tabs.SelectedIndex;
            var rect = e.Bounds;
            using (var bg = new SolidBrush(selected ? Surface : Background))
                e.Graphics.FillRectangle(bg, rect);
            if (selected)
            {
                using (var accent = new Pen(Accent, 3))
                    e.Graphics.DrawLine(accent, rect.Left + 8, rect.Bottom - 2, rect.Right - 8, rect.Bottom - 2);
            }
            var text = tabs.TabPages[e.Index].Text;
            var color = selected ? Primary : TextSecondary;
            TextRenderer.DrawText(e.Graphics, text, tabs.Font, rect, color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        public static void ApplyToDataGridView(DataGridView grid)
        {
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Border;
            grid.RowHeadersVisible = false;
            grid.EnableHeadersVisualStyles = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.DefaultCellStyle.Font = FontUi;
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(207, 226, 255);
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.AlternatingRowsDefaultCellStyle.BackColor = GridAltRow;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Primary;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = FontUiBold;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.ColumnHeadersHeight = 38;
            grid.RowTemplate.Height = 32;
        }

        public static void ApplyToComboBox(ComboBox cb)
        {
            cb.FlatStyle = FlatStyle.Flat;
            cb.Font = FontUi;
            cb.BackColor = Surface;
        }

        public static void ApplyToTextBox(TextBox tb)
        {
            tb.Font = FontUi;
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.BackColor = Surface;
        }

        public static void ApplyToLabel(Label lbl, bool secondary = false)
        {
            lbl.Font = secondary ? FontUi : FontUiBold;
            lbl.ForeColor = secondary ? TextSecondary : TextPrimary;
        }

        public static void StyleSubtotalGridRows(DataGridView grid, string markerColumn = "Тип строки")
        {
            if (!grid.Columns.Contains(markerColumn)) return;
            foreach (DataGridViewRow row in grid.Rows)
            {
                var val = row.Cells[markerColumn].Value?.ToString() ?? "";
                if (val == "Итого по группе")
                    row.DefaultCellStyle.BackColor = SubtotalRow;
                else if (val == "ИТОГО")
                    row.DefaultCellStyle.BackColor = GrandTotalRow;
            }
        }
    }
}
