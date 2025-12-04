Write-Host "Verificando dados nas colunas de localizacao..." -ForegroundColor Cyan

# Solicitar senha
$Password = Read-Host "Digite a senha do usuario postgres" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
$PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

# Executar a consulta
$env:PGPASSWORD = $PlainPassword

Write-Host "Verificando dados na coluna localizacao..." -ForegroundColor Yellow

psql -h localhost -p 5432 -d singleone -U postgres -c "SELECT COUNT(*) as Total, COUNT(localizacao) as Com_Localizacao, COUNT(localidade_id) as Com_LocalidadeId FROM equipamentos;"

Write-Host "Verificando alguns registros..." -ForegroundColor Yellow

psql -h localhost -p 5432 -d singleone -U postgres -c "SELECT id, localizacao, localidade_id FROM equipamentos WHERE localizacao IS NOT NULL OR localidade_id IS NOT NULL LIMIT 10;"

# Limpar senha da memoria
$env:PGPASSWORD = $null
