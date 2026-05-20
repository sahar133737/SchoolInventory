using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using WindowsFormsApp1;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1.Services
{
    /// <summary>
    /// Учёт распоряжений имуществом: выдача, перемещение, назначение МОЛ, списание.
    /// </summary>
    public class MovementManager
    {
        private readonly DbController _db = new DbController();

        public void EnsureSchema()
        {
            string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MovementTypes]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[MovementTypes] (
        [TypeID] INT IDENTITY(1,1) PRIMARY KEY,
        [TypeName] NVARCHAR(100) NOT NULL UNIQUE
    );
    INSERT INTO MovementTypes (TypeName) VALUES
        (N'Выдача в пользование'),
        (N'Возврат на склад'),
        (N'Перемещение между кабинетами'),
        (N'Назначение ответственного (МОЛ)'),
        (N'Списание с учёта');
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Movements]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[Movements] (
        [MovementID] INT IDENTITY(1,1) PRIMARY KEY,
        [InventoryID] INT NOT NULL,
        [TypeID] INT NOT NULL,
        [FromClassroomID] INT NULL,
        [ToClassroomID] INT NULL,
        [FromPersonID] INT NULL,
        [ToPersonID] INT NULL,
        [MovementDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [DocumentNumber] NVARCHAR(50) NULL,
        [Reason] NVARCHAR(500) NULL,
        [PerformedBy] NVARCHAR(100) NULL,
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Movements_Inventory FOREIGN KEY (InventoryID) REFERENCES Inventory(InventoryID),
        CONSTRAINT FK_Movements_Type FOREIGN KEY (TypeID) REFERENCES MovementTypes(TypeID)
    );
    CREATE INDEX IX_Movements_Date ON Movements(MovementDate);
    CREATE INDEX IX_Movements_Inventory ON Movements(InventoryID);
END";
            _db.ExecuteNonQuery(sql);
        }

        public DataTable GetMovementTypes() =>
            _db.GetData("SELECT TypeID, TypeName FROM MovementTypes ORDER BY TypeID");

        public DataTable GetMovements(DateTime? from = null, DateTime? to = null, int? typeId = null, string search = null)
        {
            var parts = new List<string> { "1=1" };
            var pars = new List<SqlParameter>();
            if (from.HasValue) { parts.Add("m.MovementDate >= @From"); pars.Add(new SqlParameter("@From", from.Value.Date)); }
            if (to.HasValue) { parts.Add("m.MovementDate < @To"); pars.Add(new SqlParameter("@To", to.Value.Date.AddDays(1))); }
            if (typeId.HasValue && typeId.Value > 0) { parts.Add("m.TypeID = @TypeId"); pars.Add(new SqlParameter("@TypeId", typeId.Value)); }
            if (!string.IsNullOrWhiteSpace(search))
            {
                parts.Add("(i.ItemName LIKE @S OR i.InventoryNumber LIKE @S OR m.DocumentNumber LIKE @S)");
                pars.Add(new SqlParameter("@S", "%" + search.Trim() + "%"));
            }

            string sql = $@"
SELECT m.MovementID,
       m.MovementDate AS [Дата],
       t.TypeName AS [Вид распоряжения],
       i.InventoryNumber AS [Инв. номер],
       i.ItemName AS [Наименование],
       fc.RoomNumber + N' — ' + ISNULL(fc.RoomName, N'') AS [Откуда (кабинет)],
       tc.RoomNumber + N' — ' + ISNULL(tc.RoomName, N'') AS [Куда (кабинет)],
       LTRIM(RTRIM(ISNULL(fp.LastName, N'') + N' ' + ISNULL(fp.FirstName, N''))) AS [Откуда (МОЛ)],
       LTRIM(RTRIM(ISNULL(tp.LastName, N'') + N' ' + ISNULL(tp.FirstName, N''))) AS [Куда (МОЛ)],
       m.DocumentNumber AS [№ распоряжения],
       m.Reason AS [Основание],
       m.PerformedBy AS [Оформил]
FROM Movements m
INNER JOIN MovementTypes t ON m.TypeID = t.TypeID
INNER JOIN Inventory i ON m.InventoryID = i.InventoryID
LEFT JOIN Classrooms fc ON m.FromClassroomID = fc.ClassroomID
LEFT JOIN Classrooms tc ON m.ToClassroomID = tc.ClassroomID
LEFT JOIN ResponsiblePersons fp ON m.FromPersonID = fp.PersonID
LEFT JOIN ResponsiblePersons tp ON m.ToPersonID = tp.PersonID
WHERE {string.Join(" AND ", parts)}
ORDER BY m.MovementDate DESC, m.MovementID DESC";
            return _db.GetData(sql, pars.Count > 0 ? pars.ToArray() : null);
        }

        public DataRow GetInventoryCurrentLocation(int inventoryId)
        {
            string sql = @"SELECT i.InventoryID, i.ItemName, i.InventoryNumber, i.Status, i.CurrentState,
                i.ClassroomID, i.ResponsiblePersonID,
                r.RoomNumber + N' — ' + ISNULL(r.RoomName, N'') AS ClassroomName,
                LTRIM(RTRIM(ISNULL(p.LastName, N'') + N' ' + ISNULL(p.FirstName, N''))) AS PersonName
            FROM Inventory i
            LEFT JOIN Classrooms r ON i.ClassroomID = r.ClassroomID
            LEFT JOIN ResponsiblePersons p ON i.ResponsiblePersonID = p.PersonID
            WHERE i.InventoryID = @id";
            var table = _db.GetData(sql, new[] { new SqlParameter("@id", inventoryId) });
            return table.Rows.Count > 0 ? table.Rows[0] : null;
        }

        public bool RegisterMovement(int inventoryId, int typeId,
            int? fromClassroomId, int? toClassroomId,
            int? fromPersonId, int? toPersonId,
            DateTime movementDate, string documentNumber, string reason, string performedBy)
        {
            if (inventoryId <= 0 || typeId <= 0)
                throw new InvalidOperationException("Некорректные данные объекта или вида распоряжения.");

            string typeName = _db.ExecuteScalar("SELECT TypeName FROM MovementTypes WHERE TypeID=@t",
                new[] { new SqlParameter("@t", typeId) })?.ToString() ?? "";

            if (string.IsNullOrEmpty(typeName))
                throw new InvalidOperationException("Выбранный вид распоряжения не найден в базе данных.");

            var check = DispositionValidator.Validate(
                inventoryId, typeId, typeName,
                fromClassroomId, toClassroomId, fromPersonId, toPersonId,
                movementDate, documentNumber, reason);
            if (!check.IsValid)
                throw new InvalidOperationException(check.Message);

            string sql = @"INSERT INTO Movements (InventoryID, TypeID, FromClassroomID, ToClassroomID,
                FromPersonID, ToPersonID, MovementDate, DocumentNumber, Reason, PerformedBy)
                VALUES (@Inv, @Type, @Fc, @Tc, @Fp, @Tp, @Date, @Doc, @Reason, @By)";
            var p = new[]
            {
                new SqlParameter("@Inv", inventoryId),
                new SqlParameter("@Type", typeId),
                new SqlParameter("@Fc", (object)fromClassroomId ?? DBNull.Value),
                new SqlParameter("@Tc", (object)toClassroomId ?? DBNull.Value),
                new SqlParameter("@Fp", (object)fromPersonId ?? DBNull.Value),
                new SqlParameter("@Tp", (object)toPersonId ?? DBNull.Value),
                new SqlParameter("@Date", movementDate),
                new SqlParameter("@Doc", documentNumber ?? ""),
                new SqlParameter("@Reason", reason ?? ""),
                new SqlParameter("@By", performedBy ?? "")
            };
            try
            {
                if (_db.ExecuteNonQuery(sql, p) <= 0) return false;
                ApplyInventoryAfterDisposition(inventoryId, typeName, toClassroomId, toPersonId);
                Logger.Info($"Распоряжение: {typeName}, инв. ID={inventoryId}, документ {documentNumber}, оформил {performedBy}");
                return true;
            }
            catch (SqlException ex)
            {
                Logger.Error("Ошибка SQL при регистрации распоряжения", ex);
                throw;
            }
        }

        private void ApplyInventoryAfterDisposition(int inventoryId, string typeName, int? toClassroomId, int? toPersonId)
        {
            var updates = new List<string>();
            var up = new List<SqlParameter> { new SqlParameter("@Id", inventoryId) };

            if (typeName.IndexOf("Списание", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                updates.Add("Status = N'Списан'");
                updates.Add("CurrentState = N'Списано'");
            }
            else if (typeName.IndexOf("Возврат", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (toClassroomId.HasValue) { updates.Add("ClassroomID = @Room"); up.Add(new SqlParameter("@Room", toClassroomId.Value)); }
                updates.Add("Status = N'На складе'");
            }
            else
            {
                if (toClassroomId.HasValue) { updates.Add("ClassroomID = @Room"); up.Add(new SqlParameter("@Room", toClassroomId.Value)); }
                if (toPersonId.HasValue) { updates.Add("ResponsiblePersonID = @Person"); up.Add(new SqlParameter("@Person", toPersonId.Value)); }
                if (updates.Count > 0)
                    updates.Add("Status = N'В эксплуатации'");
            }

            if (updates.Count > 0)
                _db.ExecuteNonQuery($"UPDATE Inventory SET {string.Join(", ", updates)} WHERE InventoryID=@Id", up.ToArray());
        }

        public bool DeleteMovement(int movementId)
        {
            bool ok = _db.ExecuteNonQuery("DELETE FROM Movements WHERE MovementID=@id",
                new[] { new SqlParameter("@id", movementId) }) > 0;
            if (ok) Logger.Warning($"Удалена запись распоряжения MovementID={movementId}");
            return ok;
        }
    }
}
