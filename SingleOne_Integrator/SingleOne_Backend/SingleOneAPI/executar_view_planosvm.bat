@echo off
echo ========================================
echo VERIFICANDO VIEW PLANOSVM
echo ========================================
echo.

set PGPASSWORD=postgres

"C:\Program Files\PostgreSQL\16\bin\psql.exe" -h 127.0.0.1 -U postgres -d singleone -f SQL_VERIFICAR_VIEW_PLANOSVM.sql

echo.
echo ========================================
echo VERIFICACAO CONCLUIDA!
echo ========================================
pause

