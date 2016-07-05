@echo off
echo Chinook Database Version 1.4
echo.

if "%1"=="" goto MENU
if not exist %1 goto ERROR

set SQLFILE=%1
goto RUNSQL

:ERROR
echo The file %1 does not exist.
echo.
goto END

:MENU
echo Options:
echo.
echo 1. Run Chinook_MySql.sql
echo 2. Run Chinook_MySql_AutoIncrementPKs.sql
echo 3. Exit
echo.
choice /c 123
if (%ERRORLEVEL%)==(1) set SQLFILE=Chinook_MySql.sql
if (%ERRORLEVEL%)==(2) set SQLFILE=Chinook_MySql_AutoIncrementPKs.sql
if (%ERRORLEVEL%)==(3) goto END

:RUNSQL
echo.
echo Running %SQLFILE%...
"C:\Program Files\MySQL\MySQL Server 5.7\bin\mysql.exe" -h localhost -u root --password=toor <%SQLFILE%

:END
echo.
set SQLFILE=

