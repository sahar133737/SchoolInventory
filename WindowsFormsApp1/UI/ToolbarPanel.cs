using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1.UI
{
    /// <summary>
    /// Панель кнопок с переносом на всю ширину вкладки.
    /// </summary>
    public class ToolbarPanel : Panel
    {
        private readonly FlowLayoutPanel _flow;

        public ToolbarPanel()
        {
            Dock = DockStyle.Top;
            AutoSize = false;
            BackColor = AppTheme.Surface;
            Padding = new Padding(6, 6, 6, 6);
            MinimumSize = new Size(200, 44);

            _flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0)
            };
            Controls.Add(_flow);
            Resize += ToolbarPanel_Resize;
        }

        private void ToolbarPanel_Resize(object sender, System.EventArgs e) => UpdateHeight();

        private void UpdateHeight()
        {
            int w = System.Math.Max(100, ClientSize.Width - Padding.Horizontal);
            var preferred = _flow.GetPreferredSize(new Size(w, 0));
            _flow.Height = preferred.Height;
            Height = preferred.Height + Padding.Vertical;
        }

        public void AddButton(Button button, bool primary = false)
        {
            button.AutoSize = false;
            int w = TextRenderer.MeasureText(button.Text, button.Font).Width + 32;
            button.Size = new Size(System.Math.Max(110, w), 34);
            button.Margin = new Padding(0, 0, 8, 6);
            AppTheme.ApplyToButton(button, primary);
            _flow.Controls.Add(button);
            UpdateHeight();
        }

        public void AddControl(Control control)
        {
            control.Margin = new Padding(0, 0, 8, 6);
            _flow.Controls.Add(control);
            UpdateHeight();
        }
    }
}
