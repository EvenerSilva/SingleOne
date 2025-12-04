# Script PowerShell para executar o SQL de criação da tabela cargosconfianca

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Criando tabela de Cargos de Confiança" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Solicita a senha do banco
$env:PGPASSWORD = Read-Host "Digite a senha do PostgreSQL" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($env:PGPASSWORD)
$env:PGPASSWORD = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

# Configurações do banco
$DB_HOST = "localhost"
$DB_PORT = "5432"
$DB_NAME = "singleone"
$DB_USER = "postgres"

Write-Host "Conectando ao banco de dados..." -ForegroundColor Yellow

# Executa o script SQL
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f "criar-tabela-cargosconfianca.sql"

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✓ Tabela criada/verificada com sucesso!" -ForegroundColor Green
    Write-Host ""
    
    # Verifica se há registros
    Write-Host "Verificando registros existentes..." -ForegroundColor Yellow
    $query = "SELECT COUNT(*) as total FROM cargosconfianca;"
    psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c $query
} else {
    Write-Host ""
    Write-Host "✗ Erro ao criar/verificar tabela!" -ForegroundColor Red
}

# Limpa a senha da memória
Remove-Item Env:\PGPASSWORD

Write-Host ""
Write-Host "Pressione qualquer tecla para sair..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

