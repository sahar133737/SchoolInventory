using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WindowsFormsApp1.UI;
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
        private DataTable categoriesData;

        public CategoriesForm()
        {
            categoryManager = new CategoryManager();
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
                Text = "Управление категориями",
                Dock = DockStyle.Top,
                Height = 32,
                Font = AppTheme.FontHeader,
                ForeColor = AppTheme.Primary
            };

            var toolbar = new ToolbarPanel();
            btnAdd = new Button { Text = "Добавить" };
            btnEdit = new Button { Text = "Изменить" };
            btnDelete = new Button { Text = "Удалить" };
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            toolbar.AddButton(btnAdd, true);
            toolbar.AddButton(btnEdit);
            toolbar.AddButton(btnDelete);

            gridCategories = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.ApplyToDataGridView(gridCategories);

            Controls.Add(gridCategories);
            Controls.Add(toolbar);
            Controls.Add(header);
            ResumeLayout(false);
        }

        public void LoadCategories()
        {
            try
            {
                Logger.Info("Загрузка списка категорий");
                categoriesData = categoryManager.GetAllCategories();
                gridCategories.DataSource = categoriesData;
                GridHelper.LocalizeCategoriesGrid(gridCategories);
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
            AppTheme.ApplyToForm(inputForm);

            var label = new Label { Text = prompt, Location = new Point(20, 20), AutoSize = true, Font = AppTheme.FontUi };
            var textBox = new TextBox { Text = defaultValue, Location = new Point(20, 50), Size = new Size(340, 25) };
            AppTheme.ApplyToTextBox(textBox);
            var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(180, 90), Size = new Size(80, 34) };
            var btnCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel, Location = new Point(270, 90), Size = new Size(90, 34) };
            AppTheme.ApplyToButton(btnOk, true);
            AppTheme.ApplyToButton(btnCancel);

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

