using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1.UI
{
    /// <summary>
    /// Ряд кнопок в TableLayoutPanel — не обрезается, равная ширина колонок.
    /// </summary>
    public class ActionButtonRow : Panel
    {
        public ActionButtonRow()
        {
            Dock = DockStyle.Top;
            Height = 46;
            MinimumSize = new Size(400, 46);
            BackColor = AppTheme.Background;
            Padding = new Padding(0, 4, 0, 4);
        }

        public Button AddButton(string text, bool primary = false)
        {
            var grid = Controls.Count == 0 ? CreateGrid() : (TableLayoutPanel)Controls[0];
            int col = grid.ColumnCount;
            grid.ColumnCount = col + 1;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / grid.ColumnCount));
            RecalculateColumnWidths(grid);

            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(col == 0 ? 0 : 4, 0, 0, 0),
                MinimumSize = new Size(80, 36)
            };
            AppTheme.ApplyToButton(btn, primary);
            grid.Controls.Add(btn, col, 0);
            return btn;
        }

        private TableLayoutPanel CreateGrid()
        {
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 0,
                RowCount = 1,
                Margin = new Padding(0)
            };
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(grid);
            return grid;
        }

        private static void RecalculateColumnWidths(TableLayoutPanel grid)
        {
            float pct = 100f / grid.ColumnCount;
            grid.ColumnStyles.Clear();
            for (int i = 0; i < grid.ColumnCount; i++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, pct));
        }
    }
}
