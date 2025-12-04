Write-Host "Executando alteracao da tabela localidades..." -ForegroundColor Cyan

# Solicitar senha
$Password = Read-Host "Digite a senha do usuario postgres" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
$PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

# Executar o script SQL
$env:PGPASSWORD = $PlainPassword

Write-Host "Executando script SQL..." -ForegroundColor Yellow

psql -h localhost -p 5432 -d singleone -U postgres -f ".\scripts\alterar-tabela-localidades-postgres.sql"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Script executado com sucesso!" -ForegroundColor Green
    Write-Host "Campos cidade e estado foram adicionados a tabela localidades." -ForegroundColor Green
} else {
    Write-Host "Erro ao executar o script. Codigo: $LASTEXITCODE" -ForegroundColor Red
}

# Limpar senha da memoria
$env:PGPASSWORD = $null
