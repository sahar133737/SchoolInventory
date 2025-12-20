-- =============================================
-- Скрипт создания базы данных SchoolInventoryDB
-- Версия: 2.0
-- Дата: 2024-12-20
-- =============================================

USE master;
GO

-- Создание базы данных, если она не существует
-- ПРИМЕЧАНИЕ: Путь к файлам будет использован по умолчанию SQL Server
-- Если нужно указать конкретный путь, раскомментируйте и измените FILENAME
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SchoolInventoryDB')
BEGIN
    -- Вариант 1: Создание с путями по умолчанию (рекомендуется)
    CREATE DATABASE SchoolInventoryDB;
    
    -- Вариант 2: Создание с указанием конкретных путей (раскомментируйте при необходимости)
    /*
    CREATE DATABASE SchoolInventoryDB
    ON 
    ( NAME = 'SchoolInventoryDB',
      FILENAME = 'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\SchoolInventoryDB.mdf',
      SIZE = 100MB,
      MAXSIZE = 1GB,
      FILEGROWTH = 10MB )
    LOG ON 
    ( NAME = 'SchoolInventoryDB_Log',
      FILENAME = 'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\SchoolInventoryDB_Log.ldf',
      SIZE = 10MB,
      MAXSIZE = 100MB,
      FILEGROWTH = 10% );
    */
    
    PRINT 'База данных SchoolInventoryDB успешно создана.';
END
ELSE
BEGIN
    PRINT 'База данных SchoolInventoryDB уже существует.';
END
GO

USE SchoolInventoryDB;
GO

-- =============================================
-- Создание таблиц
-- =============================================

-- Таблица: Categories (Категории)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Categories] (
        [CategoryID] INT IDENTITY(1,1) PRIMARY KEY,
        [CategoryName] NVARCHAR(100) NOT NULL UNIQUE
    );
    PRINT 'Таблица Categories создана.';
END
ELSE
BEGIN
    PRINT 'Таблица Categories уже существует.';
END
GO

-- Таблица: Classrooms (Кабинеты)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Classrooms]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Classrooms] (
        [ClassroomID] INT IDENTITY(1,1) PRIMARY KEY,
        [RoomNumber] NVARCHAR(20) NOT NULL,
        [RoomName] NVARCHAR(100) NOT NULL
    );
    PRINT 'Таблица Classrooms создана.';
END
ELSE
BEGIN
    PRINT 'Таблица Classrooms уже существует.';
END
GO

-- Таблица: ResponsiblePersons (Ответственные лица)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResponsiblePersons]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ResponsiblePersons] (
        [PersonID] INT IDENTITY(1,1) PRIMARY KEY,
        [FirstName] NVARCHAR(50) NOT NULL,
        [LastName] NVARCHAR(50) NOT NULL
    );
    PRINT 'Таблица ResponsiblePersons создана.';
END
ELSE
BEGIN
    PRINT 'Таблица ResponsiblePersons уже существует.';
END
GO

-- Таблица: Users (Пользователи системы)
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
    PRINT 'Таблица Users создана.';
END
ELSE
BEGIN
    PRINT 'Таблица Users уже существует.';
END
GO

-- Таблица: Inventory (Инвентарь)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Inventory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Inventory] (
        [InventoryID] INT IDENTITY(1,1) PRIMARY KEY,
        [ItemName] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [CategoryID] INT NULL,
        [ClassroomID] INT NULL,
        [ResponsiblePersonID] INT NULL,
        [InventoryNumber] NVARCHAR(50) NULL,
        [PurchaseDate] DATETIME NULL,
        [PurchasePrice] DECIMAL(18,2) NULL,
        [CurrentState] NVARCHAR(50) NULL,
        [Status] NVARCHAR(50) NULL DEFAULT 'Активен',
        CONSTRAINT [FK_Inventory_Categories] FOREIGN KEY ([CategoryID]) 
            REFERENCES [dbo].[Categories]([CategoryID]) ON DELETE SET NULL,
        CONSTRAINT [FK_Inventory_Classrooms] FOREIGN KEY ([ClassroomID]) 
            REFERENCES [dbo].[Classrooms]([ClassroomID]) ON DELETE SET NULL,
        CONSTRAINT [FK_Inventory_ResponsiblePersons] FOREIGN KEY ([ResponsiblePersonID]) 
            REFERENCES [dbo].[ResponsiblePersons]([PersonID]) ON DELETE SET NULL
    );
    PRINT 'Таблица Inventory создана.';
END
ELSE
BEGIN
    PRINT 'Таблица Inventory уже существует.';
END
GO

-- =============================================
-- Создание индексов для повышения производительности
-- =============================================

-- Индекс на InventoryNumber для быстрого поиска
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Inventory_InventoryNumber' AND object_id = OBJECT_ID('Inventory'))
BEGIN
    CREATE INDEX [IX_Inventory_InventoryNumber] ON [dbo].[Inventory]([InventoryNumber]);
    PRINT 'Индекс IX_Inventory_InventoryNumber создан.';
END
GO

-- Индекс на CategoryID
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Inventory_CategoryID' AND object_id = OBJECT_ID('Inventory'))
BEGIN
    CREATE INDEX [IX_Inventory_CategoryID] ON [dbo].[Inventory]([CategoryID]);
    PRINT 'Индекс IX_Inventory_CategoryID создан.';
END
GO

-- Индекс на ClassroomID
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Inventory_ClassroomID' AND object_id = OBJECT_ID('Inventory'))
BEGIN
    CREATE INDEX [IX_Inventory_ClassroomID] ON [dbo].[Inventory]([ClassroomID]);
    PRINT 'Индекс IX_Inventory_ClassroomID создан.';
END
GO

-- Индекс на ResponsiblePersonID
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Inventory_ResponsiblePersonID' AND object_id = OBJECT_ID('Inventory'))
BEGIN
    CREATE INDEX [IX_Inventory_ResponsiblePersonID] ON [dbo].[Inventory]([ResponsiblePersonID]);
    PRINT 'Индекс IX_Inventory_ResponsiblePersonID создан.';
END
GO

-- Индекс на CurrentState для фильтрации
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Inventory_CurrentState' AND object_id = OBJECT_ID('Inventory'))
BEGIN
    CREATE INDEX [IX_Inventory_CurrentState] ON [dbo].[Inventory]([CurrentState]);
    PRINT 'Индекс IX_Inventory_CurrentState создан.';
END
GO

-- Индекс на ItemName для поиска
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Inventory_ItemName' AND object_id = OBJECT_ID('Inventory'))
BEGIN
    CREATE INDEX [IX_Inventory_ItemName] ON [dbo].[Inventory]([ItemName]);
    PRINT 'Индекс IX_Inventory_ItemName создан.';
END
GO

-- Индекс на Username для быстрой авторизации
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE INDEX [IX_Users_Username] ON [dbo].[Users]([Username]);
    PRINT 'Индекс IX_Users_Username создан.';
END
GO

PRINT '=============================================';
PRINT 'Структура базы данных успешно создана!';
PRINT '=============================================';
GO

