Write-Host "Verificando campos da tabela equipamentos..." -ForegroundColor Cyan

# Solicitar senha
$Password = Read-Host "Digite a senha do usuario postgres" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
$PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

# Executar a consulta
$env:PGPASSWORD = $PlainPassword

Write-Host "Executando consulta..." -ForegroundColor Yellow

psql -h localhost -p 5432 -d singleone -U postgres -c "SELECT column_name as Campo, data_type as Tipo, is_nullable as Permite_Nulo, character_maximum_length as Tamanho_Max FROM information_schema.columns WHERE table_name = 'equipamentos' ORDER BY ordinal_position;"

# Limpar senha da memoria
$env:PGPASSWORD = $null
