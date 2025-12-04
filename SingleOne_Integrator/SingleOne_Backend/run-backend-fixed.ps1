Write-Host "🔧 Iniciando backend SingleOne..." -ForegroundColor Green

# Verificar se estamos no diretório correto
if (-not (Test-Path "SingleOne_Backend\SingleOneAPI\SingleOneAPI.csproj")) {
    Write-Host "❌ Execute este script na raiz do projeto SingleOne" -ForegroundColor Red
    exit 1
}

# Navegar para o diretório do backend
Set-Location "SingleOne_Backend\SingleOneAPI"

# Verificar se .NET está instalado
try {
    dotnet --version | Out-Null
    Write-Host "✅ .NET SDK encontrado" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET 6.0 SDK não está instalado" -ForegroundColor Red
    exit 1
}

# Verificar se PostgreSQL está rodando
Write-Host "🗄️  Verificando PostgreSQL..." -ForegroundColor Yellow
try {
    $testConnection = Test-NetConnection -ComputerName 127.0.0.1 -Port 5432 -InformationLevel Quiet
    if ($testConnection.TcpTestSucceeded) {
        Write-Host "✅ PostgreSQL está acessível" -ForegroundColor Green
    } else {
        Write-Host "⚠️  PostgreSQL não está rodando na porta 5432" -ForegroundColor Yellow
        Write-Host "   Certifique-se de que o PostgreSQL está instalado e rodando" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️  Não foi possível verificar o PostgreSQL" -ForegroundColor Yellow
}

# Restaurar dependências se necessário
if (-not (Test-Path "bin") -or -not (Test-Path "obj")) {
    Write-Host "📦 Restaurando dependências..." -ForegroundColor Yellow
    dotnet restore
}

# Definir variáveis de ambiente para o banco de dados
Write-Host "🔧 Configurando variáveis de ambiente..." -ForegroundColor Yellow
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

Write-Host "✅ Variáveis de ambiente configuradas:" -ForegroundColor Green
Write-Host "   🗄️  DB_HOST: $env:DB_HOST" -ForegroundColor White
Write-Host "   👤 DB_USER: $env:DB_USER" -ForegroundColor White
Write-Host "   🔑 DB_PASSWORD: $env:DB_PASSWORD" -ForegroundColor White
Write-Host ""

# Testar conexão antes de iniciar a aplicação
Write-Host "🔗 Testando conexão com banco..." -ForegroundColor Yellow
$env:PGPASSWORD = $env:DB_PASSWORD
$testResult = psql -h $env:DB_HOST -U $env:DB_USER -d singleone -c "SELECT 1;" 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Conexão com banco confirmada" -ForegroundColor Green
} else {
    Write-Host "❌ Erro na conexão com banco. Verifique as configurações." -ForegroundColor Red
    Write-Host "   Host: $env:DB_HOST" -ForegroundColor White
    Write-Host "   User: $env:DB_USER" -ForegroundColor White
    Write-Host "   Password: $env:DB_PASSWORD" -ForegroundColor White
    exit 1
}

# Executar migrations
Write-Host "🔄 Executando migrations..." -ForegroundColor Yellow
try {
    dotnet ef database update
} catch {
    Write-Host "⚠️  Erro ao executar migrations. Continuando..." -ForegroundColor Yellow
}

# Executar aplicação
Write-Host "🚀 Iniciando backend..." -ForegroundColor Green
Write-Host "   📚 Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host "   🔧 API: http://localhost:5000/api/" -ForegroundColor Cyan
Write-Host ""
Write-Host "Pressione Ctrl+C para parar" -ForegroundColor Yellow
Write-Host ""

dotnet run
