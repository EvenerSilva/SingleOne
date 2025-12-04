@echo off
echo ========================================
echo VERIFICANDO COLUNAS DA TABELA TELEFONIALINHAS
echo ========================================
echo.

set PGPASSWORD=postgres

"C:\Program Files\PostgreSQL\16\bin\psql.exe" -h 127.0.0.1 -U postgres -d singleone -f SQL_VERIFICAR_COLUNAS_LINHAS.sql

echo.
echo ========================================
echo VERIFICACAO CONCLUIDA!
echo ========================================
pause

