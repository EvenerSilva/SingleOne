@echo off
REM ========================================
REM Script para corrigir a view termoentregavm
REM Remove referência à coluna e.localizacao que não existe mais
REM ========================================

echo [CORRIGIR_TERMOENTREGAVM] Iniciando correção da view termoentregavm...

REM Configurar variáveis de ambiente do PostgreSQL
set PGHOST=127.0.0.1
set PGPORT=5432
set PGUSER=postgres
set PGPASSWORD=Admin@2025
set PGDATABASE=singleone

REM Executar o script SQL
REM Tentar PostgreSQL 17 primeiro, depois 16
if exist "C:\Program Files\PostgreSQL\17\bin\psql.exe" (
    "C:\Program Files\PostgreSQL\17\bin\psql.exe" -h %PGHOST% -U %PGUSER% -d %PGDATABASE% -f "%~dp0Scripts\SQL_CORRIGIR_TERMOENTREGAVM.sql"
) else if exist "C:\Program Files\PostgreSQL\16\bin\psql.exe" (
    "C:\Program Files\PostgreSQL\16\bin\psql.exe" -h %PGHOST% -U %PGUSER% -d %PGDATABASE% -f "%~dp0Scripts\SQL_CORRIGIR_TERMOENTREGAVM.sql"
) else (
    echo ERRO: psql.exe não encontrado. Verifique a instalação do PostgreSQL.
    pause
    exit /b 1
)

if %ERRORLEVEL% EQU 0 (
    echo [CORRIGIR_TERMOENTREGAVM] View corrigida com sucesso!
) else (
    echo [CORRIGIR_TERMOENTREGAVM] ERRO ao corrigir a view. Verifique os logs acima.
    pause
    exit /b %ERRORLEVEL%
)

echo [CORRIGIR_TERMOENTREGAVM] Processo concluído.
pause

