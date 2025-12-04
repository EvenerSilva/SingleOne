@echo off
echo ========================================
echo CORRIGINDO LINHAS COM EMUSO = TRUE
echo ========================================
echo.
echo ATENCAO: Este script vai corrigir as ultimas 4500 linhas!
echo.
pause

set PGPASSWORD=postgres

"C:\Program Files\PostgreSQL\16\bin\psql.exe" -h 127.0.0.1 -U postgres -d singleone -f SQL_CORRIGIR_EMUSO.sql

echo.
echo ========================================
echo CORRECAO CONCLUIDA!
echo ========================================
pause

