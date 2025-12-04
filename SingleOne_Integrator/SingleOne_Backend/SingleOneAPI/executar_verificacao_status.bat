@echo off
echo ========================================
echo VERIFICANDO E CORRIGINDO STATUS DAS LINHAS
echo ========================================
echo.

set PGPASSWORD=postgres

"C:\Program Files\PostgreSQL\16\bin\psql.exe" -h 127.0.0.1 -U postgres -d singleone -f SQL_VERIFICAR_STATUS_ATUAL.sql

echo.
echo ========================================
echo VERIFICACAO E CORRECAO CONCLUIDA!
echo ========================================
pause

