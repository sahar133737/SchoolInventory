using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    /// <summary>
    /// Тип справки для разных форм
    /// </summary>
    public enum HelpContext
    {
        General,
        Login,
        Inventory,
        AddInventory,
        EditInventory,
        Reports,
        UserManagement,
        Categories,
        Statistics
    }

    public partial class HelpForm : Form
    {
        private RichTextBox rtbHelp;
        private Button btnClose;
        private Panel headerPanel;
        private Label headerTitle;
        private TreeView treeNavigation;
        private Panel navigationPanel;
        private SplitContainer splitContainer;
        private bool isDragging = false;
        private Point lastMousePosition;
        private HelpContext currentContext;

        public HelpForm() : this(HelpContext.General)
        {
        }

        public HelpForm(HelpContext context)
        {
            currentContext = context;
            InitializeComponent();
            this.DoubleBuffered = true;
            BuildNavigationTree();
            SelectHelpSection(context);
        }

        private void InitializeComponent()
        {
            this.headerPanel = new Panel();
            this.headerTitle = new Label();
            this.rtbHelp = new RichTextBox();
            this.btnClose = new Button();
            this.treeNavigation = new TreeView();
            this.navigationPanel = new Panel();
            this.splitContainer = new SplitContainer();
            this.SuspendLayout();

            // Header Panel
            this.headerPanel.Dock = DockStyle.Top;
            this.headerPanel.Height = 50;
            this.headerPanel.BackColor = NeomorphicStyle.SurfaceColor;
            this.headerPanel.Padding = new Padding(15, 8, 50, 8);
            this.headerPanel.Paint += HeaderPanel_Paint;
            this.headerPanel.MouseDown += HeaderPanel_MouseDown;
            this.headerPanel.MouseMove += HeaderPanel_MouseMove;
            this.headerPanel.MouseUp += HeaderPanel_MouseUp;

            // Header Title
            this.headerTitle.Dock = DockStyle.Fill;
            this.headerTitle.Text = "📖 Справка - Школьная инвентаризация";
            this.headerTitle.TextAlign = ContentAlignment.MiddleLeft;
            this.headerTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.headerTitle.ForeColor = NeomorphicStyle.TextColor;
            this.headerTitle.BackColor = Color.Transparent;
            this.headerPanel.Controls.Add(this.headerTitle);

            // Close Button
            this.btnClose.Text = "×";
            this.btnClose.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.btnClose.Size = new Size(40, 40);
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.BackColor = Color.Transparent;
            this.btnClose.ForeColor = NeomorphicStyle.TextColor;
            this.btnClose.Cursor = Cursors.Hand;
            this.btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnClose.Location = new Point(this.headerPanel.Width - 45, 5);
            this.btnClose.Click += (s, e) => this.Close();
            this.btnClose.MouseEnter += (s, e) => btnClose.ForeColor = NeomorphicStyle.ErrorColor;
            this.btnClose.MouseLeave += (s, e) => btnClose.ForeColor = NeomorphicStyle.TextColor;
            this.headerPanel.Controls.Add(this.btnClose);
            this.headerPanel.Resize += (s, e) => { btnClose.Location = new Point(this.headerPanel.Width - 45, 5); };

            // SplitContainer
            this.splitContainer.Dock = DockStyle.Fill;
            this.splitContainer.SplitterDistance = 250;
            this.splitContainer.BackColor = NeomorphicStyle.BackgroundColor;
            this.splitContainer.Panel1.BackColor = NeomorphicStyle.SurfaceColor;
            this.splitContainer.Panel2.BackColor = NeomorphicStyle.BackgroundColor;

            // TreeView Navigation
            this.treeNavigation.Dock = DockStyle.Fill;
            this.treeNavigation.BackColor = NeomorphicStyle.SurfaceColor;
            this.treeNavigation.ForeColor = NeomorphicStyle.TextColor;
            this.treeNavigation.Font = new Font("Segoe UI", 10F);
            this.treeNavigation.BorderStyle = BorderStyle.None;
            this.treeNavigation.FullRowSelect = true;
            this.treeNavigation.ShowLines = true;
            this.treeNavigation.ShowPlusMinus = true;
            this.treeNavigation.ItemHeight = 28;
            this.treeNavigation.AfterSelect += TreeNavigation_AfterSelect;
            this.splitContainer.Panel1.Controls.Add(this.treeNavigation);

            // RichTextBox для справки
            this.rtbHelp.Dock = DockStyle.Fill;
            this.rtbHelp.BackColor = NeomorphicStyle.BackgroundColor;
            this.rtbHelp.ForeColor = NeomorphicStyle.TextColor;
            this.rtbHelp.Font = new Font("Consolas", 10F);
            this.rtbHelp.BorderStyle = BorderStyle.None;
            this.rtbHelp.ReadOnly = true;
            this.rtbHelp.Padding = new Padding(20);
            this.rtbHelp.Margin = new Padding(10);
            this.splitContainer.Panel2.Controls.Add(this.rtbHelp);

            // Form
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = NeomorphicStyle.BackgroundColor;
            this.ClientSize = new Size(1000, 700);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Справка";
            this.KeyPreview = true;
            this.KeyDown += HelpForm_KeyDown;

            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.headerPanel);

            this.ResumeLayout(false);
        }

        private void BuildNavigationTree()
        {
            treeNavigation.Nodes.Clear();

            // Общая информация
            var nodeGeneral = new TreeNode("📚 Общая информация") { Tag = HelpContext.General };
            treeNavigation.Nodes.Add(nodeGeneral);

            // Авторизация
            var nodeLogin = new TreeNode("🔐 Авторизация") { Tag = HelpContext.Login };
            treeNavigation.Nodes.Add(nodeLogin);

            // Работа с инвентарем
            var nodeInventory = new TreeNode("📦 Работа с инвентарем") { Tag = HelpContext.Inventory };
            nodeInventory.Nodes.Add(new TreeNode("➕ Добавление инвентаря") { Tag = HelpContext.AddInventory });
            nodeInventory.Nodes.Add(new TreeNode("✏️ Редактирование инвентаря") { Tag = HelpContext.EditInventory });
            treeNavigation.Nodes.Add(nodeInventory);

            // Отчеты
            var nodeReports = new TreeNode("📊 Отчеты") { Tag = HelpContext.Reports };
            treeNavigation.Nodes.Add(nodeReports);

            // Управление пользователями
            var nodeUsers = new TreeNode("👥 Управление пользователями") { Tag = HelpContext.UserManagement };
            treeNavigation.Nodes.Add(nodeUsers);

            // Категории
            var nodeCategories = new TreeNode("📁 Управление категориями") { Tag = HelpContext.Categories };
            treeNavigation.Nodes.Add(nodeCategories);

            // Статистика
            var nodeStats = new TreeNode("📈 Статистика") { Tag = HelpContext.Statistics };
            treeNavigation.Nodes.Add(nodeStats);

            treeNavigation.ExpandAll();
        }

        private void TreeNavigation_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is HelpContext context)
            {
                rtbHelp.Text = GetHelpTextForContext(context);
            }
        }

        private void SelectHelpSection(HelpContext context)
        {
            foreach (TreeNode node in treeNavigation.Nodes)
            {
                if (node.Tag is HelpContext nodeContext && nodeContext == context)
                {
                    treeNavigation.SelectedNode = node;
                    return;
                }
                foreach (TreeNode childNode in node.Nodes)
                {
                    if (childNode.Tag is HelpContext childContext && childContext == context)
                    {
                        treeNavigation.SelectedNode = childNode;
                        return;
                    }
                }
            }
            // По умолчанию - общая информация
            if (treeNavigation.Nodes.Count > 0)
            {
                treeNavigation.SelectedNode = treeNavigation.Nodes[0];
            }
        }

        private string GetHelpTextForContext(HelpContext context)
        {
            switch (context)
            {
                case HelpContext.Login:
                    return GetLoginHelp();
                case HelpContext.Inventory:
                    return GetInventoryHelp();
                case HelpContext.AddInventory:
                    return GetAddInventoryHelp();
                case HelpContext.EditInventory:
                    return GetEditInventoryHelp();
                case HelpContext.Reports:
                    return GetReportsHelp();
                case HelpContext.UserManagement:
                    return GetUserManagementHelp();
                case HelpContext.Categories:
                    return GetCategoriesHelp();
                case HelpContext.Statistics:
                    return GetStatisticsHelp();
                default:
                    return GetGeneralHelp();
            }
        }

        private string GetGeneralHelp()
        {
            return @"
╔══════════════════════════════════════════════════════════════════╗
║             СПРАВКА ПО ПРОГРАММЕ ""ШКОЛЬНАЯ ИНВЕНТАРИЗАЦИЯ""      ║
╚══════════════════════════════════════════════════════════════════╝

ОБЩАЯ ИНФОРМАЦИЯ
────────────────────────────────────────────────────────────────────

Программа ""Школьная инвентаризация"" предназначена для:

  ✓ Учета школьного инвентаря и оборудования
  ✓ Управления категориями и классификацией предметов
  ✓ Назначения ответственных лиц за инвентарь
  ✓ Формирования отчетов по различным критериям
  ✓ Экспорта данных в Excel для дальнейшей обработки
  ✓ Управления пользователями системы

СТРУКТУРА ПРОГРАММЫ
────────────────────────────────────────────────────────────────────

📦 ИНВЕНТАРЬ     - Главный модуль для работы с инвентарем
📊 ОТЧЕТЫ        - Формирование и экспорт отчетов
👥 ПОЛЬЗОВАТЕЛИ  - Управление учетными записями (только для админа)
📁 КАТЕГОРИИ     - Управление категориями инвентаря
📈 СТАТИСТИКА    - Аналитика и статистические данные

ГОРЯЧИЕ КЛАВИШИ
────────────────────────────────────────────────────────────────────

  F1        - Открыть справку
  ESC       - Закрыть текущее окно
  Enter     - Подтвердить действие
  Ctrl+N    - Добавить новую запись
  Ctrl+E    - Редактировать выбранную запись
  Delete    - Удалить выбранную запись

ТРЕБОВАНИЯ К СИСТЕМЕ
────────────────────────────────────────────────────────────────────

  • Операционная система: Windows 7/8/10/11
  • .NET Framework 4.8 или выше
  • Microsoft SQL Server 2012+
  • Минимум 100 МБ свободного места
  • Разрешение экрана: 1280x720 или выше

────────────────────────────────────────────────────────────────────
Версия: 2.0 | Дата обновления: " + DateTime.Now.ToString("dd.MM.yyyy");
        }

        private string GetLoginHelp()
        {
            return @"
╔══════════════════════════════════════════════════════════════════╗
║                         АВТОРИЗАЦИЯ                              ║
╚══════════════════════════════════════════════════════════════════╝

ВХОД В СИСТЕМУ
────────────────────────────────────────────────────────────────────

Для входа в программу необходимо ввести:
  • Имя пользователя (логин)
  • Пароль

УЧЕТНЫЕ ДАННЫЕ ПО УМОЛЧАНИЮ
────────────────────────────────────────────────────────────────────

  Логин:    admin
  Пароль:   admin

⚠️ ВНИМАНИЕ: После первого входа настоятельно рекомендуется
   изменить пароль администратора для безопасности!

РОЛИ ПОЛЬЗОВАТЕЛЕЙ
────────────────────────────────────────────────────────────────────

  👑 Admin (Администратор)
     • Полный доступ ко всем функциям
     • Управление пользователями
     • Управление категориями
     • Просмотр статистики

  👤 User (Пользователь)
     • Просмотр инвентаря
     • Добавление записей
     • Редактирование записей
     • Формирование отчетов

РЕШЕНИЕ ПРОБЛЕМ
────────────────────────────────────────────────────────────────────

❓ Забыли пароль?
   → Обратитесь к администратору системы

❓ Ошибка подключения к базе данных?
   → Проверьте, что SQL Server запущен
   → Проверьте настройки подключения в App.config

❓ Учетная запись заблокирована?
   → Обратитесь к администратору для разблокировки";
        }

        private string GetInventoryHelp()
        {
            return @"
╔══════════════════════════════════════════════════════════════════╗
║                    РАБОТА С ИНВЕНТАРЕМ                           ║
╚══════════════════════════════════════════════════════════════════╝

ГЛАВНАЯ ТАБЛИЦА ИНВЕНТАРЯ
────────────────────────────────────────────────────────────────────

Таблица отображает все записи инвентаря со следующими полями:

  📌 Наименование     - Название предмета
  📁 Категория        - Категория инвентаря
  🔢 Инв. номер       - Инвентарный номер
  🏫 Кабинет          - Место расположения
  👤 Ответственный    - Ответственное лицо
  📅 Дата покупки     - Дата приобретения
  💰 Цена             - Стоимость
  🔧 Состояние        - Текущее состояние
  📋 Статус           - Статус предмета

ОПЕРАЦИИ
────────────────────────────────────────────────────────────────────

  [Обновить]    - Перезагрузить данные из базы
  [Добавить]    - Добавить новую запись
  [Изменить]    - Редактировать выбранную запись
  [Удалить]     - Удалить выбранную запись
  [Очистить]    - Очистить поле поиска

ПОИСК
────────────────────────────────────────────────────────────────────

Введите текст в поле поиска для фильтрации записей.
Поиск выполняется по:
  • Наименованию
  • Описанию

Поиск выполняется автоматически при вводе текста.

СОСТОЯНИЯ ИНВЕНТАРЯ
────────────────────────────────────────────────────────────────────

  ✅ Отличное           - Новое или в идеальном состоянии
  👍 Хорошее            - Небольшой износ
  ⚠️ Удовлетворительное - Заметный износ
  🔧 Требует ремонта    - Нуждается в ремонте
  ❌ Неисправное        - Не работает
  📦 Списано            - Выведено из эксплуатации";
        }

        private string GetAddInventoryHelp()
        {
            return @"
╔══════════════════════════════════════════════════════════════════╗
║                  ДОБАВЛЕНИЕ ИНВЕНТАРЯ                            ║
╚══════════════════════════════════════════════════════════════════╝

ПРОЦЕСС ДОБАВЛЕНИЯ
────────────────────────────────────────────────────────────────────

1. Нажмите кнопку [Добавить] на главной форме
2. Заполните все обязательные поля
3. Нажмите [Сохранить] для добавления записи

ОБЯЗАТЕЛЬНЫЕ ПОЛЯ
────────────────────────────────────────────────────────────────────

  📌 Наименование *
     Введите название предмета (обязательно)

  📁 Категория *
     Выберите категорию из списка

  🏫 Кабинет *
     Выберите кабинет размещения

  👤 Ответственный *
     Выберите ответственное лицо

  🔢 Инвентарный номер *
     Уникальный номер предмета
     (только буквы, цифры и дефисы)

  📅 Дата покупки *
     Дата приобретения предмета

  💰 Цена *
     Стоимость предмета (от 0 до 100 000 000)

  🔧 Состояние *
     Текущее состояние предмета

НЕОБЯЗАТЕЛЬНЫЕ ПОЛЯ
────────────────────────────────────────────────────────────────────

  📝 Описание
     Дополнительная информация о предмете

ВАЛИДАЦИЯ
────────────────────────────────────────────────────────────────────

При сохранении система проверяет:
  ✓ Заполнение обязательных полей
  ✓ Корректность инвентарного номера
  ✓ Допустимый диапазон цены";
        }

        private string GetEditInventoryHelp()
        {
            return @"
╔══════════════════════════════════════════════════════════════════╗
║                 РЕДАКТИРОВАНИЕ ИНВЕНТАРЯ                         ║
╚══════════════════════════════════════════════════════════════════╝

ОТКРЫТИЕ РЕДАКТИРОВАНИЯ
────────────────────────────────────────────────────────────────────

Способ 1: Выберите запись → нажмите [Изменить]
Способ 2: Дважды кликните по записи в таблице

ПРОЦЕСС РЕДАКТИРОВАНИЯ
────────────────────────────────────────────────────────────────────

1. Откроется форма с текущими данными записи
2. Внесите необходимые изменения
3. Нажмите [Сохранить] для применения изменений
4. Нажмите [Отмена] для отмены изменений

РЕДАКТИРУЕМЫЕ ПОЛЯ
────────────────────────────────────────────────────────────────────

Все поля, доступные при добавлении, также доступны
для редактирования:

  • Наименование
  • Описание
  • Категория
  • Кабинет
  • Ответственный
  • Инвентарный номер
  • Дата покупки
  • Цена
  • Состояние

СОВЕТЫ
────────────────────────────────────────────────────────────────────

💡 Регулярно обновляйте состояние инвентаря
💡 Своевременно меняйте ответственных лиц
💡 Фиксируйте изменения в описании";
        }

        private string GetReportsHelp()
        {
            return @"
╔══════════════════════════════════════════════════════════════════╗
║                         ОТЧЕТЫ                                   ║
╚══════════════════════════════════════════════════════════════════╝

ТИПЫ ОТЧЕТОВ
────────────────────────────────────────────────────────────────────

  📋 Полный отчет по инвентарю
     Все записи без фильтрации

  📁 Отчет по категориям
     Группировка по категориям инвентаря

  🔧 Отчет по состоянию
     Группировка по текущему состоянию

  🏫 Отчет по кабинетам
     Группировка по месту размещения

  👥 Отчет по ответственным
     Группировка по ответственным лицам

ФОРМИРОВАНИЕ ОТЧЕТА
────────────────────────────────────────────────────────────────────

1. Перейдите на вкладку ""Отчеты""
2. Выберите тип отчета из выпадающего списка
3. Нажмите [Сформировать отчет]
4. Просмотрите результаты в таблице

ЭКСПОРТ В EXCEL
────────────────────────────────────────────────────────────────────

1. Сформируйте нужный отчет
2. Нажмите [Экспорт в Excel]
3. Выберите место сохранения файла
4. Файл будет сохранен в формате CSV

ФОРМАТ ЭКСПОРТА
────────────────────────────────────────────────────────────────────

  📄 Формат: CSV (Comma-Separated Values)
  📝 Кодировка: UTF-8 с BOM
  ✓ Совместимость с Microsoft Excel
  ✓ Поддержка кириллицы";
        }

        private string GetUserManagementHelp()
        {
            return @"
╔══════════════════════════════════════════════════════════════════╗
║                  УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ                       ║
╚══════════════════════════════════════════════════════════════════╝

⚠️ ДОСТУП ТОЛЬКО ДЛЯ АДМИНИСТРАТОРОВ

СПИСОК ПОЛЬЗОВАТЕЛЕЙ
────────────────────────────────────────────────────────────────────

Таблица отображает всех пользователей системы:

  👤 Логин           - Имя для входа
  📛 Полное имя      - ФИО пользователя
  🎭 Роль            - Admin / User
  📅 Дата создания   - Когда создан аккаунт
  ✓ Активен          - Статус учетной записи

ОПЕРАЦИИ
────────────────────────────────────────────────────────────────────

  [Добавить]     - Создать нового пользователя
  [Изменить]     - Редактировать пользователя
  [Деактивировать] - Заблокировать пользователя
  [Сбросить пароль] - Сбросить пароль пользователя

СОЗДАНИЕ ПОЛЬЗОВАТЕЛЯ
────────────────────────────────────────────────────────────────────

1. Нажмите [Добавить]
2. Заполните поля:
   • Логин (уникальный)
   • Пароль
   • Полное имя
   • Роль (Admin/User)
3. Нажмите [Сохранить]

РОЛИ И ПРАВА
────────────────────────────────────────────────────────────────────

👑 АДМИНИСТРАТОР (Admin):
   ✓ Все права пользователя
   ✓ Управление пользователями
   ✓ Управление категориями
   ✓ Доступ к статистике
   ✓ Удаление записей

👤 ПОЛЬЗОВАТЕЛЬ (User):
   ✓ Просмотр инвентаря
   ✓ Добавление записей
   ✓ Редактирование записей
   ✓ Формирование отчетов";
        }

        private string GetCategoriesHelp()
        {
            return @"
╔══════════════════════════════════════════════════════════════════╗
║                   УПРАВЛЕНИЕ КАТЕГОРИЯМИ                         ║
╚══════════════════════════════════════════════════════════════════╝

⚠️ ДОСТУП ТОЛЬКО ДЛЯ АДМИНИСТРАТОРОВ

НАЗНАЧЕНИЕ
────────────────────────────────────────────────────────────────────

Категории используются для классификации инвентаря:
  • Мебель
  • Компьютерная техника
  • Спортивный инвентарь
  • Учебные материалы
  • И другие...

ОПЕРАЦИИ
────────────────────────────────────────────────────────────────────

  [Добавить]     - Создать новую категорию
  [Изменить]     - Переименовать категорию
  [Удалить]      - Удалить категорию

⚠️ ВАЖНО: Удаление категории возможно только если
   к ней не привязаны записи инвентаря!

СОВЕТЫ
────────────────────────────────────────────────────────────────────

💡 Создавайте понятные названия категорий
💡 Избегайте дублирования категорий
💡 Объединяйте похожие категории";
        }

        private string GetStatisticsHelp()
        {
            return @"
╔══════════════════════════════════════════════════════════════════╗
║                       СТАТИСТИКА                                 ║
╚══════════════════════════════════════════════════════════════════╝

⚠️ ДОСТУП ТОЛЬКО ДЛЯ АДМИНИСТРАТОРОВ

ПОКАЗАТЕЛИ
────────────────────────────────────────────────────────────────────

📊 ОБЩАЯ СТАТИСТИКА:
   • Общее количество инвентаря
   • Общая стоимость
   • Средняя стоимость предмета

📁 ПО КАТЕГОРИЯМ:
   • Количество в каждой категории
   • Стоимость по категориям
   • Диаграмма распределения

🔧 ПО СОСТОЯНИЮ:
   • Количество исправных
   • Количество требующих ремонта
   • Количество списанных

🏫 ПО КАБИНЕТАМ:
   • Распределение по кабинетам
   • Стоимость оборудования в кабинете

👥 ПО ОТВЕТСТВЕННЫМ:
   • Количество у каждого ответственного
   • Стоимость закрепленного имущества

ЭКСПОРТ
────────────────────────────────────────────────────────────────────

Все статистические данные можно экспортировать
в Excel для дальнейшего анализа.";
        }

        private void HelpForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

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

        private void HeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = headerPanel.ClientRectangle;

            using (SolidBrush brush = new SolidBrush(NeomorphicStyle.SurfaceColor))
            {
                g.FillRectangle(brush, rect);
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
