# =============================================
# PowerShell скрипт для установки базы данных
# Школьная инвентаризация v2.0
# =============================================

param(
    [Parameter(Mandatory=$true)]
    [string]$ServerInstance = "localhost",
    
    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = "SchoolInventoryDB"
)

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Установка базы данных SchoolInventoryDB" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$createScript = Join-Path $scriptPath "CreateDatabase.sql"
$seedScript = Join-Path $scriptPath "SeedData.sql"

# Проверка наличия файлов
if (-not (Test-Path $createScript)) {
    Write-Host "ОШИБКА: Файл CreateDatabase.sql не найден!" -ForegroundColor Red
    Write-Host "Путь: $createScript" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $seedScript)) {
    Write-Host "ОШИБКА: Файл SeedData.sql не найден!" -ForegroundColor Red
    Write-Host "Путь: $seedScript" -ForegroundColor Red
    exit 1
}

# Проверка наличия модуля SqlServer
if (-not (Get-Module -ListAvailable -Name SqlServer)) {
    Write-Host "Предупреждение: Модуль SqlServer не установлен." -ForegroundColor Yellow
    Write-Host "Попытка установки..." -ForegroundColor Yellow
    try {
        Install-Module -Name SqlServer -Scope CurrentUser -Force -AllowClobber
        Write-Host "Модуль SqlServer установлен успешно." -ForegroundColor Green
    }
    catch {
        Write-Host "ОШИБКА: Не удалось установить модуль SqlServer." -ForegroundColor Red
        Write-Host "Установите вручную: Install-Module -Name SqlServer" -ForegroundColor Yellow
        Write-Host "Или используйте sqlcmd для выполнения скриптов вручную." -ForegroundColor Yellow
        exit 1
    }
}

Import-Module SqlServer -ErrorAction Stop

Write-Host "Сервер: $ServerInstance" -ForegroundColor Green
Write-Host "База данных: $DatabaseName" -ForegroundColor Green
Write-Host ""

# Шаг 1: Создание структуры
Write-Host "ШАГ 1: Создание структуры базы данных..." -ForegroundColor Yellow
try {
    $createScriptContent = Get-Content $createScript -Raw -Encoding UTF8
    Invoke-Sqlcmd -ServerInstance $ServerInstance -Database "master" -Query $createScriptContent -ErrorAction Stop
    Write-Host "✓ Структура базы данных создана успешно." -ForegroundColor Green
}
catch {
    Write-Host "✗ ОШИБКА при создании структуры:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Шаг 2: Заполнение данными
Write-Host ""
Write-Host "ШАГ 2: Заполнение тестовыми данными..." -ForegroundColor Yellow
try {
    $seedScriptContent = Get-Content $seedScript -Raw -Encoding UTF8
    Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $DatabaseName -Query $seedScriptContent -ErrorAction Stop
    Write-Host "✓ Тестовые данные добавлены успешно." -ForegroundColor Green
}
catch {
    Write-Host "✗ ОШИБКА при заполнении данными:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host "Структура БД создана, но данные не добавлены." -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Установка завершена успешно!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Для входа в программу используйте:" -ForegroundColor White
Write-Host "  Логин: admin" -ForegroundColor Yellow
Write-Host "  Пароль: admin" -ForegroundColor Yellow
Write-Host ""
Write-Host "Или:" -ForegroundColor White
Write-Host "  Логин: user1" -ForegroundColor Yellow
Write-Host "  Пароль: 123456" -ForegroundColor Yellow
Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan

