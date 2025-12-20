using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private InventoryManager inventoryManager = new InventoryManager();
        private DataTable inventoryTable;
			private Image photoPlaceholder64;
        private string loggedInUsername;
        private string loggedInFullName;
        private string loggedInRole;
        private Label lblUserInfo;

        public Form1() : this("admin", "Администратор", "Admin")
        {
        }

        public Form1(string username, string fullName, string role)
        {
            loggedInUsername = username;
            loggedInFullName = fullName;
            loggedInRole = role;
            InitializeComponent();
            
            // Инициализируем форму отчетов
            reportsForm = new ReportsForm(inventoryManager);
            tabReports.Controls.Add(reportsForm);
            reportsForm.Dock = DockStyle.Fill;
            
            // Инициализируем форму статистики
            statisticsForm = new StatisticsForm(inventoryManager);
            tabStatistics.Controls.Add(statisticsForm);
            statisticsForm.Dock = DockStyle.Fill;
            
            // Инициализируем форму управления пользователями (только для админа)
            userManagementForm = new UserManagementForm();
            tabUsers.Controls.Add(userManagementForm);
            userManagementForm.Dock = DockStyle.Fill;
            
            // Инициализируем форму категорий
            categoriesForm = new CategoriesForm();
            tabCategories.Controls.Add(categoriesForm);
            categoriesForm.Dock = DockStyle.Fill;
            
            // Настраиваем видимость вкладок в зависимости от роли
            ConfigureTabsForRole();
            
            // Обработка переключения вкладок для ленивой загрузки
            tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;
            
            // Включаем обработку клавиш для F1
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }

        private void ConfigureTabsForRole()
        {
            // Скрываем вкладки управления для обычных пользователей
            if (loggedInRole != "Admin")
            {
                // Удаляем вкладки для не-админов
                if (tabMain.TabPages.Contains(tabUsers))
                    tabMain.TabPages.Remove(tabUsers);
                if (tabMain.TabPages.Contains(tabCategories))
                    tabMain.TabPages.Remove(tabCategories);
            }
        }

        private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Ленивая загрузка данных при переключении вкладок
            if (tabMain.SelectedTab == tabStatistics)
            {
                statisticsForm.LoadStatistics();
            }
            else if (tabMain.SelectedTab == tabUsers)
            {
                userManagementForm.LoadUsers();
            }
            else if (tabMain.SelectedTab == tabCategories)
            {
                categoriesForm.LoadCategories();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Utils.Logger.Info($"Запуск приложения. Пользователь: {loggedInFullName} ({loggedInRole})");
                ApplyModernStyling();
                LoadInventory();
            }
            catch (Exception ex)
            {
                Utils.Logger.Error("Ошибка при загрузке главной формы", ex);
                MessageBox.Show($"Ошибка при загрузке приложения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyModernStyling()
        {
            // Применяем неоморфный стиль к форме
            NeomorphicStyle.ApplyNeomorphicStyle(this);
            this.BackColor = NeomorphicStyle.BackgroundColor;
            tabInventory.BackColor = NeomorphicStyle.BackgroundColor;
            tabReports.BackColor = NeomorphicStyle.BackgroundColor;
            
            // Стилизация кнопок в неоморфном стиле
            ApplyNeomorphicButton(btnAdd);
            ApplyNeomorphicButton(btnEdit);
            ApplyNeomorphicButton(btnDelete);
            ApplyNeomorphicButton(btnRefresh);
            ApplyNeomorphicButton(btnClearSearch);
            ApplyNeomorphicButton(btnGenerateData);

            // Стилизация текстового поля поиска - создаем неоморфный контейнер
            CreateNeomorphicSearchBox();

            // Стилизация DataGridView в неоморфном стиле
            gridInventory.BackgroundColor = NeomorphicStyle.BackgroundColor;
            gridInventory.BorderStyle = BorderStyle.None;
            gridInventory.GridColor = Color.FromArgb(NeomorphicStyle.DarkShadow.R, NeomorphicStyle.DarkShadow.G, NeomorphicStyle.DarkShadow.B);
            gridInventory.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            gridInventory.DefaultCellStyle.BackColor = NeomorphicStyle.SurfaceColor;
            gridInventory.DefaultCellStyle.ForeColor = NeomorphicStyle.TextColor;
            gridInventory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 45);
            gridInventory.ColumnHeadersDefaultCellStyle.BackColor = NeomorphicStyle.SurfaceColor;
            gridInventory.ColumnHeadersDefaultCellStyle.ForeColor = NeomorphicStyle.TextColor;
            gridInventory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            gridInventory.ColumnHeadersDefaultCellStyle.SelectionBackColor = NeomorphicStyle.SurfaceColor;
            gridInventory.RowHeadersDefaultCellStyle.BackColor = NeomorphicStyle.SurfaceColor;
            gridInventory.EnableHeadersVisualStyles = false;
            gridInventory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridInventory.DefaultCellStyle.SelectionBackColor = NeomorphicStyle.AccentColor;
            gridInventory.DefaultCellStyle.SelectionForeColor = Color.White;
            gridInventory.AllowUserToAddRows = false;
            gridInventory.AllowUserToDeleteRows = false;
            
            // Добавляем неоморфную панель вокруг DataGridView
            CreateNeomorphicGridPanel();

            // Стилизация TabControl в неоморфном стиле
            tabMain.Font = new Font("Segoe UI", 10F);
            tabMain.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabMain.DrawItem += TabMain_DrawItemNeomorphic;

            // Стилизация StatusStrip
            statusStrip1.BackColor = NeomorphicStyle.SurfaceColor;
            statusStrip1.Font = new Font("Segoe UI", 9F);
            toolStripStatusLabelCount.ForeColor = NeomorphicStyle.TextColor;

            // Стилизация Label
            lblSearch.ForeColor = NeomorphicStyle.TextColor;
            lblSearch.Font = new Font("Segoe UI", 10F);

            // Добавляем информацию о пользователе
            AddUserInfoLabel();
        }

        private void ApplyNeomorphicButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = NeomorphicStyle.SurfaceColor;
            btn.ForeColor = NeomorphicStyle.TextColor;
            btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            //btn.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            btn.Paint += (s, e) => DrawNeomorphicButton(e.Graphics, btn);
            btn.MouseDown += (s, e) => btn.Invalidate();
            btn.MouseUp += (s, e) => btn.Invalidate();
            btn.MouseLeave += (s, e) => btn.Invalidate();
        }

        private void DrawNeomorphicButton(Graphics g, Button button)
        {
            Rectangle rect = button.ClientRectangle;
            bool isPressed = button.ClientRectangle.Contains(button.PointToClient(Control.MousePosition)) &&
                            Control.MouseButtons == MouseButtons.Left;

            // Основной фон
            using (SolidBrush brush = new SolidBrush(NeomorphicStyle.SurfaceColor))
            {
                g.FillRoundedRectangle(brush, rect, 12);
            }

            if (isPressed)
            {
                // Внутренняя тень (pressed)
                Rectangle innerRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
                using (Pen darkPen = new Pen(NeomorphicStyle.DarkShadow, 3))
                {
                    g.DrawRoundedRectangle(darkPen, innerRect, 10);
                }
            }
            else
            {
                // Внешние тени (raised)
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
                g.DrawString(button.Text, button.Font, textBrush, rect, sf);
            }
        }

        private void TabMain_DrawItemNeomorphic(object sender, DrawItemEventArgs e)
        {
            TabControl tab = sender as TabControl;
            TabPage tabPage = tab.TabPages[e.Index];
            Rectangle tabBounds = tab.GetTabRect(e.Index);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (e.Index == tab.SelectedIndex)
            {
                // Выбранная вкладка - неоморфный стиль
                using (SolidBrush brush = new SolidBrush(NeomorphicStyle.SurfaceColor))
                {
                    e.Graphics.FillRoundedRectangle(brush, tabBounds, 8);
                }
                using (SolidBrush textBrush = new SolidBrush(NeomorphicStyle.AccentColor))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    e.Graphics.DrawString(tabPage.Text, tab.Font, textBrush, tabBounds, sf);
                }
            }
            else
            {
                // Невыбранная вкладка
                using (SolidBrush brush = new SolidBrush(NeomorphicStyle.BackgroundColor))
                {
                    e.Graphics.FillRectangle(brush, tabBounds);
                }
                using (SolidBrush textBrush = new SolidBrush(NeomorphicStyle.TextColor))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    e.Graphics.DrawString(tabPage.Text, tab.Font, textBrush, tabBounds, sf);
                }
            }
        }


        private Panel searchPanel;
        private Panel gridPanel;

        private void CreateNeomorphicSearchBox()
        {
            // Сохраняем оригинальные свойства
            Point originalLocation = txtSearch.Location;
            Size originalSize = txtSearch.Size;
            AnchorStyles originalAnchor = txtSearch.Anchor;

            // Создаем панель для поиска с неоморфным эффектом
            searchPanel = new Panel();
            searchPanel.Location = new Point(originalLocation.X - 2, originalLocation.Y - 2);
            searchPanel.Size = new Size(originalSize.Width + 4, originalSize.Height + 4);
            searchPanel.Anchor = originalAnchor;
            searchPanel.BackColor = Color.Transparent;
            searchPanel.Paint += SearchPanel_Paint;

            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.BackColor = NeomorphicStyle.BackgroundColor;
            txtSearch.ForeColor = NeomorphicStyle.TextColor;
            txtSearch.Font = new Font("Segoe UI", 10F);
            txtSearch.Location = new Point(2, 2);
            txtSearch.Size = new Size(searchPanel.Width - 4, searchPanel.Height - 4);
            txtSearch.Anchor = AnchorStyles.None;
            txtSearch.GotFocus += (s, e) => searchPanel.Invalidate();
            txtSearch.LostFocus += (s, e) => searchPanel.Invalidate();

            searchPanel.Controls.Add(txtSearch);
            tabInventory.Controls.Add(searchPanel);
            searchPanel.BringToFront();
            txtSearch.BringToFront();
        }

        private void SearchPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = panel.ClientRectangle;
            bool isFocused = txtSearch.Focused;

            // Фон
            using (SolidBrush brush = new SolidBrush(NeomorphicStyle.BackgroundColor))
            {
                g.FillRoundedRectangle(brush, rect, 8);
            }

            // Неоморфные тени
            if (!isFocused)
            {
                Rectangle lightRect = new Rectangle(rect.X - 2, rect.Y - 2, rect.Width, rect.Height);
                Rectangle darkRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width, rect.Height);

                using (GraphicsPath lightPath = NeomorphicStyle.CreateRoundedRectangle(lightRect, 8))
                using (GraphicsPath darkPath = NeomorphicStyle.CreateRoundedRectangle(darkRect, 8))
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
                    g.DrawRoundedRectangle(focusPen, rect, 8);
                }
            }
        }

        private void CreateNeomorphicGridPanel()
        {
            // Создаем панель вокруг DataGridView с неоморфным эффектом
            gridPanel = new Panel();
            gridPanel.Location = new Point(gridInventory.Location.X - 3, gridInventory.Location.Y - 3);
            gridPanel.Size = new Size(gridInventory.Width + 6, gridInventory.Height + 6);
            gridPanel.Anchor = gridInventory.Anchor;
            gridPanel.BackColor = Color.Transparent;
            gridPanel.Paint += GridPanel_Paint;

            // Перемещаем DataGridView на панель
            Point gridLocation = gridInventory.Location;
            Size gridSize = gridInventory.Size;
            AnchorStyles gridAnchor = gridInventory.Anchor;

            gridInventory.Location = new Point(3, 3);
            gridInventory.Size = new Size(gridSize.Width, gridSize.Height);
            gridInventory.Anchor = AnchorStyles.None;
            gridInventory.Dock = DockStyle.Fill;

            gridPanel.Controls.Add(gridInventory);
            tabInventory.Controls.Add(gridPanel);
            gridPanel.SendToBack();
        }

        private void GridPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = panel.ClientRectangle;

            // Основной фон
            using (SolidBrush brush = new SolidBrush(NeomorphicStyle.SurfaceColor))
            {
                g.FillRoundedRectangle(brush, rect, 12);
            }

            // Внешние тени для неоморфного эффекта
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

        private void AddUserInfoLabel()
        {
            lblUserInfo = new Label();
            lblUserInfo.AutoSize = true;
            lblUserInfo.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lblUserInfo.ForeColor = NeomorphicStyle.TextColor;
            lblUserInfo.Text = $"Пользователь: {loggedInFullName} ({loggedInRole})";
            lblUserInfo.Location = new Point(10, 5);
            statusStrip1.Items.Add(new ToolStripControlHost(lblUserInfo));
        }

        private void LoadInventory()
        {
            try
            {
                Utils.Logger.Debug("Загрузка данных инвентаря");
                inventoryTable = inventoryManager.GetAllInventory();
                gridInventory.DataSource = inventoryTable;
                ConfigureGridColumns();
                gridInventory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 60);
                UpdateStatusCount();
                Utils.Logger.Info($"Загружено записей инвентаря: {inventoryTable.Rows.Count}");
            }
            catch (Exception ex)
            {
                Utils.Logger.Error("Ошибка загрузки данных инвентаря", ex);
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                LoadInventory();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка обновления инвентаря", ex);
                MessageBox.Show($"Ошибка обновления данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                Utils.Logger.Info("Открытие диалога добавления инвентаря");
                using (var dlg = new InventoryAddDialog(inventoryManager))
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        Utils.Logger.Info("Инвентарь успешно добавлен");
                        LoadInventory();
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.Logger.Error("Ошибка при добавлении инвентаря", ex);
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            int? id = GetSelectedInventoryId();
            if (id == null) return;
            using (var dlg = new InventoryEditDialog(inventoryManager, id.Value))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadInventory();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            int? id = GetSelectedInventoryId();
            if (id == null) return;
            if (MessageBox.Show("Удалить выбранную запись?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    Utils.Logger.Info($"Попытка удаления инвентаря с ID: {id.Value}");
                    bool ok = inventoryManager.DeleteInventoryItem(id.Value);
                    if (!ok)
                    {
                        Utils.Logger.Warning($"Не удалось удалить запись с ID: {id.Value}");
                        MessageBox.Show("Запись не удалена.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        Utils.Logger.Info($"Инвентарь с ID {id.Value} успешно удален");
                    }
                    LoadInventory();
                }
                catch (Exception ex)
                {
                    Utils.Logger.Error($"Ошибка удаления инвентаря с ID: {id.Value}", ex);
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private int? GetSelectedInventoryId()
        {
            try
            {
                if (gridInventory.CurrentRow == null || gridInventory.CurrentRow.Index < 0)
                {
                    MessageBox.Show("Выберите запись в таблице.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return null;
                }

                //if (!gridInventory.CurrentRow.Cells.Contains("InventoryID"))
                //{
                //    MessageBox.Show("Ошибка: колонка InventoryID не найдена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    return null;
                //}

                object value = gridInventory.CurrentRow.Cells["InventoryID"].Value;
                if (value == null || value == DBNull.Value)
                {
                    MessageBox.Show("Не удалось получить ID записи.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                return Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении ID записи: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (inventoryTable == null) return;
            string text = txtSearch.Text.Replace("'", "''");
            if (string.IsNullOrWhiteSpace(text))
                inventoryTable.DefaultView.RowFilter = string.Empty;
            else
                inventoryTable.DefaultView.RowFilter = $"ItemName LIKE '%{text}%' OR Description LIKE '%{text}%'";
            UpdateStatusCount();
        }

        private void gridInventory_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Проверяем, что клик был по ячейке данных, а не по заголовку
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    // Проверяем, что строка существует и не является новой строкой
                    if (gridInventory.Rows[e.RowIndex] != null && 
                        !gridInventory.Rows[e.RowIndex].IsNewRow)
                    {
                        // Устанавливаем выбранную строку как текущую
                        gridInventory.CurrentCell = gridInventory.Rows[e.RowIndex].Cells[e.ColumnIndex];
                        gridInventory.Rows[e.RowIndex].Selected = true;
                        
                        // Вызываем обработчик редактирования
                        btnEdit_Click(sender, e);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке двойного клика: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
        }


        private void ConfigureGridColumns()
        {
            if (gridInventory.Columns.Contains("InventoryID"))
            {
                gridInventory.Columns["InventoryID"].HeaderText = "ID";
                // Скрываем колонку ID из отображения, оставляя её в данных для выбора/редактирования
                gridInventory.Columns["InventoryID"].Visible = false;
            }
            // удалены все фото-колонки
            if (gridInventory.Columns.Contains("ItemName")) gridInventory.Columns["ItemName"].HeaderText = "Наименование";
            if (gridInventory.Columns.Contains("Description")) gridInventory.Columns["Description"].HeaderText = "Описание";
            if (gridInventory.Columns.Contains("CategoryName")) gridInventory.Columns["CategoryName"].HeaderText = "Категория";
            if (gridInventory.Columns.Contains("Classroom")) gridInventory.Columns["Classroom"].HeaderText = "Кабинет";
            if (gridInventory.Columns.Contains("ResponsiblePerson")) gridInventory.Columns["ResponsiblePerson"].HeaderText = "Ответственный";
            if (gridInventory.Columns.Contains("InventoryNumber")) gridInventory.Columns["InventoryNumber"].HeaderText = "Инв. номер";
            if (gridInventory.Columns.Contains("PurchaseDate"))
            {
                gridInventory.Columns["PurchaseDate"].HeaderText = "Дата покупки";
                gridInventory.Columns["PurchaseDate"].DefaultCellStyle.Format = "d";
            }
            if (gridInventory.Columns.Contains("PurchasePrice"))
            {
                gridInventory.Columns["PurchasePrice"].HeaderText = "Цена";
                gridInventory.Columns["PurchasePrice"].DefaultCellStyle.Format = "N2";
                gridInventory.Columns["PurchasePrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (gridInventory.Columns.Contains("CurrentState")) gridInventory.Columns["CurrentState"].HeaderText = "Состояние";
            if (gridInventory.Columns.Contains("Status")) gridInventory.Columns["Status"].HeaderText = "Статус";

            // желаемый порядок
            string[] order = { "ItemName", "CategoryName", "InventoryNumber", "Classroom", "ResponsiblePerson", "PurchaseDate", "PurchasePrice", "CurrentState", "Status", "Description" };
            int index = 0;
            foreach (var name in order)
            {
                if (gridInventory.Columns.Contains(name))
                    gridInventory.Columns[name].DisplayIndex = index++;
            }
        }

        private void UpdateStatusCount()
        {
            if (inventoryTable == null) return;
            int total = inventoryTable.Rows.Count;
            int shown = inventoryTable.DefaultView.Count;
            toolStripStatusLabelCount.Text = $"Записей: {shown} / {total}";
        }

        private void gridInventory_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            // Рисуем номера строк ТОЛЬКО если видимы заголовки строк
            var grid = (DataGridView)sender;
            if (!grid.RowHeadersVisible) return;

            var bounds = new System.Drawing.Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, bounds, e.InheritedRowStyle.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                // Определяем контекст справки в зависимости от активной вкладки
                HelpContext context = HelpContext.Inventory;
                if (tabMain.SelectedTab == tabReports)
                    context = HelpContext.Reports;
                else if (tabMain.SelectedTab == tabStatistics)
                    context = HelpContext.Statistics;
                else if (tabMain.SelectedTab == tabUsers)
                    context = HelpContext.UserManagement;
                else if (tabMain.SelectedTab == tabCategories)
                    context = HelpContext.Categories;

                using (HelpForm helpForm = new HelpForm(context))
                {
                    helpForm.ShowDialog(this);
                }
                e.Handled = true;
            }
        }

        private void btnGenerateData_Click(object sender, EventArgs e)
        {
            try
            {
                if (loggedInRole != "Admin")
                {
                    MessageBox.Show("Генерация тестовых данных доступна только для администраторов.", 
                        "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (TestDataForm testDataForm = new TestDataForm())
                {
                    testDataForm.ShowDialog(this);
                    // Обновляем данные после генерации
                    if (tabMain.SelectedTab == tabInventory)
                        LoadInventory();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка открытия формы генерации данных", ex);
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
