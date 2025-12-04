# Script para corrigir estrutura da tabela cargosconfianca

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  CORRIGIR ESTRUTURA CARGOS CONFIANCA" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$senha = Read-Host "Digite a senha do PostgreSQL (usuario postgres)" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($senha)
$env:PGPASSWORD = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

Write-Host "Executando correcao..." -ForegroundColor Yellow

try {
    & "C:\Program Files\PostgreSQL\17\bin\psql.exe" `
        -h localhost `
        -p 5432 `
        -U postgres `
        -d singleone `
        -f corrigir-estrutura-cargosconfianca.sql
    
    Write-Host ""
    Write-Host "Estrutura corrigida com sucesso!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Agora tente salvar novamente no frontend!" -ForegroundColor Yellow
} catch {
    Write-Host ""
    Write-Host "Erro!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
} finally {
    $env:PGPASSWORD = $null
}

