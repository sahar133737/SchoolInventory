using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace WindowsFormsApp1.Utils
{
    /// <summary>
    /// Заполнение базы реалистичными учётными данными школьного инвентаря.
    /// </summary>
    public static class TestDataGenerator
    {
        private static readonly Random Rnd = new Random();

        private static readonly string[] PatronymicsMale =
        {
            "Иванович", "Петрович", "Сергеевич", "Александрович", "Дмитриевич", "Андреевич", "Алексеевич",
            "Владимирович", "Николаевич", "Михайлович", "Викторович", "Павлович", "Олегович", "Юрьевич"
        };

        private static readonly string[] PatronymicsFemale =
        {
            "Ивановна", "Петровна", "Сергеевна", "Александровна", "Дмитриевна", "Андреевна", "Алексеевна",
            "Владимировна", "Николаевна", "Михайловна", "Викторовна", "Павловна", "Олеговна", "Юрьевна"
        };

        private static readonly string[] MaleFirst =
            { "Иван", "Пётр", "Сергей", "Александр", "Дмитрий", "Андрей", "Алексей", "Максим", "Владимир", "Николай", "Михаил", "Павел", "Артём", "Роман" };

        private static readonly string[] FemaleFirst =
            { "Мария", "Анна", "Елена", "Ольга", "Татьяна", "Наталья", "Ирина", "Светлана", "Екатерина", "Юлия", "Виктория", "Дарья", "Алина", "Полина" };

        private static readonly string[] LastNames =
        {
            "Иванов", "Петров", "Сидоров", "Смирнов", "Кузнецов", "Попов", "Соколов", "Лебедев", "Новиков", "Морозов",
            "Волков", "Алексеев", "Семёнов", "Егоров", "Павлов", "Козлов", "Степанов", "Николаев", "Орлов", "Захаров",
            "Фёдоров", "Михайлов", "Беляев", "Тарасов", "Борисов", "Комаров", "Киселёв", "Макаров", "Андреев", "Куликов"
        };

        private static readonly string[] Categories =
        {
            "Мебель учебных кабинетов", "Мебель административных помещений", "Компьютерная и периферийная техника",
            "Мультимедийное оборудование", "Спортивный инвентарь", "Учебно-наглядные пособия", "Канцелярские принадлежности",
            "Офисная оргтехника", "Музыкальные инструменты", "Лабораторное оборудование", "Библиотечный фонд",
            "Медицинское и санитарное оборудование", "Оборудование пищеблока", "Средства пожарной безопасности",
            "Хозяйственный инвентарь", "Текстиль и швейное оборудование", "Осветительные приборы",
            "Климатическое оборудование", "Инструмент и оборудование мастерских", "Инвентарь актового зала"
        };

        private static readonly (string Number, string Name)[] Classrooms =
        {
            ("101", "Кабинет математики"), ("102", "Кабинет русского языка"), ("103", "Кабинет литературы"),
            ("104", "Кабинет истории и обществознания"), ("105", "Кабинет информатики"),
            ("106", "Кабинет физики"), ("107", "Кабинет химии"), ("108", "Кабинет биологии"),
            ("109", "Кабинет географии"), ("110", "Кабинет иностранного языка"),
            ("201", "Кабинет начальных классов"), ("202", "Кабинет начальных классов"),
            ("203", "Кабинет технологии"), ("204", "Кабинет изобразительного искусства"),
            ("205", "Кабинет музыки"), ("206", "Кабинет ОБЖ"), ("207", "Кабинет психолога и педагога"),
            ("208", "Методический кабинет"), ("209", "Кабинет дополнительного образования"),
            ("210", "Ресурсный центр"),
            ("301", "Спортивный зал"), ("302", "Склад спортивного инвентаря"),
            ("303", "Библиотека"), ("304", "Читальный зал"), ("305", "Актовый зал"),
            ("306", "Столовая"), ("307", "Медицинский кабинет"), ("308", "Администрация"),
            ("309", "Приёмная директора"), ("310", "Бухгалтерия и кадры")
        };

        private static readonly string[] States = { "Отличное", "Хорошее", "Удовлетворительное", "Требует ремонта", "Неисправное" };
        private static readonly string[] Statuses = { "В эксплуатации", "В эксплуатации", "В эксплуатации", "На складе" };

        private static readonly string[] Suppliers =
        {
            "ООО «Комплекс-Снаб»", "АО «РегионТорг»", "ИП Козлов В.С.", "ООО «Школьная мебель»",
            "ООО «ИнфоТех Плюс»", "ООО «СпортИнвент»", "ГК «Просвещение»", "ООО «Лаборатория-Про»"
        };

        private static readonly CatalogEntry[] Catalog =
        {
            new CatalogEntry("Стол ученический двухместный", "Мебель учебных кабинетов", 3200, 5500, "ЛДСП, металлокаркас, регулировка по высоте"),
            new CatalogEntry("Стул ученический", "Мебель учебных кабинетов", 900, 1800, "Пластиковое сиденье, хромированный каркас"),
            new CatalogEntry("Стол учительский", "Мебель учебных кабинетов", 7500, 14000, "Тумба с ящиками, столешница 140×70 см"),
            new CatalogEntry("Кресло учителя", "Мебель учебных кабинетов", 4500, 12000, "Ортопедическое, с подлокотниками"),
            new CatalogEntry("Доска маркерная", "Мебель учебных кабинетов", 6000, 15000, "Керамическое покрытие 3×1,2 м"),
            new CatalogEntry("Доска меловая", "Мебель учебных кабинетов", 3500, 8000, "Трёхсекционная, алюминиевая рама"),
            new CatalogEntry("Шкаф для учебников", "Мебель учебных кабинетов", 8000, 22000, "Закрытый, 4 полки"),
            new CatalogEntry("Шкаф для одежды", "Мебель учебных кабинетов", 5000, 11000, "Металлический, 2 отделения"),
            new CatalogEntry("Стеллаж архивный", "Мебель административных помещений", 4000, 9500, "5 полок, 180×90×40 см"),
            new CatalogEntry("Сейф металлический", "Мебель административных помещений", 12000, 35000, "Класс взломостойкости I"),
            new CatalogEntry("Системный блок", "Компьютерная и периферийная техника", 28000, 52000, "Intel Core i5, 8 ГБ ОЗУ, SSD 256 ГБ"),
            new CatalogEntry("Монитор 24\"", "Компьютерная и периферийная техника", 9000, 18000, "Full HD, HDMI, регулировка наклона"),
            new CatalogEntry("Ноутбук", "Компьютерная и периферийная техника", 38000, 75000, "Для педагога, Windows 11 Pro"),
            new CatalogEntry("Принтер лазерный", "Компьютерная и периферийная техника", 12000, 28000, "А4, двусторонняя печать, Wi-Fi"),
            new CatalogEntry("МФУ", "Офисная оргтехника", 18000, 42000, "Печать, сканирование, копирование А4"),
            new CatalogEntry("Проектор мультимедийный", "Мультимедийное оборудование", 25000, 65000, "3500 люмен, HDMI, потолочное крепление"),
            new CatalogEntry("Интерактивная панель 65\"", "Мультимедийное оборудование", 95000, 180000, "4K, встроенный ПК, стилус"),
            new CatalogEntry("Экран проекционный", "Мультимедийное оборудование", 3500, 9000, "Настенный, 200×200 см"),
            new CatalogEntry("Акустическая система", "Мультимедийное оборудование", 8000, 25000, "Комплект для актового зала"),
            new CatalogEntry("Мяч баскетбольный", "Спортивный инвентарь", 1800, 4500, "Размер 7, кожзам"),
            new CatalogEntry("Мяч футбольный", "Спортивный инвентарь", 1500, 3800, "Размер 5, сшивной"),
            new CatalogEntry("Мяч волейбольный", "Спортивный инвентарь", 1400, 3500, "Официальный размер"),
            new CatalogEntry("Гимнастические коврики", "Спортивный инвентарь", 2500, 6000, "Комплект 10 шт., 1×2 м"),
            new CatalogEntry("Скакалки", "Спортивный инвентарь", 400, 1200, "Набор 20 шт., длина 2,5 м"),
            new CatalogEntry("Гантели разборные", "Спортивный инвентарь", 3500, 8000, "Набор 2–10 кг"),
            new CatalogEntry("Теннисный стол", "Спортивный инвентарь", 18000, 35000, "Складной, для помещений"),
            new CatalogEntry("Глобус политический", "Учебно-наглядные пособия", 2200, 5500, "Диаметр 32 см, подсветка"),
            new CatalogEntry("Карта физическая России", "Учебно-наглядные пособия", 1200, 3500, "Настенная, 1:4 000 000"),
            new CatalogEntry("Микроскоп биологический", "Лабораторное оборудование", 12000, 28000, "Увеличение 40–400×, LED"),
            new CatalogEntry("Модель Солнечной системы", "Учебно-наглядные пособия", 4500, 12000, "Подвесная наглядная модель"),
            new CatalogEntry("Периодическая таблица Менделеева", "Учебно-наглядные пособия", 800, 2500, "Настенная, 1×1,5 м"),
            new CatalogEntry("Комплект лабораторной посуды", "Лабораторное оборудование", 5000, 15000, "Стекло, 25 наименований"),
            new CatalogEntry("Весы лабораторные", "Лабораторное оборудование", 8000, 22000, "Точность 0,01 г"),
            new CatalogEntry("Фортепиано цифровое", "Музыкальные инструменты", 45000, 95000, "88 клавиш, педаль, стойка"),
            new CatalogEntry("Гитара акустическая", "Музыкальные инструменты", 3500, 9000, "Для класса сольфеджио"),
            new CatalogEntry("Баян", "Музыкальные инструменты", 25000, 55000, "Учебный, 96 басов"),
            new CatalogEntry("Аптечка первой помощи", "Медицинское и санитарное оборудование", 1200, 3500, "Укомплектована по приказу Минздрава"),
            new CatalogEntry("Кровать медицинская", "Медицинское и санитарное оборудование", 8000, 18000, "Складная, с матрасом"),
            new CatalogEntry("Тонометр", "Медицинское и санитарное оборудование", 1500, 4500, "Автоматический, с адаптером"),
            new CatalogEntry("Огнетушитель ОП-4", "Средства пожарной безопасности", 1800, 4200, "Порошковый, повторная зарядка"),
            new CatalogEntry("Пожарная сигнализация", "Средства пожарной безопасности", 15000, 45000, "Пункт управления, 8 датчиков"),
            new CatalogEntry("Кондиционер настенный", "Климатическое оборудование", 22000, 48000, "Сплит-система 3,5 кВт"),
            new CatalogEntry("Обогреватель масляный", "Климатическое оборудование", 2500, 6000, "7 секций, терморегулятор"),
            new CatalogEntry("Вентилятор напольный", "Климатическое оборудование", 1200, 3500, "Осевой, 3 скорости"),
            new CatalogEntry("Люстра потолочная", "Осветительные приборы", 3500, 12000, "Светодиодная, 4 плафона"),
            new CatalogEntry("Светильник потолочный", "Осветительные приборы", 800, 2500, "LED 36 Вт, рассеянный свет"),
            new CatalogEntry("Лампа настольная", "Осветительные приборы", 600, 2200, "Светодиодная, гибкий кронштейн"),
            new CatalogEntry("Швейная машина", "Текстиль и швейное оборудование", 12000, 28000, "Электромеханическая, 32 операции"),
            new CatalogEntry("Плита электрическая", "Оборудование пищеблока", 18000, 42000, "4 конфорки, духовой шкаф"),
            new CatalogEntry("Холодильник", "Оборудование пищеблока", 22000, 55000, "Двухкамерный, класс A+"),
            new CatalogEntry("Микроволновая печь", "Оборудование пищеблока", 4500, 12000, "23 л, гриль"),
        };

        private sealed class CatalogEntry
        {
            public string Name { get; }
            public string Category { get; }
            public decimal PriceMin { get; }
            public decimal PriceMax { get; }
            public string Spec { get; }

            public CatalogEntry(string name, string category, decimal priceMin, decimal priceMax, string spec)
            {
                Name = name;
                Category = category;
                PriceMin = priceMin;
                PriceMax = priceMax;
                Spec = spec;
            }
        }

        public static void GenerateAllTestData(Action<int, string> updateProgress = null)
        {
            try
            {
                Logger.Info("Начало заполнения базы учётными данными");
                int totalSteps = 5;
                int step = 0;

                SafeUpdateProgress(updateProgress, step++, totalSteps, "Добавление пользователей...");
                GenerateUsers(100);

                SafeUpdateProgress(updateProgress, step++, totalSteps, "Добавление категорий...");
                GenerateCategories();

                SafeUpdateProgress(updateProgress, step++, totalSteps, "Добавление кабинетов...");
                GenerateClassrooms();

                SafeUpdateProgress(updateProgress, step++, totalSteps, "Добавление ответственных лиц...");
                GenerateResponsiblePersons(100);

                SafeUpdateProgress(updateProgress, step++, totalSteps, "Добавление инвентарных карточек...");
                GenerateInventory(100);

                SafeUpdateProgress(updateProgress, totalSteps, totalSteps, "Готово!");
                Logger.Info("Заполнение базы учётными данными завершено");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка заполнения базы данными", ex);
                throw;
            }
        }

        private static void SafeUpdateProgress(Action<int, string> updateProgress, int current, int total, string message)
        {
            if (updateProgress == null) return;
            int percent = total > 0 ? (int)((double)current / total * 100) : 0;
            updateProgress(percent, message);
        }

        private static void GenerateUsers(int count)
        {
            var db = new DbController();
            var usedLogins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            const string query = "INSERT INTO Users (Username, Password, FullName, Role, IsActive) VALUES (@Username, @Password, @FullName, @Role, @IsActive)";

            for (int i = 0; i < count; i++)
            {
                bool female = Rnd.Next(2) == 0;
                string first = female ? Pick(FemaleFirst) : Pick(MaleFirst);
                string last = Pick(LastNames);
                string patronymic = female ? Pick(PatronymicsFemale) : Pick(PatronymicsMale);
                string fullName = $"{last} {first} {patronymic}";
                string login = BuildLogin(last, first, usedLogins);
                string role = Rnd.Next(12) == 0 ? "Admin" : "User";
                bool isActive = Rnd.Next(25) != 0;

                TryInsert(db, query, new[]
                {
                    new SqlParameter("@Username", login),
                    new SqlParameter("@Password", "123456"),
                    new SqlParameter("@FullName", fullName),
                    new SqlParameter("@Role", role),
                    new SqlParameter("@IsActive", isActive)
                });
            }
            Logger.Info($"Добавлено до {count} пользователей");
        }

        private static string BuildLogin(string lastName, string firstName, HashSet<string> used)
        {
            string baseLogin = Transliterate(lastName) + "." + Transliterate(firstName.Substring(0, 1));
            string login = baseLogin;
            int n = 2;
            while (used.Contains(login))
                login = baseLogin + n++;
            used.Add(login);
            return login;
        }

        private static string Transliterate(string s)
        {
            var map = new Dictionary<char, string>
            {
                {'а',"a"},{'б',"b"},{'в',"v"},{'г',"g"},{'д',"d"},{'е',"e"},{'ё',"e"},{'ж',"zh"},{'з',"z"},
                {'и',"i"},{'й',"y"},{'к',"k"},{'л',"l"},{'м',"m"},{'н',"n"},{'о',"o"},{'п',"p"},{'р',"r"},
                {'с',"s"},{'т',"t"},{'у',"u"},{'ф',"f"},{'х',"kh"},{'ц',"ts"},{'ч',"ch"},{'ш',"sh"},
                {'щ',"sch"},{'ъ',""},{'ы',"y"},{'ь',""},{'э',"e"},{'ю',"yu"},{'я',"ya"},
                {'А',"a"},{'Б',"b"},{'В',"v"},{'Г',"g"},{'Д',"d"},{'Е',"e"},{'Ё',"e"},{'Ж',"zh"},{'З',"z"},
                {'И',"i"},{'Й',"y"},{'К',"k"},{'Л',"l"},{'М',"m"},{'Н',"n"},{'О',"o"},{'П',"p"},{'Р',"r"},
                {'С',"s"},{'Т',"t"},{'У',"u"},{'Ф',"f"},{'Х',"kh"},{'Ц',"ts"},{'Ч',"ch"},{'Ш',"sh"},
                {'Щ',"sch"},{'Ъ',""},{'Ы',"y"},{'Ь',""},{'Э',"e"},{'Ю',"yu"},{'Я',"ya"}
            };
            var sb = new StringBuilder();
            foreach (char c in s)
                sb.Append(map.TryGetValue(c, out string lat) ? lat : c.ToString().ToLowerInvariant());
            return sb.ToString().ToLowerInvariant();
        }

        private static void GenerateCategories()
        {
            var db = new DbController();
            foreach (string cat in Categories)
            {
                TryInsert(db, "INSERT INTO Categories (CategoryName) VALUES (@CategoryName)",
                    new[] { new SqlParameter("@CategoryName", cat) });
            }
            // Дополнительные уточняющие категории без слов «тест», «демо»
            string[] extra =
            {
                "Инвентарь спортзала", "Оборудование начальной школы", "Техника для кабинета информатики",
                "Инвентарь библиотеки", "Оснащение медкабинета", "Инвентарь столовой",
                "Оборудование кабинета химии", "Оборудование кабинета физики", "Наглядность по истории",
                "Кабинет иностранного языка — техника"
            };
            foreach (string cat in extra)
            {
                TryInsert(db, "INSERT INTO Categories (CategoryName) VALUES (@CategoryName)",
                    new[] { new SqlParameter("@CategoryName", cat) });
            }
            Logger.Info("Категории добавлены");
        }

        private static void GenerateClassrooms()
        {
            var db = new DbController();
            foreach (var room in Classrooms)
            {
                TryInsert(db, "INSERT INTO Classrooms (RoomNumber, RoomName) VALUES (@RoomNumber, @RoomName)",
                    new[]
                    {
                        new SqlParameter("@RoomNumber", room.Number),
                        new SqlParameter("@RoomName", room.Name)
                    });
            }
            Logger.Info("Кабинеты добавлены");
        }

        private static void GenerateResponsiblePersons(int count)
        {
            var db = new DbController();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < count; i++)
            {
                bool female = Rnd.Next(2) == 0;
                string first = female ? Pick(FemaleFirst) : Pick(MaleFirst);
                string last = Pick(LastNames);
                string key = last + "|" + first;
                if (!used.Add(key)) { i--; continue; }

                TryInsert(db, "INSERT INTO ResponsiblePersons (FirstName, LastName) VALUES (@FirstName, @LastName)",
                    new[]
                    {
                        new SqlParameter("@FirstName", first),
                        new SqlParameter("@LastName", last)
                    });
            }
            Logger.Info($"Добавлено до {count} ответственных лиц");
        }

        private static void GenerateInventory(int count)
        {
            var db = new DbController();
            var categories = db.GetData("SELECT CategoryID, CategoryName FROM Categories");
            var classrooms = db.GetData("SELECT ClassroomID, RoomNumber FROM Classrooms");
            var persons = db.GetData("SELECT PersonID FROM ResponsiblePersons");

            if (categories.Rows.Count == 0 || classrooms.Rows.Count == 0 || persons.Rows.Count == 0)
                throw new Exception("Сначала должны быть созданы категории, кабинеты и ответственные лица.");

            var catByName = categories.AsEnumerable()
                .ToDictionary(r => r.Field<string>("CategoryName"), r => r.Field<int>("CategoryID"), StringComparer.OrdinalIgnoreCase);

            var usedNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            const string query = @"INSERT INTO Inventory (ItemName, Description, CategoryID, ClassroomID, 
                ResponsiblePersonID, InventoryNumber, PurchaseDate, PurchasePrice, CurrentState, Status)
                VALUES (@ItemName, @Description, @CategoryID, @ClassroomID, 
                @ResponsiblePersonID, @InventoryNumber, @PurchaseDate, @PurchasePrice, @CurrentState, @Status)";

            for (int i = 0; i < count; i++)
            {
                var entry = Catalog[Rnd.Next(Catalog.Length)];
                int categoryId = ResolveCategoryId(entry.Category, catByName, categories);
                var roomRow = classrooms.Rows[Rnd.Next(classrooms.Rows.Count)];
                int classroomId = roomRow.Field<int>("ClassroomID");
                string roomNumber = roomRow.Field<string>("RoomNumber");
                int personId = Convert.ToInt32(persons.Rows[Rnd.Next(persons.Rows.Count)]["PersonID"]);

                int year = Rnd.Next(2016, DateTime.Today.Year + 1);
                DateTime purchaseDate = RandomSchoolDate(year);
                decimal price = Math.Round(entry.PriceMin + (decimal)Rnd.NextDouble() * (entry.PriceMax - entry.PriceMin), 2);
                string invNumber = BuildInventoryNumber(year, roomNumber, usedNumbers);
                string state = Pick(States);
                string status = state == "Неисправное" || state == "Требует ремонта"
                    ? (Rnd.Next(3) == 0 ? "На складе" : "В эксплуатации")
                    : Pick(Statuses);
                string contract = Rnd.Next(100, 999).ToString();
                string supplier = Pick(Suppliers);
                string description =
                    $"{entry.Spec}. Поставка по договору № {contract}/{year} от {purchaseDate:dd.MM.yyyy}. " +
                    $"Поставщик: {supplier}. Принято на ответственное хранение.";

                string itemName = entry.Name;
                if (Rnd.Next(4) == 0)
                    itemName += $" ({roomNumber})";

                TryInsert(db, query, new[]
                {
                    new SqlParameter("@ItemName", itemName),
                    new SqlParameter("@Description", description),
                    new SqlParameter("@CategoryID", categoryId),
                    new SqlParameter("@ClassroomID", classroomId),
                    new SqlParameter("@ResponsiblePersonID", personId),
                    new SqlParameter("@InventoryNumber", invNumber),
                    new SqlParameter("@PurchaseDate", purchaseDate),
                    new SqlParameter("@PurchasePrice", price),
                    new SqlParameter("@CurrentState", state),
                    new SqlParameter("@Status", status)
                });
            }
            Logger.Info($"Добавлено до {count} карточек инвентаря");
        }

        private static int ResolveCategoryId(string preferred, Dictionary<string, int> byName, DataTable all)
        {
            if (byName.TryGetValue(preferred, out int id)) return id;
            return Convert.ToInt32(all.Rows[Rnd.Next(all.Rows.Count)]["CategoryID"]);
        }

        private static string BuildInventoryNumber(int year, string roomNumber, HashSet<string> used)
        {
            string num;
            do
            {
                int seq = Rnd.Next(1, 99999);
                num = $"ОС-{year}-{roomNumber}-{seq:D4}";
            } while (!used.Add(num));
            return num;
        }

        private static DateTime RandomSchoolDate(int year)
        {
            int month = Rnd.Next(8, 10) == 0 ? Rnd.Next(1, 7) : Rnd.Next(8, 13);
            if (month > 12) month = 12;
            int day = Rnd.Next(1, DateTime.DaysInMonth(year, month) + 1);
            return new DateTime(year, month, day);
        }

        private static void TryInsert(DbController db, string query, SqlParameter[] parameters)
        {
            try
            {
                db.ExecuteNonQuery(query, parameters);
            }
            catch (SqlException ex)
            {
                if (ex.Number != 2627 && ex.Number != 2601)
                    throw;
            }
        }

        private static string Pick(string[] array) => array[Rnd.Next(array.Length)];
    }
}
