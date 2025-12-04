# Script para aplicar o campo usarpadrao na tabela cargosconfianca

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  ADICIONAR CAMPO usarpadrao - CARGOS CONFIANCA" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

Write-Host "Digite a senha do PostgreSQL:" -ForegroundColor Yellow
$securePassword = Read-Host -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
$env:PGPASSWORD = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

try {
    $PGHOST = "localhost"
    $PGPORT = "5432"
    $PGUSER = "postgres"
    $PGDATABASE = "singleone"

    Write-Host "Conectando ao banco de dados..." -ForegroundColor Yellow
    Write-Host "Host: $PGHOST" -ForegroundColor Gray
    Write-Host "Database: $PGDATABASE" -ForegroundColor Gray
    Write-Host ""

    $sqlFile = "adicionar-campo-usarpadrao-cargosconfianca.sql"
    
    if (Test-Path $sqlFile) {
        Write-Host "Executando script SQL: $sqlFile" -ForegroundColor Yellow
        
        $result = & "C:\Program Files\PostgreSQL\17\bin\psql.exe" -h $PGHOST -p $PGPORT -U $PGUSER -d $PGDATABASE -f $sqlFile 2>&1
        
        Write-Host ""
        Write-Host "Resultado:" -ForegroundColor Cyan
        Write-Host $result -ForegroundColor White
        Write-Host ""
        Write-Host "Campo usarpadrao adicionado com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "Arquivo SQL nao encontrado: $sqlFile" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "Erro ao executar script:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
} finally {
    $env:PGPASSWORD = $null
}
