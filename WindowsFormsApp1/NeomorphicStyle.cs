using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public static class NeomorphicStyle
    {
        // Новая современная цветовая палитра - темная тема с акцентами
        public static readonly Color BackgroundColor = Color.FromArgb(30, 30, 40);
        public static readonly Color LightShadow = Color.FromArgb(50, 50, 60);
        public static readonly Color DarkShadow = Color.FromArgb(15, 15, 20);
        public static readonly Color SurfaceColor = Color.FromArgb(40, 40, 50);
        public static readonly Color TextColor = Color.FromArgb(230, 230, 240);
        public static readonly Color AccentColor = Color.FromArgb(100, 150, 255);
        public static readonly Color SecondaryAccent = Color.FromArgb(150, 200, 100);
        public static readonly Color WarningColor = Color.FromArgb(255, 180, 80);
        public static readonly Color ErrorColor = Color.FromArgb(255, 100, 100);

        // Рисование неоморфной кнопки
        public static void DrawNeomorphicButton(Graphics g, Rectangle rect, string text, Font font, bool isPressed = false)
        {
            // Основной фон
            using (SolidBrush brush = new SolidBrush(SurfaceColor))
            {
                g.FillRoundedRectangle(brush, rect, 15);
            }

            if (isPressed)
            {
                // Внутренняя тень (pressed state)
                using (Pen lightPen = new Pen(LightShadow, 2))
                using (Pen darkPen = new Pen(DarkShadow, 2))
                {
                    g.DrawRoundedRectangle(darkPen, rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4, 13);
                    g.DrawRoundedRectangle(lightPen, rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2, 14);
                }
            }
            else
            {
                // Внешние тени (raised state)
                Rectangle lightRect = new Rectangle(rect.X - 3, rect.Y - 3, rect.Width, rect.Height);
                Rectangle darkRect = new Rectangle(rect.X + 3, rect.Y + 3, rect.Width, rect.Height);

                using (GraphicsPath lightPath = CreateRoundedRectangle(lightRect, 15))
                using (GraphicsPath darkPath = CreateRoundedRectangle(darkRect, 15))
                {
                    using (Pen lightPen = new Pen(LightShadow, 6))
                    using (Pen darkPen = new Pen(DarkShadow, 6))
                    {
                        lightPen.LineJoin = LineJoin.Round;
                        darkPen.LineJoin = LineJoin.Round;
                        g.DrawPath(lightPen, lightPath);
                        g.DrawPath(darkPen, darkPath);
                    }
                }
            }

            // Текст
            using (SolidBrush textBrush = new SolidBrush(TextColor))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(text, font, textBrush, rect, sf);
            }
        }

        // Рисование неоморфного поля ввода
        public static void DrawNeomorphicTextBox(Graphics g, Rectangle rect, string text, Font font, bool isFocused = false)
        {
            // Основной фон
            using (SolidBrush brush = new SolidBrush(SurfaceColor))
            {
                g.FillRoundedRectangle(brush, rect, 12);
            }

            // Внутренние тени
            Rectangle innerRect = new Rectangle(rect.X + 3, rect.Y + 3, rect.Width - 6, rect.Height - 6);
            using (GraphicsPath innerPath = CreateRoundedRectangle(innerRect, 9))
            {
                using (Pen lightPen = new Pen(LightShadow, 3))
                using (Pen darkPen = new Pen(DarkShadow, 3))
                {
                    g.DrawPath(lightPen, innerPath);
                }
            }

            // Внешние тени
            Rectangle lightRect = new Rectangle(rect.X - 2, rect.Y - 2, rect.Width, rect.Height);
            Rectangle darkRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width, rect.Height);

            using (GraphicsPath lightPath = CreateRoundedRectangle(lightRect, 12))
            using (GraphicsPath darkPath = CreateRoundedRectangle(darkRect, 12))
            {
                using (Pen lightPen = new Pen(LightShadow, 4))
                using (Pen darkPen = new Pen(DarkShadow, 4))
                {
                    g.DrawPath(lightPen, lightPath);
                    g.DrawPath(darkPen, darkPath);
                }
            }

            // Фокусная рамка
            if (isFocused)
            {
                using (Pen focusPen = new Pen(AccentColor, 2))
                {
                    g.DrawRoundedRectangle(focusPen, rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2, 11);
                }
            }

            // Текст
            if (!string.IsNullOrEmpty(text))
            {
                Rectangle textRect = new Rectangle(rect.X + 10, rect.Y, rect.Width - 20, rect.Height);
                using (SolidBrush textBrush = new SolidBrush(TextColor))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(text, font, textBrush, textRect, sf);
                }
            }
        }

        // Создание скругленного прямоугольника
        public static GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        // Применение неоморфного стиля к форме
        public static void ApplyNeomorphicStyle(Form form)
        {
            form.BackColor = BackgroundColor;
            form.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        }

        // Применение неоморфного стиля к кнопке
        public static void ApplyNeomorphicButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = SurfaceColor;
            button.ForeColor = TextColor;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.Paint += (s, e) =>
            {
                bool isPressed = button.ClientRectangle.Contains(button.PointToClient(Control.MousePosition)) &&
                                Control.MouseButtons == MouseButtons.Left;
                DrawNeomorphicButton(e.Graphics, button.ClientRectangle, button.Text, button.Font, isPressed);
            };
        }

        // Применение неоморфного стиля к TextBox
        public static void ApplyNeomorphicTextBox(TextBox textBox)
        {
            textBox.BorderStyle = BorderStyle.None;
            textBox.BackColor = SurfaceColor;
            textBox.ForeColor = TextColor;
            textBox.Font = new Font("Segoe UI", 10F);
            textBox.Padding = new Padding(10, 8, 10, 8);
        }
    }

    // Расширения для Graphics
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using (GraphicsPath path = NeomorphicStyle.CreateRoundedRectangle(rect, radius))
            {
                g.FillPath(brush, path);
            }
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle rect, int radius)
        {
            using (GraphicsPath path = NeomorphicStyle.CreateRoundedRectangle(rect, radius))
            {
                g.DrawPath(pen, path);
            }
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, int x, int y, int width, int height, int radius)
        {
            g.DrawRoundedRectangle(pen, new Rectangle(x, y, width, height), radius);
        }
    }

    // Кастомная неоморфная кнопка
    public class NeomorphicButton : Button
    {
        private bool isPressed = false;

        public NeomorphicButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = NeomorphicStyle.SurfaceColor;
            ForeColor = NeomorphicStyle.TextColor;
            Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            NeomorphicStyle.DrawNeomorphicButton(e.Graphics, ClientRectangle, Text, Font, isPressed);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isPressed = false;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isPressed = false;
            Invalidate();
        }
    }
}

