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
echo 1. Run Chinook_PostgreSql.sql
echo 2. Exit
echo.
choice /c 123
if (%ERRORLEVEL%)==(1) set SQLFILE=Chinook_PostgreSql.sql
if (%ERRORLEVEL%)==(2) goto END

:RUNSQL
echo.
echo Running %SQLFILE%...
SET "PGOPTIONS=-c client_min_messages=WARNING"
"C:\Program Files\PostgreSQL\pg95\bin\dropdb" --if-exists -U postgres Chinook
"C:\Program Files\PostgreSQL\pg95\bin\createdb" -U postgres Chinook
"C:\Program Files\PostgreSQL\pg95\bin\psql" -f %SQLFILE% -q Chinook postgres


:END
echo.
set SQLFILE=

