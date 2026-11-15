@echo off
setlocal EnableExtensions

REM Crea la base de datos pizzeriaBD con los archivos MDF/LDF en ESTA MISMA carpeta.
REM Uso:
REM   crearBasePizzeria.bat [SERVIDOR]
REM Ejemplos:
REM   crearBasePizzeria.bat
REM   crearBasePizzeria.bat "(localdb)\MSSQLLocalDB"
REM   crearBasePizzeria.bat ".\SQLEXPRESS"

set "DB_NAME=pizzeriaBD"
set "SCRIPT_DIR=%~dp0"
if "%SCRIPT_DIR:~-1%"=="\" set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"
set "MDF_PATH=%SCRIPT_DIR%\pizzeriaBD.mdf"
set "LDF_PATH=%SCRIPT_DIR%\pizzeriaBD_log.ldf"
set "SERVER=%~1"
if "%SERVER%"=="" set "SERVER=(localdb)\MSSQLLocalDB"

echo ==========================================================
echo  Crear base de datos en carpeta del proyecto
echo ==========================================================
echo  Servidor : %SERVER%
echo  Base     : %DB_NAME%
echo  MDF      : %MDF_PATH%
echo  LDF      : %LDF_PATH%
echo.

where sqlcmd >nul 2>nul
if errorlevel 1 (
  echo [ERROR] No se encontro "sqlcmd" en el PATH.
  exit /b 1
)

echo %SERVER% | findstr /I /C:"(localdb)" >nul
if not errorlevel 1 (
  where sqllocaldb >nul 2>nul
  if not errorlevel 1 sqllocaldb start MSSQLLocalDB >nul 2>nul
)

set "SQL_FILE=%TEMP%\crearBasePizzeria_%RANDOM%%RANDOM%.sql"

