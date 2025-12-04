@echo off
echo ========================================
echo CORRIGINDO VIEW PLANOSVM
echo ========================================
echo.
echo ATENCAO: Esta operacao vai:
echo 1. Ver a definicao atual da view
echo 2. Criar backup da view
echo 3. Recriar a view com calculo correto de emuso
echo.
pause

set PGPASSWORD=postgres

"C:\Program Files\PostgreSQL\16\bin\psql.exe" -h 127.0.0.1 -U postgres -d singleone -f SQL_CORRIGIR_VIEW.sql

echo.
echo ========================================
echo CORRECAO CONCLUIDA!
echo AGORA REINICIE O BACKEND
echo ========================================
pause

