Write-Host "🔧 Iniciando backend SingleOne..." -ForegroundColor Green

# Navegar para o diretório do backend
if (Test-Path "SingleOneAPI") {
    Set-Location "SingleOneAPI"
} else {
    Write-Host "❌ Diretório SingleOneAPI não encontrado" -ForegroundColor Red
    exit 1
}

# Verificar se .NET está instalado
try {
    dotnet --version | Out-Null
    Write-Host "✅ .NET SDK encontrado" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK não está instalado" -ForegroundColor Red
    exit 1
}

# Definir variáveis de ambiente
Write-Host "🔧 Configurando variáveis de ambiente..." -ForegroundColor Yellow
$env:DB_HOST = "127.0.0.1"
$env:DB_USER = "postgres"
$env:DB_PASSWORD = "Admin@2025"
$env:SITE_URL = "http://localhost:4200"

# Configurações SMTP Brevo (antigo SendBlue)
$env:SMTP_HOST = "smtp-relay.brevo.com"
$env:SMTP_PORT = "587"
$env:SMTP_LOGIN = "teste@singleone.tech"
$env:SMTP_PASSWORD = "teste123"
$env:SMTP_ENABLESSL = "true"
$env:SMTP_FROM = "teste@singleone.tech"

Write-Host "✅ Variáveis configuradas:" -ForegroundColor Green
Write-Host "   🗄️  DB_HOST: $env:DB_HOST" -ForegroundColor White
Write-Host "   👤 DB_USER: $env:DB_USER" -ForegroundColor White
Write-Host "   🔑 DB_PASSWORD: $env:DB_PASSWORD" -ForegroundColor White
Write-Host "   📧 SMTP_HOST: $env:SMTP_HOST" -ForegroundColor White
Write-Host "   📨 SMTP_FROM: $env:SMTP_FROM" -ForegroundColor White

# Testar conexão com banco
Write-Host "🔗 Testando conexão com banco..." -ForegroundColor Yellow
$env:PGPASSWORD = $env:DB_PASSWORD
$testResult = psql -h $env:DB_HOST -U $env:DB_USER -d singleone -c "SELECT 1;" 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Conexão com banco confirmada" -ForegroundColor Green
} else {
    Write-Host "❌ Erro na conexão com banco" -ForegroundColor Red
    Write-Host "   Verificando se appsettings.Development.json tem a string de conexão..." -ForegroundColor Yellow
}

# Executar aplicação
Write-Host "🚀 Iniciando backend..." -ForegroundColor Green
Write-Host "   📚 Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host "   🔧 API: http://localhost:5000/api/" -ForegroundColor Cyan
Write-Host ""
Write-Host "Pressione Ctrl+C para parar" -ForegroundColor Yellow
Write-Host ""

dotnet run