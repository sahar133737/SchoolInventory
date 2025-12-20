using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
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
        private Panel panelUsers;
        private DataTable usersData;

        public UserManagementForm()
        {
            userManager = new UserManager();
            InitializeComponent();
            ApplyNeomorphicStyle();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Panel
            panelUsers = new Panel();
            panelUsers.Dock = DockStyle.Fill;
            panelUsers.Padding = new Padding(20);
            panelUsers.BackColor = NeomorphicStyle.BackgroundColor;

            // Заголовок
            var lblTitle = new Label();
            lblTitle.Text = "👥 Управление пользователями";
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = NeomorphicStyle.AccentColor;
            lblTitle.Location = new Point(20, 10);
            lblTitle.AutoSize = true;

            // Кнопки
            btnAddUser = new Button();
            btnAddUser.Text = "➕ Добавить";
            btnAddUser.Location = new Point(20, 50);
            btnAddUser.Size = new Size(150, 40);
            btnAddUser.Click += BtnAddUser_Click;

            btnEditUser = new Button();
            btnEditUser.Text = "✏️ Изменить";
            btnEditUser.Location = new Point(180, 50);
            btnEditUser.Size = new Size(150, 40);
            btnEditUser.Click += BtnEditUser_Click;

            btnDeactivateUser = new Button();
            btnDeactivateUser.Text = "🚫 Деактивировать";
            btnDeactivateUser.Location = new Point(340, 50);
            btnDeactivateUser.Size = new Size(170, 40);
            btnDeactivateUser.Click += BtnDeactivateUser_Click;

            btnResetPassword = new Button();
            btnResetPassword.Text = "🔑 Сбросить пароль";
            btnResetPassword.Location = new Point(520, 50);
            btnResetPassword.Size = new Size(170, 40);
            btnResetPassword.Click += BtnResetPassword_Click;

            // DataGridView
            gridUsers = new DataGridView();
            gridUsers.Location = new Point(20, 100);
            gridUsers.Size = new Size(800, 400);
            gridUsers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridUsers.AllowUserToAddRows = false;
            gridUsers.AllowUserToDeleteRows = false;
            gridUsers.ReadOnly = true;
            gridUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridUsers.BackgroundColor = NeomorphicStyle.SurfaceColor;
            gridUsers.GridColor = NeomorphicStyle.DarkShadow;
            gridUsers.DefaultCellStyle.BackColor = NeomorphicStyle.SurfaceColor;
            gridUsers.DefaultCellStyle.ForeColor = NeomorphicStyle.TextColor;
            gridUsers.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            gridUsers.DefaultCellStyle.SelectionBackColor = NeomorphicStyle.AccentColor;
            gridUsers.DefaultCellStyle.SelectionForeColor = Color.White;
            gridUsers.ColumnHeadersDefaultCellStyle.BackColor = NeomorphicStyle.SurfaceColor;
            gridUsers.ColumnHeadersDefaultCellStyle.ForeColor = NeomorphicStyle.TextColor;
            gridUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            gridUsers.EnableHeadersVisualStyles = false;
            gridUsers.RowHeadersVisible = false;
            gridUsers.BorderStyle = BorderStyle.None;

            panelUsers.Controls.Add(lblTitle);
            panelUsers.Controls.Add(btnAddUser);
            panelUsers.Controls.Add(btnEditUser);
            panelUsers.Controls.Add(btnDeactivateUser);
            panelUsers.Controls.Add(btnResetPassword);
            panelUsers.Controls.Add(gridUsers);

            this.Controls.Add(panelUsers);
            this.Size = new Size(860, 520);
            this.BackColor = NeomorphicStyle.BackgroundColor;

            this.ResumeLayout(false);
        }

        private void ApplyNeomorphicStyle()
        {
            ApplyButtonStyle(btnAddUser);
            ApplyButtonStyle(btnEditUser);
            ApplyButtonStyle(btnDeactivateUser);
            ApplyButtonStyle(btnResetPassword);
        }

        private void ApplyButtonStyle(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = NeomorphicStyle.SurfaceColor;
            button.ForeColor = NeomorphicStyle.TextColor;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.Paint += (s, e) => DrawNeomorphicButton(e.Graphics, button);
        }

        private void DrawNeomorphicButton(Graphics g, Button button)
        {
            Rectangle rect = button.ClientRectangle;
            bool isPressed = button.ClientRectangle.Contains(button.PointToClient(Control.MousePosition)) &&
                            Control.MouseButtons == MouseButtons.Left;

            using (SolidBrush brush = new SolidBrush(NeomorphicStyle.SurfaceColor))
            {
                g.FillRoundedRectangle(brush, rect, 12);
            }

            if (!isPressed)
            {
                Rectangle lightRect = new Rectangle(rect.X - 3, rect.Y - 3, rect.Width, rect.Height);
                Rectangle darkRect = new Rectangle(rect.X + 3, rect.Y + 3, rect.Width, rect.Height);

                using (GraphicsPath lightPath = NeomorphicStyle.CreateRoundedRectangle(lightRect, 12))
                using (GraphicsPath darkPath = NeomorphicStyle.CreateRoundedRectangle(darkRect, 12))
                {
                    using (Pen lightPen = new Pen(NeomorphicStyle.LightShadow, 5))
                    using (Pen darkPen = new Pen(NeomorphicStyle.DarkShadow, 5))
                    {
                        lightPen.LineJoin = LineJoin.Round;
                        darkPen.LineJoin = LineJoin.Round;
                        g.DrawPath(lightPen, lightPath);
                        g.DrawPath(darkPen, darkPath);
                    }
                }
            }

            using (SolidBrush textBrush = new SolidBrush(NeomorphicStyle.TextColor))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(button.Text, button.Font, textBrush, rect, sf);
            }
        }

        public void LoadUsers()
        {
            try
            {
                Logger.Info("Загрузка списка пользователей");
                usersData = userManager.GetAllUsers();
                gridUsers.DataSource = usersData;
                ConfigureGridColumns();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки пользователей", ex);
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridColumns()
        {
            if (gridUsers.Columns.Contains("UserID"))
                gridUsers.Columns["UserID"].Visible = false;
            if (gridUsers.Columns.Contains("Password"))
                gridUsers.Columns["Password"].Visible = false;
            if (gridUsers.Columns.Contains("Username"))
                gridUsers.Columns["Username"].HeaderText = "Логин";
            if (gridUsers.Columns.Contains("FullName"))
                gridUsers.Columns["FullName"].HeaderText = "Полное имя";
            if (gridUsers.Columns.Contains("Role"))
                gridUsers.Columns["Role"].HeaderText = "Роль";
            if (gridUsers.Columns.Contains("CreatedDate"))
            {
                gridUsers.Columns["CreatedDate"].HeaderText = "Дата создания";
                gridUsers.Columns["CreatedDate"].DefaultCellStyle.Format = "d";
            }
            if (gridUsers.Columns.Contains("IsActive"))
                gridUsers.Columns["IsActive"].HeaderText = "Активен";
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
                {
                    Logger.Info("Добавлен новый пользователь");
                    LoadUsers();
                }
            }
        }

        private void BtnEditUser_Click(object sender, EventArgs e)
        {
            int? userId = GetSelectedUserId();
            if (userId == null) return;

            using (var dialog = new UserEditDialog(userManager, userId))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Logger.Info($"Обновлен пользователь с ID: {userId}");
                    LoadUsers();
                }
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

            if (MessageBox.Show($"Деактивировать пользователя {username}?", "Подтверждение", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    userManager.DeactivateUser(userId.Value);
                    Logger.Info($"Деактивирован пользователь: {username}");
                    LoadUsers();
                    MessageBox.Show("Пользователь деактивирован.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Ошибка деактивации пользователя {username}", ex);
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnResetPassword_Click(object sender, EventArgs e)
        {
            int? userId = GetSelectedUserId();
            if (userId == null) return;

            string username = gridUsers.CurrentRow.Cells["Username"].Value?.ToString();
            
            if (MessageBox.Show($"Сбросить пароль пользователя {username} на '123456'?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    userManager.ResetPassword(userId.Value, "123456");
                    Logger.Info($"Сброшен пароль пользователя: {username}");
                    MessageBox.Show("Пароль сброшен на '123456'.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Ошибка сброса пароля пользователя {username}", ex);
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    // Диалог редактирования пользователя
    public class UserEditDialog : Form
    {
        private UserManager userManager;
        private int? userId;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtFullName;
        private ComboBox cbRole;
        private CheckBox chkActive;
        private Button btnSave;
        private Button btnCancel;

        public UserEditDialog(UserManager userManager, int? userId)
        {
            this.userManager = userManager;
            this.userId = userId;
            InitializeComponents();
            if (userId.HasValue)
                LoadUser();
        }

        private void InitializeComponents()
        {
            this.Text = userId.HasValue ? "Редактирование пользователя" : "Добавление пользователя";
            this.Size = new Size(450, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = NeomorphicStyle.BackgroundColor;

            int y = 20;
            int labelWidth = 120;
            int inputWidth = 280;

            var lblUsername = new Label { Text = "Логин:", Location = new Point(20, y), Width = labelWidth, ForeColor = NeomorphicStyle.TextColor };
            txtUsername = new TextBox { Location = new Point(150, y - 3), Width = inputWidth, BackColor = NeomorphicStyle.SurfaceColor, ForeColor = NeomorphicStyle.TextColor };
            y += 40;

            var lblPassword = new Label { Text = "Пароль:", Location = new Point(20, y), Width = labelWidth, ForeColor = NeomorphicStyle.TextColor };
            txtPassword = new TextBox { Location = new Point(150, y - 3), Width = inputWidth, PasswordChar = '●', BackColor = NeomorphicStyle.SurfaceColor, ForeColor = NeomorphicStyle.TextColor };
            y += 40;

            var lblFullName = new Label { Text = "Полное имя:", Location = new Point(20, y), Width = labelWidth, ForeColor = NeomorphicStyle.TextColor };
            txtFullName = new TextBox { Location = new Point(150, y - 3), Width = inputWidth, BackColor = NeomorphicStyle.SurfaceColor, ForeColor = NeomorphicStyle.TextColor };
            y += 40;

            var lblRole = new Label { Text = "Роль:", Location = new Point(20, y), Width = labelWidth, ForeColor = NeomorphicStyle.TextColor };
            cbRole = new ComboBox { Location = new Point(150, y - 3), Width = inputWidth, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = NeomorphicStyle.SurfaceColor, ForeColor = NeomorphicStyle.TextColor };
            cbRole.Items.AddRange(new string[] { "Admin", "User" });
            cbRole.SelectedIndex = 1;
            y += 40;

            chkActive = new CheckBox { Text = "Активен", Location = new Point(150, y), Checked = true, ForeColor = NeomorphicStyle.TextColor };
            y += 50;

            btnSave = new Button { Text = "Сохранить", Location = new Point(150, y), Size = new Size(120, 35), BackColor = NeomorphicStyle.AccentColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button { Text = "Отмена", Location = new Point(280, y), Size = new Size(100, 35), DialogResult = DialogResult.Cancel, BackColor = NeomorphicStyle.SurfaceColor, ForeColor = NeomorphicStyle.TextColor, FlatStyle = FlatStyle.Flat };
            btnCancel.FlatAppearance.BorderSize = 0;

            this.Controls.AddRange(new Control[] { lblUsername, txtUsername, lblPassword, txtPassword, lblFullName, txtFullName, lblRole, cbRole, chkActive, btnSave, btnCancel });
        }

        private void LoadUser()
        {
            DataTable dt = userManager.GetUserById(userId.Value);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                txtUsername.Text = row["Username"].ToString();
                txtUsername.ReadOnly = true;
                txtFullName.Text = row["FullName"].ToString();
                cbRole.SelectedItem = row["Role"].ToString();
                chkActive.Checked = Convert.ToBoolean(row["IsActive"]);
            }
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
                    {
                        userManager.ResetPassword(userId.Value, txtPassword.Text);
                    }
                }
                else
                {
                    userManager.CreateUser(txtUsername.Text, txtPassword.Text, txtFullName.Text, cbRole.SelectedItem.ToString());
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

