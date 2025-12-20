using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1
{
    public partial class CategoriesForm : UserControl
    {
        private CategoryManager categoryManager;
        private DataGridView gridCategories;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Panel panelCategories;
        private DataTable categoriesData;

        public CategoriesForm()
        {
            categoryManager = new CategoryManager();
            InitializeComponent();
            ApplyNeomorphicStyle();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            panelCategories = new Panel();
            panelCategories.Dock = DockStyle.Fill;
            panelCategories.Padding = new Padding(20);
            panelCategories.BackColor = NeomorphicStyle.BackgroundColor;

            // Заголовок
            var lblTitle = new Label();
            lblTitle.Text = "📁 Управление категориями";
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = NeomorphicStyle.AccentColor;
            lblTitle.Location = new Point(20, 10);
            lblTitle.AutoSize = true;

            // Кнопки
            btnAdd = new Button();
            btnAdd.Text = "➕ Добавить";
            btnAdd.Location = new Point(20, 50);
            btnAdd.Size = new Size(150, 40);
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button();
            btnEdit.Text = "✏️ Изменить";
            btnEdit.Location = new Point(180, 50);
            btnEdit.Size = new Size(150, 40);
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button();
            btnDelete.Text = "🗑️ Удалить";
            btnDelete.Location = new Point(340, 50);
            btnDelete.Size = new Size(150, 40);
            btnDelete.Click += BtnDelete_Click;

            // DataGridView
            gridCategories = new DataGridView();
            gridCategories.Location = new Point(20, 100);
            gridCategories.Size = new Size(500, 400);
            gridCategories.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridCategories.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridCategories.AllowUserToAddRows = false;
            gridCategories.AllowUserToDeleteRows = false;
            gridCategories.ReadOnly = true;
            gridCategories.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridCategories.BackgroundColor = NeomorphicStyle.SurfaceColor;
            gridCategories.GridColor = NeomorphicStyle.DarkShadow;
            gridCategories.DefaultCellStyle.BackColor = NeomorphicStyle.SurfaceColor;
            gridCategories.DefaultCellStyle.ForeColor = NeomorphicStyle.TextColor;
            gridCategories.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            gridCategories.DefaultCellStyle.SelectionBackColor = NeomorphicStyle.AccentColor;
            gridCategories.DefaultCellStyle.SelectionForeColor = Color.White;
            gridCategories.ColumnHeadersDefaultCellStyle.BackColor = NeomorphicStyle.SurfaceColor;
            gridCategories.ColumnHeadersDefaultCellStyle.ForeColor = NeomorphicStyle.TextColor;
            gridCategories.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            gridCategories.EnableHeadersVisualStyles = false;
            gridCategories.RowHeadersVisible = false;
            gridCategories.BorderStyle = BorderStyle.None;

            panelCategories.Controls.Add(lblTitle);
            panelCategories.Controls.Add(btnAdd);
            panelCategories.Controls.Add(btnEdit);
            panelCategories.Controls.Add(btnDelete);
            panelCategories.Controls.Add(gridCategories);

            this.Controls.Add(panelCategories);
            this.Size = new Size(560, 520);
            this.BackColor = NeomorphicStyle.BackgroundColor;

            this.ResumeLayout(false);
        }

        private void ApplyNeomorphicStyle()
        {
            ApplyButtonStyle(btnAdd);
            ApplyButtonStyle(btnEdit);
            ApplyButtonStyle(btnDelete);
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

        public void LoadCategories()
        {
            try
            {
                Logger.Info("Загрузка списка категорий");
                categoriesData = categoryManager.GetAllCategories();
                gridCategories.DataSource = categoriesData;
                ConfigureGridColumns();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки категорий", ex);
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridColumns()
        {
            if (gridCategories.Columns.Contains("CategoryID"))
                gridCategories.Columns["CategoryID"].Visible = false;
            if (gridCategories.Columns.Contains("CategoryName"))
                gridCategories.Columns["CategoryName"].HeaderText = "Название категории";
        }

        private int? GetSelectedCategoryId()
        {
            if (gridCategories.CurrentRow == null || gridCategories.CurrentRow.Index < 0)
            {
                MessageBox.Show("Выберите категорию в таблице.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            return Convert.ToInt32(gridCategories.CurrentRow.Cells["CategoryID"].Value);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string name = ShowInputDialog("Добавление категории", "Введите название категории:", "");
            if (!string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    categoryManager.AddCategory(name);
                    Logger.Info($"Добавлена категория: {name}");
                    LoadCategories();
                    MessageBox.Show("Категория добавлена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Ошибка добавления категории: {name}", ex);
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            int? categoryId = GetSelectedCategoryId();
            if (categoryId == null) return;

            string currentName = gridCategories.CurrentRow.Cells["CategoryName"].Value?.ToString();
            string newName = ShowInputDialog("Редактирование категории", "Введите новое название:", currentName);
            
            if (!string.IsNullOrWhiteSpace(newName) && newName != currentName)
            {
                try
                {
                    categoryManager.UpdateCategory(categoryId.Value, newName);
                    Logger.Info($"Обновлена категория ID {categoryId}: {newName}");
                    LoadCategories();
                    MessageBox.Show("Категория обновлена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Ошибка обновления категории ID {categoryId}", ex);
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            int? categoryId = GetSelectedCategoryId();
            if (categoryId == null) return;

            string categoryName = gridCategories.CurrentRow.Cells["CategoryName"].Value?.ToString();

            if (MessageBox.Show($"Удалить категорию '{categoryName}'?\n\nВНИМАНИЕ: Удаление возможно только если к категории не привязаны записи!", 
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (categoryManager.DeleteCategory(categoryId.Value))
                    {
                        Logger.Info($"Удалена категория: {categoryName}");
                        LoadCategories();
                        MessageBox.Show("Категория удалена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить категорию. Возможно, к ней привязаны записи инвентаря.", 
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Ошибка удаления категории: {categoryName}", ex);
                    MessageBox.Show($"Ошибка: {ex.Message}\n\nВозможно, к категории привязаны записи инвентаря.", 
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string ShowInputDialog(string title, string prompt, string defaultValue)
        {
            Form inputForm = new Form();
            inputForm.Text = title;
            inputForm.Size = new Size(400, 180);
            inputForm.StartPosition = FormStartPosition.CenterParent;
            inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputForm.MaximizeBox = false;
            inputForm.MinimizeBox = false;
            inputForm.BackColor = NeomorphicStyle.BackgroundColor;

            Label label = new Label();
            label.Text = prompt;
            label.Location = new Point(20, 20);
            label.AutoSize = true;
            label.ForeColor = NeomorphicStyle.TextColor;

            TextBox textBox = new TextBox();
            textBox.Text = defaultValue;
            textBox.Location = new Point(20, 50);
            textBox.Size = new Size(340, 25);
            textBox.BackColor = NeomorphicStyle.SurfaceColor;
            textBox.ForeColor = NeomorphicStyle.TextColor;

            Button btnOk = new Button();
            btnOk.Text = "OK";
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(180, 90);
            btnOk.Size = new Size(80, 35);
            btnOk.BackColor = NeomorphicStyle.AccentColor;
            btnOk.ForeColor = Color.White;
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.FlatAppearance.BorderSize = 0;

            Button btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(270, 90);
            btnCancel.Size = new Size(90, 35);
            btnCancel.BackColor = NeomorphicStyle.SurfaceColor;
            btnCancel.ForeColor = NeomorphicStyle.TextColor;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;

            inputForm.Controls.AddRange(new Control[] { label, textBox, btnOk, btnCancel });
            inputForm.AcceptButton = btnOk;
            inputForm.CancelButton = btnCancel;

            if (inputForm.ShowDialog() == DialogResult.OK)
            {
                return textBox.Text.Trim();
            }
            return null;
        }
    }

    // Менеджер категорий
    public class CategoryManager
    {
        private DbController db = new DbController();

        public DataTable GetAllCategories()
        {
            return db.GetData("SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName");
        }

        public bool AddCategory(string categoryName)
        {
            string query = "INSERT INTO Categories (CategoryName) VALUES (@CategoryName)";
            var parameters = new System.Data.SqlClient.SqlParameter[]
            {
                new System.Data.SqlClient.SqlParameter("@CategoryName", categoryName)
            };
            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool UpdateCategory(int categoryId, string categoryName)
        {
            string query = "UPDATE Categories SET CategoryName = @CategoryName WHERE CategoryID = @CategoryID";
            var parameters = new System.Data.SqlClient.SqlParameter[]
            {
                new System.Data.SqlClient.SqlParameter("@CategoryID", categoryId),
                new System.Data.SqlClient.SqlParameter("@CategoryName", categoryName)
            };
            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool DeleteCategory(int categoryId)
        {
            // Проверяем, есть ли записи инвентаря с этой категорией
            string checkQuery = "SELECT COUNT(*) FROM Inventory WHERE CategoryID = @CategoryID";
            var checkParams = new System.Data.SqlClient.SqlParameter[]
            {
                new System.Data.SqlClient.SqlParameter("@CategoryID", categoryId)
            };
            int count = Convert.ToInt32(db.ExecuteScalar(checkQuery, checkParams));
            
            if (count > 0)
            {
                return false; // Нельзя удалить - есть связанные записи
            }

            string query = "DELETE FROM Categories WHERE CategoryID = @CategoryID";
            var parameters = new System.Data.SqlClient.SqlParameter[]
            {
                new System.Data.SqlClient.SqlParameter("@CategoryID", categoryId)
            };
            return db.ExecuteNonQuery(query, parameters) > 0;
        }
    }
}

