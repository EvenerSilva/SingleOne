Write-Host "Executando migracao de dados de localizacao..." -ForegroundColor Cyan

# Solicitar senha
$Password = Read-Host "Digite a senha do usuario postgres" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
$PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

# Executar o script SQL
$env:PGPASSWORD = $PlainPassword

Write-Host "Executando script de migracao..." -ForegroundColor Yellow

psql -h localhost -p 5432 -d singleone -U postgres -f ".\migrar-localizacao.sql"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migracao executada com sucesso!" -ForegroundColor Green
} else {
    Write-Host "Erro na migracao. Codigo: $LASTEXITCODE" -ForegroundColor Red
}

# Limpar senha da memoria
$env:PGPASSWORD = $null