> "%SQL_FILE%" echo SET NOCOUNT ON;
>>"%SQL_FILE%" echo IF DB_ID(N'%DB_NAME%') IS NOT NULL
>>"%SQL_FILE%" echo BEGIN
>>"%SQL_FILE%" echo     PRINT N'La base %DB_NAME% ya existe. Se eliminara para recrearla en la carpeta indicada.';
>>"%SQL_FILE%" echo     ALTER DATABASE [%DB_NAME%] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
>>"%SQL_FILE%" echo     DROP DATABASE [%DB_NAME%];
>>"%SQL_FILE%" echo END
>>"%SQL_FILE%" echo GO
>>"%SQL_FILE%" echo CREATE DATABASE [%DB_NAME%]
>>"%SQL_FILE%" echo ON PRIMARY
>>"%SQL_FILE%" echo ^( NAME = N'%DB_NAME%', FILENAME = N'%MDF_PATH%', SIZE = 8MB, FILEGROWTH = 8MB ^)
>>"%SQL_FILE%" echo LOG ON
>>"%SQL_FILE%" echo ^( NAME = N'%DB_NAME%_log', FILENAME = N'%LDF_PATH%', SIZE = 8MB, FILEGROWTH = 8MB ^);
>>"%SQL_FILE%" echo GO
>>"%SQL_FILE%" echo ALTER DATABASE [%DB_NAME%] SET RECOVERY SIMPLE;
>>"%SQL_FILE%" echo GO
>>"%SQL_FILE%" echo USE [%DB_NAME%];
>>"%SQL_FILE%" echo GO
>>"%SQL_FILE%" echo IF OBJECT_ID(N'dbo.DetallePedidos', N'U') IS NOT NULL DROP TABLE dbo.DetallePedidos;
>>"%SQL_FILE%" echo IF OBJECT_ID(N'dbo.Pedidos', N'U') IS NOT NULL DROP TABLE dbo.Pedidos;
>>"%SQL_FILE%" echo IF OBJECT_ID(N'dbo.Clientes', N'U') IS NOT NULL DROP TABLE dbo.Clientes;
>>"%SQL_FILE%" echo GO
>>"%SQL_FILE%" echo CREATE TABLE dbo.Clientes (
>>"%SQL_FILE%" echo     IdCliente INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY,
>>"%SQL_FILE%" echo     Nombre NVARCHAR(100) NOT NULL,
>>"%SQL_FILE%" echo     Telefono NVARCHAR(30) NOT NULL
>>"%SQL_FILE%" echo );
>>"%SQL_FILE%" echo CREATE TABLE dbo.Pedidos (
>>"%SQL_FILE%" echo     IdPedido INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pedidos PRIMARY KEY,
>>"%SQL_FILE%" echo     IdCliente INT NOT NULL,
>>"%SQL_FILE%" echo     FechaHora DATETIME2(0) NOT NULL CONSTRAINT DF_Pedidos_FechaHora DEFAULT (SYSDATETIME()),
>>"%SQL_FILE%" echo     EsDelivery BIT NOT NULL CONSTRAINT DF_Pedidos_EsDelivery DEFAULT (0),
>>"%SQL_FILE%" echo     AplicaConvenioEstudiante BIT NOT NULL CONSTRAINT DF_Pedidos_Convenio DEFAULT (0),
>>"%SQL_FILE%" echo     Subtotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_Pedidos_Subtotal DEFAULT (0),
>>"%SQL_FILE%" echo     Descuento DECIMAL(18,2) NOT NULL CONSTRAINT DF_Pedidos_Descuento DEFAULT (0),
>>"%SQL_FILE%" echo     DeliveryCosto DECIMAL(18,2) NOT NULL CONSTRAINT DF_Pedidos_DeliveryCosto DEFAULT (0),
>>"%SQL_FILE%" echo     Iva DECIMAL(18,2) NOT NULL CONSTRAINT DF_Pedidos_Iva DEFAULT (0),
>>"%SQL_FILE%" echo     Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_Pedidos_Total DEFAULT (0),
>>"%SQL_FILE%" echo     CONSTRAINT FK_Pedidos_Clientes FOREIGN KEY (IdCliente) REFERENCES dbo.Clientes(IdCliente)
>>"%SQL_FILE%" echo );
>>"%SQL_FILE%" echo CREATE TABLE dbo.DetallePedidos (
>>"%SQL_FILE%" echo     IdDetalle INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DetallePedidos PRIMARY KEY,
>>"%SQL_FILE%" echo     IdPedido INT NOT NULL,
>>"%SQL_FILE%" echo     Tamano NVARCHAR(50) NOT NULL,
>>"%SQL_FILE%" echo     TipoMasa NVARCHAR(50) NOT NULL,
>>"%SQL_FILE%" echo     QuesoExtra BIT NOT NULL CONSTRAINT DF_Detalle_QuesoExtra DEFAULT (0),
>>"%SQL_FILE%" echo     Pepperoni BIT NOT NULL CONSTRAINT DF_Detalle_Pepperoni DEFAULT (0),
>>"%SQL_FILE%" echo     Champinones BIT NOT NULL CONSTRAINT DF_Detalle_Champinones DEFAULT (0),
>>"%SQL_FILE%" echo     Aceitunas BIT NOT NULL CONSTRAINT DF_Detalle_Aceitunas DEFAULT (0),
>>"%SQL_FILE%" echo     Tocino BIT NOT NULL CONSTRAINT DF_Detalle_Tocino DEFAULT (0),
>>"%SQL_FILE%" echo     Cantidad INT NOT NULL CONSTRAINT DF_Detalle_Cantidad DEFAULT (1),
>>"%SQL_FILE%" echo     PrecioUnitario DECIMAL(18,2) NOT NULL CONSTRAINT DF_Detalle_PrecioUnitario DEFAULT (0),
>>"%SQL_FILE%" echo     SubtotalItem DECIMAL(18,2) NOT NULL CONSTRAINT DF_Detalle_SubtotalItem DEFAULT (0),
>>"%SQL_FILE%" echo     Resumen NVARCHAR(300) NOT NULL CONSTRAINT DF_Detalle_Resumen DEFAULT (N''),
>>"%SQL_FILE%" echo     CONSTRAINT FK_DetallePedidos_Pedidos FOREIGN KEY (IdPedido) REFERENCES dbo.Pedidos(IdPedido) ON DELETE CASCADE
>>"%SQL_FILE%" echo );
>>"%SQL_FILE%" echo CREATE INDEX IX_Pedidos_IdCliente ON dbo.Pedidos(IdCliente);
>>"%SQL_FILE%" echo CREATE INDEX IX_DetallePedidos_IdPedido ON dbo.DetallePedidos(IdPedido);
>>"%SQL_FILE%" echo GO
>>"%SQL_FILE%" echo PRINT N'Base de datos creada en la carpeta indicada y esquema aplicado.';
>>"%SQL_FILE%" echo GO

echo Ejecutando creacion en SQL Server...
sqlcmd -S "%SERVER%" -E -b -i "%SQL_FILE%"
set "RC=%ERRORLEVEL%"
del /q "%SQL_FILE%" >nul 2>nul

echo.
if "%RC%"=="0" (
  echo [OK] Archivos creados en: %SCRIPT_DIR%
  if exist "%MDF_PATH%" (echo   - pizzeriaBD.mdf) else (echo   - [AVISO] No se ve pizzeriaBD.mdf aun)
  if exist "%LDF_PATH%" (echo   - pizzeriaBD_log.ldf) else (echo   - [AVISO] No se ve pizzeriaBD_log.ldf aun)
  echo.
  echo Configure la app para usar la misma instancia/servidor: %SERVER%
  exit /b 0
) else (
  echo [ERROR] No se pudo crear la base ^(codigo %RC%^).
  echo Verifique permisos de escritura en la carpeta y permisos en SQL Server.
  exit /b %RC%
)
