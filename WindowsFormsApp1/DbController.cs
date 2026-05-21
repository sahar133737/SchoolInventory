using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using WindowsFormsApp1.Models;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1
{
    public class DbController
    {
        private string connectionString;
        
        public DbController()
        {
            connectionString = AppConfig.ConnectionString;
        }
        
        public DbController(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        // Метод для выполнения запросов без возврата данных
        public int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                            command.Parameters.AddRange(parameters);
                        
                        int result = command.ExecuteNonQuery();
                        Logger.Info($"Выполнен запрос: {query.Substring(0, Math.Min(100, query.Length))}...");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка выполнения запроса: {query.Substring(0, Math.Min(100, query.Length))}...", ex);
                throw;
            }
        }

        // Метод для получения данных
        public DataTable GetData(string query, SqlParameter[] parameters = null)
        {
            try
            {
                DataTable dataTable = new DataTable();
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                            command.Parameters.AddRange(parameters);
                        
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
                Logger.Debug($"Получено {dataTable.Rows.Count} записей из БД");
                return dataTable;
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка получения данных: {query.Substring(0, Math.Min(100, query.Length))}...", ex);
                throw;
            }
        }

        // Метод для получения скалярного значения
        public object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                            command.Parameters.AddRange(parameters);
                        
                        return command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка выполнения скалярного запроса: {query.Substring(0, Math.Min(100, query.Length))}...", ex);
                throw;
            }
        }
    }

    // Класс для работы с инвентарем
    public class InventoryManager
    {
        private DbController db = new DbController();

        private const string InventorySelectCore = @"SELECT i.InventoryID, i.ItemName, i.Description, c.CategoryName, 
                                r.RoomNumber + ' - ' + r.RoomName as Classroom,
                                p.FirstName + ' ' + p.LastName as ResponsiblePerson,
                                i.InventoryNumber, i.PurchaseDate, i.PurchasePrice, 
                                i.CurrentState, i.Status
                                FROM Inventory i
                                LEFT JOIN Categories c ON i.CategoryID = c.CategoryID
                                LEFT JOIN Classrooms r ON i.ClassroomID = r.ClassroomID
                                LEFT JOIN ResponsiblePersons p ON i.ResponsiblePersonID = p.PersonID";

        public DataTable GetAllInventory() => GetFilteredInventory(null);

        public DataTable GetFilteredInventory(InventoryFilterCriteria criteria)
        {
            try
            {
                string where = BuildFilterWhere(criteria, "i");
                string query = InventorySelectCore + where + " ORDER BY i.ItemName";
                return db.GetData(query, BuildFilterParameters(criteria));
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка получения списка инвентаря", ex);
                throw new Exception("Не удалось загрузить данные инвентаря. Проверьте подключение к базе данных.", ex);
            }
        }

        public DataTable GetInventoryForEdit(int inventoryId)
        {
            try
            {
                string query = @"SELECT InventoryID, ItemName, Description, CategoryID, ClassroomID, ResponsiblePersonID,
                                          InventoryNumber, PurchaseDate, PurchasePrice, CurrentState
                                   FROM Inventory WHERE InventoryID = @id";
                SqlParameter[] p = { new SqlParameter("@id", inventoryId) };
                return db.GetData(query, p);
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка получения данных инвентаря для редактирования (ID: {inventoryId})", ex);
                throw new Exception($"Не удалось загрузить данные записи. ID: {inventoryId}", ex);
            }
        }

        public DataTable GetCategories()
        {
            try
            {
                return db.GetData("SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка получения списка категорий", ex);
                throw new Exception("Не удалось загрузить список категорий.", ex);
            }
        }

        public DataTable GetClassrooms()
        {
            try
            {
                return db.GetData("SELECT ClassroomID, RoomNumber + ' - ' + RoomName AS Name FROM Classrooms ORDER BY RoomNumber");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка получения списка кабинетов", ex);
                throw new Exception("Не удалось загрузить список кабинетов.", ex);
            }
        }

        public DataTable GetResponsiblePersons()
        {
            try
            {
                return db.GetData("SELECT PersonID, FirstName + ' ' + LastName AS Name FROM ResponsiblePersons ORDER BY LastName");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка получения списка ответственных лиц", ex);
                throw new Exception("Не удалось загрузить список ответственных лиц.", ex);
            }
        }

        public bool AddInventoryItem(string itemName, string description, int categoryId, 
                                   int classroomId, int responsiblePersonId, string inventoryNumber,
                                   DateTime purchaseDate, decimal price, string currentState)
        {
            try
            {
                string query = @"INSERT INTO Inventory (ItemName, Description, CategoryID, ClassroomID, 
                               ResponsiblePersonID, InventoryNumber, PurchaseDate, PurchasePrice, CurrentState)
                               VALUES (@ItemName, @Description, @CategoryID, @ClassroomID, 
                               @ResponsiblePersonID, @InventoryNumber, @PurchaseDate, @PurchasePrice, @CurrentState)";
                
                SqlParameter[] parameters = {
                    new SqlParameter("@ItemName", itemName ?? (object)DBNull.Value),
                    new SqlParameter("@Description", description ?? (object)DBNull.Value),
                    new SqlParameter("@CategoryID", categoryId),
                    new SqlParameter("@ClassroomID", classroomId),
                    new SqlParameter("@ResponsiblePersonID", responsiblePersonId),
                    new SqlParameter("@InventoryNumber", inventoryNumber ?? (object)DBNull.Value),
                    new SqlParameter("@PurchaseDate", purchaseDate),
                    new SqlParameter("@PurchasePrice", price),
                    new SqlParameter("@CurrentState", currentState ?? (object)DBNull.Value)
                };

                return db.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (SqlException ex)
            {
                Logger.Error($"Ошибка добавления инвентаря: {itemName}", ex);
                if (ex.Number == 2627) // Дублирование ключа
                    throw new Exception("Инвентарный номер уже существует в базе данных.", ex);
                throw new Exception("Не удалось добавить запись. Проверьте корректность данных.", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка добавления инвентаря: {itemName}", ex);
                throw new Exception("Не удалось добавить запись инвентаря.", ex);
            }
        }

        public bool UpdateInventoryItem(int inventoryId, string itemName, string description, int categoryId,
                                        int classroomId, int responsiblePersonId, string inventoryNumber,
                                        DateTime purchaseDate, decimal price, string currentState)
        {
            try
            {
                string query = @"UPDATE Inventory SET ItemName=@ItemName, Description=@Description, CategoryID=@CategoryID,
                                  ClassroomID=@ClassroomID, ResponsiblePersonID=@ResponsiblePersonID, InventoryNumber=@InventoryNumber,
                                  PurchaseDate=@PurchaseDate, PurchasePrice=@PurchasePrice, CurrentState=@CurrentState
                                  WHERE InventoryID=@InventoryID";
                SqlParameter[] parameters = {
                    new SqlParameter("@InventoryID", inventoryId),
                    new SqlParameter("@ItemName", itemName ?? (object)DBNull.Value),
                    new SqlParameter("@Description", description ?? (object)DBNull.Value),
                    new SqlParameter("@CategoryID", categoryId),
                    new SqlParameter("@ClassroomID", classroomId),
                    new SqlParameter("@ResponsiblePersonID", responsiblePersonId),
                    new SqlParameter("@InventoryNumber", inventoryNumber ?? (object)DBNull.Value),
                    new SqlParameter("@PurchaseDate", purchaseDate),
                    new SqlParameter("@PurchasePrice", price),
                    new SqlParameter("@CurrentState", currentState ?? (object)DBNull.Value)
                };
                return db.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка обновления инвентаря (ID: {inventoryId})", ex);
                throw new Exception($"Не удалось обновить запись. ID: {inventoryId}", ex);
            }
        }

        public bool DeleteInventoryItem(int inventoryId)
        {
            try
            {
                string query = "DELETE FROM Inventory WHERE InventoryID=@id";
                SqlParameter[] p = { new SqlParameter("@id", inventoryId) };
                return db.ExecuteNonQuery(query, p) > 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка удаления инвентаря (ID: {inventoryId})", ex);
                throw new Exception($"Не удалось удалить запись. ID: {inventoryId}", ex);
            }
        }

        public string[] GetDistinctStates()
        {
            try
            {
                var dt = db.GetData(@"SELECT DISTINCT CurrentState FROM Inventory 
                    WHERE CurrentState IS NOT NULL AND LTRIM(RTRIM(CurrentState)) <> '' ORDER BY CurrentState");
                return dt.AsEnumerable().Select(r => r.Field<string>("CurrentState")).ToArray();
            }
            catch { return new string[0]; }
        }

        public string[] GetDistinctStatuses()
        {
            try
            {
                var dt = db.GetData(@"SELECT DISTINCT Status FROM Inventory 
                    WHERE Status IS NOT NULL AND LTRIM(RTRIM(Status)) <> '' ORDER BY Status");
                return dt.AsEnumerable().Select(r => r.Field<string>("Status")).ToArray();
            }
            catch { return new string[0]; }
        }

        private static string BuildFilterWhere(InventoryFilterCriteria filter, string alias)
        {
            if (filter == null || !filter.HasActiveFilters) return string.Empty;
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                parts.Add($"({alias}.ItemName LIKE @Search OR {alias}.Description LIKE @Search OR {alias}.InventoryNumber LIKE @Search)");
            if (filter.CategoryId.HasValue) parts.Add($"{alias}.CategoryID = @CategoryId");
            if (filter.ClassroomId.HasValue) parts.Add($"{alias}.ClassroomID = @ClassroomId");
            if (filter.ResponsiblePersonId.HasValue) parts.Add($"{alias}.ResponsiblePersonID = @ResponsiblePersonId");
            if (!string.IsNullOrWhiteSpace(filter.CurrentState)) parts.Add($"{alias}.CurrentState = @CurrentState");
            if (!string.IsNullOrWhiteSpace(filter.Status)) parts.Add($"{alias}.Status = @Status");
            if (filter.DateFrom.HasValue) parts.Add($"{alias}.PurchaseDate >= @DateFrom");
            if (filter.DateTo.HasValue) parts.Add($"{alias}.PurchaseDate <= @DateTo");
            if (filter.PriceFrom.HasValue) parts.Add($"{alias}.PurchasePrice >= @PriceFrom");
            if (filter.PriceTo.HasValue) parts.Add($"{alias}.PurchasePrice <= @PriceTo");
            return parts.Count > 0 ? " WHERE " + string.Join(" AND ", parts) : string.Empty;
        }

        private static SqlParameter[] BuildFilterParameters(InventoryFilterCriteria filter)
        {
            if (filter == null || !filter.HasActiveFilters) return null;
            var list = new List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                list.Add(new SqlParameter("@Search", "%" + filter.SearchText.Trim() + "%"));
            if (filter.CategoryId.HasValue) list.Add(new SqlParameter("@CategoryId", filter.CategoryId.Value));
            if (filter.ClassroomId.HasValue) list.Add(new SqlParameter("@ClassroomId", filter.ClassroomId.Value));
            if (filter.ResponsiblePersonId.HasValue) list.Add(new SqlParameter("@ResponsiblePersonId", filter.ResponsiblePersonId.Value));
            if (!string.IsNullOrWhiteSpace(filter.CurrentState)) list.Add(new SqlParameter("@CurrentState", filter.CurrentState));
            if (!string.IsNullOrWhiteSpace(filter.Status)) list.Add(new SqlParameter("@Status", filter.Status));
            if (filter.DateFrom.HasValue) list.Add(new SqlParameter("@DateFrom", filter.DateFrom.Value.Date));
            if (filter.DateTo.HasValue) list.Add(new SqlParameter("@DateTo", filter.DateTo.Value.Date));
            if (filter.PriceFrom.HasValue) list.Add(new SqlParameter("@PriceFrom", filter.PriceFrom.Value));
            if (filter.PriceTo.HasValue) list.Add(new SqlParameter("@PriceTo", filter.PriceTo.Value));
            return list.Count > 0 ? list.ToArray() : null;
        }
    }

    // Класс для работы с пользователями
    public class UserManager
    {
        private DbController db = new DbController();

        // Инициализация таблицы Users (если не существует)
        public void InitializeUsersTable()
        {
            try
            {
                string createTableQuery = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[Users] (
                            [UserID] INT IDENTITY(1,1) PRIMARY KEY,
                            [Username] NVARCHAR(50) NOT NULL UNIQUE,
                            [Password] NVARCHAR(255) NOT NULL,
                            [FullName] NVARCHAR(100) NOT NULL,
                            [Role] NVARCHAR(50) NOT NULL DEFAULT 'User',
                            [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
                            [IsActive] BIT NOT NULL DEFAULT 1
                        );

                        -- Создание начального пользователя (admin/admin)
                        IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
                        BEGIN
                            INSERT INTO Users (Username, Password, FullName, Role) 
                            VALUES ('admin', 'admin', 'Администратор', 'Admin');
                        END
                    END";

                db.ExecuteNonQuery(createTableQuery);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации таблицы пользователей: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Проверка авторизации
        public bool AuthenticateUser(string username, string password)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password AND IsActive = 1";
                SqlParameter[] parameters = {
                    new SqlParameter("@Username", username ?? ""),
                    new SqlParameter("@Password", password ?? "")
                };
                object result = db.ExecuteScalar(query, parameters);
                return result != null && Convert.ToInt32(result) > 0;
            }
            catch (SqlException ex) when (ex.Number == 2 || ex.Number == 53 || ex.Number == -1 || ex.Number == 4060)
            {
                Logger.Error($"Ошибка подключения к БД при авторизации: {username}", ex);
                throw new Exception(
                    "Не удалось подключиться к базе данных. Проверьте, что SQL Server запущен, база SchoolInventoryDB создана, " +
                    "и в файле WindowsFormsApp1.exe.config указан правильный Server (см. App.config.example).", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка авторизации пользователя: {username}", ex);
                return false;
            }
        }

        // Получение информации о пользователе
        public DataTable GetUserInfo(string username)
        {
            try
            {
                string query = "SELECT UserID, Username, FullName, Role FROM Users WHERE Username = @Username AND IsActive = 1";
                SqlParameter[] parameters = { new SqlParameter("@Username", username ?? "") };
                return db.GetData(query, parameters);
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка получения информации о пользователе: {username}", ex);
                throw new Exception("Не удалось получить информацию о пользователе.", ex);
            }
        }

        // Создание нового пользователя
        public bool CreateUser(string username, string password, string fullName, string role = "User")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
                {
                    throw new ArgumentException("Все обязательные поля должны быть заполнены.");
                }

                string query = @"INSERT INTO Users (Username, Password, FullName, Role) 
                                VALUES (@Username, @Password, @FullName, @Role)";
                SqlParameter[] parameters = {
                    new SqlParameter("@Username", username),
                    new SqlParameter("@Password", password),
                    new SqlParameter("@FullName", fullName),
                    new SqlParameter("@Role", role ?? "User")
                };
                return db.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (SqlException ex)
            {
                Logger.Error($"Ошибка создания пользователя: {username}", ex);
                if (ex.Number == 2627) // Дублирование ключа
                    throw new Exception("Пользователь с таким логином уже существует.", ex);
                throw new Exception("Не удалось создать пользователя.", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка создания пользователя: {username}", ex);
                throw;
            }
        }

        // Получение всех пользователей
        public DataTable GetAllUsers()
        {
            try
            {
                string query = "SELECT UserID, Username, FullName, Role, CreatedDate, IsActive FROM Users ORDER BY Username";
                return db.GetData(query);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка получения списка пользователей", ex);
                throw new Exception("Не удалось загрузить список пользователей.", ex);
            }
        }

        // Получение пользователя по ID
        public DataTable GetUserById(int userId)
        {
            try
            {
                string query = "SELECT UserID, Username, FullName, Role, IsActive FROM Users WHERE UserID = @UserID";
                SqlParameter[] parameters = { new SqlParameter("@UserID", userId) };
                return db.GetData(query, parameters);
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка получения пользователя (ID: {userId})", ex);
                throw new Exception($"Не удалось загрузить данные пользователя. ID: {userId}", ex);
            }
        }

        // Обновление пользователя
        public bool UpdateUser(int userId, string fullName, string role, bool isActive)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fullName))
                    throw new ArgumentException("Полное имя не может быть пустым.");

                string query = @"UPDATE Users SET FullName = @FullName, Role = @Role, IsActive = @IsActive WHERE UserID = @UserID";
                SqlParameter[] parameters = {
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@FullName", fullName),
                    new SqlParameter("@Role", role ?? "User"),
                    new SqlParameter("@IsActive", isActive)
                };
                return db.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка обновления пользователя (ID: {userId})", ex);
                throw;
            }
        }

        // Деактивация пользователя
        public bool DeactivateUser(int userId)
        {
            try
            {
                string query = "UPDATE Users SET IsActive = 0 WHERE UserID = @UserID";
                SqlParameter[] parameters = { new SqlParameter("@UserID", userId) };
                return db.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка деактивации пользователя (ID: {userId})", ex);
                throw new Exception($"Не удалось деактивировать пользователя. ID: {userId}", ex);
            }
        }

        // Сброс пароля
        public bool ResetPassword(int userId, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newPassword))
                    throw new ArgumentException("Пароль не может быть пустым.");

                string query = "UPDATE Users SET Password = @Password WHERE UserID = @UserID";
                SqlParameter[] parameters = {
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@Password", newPassword)
                };
                return db.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка сброса пароля пользователя (ID: {userId})", ex);
                throw new Exception("Не удалось сбросить пароль.", ex);
            }
        }
    }
}