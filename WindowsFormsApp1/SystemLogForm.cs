using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.UI;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1
{
    public class SystemLogForm : UserControl
    {
        private TextBox txtLog;
        private ComboBox cbLevel;

        public SystemLogForm()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.Background;
            Padding = new Padding(12);
            BuildUi();
        }

        private void BuildUi()
        {
            var header = new Label
            {
                Text = "Журнал системы",
                Dock = DockStyle.Top,
                Height = 32,
                Font = AppTheme.FontHeader,
                ForeColor = AppTheme.Primary
            };

            var toolbar = new ToolbarPanel();
            cbLevel = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 140,
                Margin = new Padding(0, 4, 8, 0)
            };
            cbLevel.Items.AddRange(new object[] { "Все", "INFO", "WARNING", "ERROR", "DEBUG" });
            cbLevel.SelectedIndex = 0;
            cbLevel.SelectedIndexChanged += (s, e) => LoadLog();

            var btnRefresh = new Button { Text = "Обновить" };
            var btnOpen = new Button { Text = "Открыть папку журналов" };
            var btnClear = new Button { Text = "Очистить старые (30 дн.)" };
            btnRefresh.Click += (s, e) => LoadLog();
            btnOpen.Click += (s, e) => Logger.OpenLogFolder();
            btnClear.Click += (s, e) =>
            {
                Logger.CleanOldLogs(30);
                LoadLog();
                MessageBox.Show("Старые файлы журнала удалены.", "Журнал", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            toolbar.AddControl(new Label { Text = "Уровень:", AutoSize = true, Margin = new Padding(0, 10, 4, 0) });
            toolbar.AddControl(cbLevel);
            toolbar.AddButton(btnRefresh, true);
            toolbar.AddButton(btnOpen);
            toolbar.AddButton(btnClear);

            txtLog = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 9f),
                BackColor = AppTheme.Surface,
                ForeColor = AppTheme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                WordWrap = false
            };

            Controls.Add(txtLog);
            Controls.Add(toolbar);
            Controls.Add(header);
        }

        public void LoadLog()
        {
            string level = cbLevel.SelectedIndex <= 0 ? null : cbLevel.SelectedItem?.ToString();
            txtLog.Text = Logger.ReadRecentEntries(500, level);
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }
    }
}
