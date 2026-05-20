using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.Services;
using WindowsFormsApp1.UI;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1
{
    /// <summary>
    /// Управление правами доступа для каждой роли.
    /// </summary>
    public class RolePermissionsForm : UserControl
    {
        public event EventHandler PermissionsSaved;

        private ComboBox cbRole;
        private DataGridView grid;
        private Button btnSave;
        private Button btnReload;
        private Label lblHint;

        public RolePermissionsForm()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.Background;
            Padding = new Padding(12);
            BuildUi();
            LoadRoles();
        }

        private void BuildUi()
        {
            var header = new Label
            {
                Text = "Управление правами доступа",
                Dock = DockStyle.Top,
                Height = 32,
                Font = AppTheme.FontHeader,
                ForeColor = AppTheme.Primary
            };

            lblHint = new Label
            {
                Text = "Выберите роль и отметьте разрешённые действия. Изменения применяются ко всем пользователям с этой ролью.",
                Dock = DockStyle.Top,
                AutoSize = true,
                MaximumSize = new Size(2000, 0),
                Font = AppTheme.FontUi,
                ForeColor = AppTheme.TextSecondary,
                Padding = new Padding(0, 0, 0, 8)
            };

            var topBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = AppTheme.Background };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            flow.Controls.Add(new Label { Text = "Роль:", AutoSize = true, Margin = new Padding(0, 10, 6, 0), Font = AppTheme.FontUiBold });
            cbRole = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 6, 16, 0) };
            AppTheme.ApplyToComboBox(cbRole);
            cbRole.SelectedIndexChanged += (s, e) => LoadPermissionsGrid();
            flow.Controls.Add(cbRole);
            topBar.Controls.Add(flow);

            var actions = new ActionButtonRow();
            btnReload = actions.AddButton("Обновить");
            btnSave = actions.AddButton("Сохранить", true);
            btnReload.Click += (s, e) => LoadPermissionsGrid();
            btnSave.Click += BtnSave_Click;

            grid = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.ApplyToDataGridView(grid);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.Columns.Clear();

            Controls.Add(grid);
            Controls.Add(actions);
            Controls.Add(topBar);
            Controls.Add(lblHint);
            Controls.Add(header);
        }

        private void LoadRoles()
        {
            try
            {
                PermissionService.Instance.EnsureSchema();
                cbRole.Items.Clear();
                foreach (string role in PermissionService.GetKnownRoles())
                    cbRole.Items.Add(role);
                if (cbRole.Items.Count > 0)
                    cbRole.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ExceptionHandler.ShowError("Ошибка загрузки ролей", ex, FindForm());
            }
        }

        public void LoadPermissionsGrid()
        {
            if (cbRole.SelectedItem == null) return;
            try
            {
                string role = cbRole.SelectedItem.ToString();
                var data = PermissionService.Instance.GetPermissionsForRole(role);
                grid.DataSource = data;
                if (grid.Columns.Contains("PermissionCode"))
                    grid.Columns["PermissionCode"].Visible = false;
                if (grid.Columns.Contains("Разрешено"))
                {
                    grid.Columns["Разрешено"].ReadOnly = role == AuthorizationHelper.RoleAdmin;
                    if (role == AuthorizationHelper.RoleAdmin)
                        lblHint.Text = "Роль «Администратор» имеет полный доступ. Редактирование отключено.";
                    else
                        lblHint.Text = "Отметьте разрешённые действия для роли «Пользователь» и нажмите «Сохранить».";
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.ShowError("Ошибка загрузки прав", ex, FindForm());
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cbRole.SelectedItem == null) return;
            string role = cbRole.SelectedItem.ToString();
            if (role == AuthorizationHelper.RoleAdmin)
            {
                ExceptionHandler.ShowWarning("Права администратора изменять нельзя.", FindForm());
                return;
            }

            if (MessageBox.Show(FindForm(),
                $"Сохранить права для роли «{role}»?\nПользователи с этой ролью получат обновлённый доступ.",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                var granted = grid.Rows.Cast<DataGridViewRow>()
                    .Where(r => !r.IsNewRow && r.Cells["Разрешено"].Value is bool b && b)
                    .Select(r => r.Cells["PermissionCode"].Value?.ToString())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();

                PermissionService.Instance.SaveRolePermissions(role, granted);
                PermissionsSaved?.Invoke(this, EventArgs.Empty);
                MessageBox.Show(FindForm(), "Права доступа сохранены и применены.", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadPermissionsGrid();
            }
            catch (Exception ex)
            {
                ExceptionHandler.ShowError("Ошибка сохранения прав", ex, FindForm());
            }
        }
    }
}
