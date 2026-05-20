using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.UI;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1
{
    public partial class UserManagementForm : UserControl
    {
        private UserManager userManager;
        private DataGridView gridUsers;
        private Button btnAddUser;
        private Button btnEditUser;
        private Button btnDeactivateUser;
        private Button btnResetPassword;
        private DataTable usersData;

        public UserManagementForm()
        {
            userManager = new UserManager();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Dock = DockStyle.Fill;
            BackColor = AppTheme.Background;
            Padding = new Padding(12);

            var header = new Label
            {
                Text = "Управление пользователями",
                Dock = DockStyle.Top,
                Height = 32,
                Font = AppTheme.FontHeader,
                ForeColor = AppTheme.Primary
            };

            var toolbar = new ToolbarPanel();
            btnAddUser = new Button { Text = "Добавить" };
            btnEditUser = new Button { Text = "Изменить" };
            btnDeactivateUser = new Button { Text = "Деактивировать" };
            btnResetPassword = new Button { Text = "Сбросить пароль" };
            btnAddUser.Click += BtnAddUser_Click;
            btnEditUser.Click += BtnEditUser_Click;
            btnDeactivateUser.Click += BtnDeactivateUser_Click;
            btnResetPassword.Click += BtnResetPassword_Click;
            toolbar.AddButton(btnAddUser, true);
            toolbar.AddButton(btnEditUser);
            toolbar.AddButton(btnDeactivateUser);
            toolbar.AddButton(btnResetPassword);

            gridUsers = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.ApplyToDataGridView(gridUsers);

            Controls.Add(gridUsers);
            Controls.Add(toolbar);
            Controls.Add(header);
            ResumeLayout(false);
        }

        public void LoadUsers()
        {
            try
            {
                Logger.Info("Загрузка списка пользователей");
                usersData = userManager.GetAllUsers();
                gridUsers.DataSource = usersData;
                GridHelper.LocalizeUsersGrid(gridUsers);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки пользователей", ex);
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int? GetSelectedUserId()
        {
            if (gridUsers.CurrentRow == null || gridUsers.CurrentRow.Index < 0)
            {
                MessageBox.Show("Выберите пользователя в таблице.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            return Convert.ToInt32(gridUsers.CurrentRow.Cells["UserID"].Value);
        }

        private void BtnAddUser_Click(object sender, EventArgs e)
        {
            using (var dialog = new UserEditDialog(userManager, null))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                    LoadUsers();
            }
        }

        private void BtnEditUser_Click(object sender, EventArgs e)
        {
            int? userId = GetSelectedUserId();
            if (userId == null) return;
            using (var dialog = new UserEditDialog(userManager, userId))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                    LoadUsers();
            }
        }

        private void BtnDeactivateUser_Click(object sender, EventArgs e)
        {
            int? userId = GetSelectedUserId();
            if (userId == null) return;
            string username = gridUsers.CurrentRow.Cells["Username"].Value?.ToString();
            if (username == "admin")
            {
                MessageBox.Show("Невозможно деактивировать администратора.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show($"Деактивировать пользователя «{username}»?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    userManager.DeactivateUser(userId.Value);
                    LoadUsers();
                    MessageBox.Show("Пользователь деактивирован.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnResetPassword_Click(object sender, EventArgs e)
        {
            int? userId = GetSelectedUserId();
            if (userId == null) return;
            string username = gridUsers.CurrentRow.Cells["Username"].Value?.ToString();
            if (MessageBox.Show($"Сбросить пароль пользователя «{username}» на «123456»?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    userManager.ResetPassword(userId.Value, "123456");
                    MessageBox.Show("Пароль сброшен на «123456».", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    public class UserEditDialog : Form
    {
        private readonly UserManager userManager;
        private readonly int? userId;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtFullName;
        private ComboBox cbRole;
        private CheckBox chkActive;

        public UserEditDialog(UserManager userManager, int? userId)
        {
            this.userManager = userManager;
            this.userId = userId;
            InitializeComponents();
            if (userId.HasValue) LoadUser();
        }

        private void InitializeComponents()
        {
            Text = userId.HasValue ? "Редактирование пользователя" : "Добавление пользователя";
            Size = new Size(450, 340);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            AppTheme.ApplyToForm(this);

            int y = 20;
            void AddRow(string label, Control input)
            {
                Controls.Add(new Label { Text = label, Location = new Point(20, y), Width = 120, Font = AppTheme.FontUi });
                input.Location = new Point(150, y - 3);
                input.Width = 260;
                Controls.Add(input);
                y += 40;
            }

            txtUsername = new TextBox();
            AppTheme.ApplyToTextBox(txtUsername);
            AddRow("Логин:", txtUsername);

            txtPassword = new TextBox { PasswordChar = '●' };
            AppTheme.ApplyToTextBox(txtPassword);
            AddRow("Пароль:", txtPassword);

            txtFullName = new TextBox();
            AppTheme.ApplyToTextBox(txtFullName);
            AddRow("Полное имя:", txtFullName);

            cbRole = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AppTheme.ApplyToComboBox(cbRole);
            cbRole.Items.AddRange(new[] { "Admin", "User" });
            cbRole.SelectedIndex = 1;
            AddRow("Роль:", cbRole);

            chkActive = new CheckBox { Text = "Активен", Location = new Point(150, y), Checked = true, Font = AppTheme.FontUi };
            Controls.Add(chkActive);
            y += 44;

            var btnSave = new Button { Text = "Сохранить", Location = new Point(150, y), Size = new Size(120, 34) };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(280, y), Size = new Size(100, 34), DialogResult = DialogResult.Cancel };
            AppTheme.ApplyToButton(btnSave, true);
            AppTheme.ApplyToButton(btnCancel);
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void LoadUser()
        {
            DataTable dt = userManager.GetUserById(userId.Value);
            if (dt.Rows.Count == 0) return;
            DataRow row = dt.Rows[0];
            txtUsername.Text = row["Username"].ToString();
            txtUsername.ReadOnly = true;
            txtFullName.Text = row["FullName"].ToString();
            cbRole.SelectedItem = row["Role"].ToString();
            chkActive.Checked = Convert.ToBoolean(row["IsActive"]);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Заполните все обязательные поля.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!userId.HasValue && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Введите пароль для нового пользователя.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                if (userId.HasValue)
                {
                    userManager.UpdateUser(userId.Value, txtFullName.Text, cbRole.SelectedItem.ToString(), chkActive.Checked);
                    if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                        userManager.ResetPassword(userId.Value, txtPassword.Text);
                }
                else
                    userManager.CreateUser(txtUsername.Text, txtPassword.Text, txtFullName.Text, cbRole.SelectedItem.ToString());
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
