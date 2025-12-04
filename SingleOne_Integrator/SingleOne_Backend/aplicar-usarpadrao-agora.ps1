# Script para adicionar campo usarpadrao

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  ADICIONAR CAMPO USARPADRAO" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Solicitar senha
$senha = Read-Host "Digite a senha do PostgreSQL (usuario postgres)" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($senha)
$env:PGPASSWORD = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

Write-Host "Executando script SQL..." -ForegroundColor Yellow

try {
    & "C:\Program Files\PostgreSQL\17\bin\psql.exe" `
        -h localhost `
        -p 5432 `
        -U postgres `
        -d singleone `
        -f adicionar-usarpadrao-simples.sql
    
    Write-Host ""
    Write-Host "Script executado com sucesso!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Proximos passos:" -ForegroundColor Yellow
    Write-Host "1. Compilar backend: dotnet build" -ForegroundColor Gray
    Write-Host "2. Reiniciar backend" -ForegroundColor Gray
    Write-Host "3. Tentar salvar novamente no frontend" -ForegroundColor Gray
} catch {
    Write-Host ""
    Write-Host "Erro ao executar script!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
} finally {
    $env:PGPASSWORD = $null
}

