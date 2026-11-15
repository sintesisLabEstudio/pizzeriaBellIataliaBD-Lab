@echo off
setlocal EnableExtensions

REM Carga/adjunta la base de datos pizzeriaBD desde el archivo MDF ubicado en esta misma carpeta.
REM Uso:
REM   cargaBasePizzeria.bat [SERVIDOR]
REM Ejemplos:
REM   cargaBasePizzeria.bat
REM   cargaBasePizzeria.bat "(localdb)\MSSQLLocalDB"
REM   cargaBasePizzeria.bat ".\SQLEXPRESS"
REM
REM Requisitos: sqlcmd instalado y accesible en PATH, y permisos sobre la instancia SQL.

set "DB_NAME=pizzeriaBD"
set "SCRIPT_DIR=%~dp0"
if "%SCRIPT_DIR:~-1%"=="\" set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"
set "MDF_PATH=%SCRIPT_DIR%\pizzeriaBD.mdf"
set "LDF_PATH=%SCRIPT_DIR%\pizzeriaBD_log.ldf"
set "SERVER=%~1"
if "%SERVER%"=="" set "SERVER=(localdb)\MSSQLLocalDB"

echo ==========================================================
echo  Carga de base de datos Pizzeria Bella Italia
echo ==========================================================
echo  Servidor : %SERVER%
echo  Base     : %DB_NAME%
echo  MDF      : %MDF_PATH%
echo  LDF      : %LDF_PATH%
echo.

where sqlcmd >nul 2>nul
if errorlevel 1 (
  echo [ERROR] No se encontro "sqlcmd" en el PATH.
  echo Instale SQL Server Command Line Utilities o use SQL Server Management Studio.
  exit /b 1
)

if not exist "%MDF_PATH%" (
  echo [ERROR] No se encontro el archivo MDF requerido:
  echo         %MDF_PATH%
  echo.
  echo Coloque "pizzeriaBD.mdf" en esta misma carpeta y vuelva a ejecutar el script.
  exit /b 1
)

REM Asegurar que LocalDB este iniciado si aplica
echo %SERVER% | findstr /I /C:"(localdb)" >nul
if not errorlevel 1 (
  where sqllocaldb >nul 2>nul
  if not errorlevel 1 (
    echo Iniciando instancia LocalDB si es necesario...
    sqllocaldb start MSSQLLocalDB >nul 2>nul
  )
)

set "SQL_FILE=%TEMP%\cargaBasePizzeria_%RANDOM%%RANDOM%.sql"

> "%SQL_FILE%" echo SET NOCOUNT ON;
>>"%SQL_FILE%" echo IF DB_ID(N'%DB_NAME%') IS NOT NULL
>>"%SQL_FILE%" echo BEGIN
>>"%SQL_FILE%" echo     PRINT N'La base %DB_NAME% ya existe. Se pondra OFFLINE y se separara para volver a adjuntarla desde el MDF indicado.';
>>"%SQL_FILE%" echo     ALTER DATABASE [%DB_NAME%] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
>>"%SQL_FILE%" echo     EXEC sp_detach_db @dbname = N'%DB_NAME%', @keepfulltextindexfile = N'true';
>>"%SQL_FILE%" echo END
>>"%SQL_FILE%" echo GO

if exist "%LDF_PATH%" goto with_ldf
echo [AVISO] No se encontro archivo LDF. Se intentara adjuntar/reconstruir el log.
>>"%SQL_FILE%" echo BEGIN TRY
>>"%SQL_FILE%" echo     CREATE DATABASE [%DB_NAME%] ON (FILENAME = N'%MDF_PATH%') FOR ATTACH_REBUILD_LOG;
>>"%SQL_FILE%" echo END TRY
>>"%SQL_FILE%" echo BEGIN CATCH
>>"%SQL_FILE%" echo     DECLARE @msg NVARCHAR(4000) = ERROR_MESSAGE();
>>"%SQL_FILE%" echo     RAISERROR(N'No se pudo adjuntar la base con ATTACH_REBUILD_LOG: %%s', 16, 1, @msg);
>>"%SQL_FILE%" echo END CATCH
goto after_attach

:with_ldf
>>"%SQL_FILE%" echo CREATE DATABASE [%DB_NAME%] ON (FILENAME = N'%MDF_PATH%'), (FILENAME = N'%LDF_PATH%') FOR ATTACH;

:after_attach
>>"%SQL_FILE%" echo GO
>>"%SQL_FILE%" echo IF DB_ID(N'%DB_NAME%') IS NOT NULL
>>"%SQL_FILE%" echo     PRINT N'Base %DB_NAME% cargada correctamente.';
>>"%SQL_FILE%" echo ELSE
>>"%SQL_FILE%" echo     RAISERROR(N'La base %DB_NAME% no quedo disponible.', 16, 1);
>>"%SQL_FILE%" echo GO

echo Ejecutando carga en SQL Server...
sqlcmd -S "%SERVER%" -E -b -i "%SQL_FILE%"
set "RC=%ERRORLEVEL%"

del /q "%SQL_FILE%" >nul 2>nul

echo.
if "%RC%"=="0" (
  echo [OK] Base de datos cargada/adjuntada correctamente.
  echo     Asegure que la aplicacion use el mismo servidor/base en appsettings.json
  echo     o en la variable de entorno PIZZERIA_CONNECTION_STRING.
  exit /b 0
)

echo [ERROR] Fallo la carga de la base de datos ^(codigo %RC%^).
echo Revise el servidor, permisos y el estado de los archivos MDF/LDF.
exit /b %RC%