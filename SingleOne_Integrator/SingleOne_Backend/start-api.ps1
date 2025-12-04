Write-Host "Iniciando API SingleOne..." -ForegroundColor Green

# Configurar variáveis de ambiente
$env:DB_HOST = "127.0.0.1"
$env:DB_USER = "postgres"
$env:DB_PASSWORD = "Admin@2025"
$env:SITE_URL = "http://localhost:4200"
$env:SMTP_HOST = "localhost"
$env:SMTP_PORT = "587"
$env:SMTP_LOGIN = ""
$env:SMTP_PASSWORD = ""
$env:SMTP_FROM = "noreply@localhost"
$env:SMTP_ENABLESSL = "false"
$env:API_URL = "http://localhost:5000/api/"

Write-Host "Variáveis de ambiente configuradas" -ForegroundColor Green

# Navegar para o projeto
cd SingleOneAPI

# Verificar se o projeto existe
if (-not (Test-Path "SingleOneAPI.csproj")) {
    Write-Host "Arquivo de projeto não encontrado" -ForegroundColor Red
    exit 1
}

Write-Host "Projeto encontrado" -ForegroundColor Green

# Verificar conexão com banco
Write-Host "Testando conexão com banco..." -ForegroundColor Yellow
$env:PGPASSWORD = $env:DB_PASSWORD
$testResult = psql -h $env:DB_HOST -U $env:DB_USER -d singleone -c "SELECT 1;" 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "Conexão com banco OK" -ForegroundColor Green
} else {
    Write-Host "Problema na conexão com banco" -ForegroundColor Red
    Write-Host "Verifique se PostgreSQL está rodando e o banco 'singleone' existe" -ForegroundColor Yellow
}

# Executar aplicação
Write-Host ""
Write-Host "Iniciando API..." -ForegroundColor Green
Write-Host "Backend: http://localhost:5000" -ForegroundColor Cyan
Write-Host "Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host "API: http://localhost:5000/api/" -ForegroundColor Cyan
Write-Host ""
Write-Host "Mantenha este terminal aberto enquanto usar o sistema" -ForegroundColor Yellow
Write-Host "Pressione Ctrl+C para parar" -ForegroundColor Yellow
Write-Host ""

dotnet run
