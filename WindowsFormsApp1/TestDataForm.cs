using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1
{
    public partial class TestDataForm : Form
    {
        private Button btnGenerate;
        private ProgressBar progressBar;
        private Label lblStatus;
        private Panel panelMain;
        private Label lblTitle;
        private bool isGenerating = false;

        public TestDataForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void InitializeComponent()
        {
            this.panelMain = new Panel();
            this.lblTitle = new Label();
            this.btnGenerate = new Button();
            this.progressBar = new ProgressBar();
            this.lblStatus = new Label();
            this.SuspendLayout();

            // Panel
            panelMain.Dock = DockStyle.Fill;
            panelMain.Padding = new Padding(30);
            panelMain.BackColor = NeomorphicStyle.BackgroundColor;

            // Title
            lblTitle.Text = "🔧 Генерация тестовых данных";
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = NeomorphicStyle.AccentColor;
            lblTitle.Location = new Point(30, 30);
            lblTitle.AutoSize = true;

            // Info label
            var lblInfo = new Label();
            lblInfo.Text = "Эта функция создаст по 100 записей в каждую таблицу базы данных:\n\n" +
                          "• Пользователи\n" +
                          "• Категории\n" +
                          "• Кабинеты\n" +
                          "• Ответственные лица\n" +
                          "• Инвентарь\n\n" +
                          "⚠️ ВНИМАНИЕ: Если данные уже существуют, дубликаты будут пропущены.";
            lblInfo.Font = new Font("Segoe UI", 10F);
            lblInfo.ForeColor = NeomorphicStyle.TextColor;
            lblInfo.Location = new Point(30, 80);
            lblInfo.Size = new Size(500, 150);
            lblInfo.AutoSize = false;

            // Button
            btnGenerate.Text = "🚀 Начать генерацию";
            btnGenerate.Location = new Point(30, 250);
            btnGenerate.Size = new Size(300, 50);
            btnGenerate.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnGenerate.Click += BtnGenerate_Click;

            // Progress bar
            progressBar.Location = new Point(30, 320);
            progressBar.Size = new Size(500, 30);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            // Status label
            lblStatus.Text = "Готов к генерации";
            lblStatus.Font = new Font("Segoe UI", 10F);
            lblStatus.ForeColor = NeomorphicStyle.TextColor;
            lblStatus.Location = new Point(30, 360);
            lblStatus.Size = new Size(500, 25);
            lblStatus.AutoSize = false;

            panelMain.Controls.Add(lblTitle);
            panelMain.Controls.Add(lblInfo);
            panelMain.Controls.Add(btnGenerate);
            panelMain.Controls.Add(progressBar);
            panelMain.Controls.Add(lblStatus);

            // Form
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = NeomorphicStyle.BackgroundColor;
            this.ClientSize = new Size(600, 450);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Генерация тестовых данных";
            this.KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };

            this.Controls.Add(panelMain);
            ApplyNeomorphicStyle();

            this.ResumeLayout(false);
        }

        private void ApplyNeomorphicStyle()
        {
            ApplyButtonStyle(btnGenerate);
        }

        private void ApplyButtonStyle(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = NeomorphicStyle.SurfaceColor;
            button.ForeColor = NeomorphicStyle.TextColor;
            button.Cursor = Cursors.Hand;
            button.Paint += (s, e) => DrawNeomorphicButton(e.Graphics, button);
        }

        private void DrawNeomorphicButton(Graphics g, Button button)
        {
            Rectangle rect = button.ClientRectangle;
            bool isPressed = button.ClientRectangle.Contains(button.PointToClient(Control.MousePosition)) &&
                            Control.MouseButtons == MouseButtons.Left && button.Enabled;

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

            using (SolidBrush textBrush = new SolidBrush(button.Enabled ? NeomorphicStyle.TextColor : Color.Gray))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(button.Text, button.Font, textBrush, rect, sf);
            }
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (isGenerating)
                return;

            if (MessageBox.Show(
                "Вы уверены, что хотите сгенерировать тестовые данные?\n\n" +
                "Будет создано по 100 записей в каждую таблицу.\n" +
                "Процесс может занять некоторое время.",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            isGenerating = true;
            btnGenerate.Enabled = false;
            btnGenerate.Text = "⏳ Генерация...";
            progressBar.Value = 0;
            lblStatus.Text = "Начало генерации...";

            try
            {
                // Запускаем генерацию в отдельном потоке
                System.Threading.Thread thread = new System.Threading.Thread(() =>
                {
                    try
                    {
                        // Создаем делегат для безопасного обновления UI
                        Action<int, string> updateProgress = (percent, message) =>
                        {
                            if (this.InvokeRequired)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    progressBar.Value = Math.Min(100, Math.Max(0, percent));
                                    lblStatus.Text = message;
                                    Application.DoEvents();
                                }));
                            }
                            else
                            {
                                progressBar.Value = Math.Min(100, Math.Max(0, percent));
                                lblStatus.Text = message;
                                Application.DoEvents();
                            }
                        };

                        TestDataGenerator.GenerateAllTestData(updateProgress);
                        
                        this.Invoke(new Action(() =>
                        {
                            btnGenerate.Enabled = true;
                            btnGenerate.Text = "✅ Готово!";
                            lblStatus.Text = "Генерация завершена успешно!";
                            progressBar.Value = 100;
                            MessageBox.Show("Тестовые данные успешно сгенерированы!\n\n• 100 пользователей\n• 100 категорий\n• 100 кабинетов\n• 100 ответственных лиц\n• 100 записей инвентаря", 
                                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Ошибка в потоке генерации данных", ex);
                        this.Invoke(new Action(() =>
                        {
                            btnGenerate.Enabled = true;
                            btnGenerate.Text = "🚀 Начать генерацию";
                            lblStatus.Text = $"Ошибка: {ex.Message}";
                            MessageBox.Show($"Ошибка генерации данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                    finally
                    {
                        this.Invoke(new Action(() =>
                        {
                            isGenerating = false;
                        }));
                    }
                });
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка запуска генерации данных", ex);
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                isGenerating = false;
                btnGenerate.Enabled = true;
                btnGenerate.Text = "🚀 Начать генерацию";
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle formRect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (SolidBrush brush = new SolidBrush(NeomorphicStyle.BackgroundColor))
            {
                g.FillRoundedRectangle(brush, formRect, 15);
            }

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
    }
}

