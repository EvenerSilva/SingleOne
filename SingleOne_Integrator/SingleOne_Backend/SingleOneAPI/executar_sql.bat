@echo off
echo ============================================
echo EXECUTANDO SQL DE CRIACAO DE TABELAS
echo ============================================
echo.
echo Este script executará o SQL no PostgreSQL
echo.
echo IMPORTANTE: Ajuste as credenciais abaixo!
echo.

REM ✅ CREDENCIAIS OBTIDAS DO appsettings.Development.json:
set PGHOST=127.0.0.1
set PGPORT=5432
set PGDATABASE=singleone
set PGUSER=postgres
set PGPASSWORD=Admin@2025

echo Executando SQL_CREATE_IMPORTACAO_TABLES_V2.sql...
psql -h %PGHOST% -p %PGPORT% -d %PGDATABASE% -U %PGUSER% -f SQL_CREATE_IMPORTACAO_TABLES_V2.sql

echo.
echo ============================================
echo CONCLUIDO!
echo ============================================
pause

