using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1.Utils
{
    /// <summary>
    /// Модуль для генерации тестовых данных
    /// </summary>
    public static class TestDataGenerator
    {
        private static readonly Random random = new Random();
        private static readonly string[] firstNames = {
            "Иван", "Петр", "Сергей", "Александр", "Дмитрий", "Андрей", "Алексей", "Максим", "Владимир", "Николай",
            "Мария", "Анна", "Елена", "Ольга", "Татьяна", "Наталья", "Ирина", "Светлана", "Екатерина", "Юлия"
        };
        private static readonly string[] lastNames = {
            "Иванов", "Петров", "Сидоров", "Смирнов", "Кузнецов", "Попов", "Соколов", "Лебедев", "Новikov", "Морозов",
            "Волков", "Алексеев", "Лебедев", "Семенов", "Егоров", "Павлов", "Козлов", "Степанов", "Николаев", "Орлов"
        };
        private static readonly string[] categories = {
            "Мебель", "Компьютерная техника", "Спортивный инвентарь", "Учебные материалы", "Канцелярские товары",
            "Офисная техника", "Музыкальные инструменты", "Химические реактивы", "Лабораторное оборудование",
            "Библиотечный фонд", "Медицинское оборудование", "Кухонное оборудование", "Сантехника", "Освещение",
            "Электрооборудование", "Строительные материалы", "Инструменты", "Текстиль", "Посуда", "Другое"
        };
        private static readonly string[] roomNames = {
            "Кабинет математики", "Кабинет физики", "Кабинет химии", "Кабинет биологии", "Кабинет информатики",
            "Кабинет русского языка", "Кабинет литературы", "Кабинет истории", "Кабинет географии", "Кабинет иностранного языка",
            "Спортивный зал", "Актовый зал", "Библиотека", "Столовая", "Медицинский кабинет", "Мастерская", "Лаборатория",
            "Музыкальный класс", "Художественный класс", "Кабинет психолога"
        };
        private static readonly string[] itemNames = {
            "Стол ученический", "Стул ученический", "Доска школьная", "Проектор", "Ноутбук", "Принтер", "Сканер",
            "Микроскоп", "Глобус", "Карта географическая", "Портрет писателя", "Периодическая таблица", "Модель атома",
            "Спортивный мяч", "Гантели", "Скакалка", "Коврик гимнастический", "Теннисный стол", "Баскетбольное кольцо",
            "Учебник", "Тетрадь", "Ручка", "Карандаш", "Линейка", "Циркуль", "Транспортир", "Калькулятор", "Лабораторная посуда",
            "Реактивы химические", "Микроскоп биологический", "Компьютер", "Монитор", "Клавиатура", "Мышь компьютерная",
            "Колонки", "Наушники", "Веб-камера", "Интерактивная доска", "Планшет", "Электронная книга", "Принтер 3D",
            "Робот-конструктор", "Набор для опытов", "Телескоп", "Бинокль", "Термометр", "Барометр", "Компас",
            "Часы настенные", "Календарь", "Плакат учебный", "Стенд информационный", "Шкаф для книг", "Стеллаж",
            "Парта", "Стол учительский", "Стул офисный", "Кресло", "Диван", "Шкаф для одежды", "Вешалка", "Зеркало",
            "Раковина", "Унитаз", "Душевая кабина", "Водонагреватель", "Кондиционер", "Обогреватель", "Вентилятор",
            "Лампа настольная", "Люстра", "Светильник", "Розетка", "Выключатель", "Кабель", "Удлинитель",
            "Инструменты столярные", "Инструменты слесарные", "Дрель", "Шуруповерт", "Паяльник", "Мультиметр",
            "Ткань для шитья", "Нитки", "Иголки", "Ножницы", "Утюг", "Швейная машина", "Оверлок",
            "Тарелка", "Чашка", "Стакан", "Ложка", "Вилка", "Нож", "Кастрюля", "Сковорода", "Чайник", "Кофеварка"
        };
        private static readonly string[] states = {
            "Отличное", "Хорошее", "Удовлетворительное", "Требует ремонта", "Неисправное", "Списано"
        };

        /// <summary>
        /// Генерация всех тестовых данных
        /// </summary>
        public static void GenerateAllTestData(Action<int, string> updateProgress = null)
        {
            try
            {
                Logger.Info("Начало генерации тестовых данных");
                
                int totalSteps = 5;
                int currentStep = 0;

                SafeUpdateProgress(updateProgress, currentStep++, totalSteps, "Генерация пользователей...");
                GenerateUsers(100);

                SafeUpdateProgress(updateProgress, currentStep++, totalSteps, "Генерация категорий...");
                GenerateCategories(100);

                SafeUpdateProgress(updateProgress, currentStep++, totalSteps, "Генерация кабинетов...");
                GenerateClassrooms(100);

                SafeUpdateProgress(updateProgress, currentStep++, totalSteps, "Генерация ответственных лиц...");
                GenerateResponsiblePersons(100);

                SafeUpdateProgress(updateProgress, currentStep++, totalSteps, "Генерация инвентаря...");
                GenerateInventory(100);

                SafeUpdateProgress(updateProgress, totalSteps, totalSteps, "Готово!");
                Logger.Info("Генерация тестовых данных завершена успешно");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка генерации тестовых данных", ex);
                throw;
            }
        }

        private static void SafeUpdateProgress(Action<int, string> updateProgress, int current, int total, string message)
        {
            if (updateProgress != null)
            {
                int percent = (int)((double)current / total * 100);
                updateProgress(percent, message);
            }
        }

        /// <summary>
        /// Генерация пользователей
        /// </summary>
        private static void GenerateUsers(int count)
        {
            try
            {
                DbController db = new DbController();
                string query = "INSERT INTO Users (Username, Password, FullName, Role, IsActive) VALUES (@Username, @Password, @FullName, @Role, @IsActive)";

                for (int i = 1; i <= count; i++)
                {
                    string username = $"user{i:D3}";
                    string password = "123456";
                    string firstName = firstNames[random.Next(firstNames.Length)];
                    string lastName = lastNames[random.Next(lastNames.Length)];
                    string fullName = $"{lastName} {firstName}";
                    string role = random.Next(10) == 0 ? "Admin" : "User"; // 10% админов
                    bool isActive = random.Next(20) != 0; // 95% активных

                    SqlParameter[] parameters = {
                        new SqlParameter("@Username", username),
                        new SqlParameter("@Password", password),
                        new SqlParameter("@FullName", fullName),
                        new SqlParameter("@Role", role),
                        new SqlParameter("@IsActive", isActive)
                    };

                    try
                    {
                        db.ExecuteNonQuery(query, parameters);
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number != 2627) // Игнорируем ошибку дублирования ключа
                            throw;
                    }
                }
                Logger.Info($"Сгенерировано {count} пользователей");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка генерации пользователей", ex);
                throw;
            }
        }

        /// <summary>
        /// Генерация категорий
        /// </summary>
        private static void GenerateCategories(int count)
        {
            try
            {
                DbController db = new DbController();
                string query = "INSERT INTO Categories (CategoryName) VALUES (@CategoryName)";

                for (int i = 1; i <= count; i++)
                {
                    string categoryName;
                    if (i <= categories.Length)
                    {
                        categoryName = categories[i - 1];
                    }
                    else
                    {
                        categoryName = $"Категория {i}";
                    }

                    SqlParameter[] parameters = {
                        new SqlParameter("@CategoryName", categoryName)
                    };

                    try
                    {
                        db.ExecuteNonQuery(query, parameters);
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number != 2627)
                            throw;
                    }
                }
                Logger.Info($"Сгенерировано {count} категорий");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка генерации категорий", ex);
                throw;
            }
        }

        /// <summary>
        /// Генерация кабинетов
        /// </summary>
        private static void GenerateClassrooms(int count)
        {
            try
            {
                DbController db = new DbController();
                string query = "INSERT INTO Classrooms (RoomNumber, RoomName) VALUES (@RoomNumber, @RoomName)";

                for (int i = 1; i <= count; i++)
                {
                    string roomNumber = i.ToString("D3");
                    string roomName;
                    if (i <= roomNames.Length)
                    {
                        roomName = roomNames[i - 1];
                    }
                    else
                    {
                        roomName = $"Кабинет {i}";
                    }

                    SqlParameter[] parameters = {
                        new SqlParameter("@RoomNumber", roomNumber),
                        new SqlParameter("@RoomName", roomName)
                    };

                    try
                    {
                        db.ExecuteNonQuery(query, parameters);
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number != 2627)
                            throw;
                    }
                }
                Logger.Info($"Сгенерировано {count} кабинетов");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка генерации кабинетов", ex);
                throw;
            }
        }

        /// <summary>
        /// Генерация ответственных лиц
        /// </summary>
        private static void GenerateResponsiblePersons(int count)
        {
            try
            {
                DbController db = new DbController();
                string query = "INSERT INTO ResponsiblePersons (FirstName, LastName) VALUES (@FirstName, @LastName)";

                for (int i = 1; i <= count; i++)
                {
                    string firstName = firstNames[random.Next(firstNames.Length)];
                    string lastName = lastNames[random.Next(lastNames.Length)];

                    SqlParameter[] parameters = {
                        new SqlParameter("@FirstName", firstName),
                        new SqlParameter("@LastName", lastName)
                    };

                    try
                    {
                        db.ExecuteNonQuery(query, parameters);
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number != 2627)
                            throw;
                    }
                }
                Logger.Info($"Сгенерировано {count} ответственных лиц");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка генерации ответственных лиц", ex);
                throw;
            }
        }

        /// <summary>
        /// Генерация инвентаря
        /// </summary>
        private static void GenerateInventory(int count)
        {
            try
            {
                DbController db = new DbController();

                // Получаем существующие ID для связей
                DataTable categories = db.GetData("SELECT CategoryID FROM Categories");
                DataTable classrooms = db.GetData("SELECT ClassroomID FROM Classrooms");
                DataTable persons = db.GetData("SELECT PersonID FROM ResponsiblePersons");

                if (categories.Rows.Count == 0 || classrooms.Rows.Count == 0 || persons.Rows.Count == 0)
                {
                    throw new Exception("Необходимо сначала сгенерировать категории, кабинеты и ответственных лиц!");
                }

                string query = @"INSERT INTO Inventory (ItemName, Description, CategoryID, ClassroomID, 
                                ResponsiblePersonID, InventoryNumber, PurchaseDate, PurchasePrice, CurrentState)
                                VALUES (@ItemName, @Description, @CategoryID, @ClassroomID, 
                                @ResponsiblePersonID, @InventoryNumber, @PurchaseDate, @PurchasePrice, @CurrentState)";

                for (int i = 1; i <= count; i++)
                {
                    string itemName = itemNames[random.Next(itemNames.Length)] + $" #{i}";
                    string description = $"Описание для {itemName}. Тестовый предмет инвентаря.";
                    int categoryId = Convert.ToInt32(categories.Rows[random.Next(categories.Rows.Count)]["CategoryID"]);
                    int classroomId = Convert.ToInt32(classrooms.Rows[random.Next(classrooms.Rows.Count)]["ClassroomID"]);
                    int personId = Convert.ToInt32(persons.Rows[random.Next(persons.Rows.Count)]["PersonID"]);
                    string inventoryNumber = $"INV-{i:D6}";
                    DateTime purchaseDate = DateTime.Now.AddDays(-random.Next(3650)); // Последние 10 лет
                    decimal price = Math.Round((decimal)(random.NextDouble() * 100000 + 100), 2); // От 100 до 100100
                    string currentState = states[random.Next(states.Length)];

                    SqlParameter[] parameters = {
                        new SqlParameter("@ItemName", itemName),
                        new SqlParameter("@Description", description),
                        new SqlParameter("@CategoryID", categoryId),
                        new SqlParameter("@ClassroomID", classroomId),
                        new SqlParameter("@ResponsiblePersonID", personId),
                        new SqlParameter("@InventoryNumber", inventoryNumber),
                        new SqlParameter("@PurchaseDate", purchaseDate),
                        new SqlParameter("@PurchasePrice", price),
                        new SqlParameter("@CurrentState", currentState)
                    };

                    try
                    {
                        db.ExecuteNonQuery(query, parameters);
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number != 2627)
                            throw;
                    }
                }
                Logger.Info($"Сгенерировано {count} записей инвентаря");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка генерации инвентаря", ex);
                throw;
            }
        }
    }
}

