using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using WindowsFormsApp1.UI;

namespace WindowsFormsApp1
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnCancel;
        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;
        private Panel panelMain;
        private Panel headerPanel;
        private Label headerTitle;
        private UserManager userManager;

        public string LoggedInUsername { get; private set; }
        public string LoggedInFullName { get; private set; }
        public string LoggedInRole { get; private set; }

        public LoginForm()
        {
            userManager = new UserManager();
            InitializeComponent();
            this.DoubleBuffered = true;
            // Центрирование не требуется — используем Dock и таблицы
        }

        // Убрано ручное центрирование — всё на Dock/таблицах

        private void InitializeComponent()
        {
            this.headerPanel = new Panel();
            this.headerTitle = new Label();
            this.panelMain = new Panel();
            this.lblTitle = new Label();
            this.lblUsername = new Label();
            this.lblPassword = new Label();
            this.txtUsername = new TextBox();
            this.txtPassword = new TextBox();
            this.btnLogin = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();

            // headerPanel (верхняя панель с заголовком и крестиком)
            this.headerPanel.Dock = DockStyle.Top;
            this.headerPanel.Height = 48;
            this.headerPanel.BackColor = NeomorphicStyle.SurfaceColor;
            this.headerPanel.Padding = new Padding(12, 6, 48, 6); // справа место под крестик
            this.headerPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (SolidBrush brush = new SolidBrush(NeomorphicStyle.SurfaceColor))
                {
                    e.Graphics.FillRectangle(brush, this.headerPanel.ClientRectangle);
                }
            };
            this.headerPanel.MouseDown += HeaderPanel_MouseDown;
            this.headerPanel.MouseMove += HeaderPanel_MouseMove;
            this.headerPanel.MouseUp += HeaderPanel_MouseUp;

            // headerTitle
            this.headerTitle.Dock = DockStyle.Fill;
            this.headerTitle.Text = "Школьная инвентаризация";
            this.headerTitle.TextAlign = ContentAlignment.MiddleLeft;
            this.headerTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.headerTitle.ForeColor = NeomorphicStyle.TextColor;
            this.headerPanel.Controls.Add(this.headerTitle);

            // Кнопка закрытия в хедере
            btnClose = new Button();
            btnClose.Text = "×";
            btnClose.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            btnClose.Size = new Size(36, 36);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.BackColor = Color.Transparent;
            btnClose.ForeColor = NeomorphicStyle.TextColor;
            btnClose.Cursor = Cursors.Hand;
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Location = new Point(this.headerPanel.Width - 42, 6);
            btnClose.Click += (s, e) => this.Close();
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.Red;
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = NeomorphicStyle.TextColor;
            this.headerPanel.Controls.Add(btnClose);
            this.headerPanel.Resize += (s, e) => { btnClose.Location = new Point(this.headerPanel.Width - 42, 6); };

            // panelMain
            this.panelMain.BackColor = NeomorphicStyle.SurfaceColor;
            this.panelMain.Dock = DockStyle.Fill;
            this.panelMain.Padding = new Padding(20);
            this.panelMain.Paint += PanelMain_PaintNeomorphic;

            // Табличный layout для аккуратной вёрстки
            var tlp = new TableLayoutPanel();
            tlp.ColumnCount = 1;
            tlp.RowCount = 8;
            tlp.Dock = DockStyle.Fill;
            tlp.BackColor = Color.Transparent;
            tlp.Padding = new Padding(24, 20, 24, 20);
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // Title
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 12F));  // spacer
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));  // username label
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));  // username input
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));  // password label
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));  // password input
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // flexible spacer
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));  // buttons area (fixed, больше высоты)

            // lblTitle
            this.lblTitle.AutoSize = false;
            this.lblTitle.Dock = DockStyle.Fill;
            this.lblTitle.Font = new Font("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.ForeColor = NeomorphicStyle.AccentColor;
            this.lblTitle.Text = "Авторизация";
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.lblTitle.BackColor = Color.Transparent;

            // lblUsername
            this.lblUsername.AutoSize = false;
            this.lblUsername.Dock = DockStyle.Fill;
            this.lblUsername.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblUsername.ForeColor = NeomorphicStyle.TextColor;
            this.lblUsername.Text = "Имя пользователя:";
            this.lblUsername.TextAlign = ContentAlignment.MiddleLeft;
            this.lblUsername.BackColor = Color.Transparent;

            // txtUsername
            this.txtUsername = new TextBox();
            CreateNeomorphicTextBox(this.txtUsername, out this.txtUsernamePanel, 0, 0, 420, 32);
            this.txtUsernamePanel.Dock = DockStyle.Fill;
            this.txtUsername.TabIndex = 0;
            this.txtUsername.KeyDown += TxtPassword_KeyDown;

            // lblPassword
            this.lblPassword.AutoSize = false;
            this.lblPassword.Dock = DockStyle.Fill;
            this.lblPassword.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblPassword.ForeColor = NeomorphicStyle.TextColor;
            this.lblPassword.Text = "Пароль:";
            this.lblPassword.TextAlign = ContentAlignment.MiddleLeft;
            this.lblPassword.BackColor = Color.Transparent;

            // txtPassword
            this.txtPassword = new TextBox();
            CreateNeomorphicTextBox(this.txtPassword, out this.txtPasswordPanel, 0, 0, 420, 32);
            this.txtPassword.PasswordChar = '●';
            this.txtPasswordPanel.Dock = DockStyle.Fill;
            this.txtPassword.TabIndex = 1;
            this.txtPassword.KeyDown += TxtPassword_KeyDown;

            // Кнопки внизу (TableLayout для стабильного центрирования)
            var buttonsLayout = new TableLayoutPanel();
            buttonsLayout.Dock = DockStyle.Fill;
            buttonsLayout.RowCount = 1;
            buttonsLayout.ColumnCount = 5;
            buttonsLayout.BackColor = Color.Transparent;
            buttonsLayout.Padding = new Padding(0, 8, 0, 8);
            buttonsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); // левый отступ
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // Войти
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 16F)); // спейсер
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // Отмена
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); // правый отступ

            // btnLogin
            this.btnLogin.BackColor = NeomorphicStyle.SurfaceColor;
            this.btnLogin.FlatStyle = FlatStyle.Flat;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
            this.btnLogin.ForeColor = NeomorphicStyle.AccentColor;
            this.btnLogin.Size = new Size(200, 46);
            this.btnLogin.TabIndex = 2;
            this.btnLogin.Text = "Войти";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Paint += BtnLogin_Paint;
            this.btnLogin.MouseDown += BtnLogin_MouseDown;
            this.btnLogin.MouseUp += BtnLogin_MouseUp;
            this.btnLogin.Click += BtnLogin_Click;

            // btnCancel
            this.btnCancel.BackColor = NeomorphicStyle.SurfaceColor;
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            this.btnCancel.ForeColor = NeomorphicStyle.TextColor;
            this.btnCancel.Size = new Size(200, 46);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Paint += BtnCancel_Paint;
            this.btnCancel.MouseDown += BtnCancel_MouseDown;
            this.btnCancel.MouseUp += BtnCancel_MouseUp;
            this.btnCancel.Click += BtnCancel_Click;

            // Добавляем кнопки в таблицу
            buttonsLayout.Controls.Add(new Panel() { Dock = DockStyle.Fill }, 0, 0);
            buttonsLayout.Controls.Add(this.btnLogin, 1, 0);
            buttonsLayout.Controls.Add(new Panel() { Width = 16, Dock = DockStyle.Fill }, 2, 0);
            buttonsLayout.Controls.Add(this.btnCancel, 3, 0);
            buttonsLayout.Controls.Add(new Panel() { Dock = DockStyle.Fill }, 4, 0);

            // Добавляем элементы в таблицу
            tlp.Controls.Add(this.lblTitle, 0, 0);
            tlp.SetColumnSpan(this.lblTitle, 1);
            tlp.Controls.Add(new Panel { Height = 1, Dock = DockStyle.Fill, BackColor = Color.Transparent }, 0, 1);
            tlp.Controls.Add(this.lblUsername, 0, 2);
            tlp.Controls.Add(this.txtUsernamePanel, 0, 3);
            tlp.Controls.Add(this.lblPassword, 0, 4);
            tlp.Controls.Add(this.txtPasswordPanel, 0, 5);
            tlp.Controls.Add(buttonsLayout, 0, 7);

            // panelMain
            this.panelMain.Controls.Clear();
            this.panelMain.Controls.Add(tlp);

            // LoginForm
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = NeomorphicStyle.BackgroundColor;
            this.ClientSize = new Size(560, 500);
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Авторизация - Школьная инвентаризация";
            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;
            this.KeyPreview = true;
            this.KeyDown += LoginForm_KeyDown;
            this.Paint += LoginForm_Paint;
            // Верхняя панель и контент
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.headerPanel);
            this.ResumeLayout(false);
            AppTheme.ApplyToForm(this);
            AppTheme.ApplyToButton(btnLogin, true);
            AppTheme.ApplyToButton(btnCancel);
        }

        private Button btnClose;
        private bool isDragging = false;
        private Point lastMousePosition;

        private void HeaderPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastMousePosition = e.Location;
            }
        }

        private void HeaderPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                this.Location = new Point(this.Location.X + e.X - lastMousePosition.X,
                                         this.Location.Y + e.Y - lastMousePosition.Y);
            }
        }

        private void HeaderPanel_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void LoginForm_Paint(object sender, PaintEventArgs e)
        {
            // Рисуем неоморфную рамку формы
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            Rectangle formRect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (SolidBrush brush = new SolidBrush(NeomorphicStyle.BackgroundColor))
            {
                g.FillRoundedRectangle(brush, formRect, 15);
            }

            // Внешние тени для формы
            Rectangle lightRect = new Rectangle(formRect.X - 3, formRect.Y - 3, formRect.Width, formRect.Height);
            Rectangle darkRect = new Rectangle(formRect.X + 3, formRect.Y + 3, formRect.Width, formRect.Height);

            using (GraphicsPath lightPath = NeomorphicStyle.CreateRoundedRectangle(lightRect, 15))
            using (GraphicsPath darkPath = NeomorphicStyle.CreateRoundedRectangle(darkRect, 15))
            {
                using (Pen lightPen = new Pen(NeomorphicStyle.LightShadow, 6))
                using (Pen darkPen = new Pen(NeomorphicStyle.DarkShadow, 6))
                {
                    lightPen.LineJoin = LineJoin.Round;
                    darkPen.LineJoin = LineJoin.Round;
                    g.DrawPath(lightPen, lightPath);
                    g.DrawPath(darkPen, darkPath);
                }
            }
        }

        private void PanelMain_PaintNeomorphic(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = panel.ClientRectangle;

            // Основной фон
            using (SolidBrush brush = new SolidBrush(NeomorphicStyle.SurfaceColor))
            {
                g.FillRoundedRectangle(brush, rect, 20);
            }

            // Внешние тени для неоморфного эффекта
            Rectangle lightRect = new Rectangle(rect.X - 4, rect.Y - 4, rect.Width, rect.Height);
            Rectangle darkRect = new Rectangle(rect.X + 4, rect.Y + 4, rect.Width, rect.Height);

            using (GraphicsPath lightPath = NeomorphicStyle.CreateRoundedRectangle(lightRect, 20))
            using (GraphicsPath darkPath = NeomorphicStyle.CreateRoundedRectangle(darkRect, 20))
            {
                using (Pen lightPen = new Pen(NeomorphicStyle.LightShadow, 8))
                using (Pen darkPen = new Pen(NeomorphicStyle.DarkShadow, 8))
                {
                    lightPen.LineJoin = LineJoin.Round;
                    darkPen.LineJoin = LineJoin.Round;
                    g.DrawPath(lightPen, lightPath);
                    g.DrawPath(darkPen, darkPath);
                }
            }
        }

        private bool isLoginPressed = false;
        private bool isCancelPressed = false;

        private Panel txtUsernamePanel;
        private Panel txtPasswordPanel;

        private void CreateNeomorphicTextBox(TextBox textBox, out Panel container, int x, int y, int width, int height)
        {
            container = new Panel();
            Panel localContainer = container; // использовать в лямбдах вместо out-параметра
            container.Location = new Point(x, y);
            container.Size = new Size(width, height + 4);
            container.BackColor = Color.Transparent;
            localContainer.Paint += (s, e) => DrawTextBoxContainer(e.Graphics, localContainer, textBox.Focused);

            textBox.Location = new Point(4, 4);
            textBox.Size = new Size(width - 8, height);
            textBox.BorderStyle = BorderStyle.None;
            textBox.BackColor = NeomorphicStyle.BackgroundColor;
            textBox.ForeColor = NeomorphicStyle.TextColor;
            textBox.Font = new Font("Segoe UI", 10F);

            localContainer.Controls.Add(textBox);
            textBox.GotFocus += (s, e) => localContainer.Invalidate();
            textBox.LostFocus += (s, e) => localContainer.Invalidate();
        }

        private void DrawTextBoxContainer(Graphics g, Panel panel, bool isFocused)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, panel.Width, panel.Height);

            // Фон
            using (SolidBrush brush = new SolidBrush(NeomorphicStyle.BackgroundColor))
            {
                g.FillRoundedRectangle(brush, rect, 10);
            }

            // Неоморфные тени
            if (!isFocused)
            {
                Rectangle lightRect = new Rectangle(rect.X - 2, rect.Y - 2, rect.Width, rect.Height);
                Rectangle darkRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width, rect.Height);

                using (GraphicsPath lightPath = NeomorphicStyle.CreateRoundedRectangle(lightRect, 10))
                using (GraphicsPath darkPath = NeomorphicStyle.CreateRoundedRectangle(darkRect, 10))
                {
                    using (Pen lightPen = new Pen(NeomorphicStyle.LightShadow, 3))
                    using (Pen darkPen = new Pen(NeomorphicStyle.DarkShadow, 3))
                    {
                        g.DrawPath(lightPen, lightPath);
                        g.DrawPath(darkPen, darkPath);
                    }
                }
            }
            else
            {
                // Фокусная рамка
                using (Pen focusPen = new Pen(NeomorphicStyle.AccentColor, 2))
                {
                    g.DrawRoundedRectangle(focusPen, rect, 10);
                }
            }
        }


        private void BtnLogin_Paint(object sender, PaintEventArgs e)
        {
            Button btn = sender as Button;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = btn.ClientRectangle;

            // Основной фон
            using (SolidBrush brush = new SolidBrush(NeomorphicStyle.SurfaceColor))
            {
                g.FillRoundedRectangle(brush, rect, 12);
            }

            if (isLoginPressed)
            {
                // Внутренняя тень
                Rectangle innerRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
                using (Pen darkPen = new Pen(NeomorphicStyle.DarkShadow, 3))
                {
                    g.DrawRoundedRectangle(darkPen, innerRect, 10);
                }
            }
            else
            {
                // Внешние тени
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

            // Текст
            using (SolidBrush textBrush = new SolidBrush(NeomorphicStyle.AccentColor))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(btn.Text, btn.Font, textBrush, rect, sf);
            }
        }

        private void BtnLogin_MouseDown(object sender, MouseEventArgs e)
        {
            isLoginPressed = true;
            btnLogin.Invalidate();
        }

        private void BtnLogin_MouseUp(object sender, MouseEventArgs e)
        {
            isLoginPressed = false;
            btnLogin.Invalidate();
        }

        private void BtnCancel_Paint(object sender, PaintEventArgs e)
        {
            Button btn = sender as Button;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = btn.ClientRectangle;

            // Основной фон
            using (SolidBrush brush = new SolidBrush(NeomorphicStyle.SurfaceColor))
            {
                g.FillRoundedRectangle(brush, rect, 12);
            }

            if (isCancelPressed)
            {
                // Внутренняя тень
                Rectangle innerRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
                using (Pen darkPen = new Pen(NeomorphicStyle.DarkShadow, 3))
                {
                    g.DrawRoundedRectangle(darkPen, innerRect, 10);
                }
            }
            else
            {
                // Внешние тени
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

            // Текст
            using (SolidBrush textBrush = new SolidBrush(NeomorphicStyle.TextColor))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(btn.Text, btn.Font, textBrush, rect, sf);
            }
        }

        private void BtnCancel_MouseDown(object sender, MouseEventArgs e)
        {
            isCancelPressed = true;
            btnCancel.Invalidate();
        }

        private void BtnCancel_MouseUp(object sender, MouseEventArgs e)
        {
            isCancelPressed = false;
            btnCancel.Invalidate();
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnLogin_Click(sender, e);
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Пожалуйста, введите имя пользователя и пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                userManager.InitializeUsersTable();

                if (userManager.AuthenticateUser(txtUsername.Text, txtPassword.Text))
                {
                    DataTable userInfo = userManager.GetUserInfo(txtUsername.Text);
                    if (userInfo.Rows.Count > 0)
                    {
                        LoggedInUsername = txtUsername.Text;
                        LoggedInFullName = userInfo.Rows[0]["FullName"].ToString();
                        LoggedInRole = userInfo.Rows[0]["Role"].ToString();
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Неверное имя пользователя или пароль.\n\nЕсли база только что установлена, попробуйте admin / admin.",
                        "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Clear();
                    txtUsername.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                using (HelpForm helpForm = new HelpForm(HelpContext.Login))
                {
                    helpForm.ShowDialog(this);
                }
                e.Handled = true;
            }
        }
    }
}
