Write-Host "Verificando constraints da tabela equipamentos..." -ForegroundColor Cyan

# Solicitar senha
$Password = Read-Host "Digite a senha do usuario postgres" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
$PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

# Executar a consulta
$env:PGPASSWORD = $PlainPassword

Write-Host "Executando consulta..." -ForegroundColor Yellow

psql -h localhost -p 5432 -d singleone -U postgres -c "SELECT conname as Constraint, contype as Tipo, pg_get_constraintdef(oid) as Definicao FROM pg_constraint WHERE conrelid = 'equipamentos'::regclass;"

# Limpar senha da memoria
$env:PGPASSWORD = $null
