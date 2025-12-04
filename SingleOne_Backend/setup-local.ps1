Write-Host "🚀 Configurando ambiente de desenvolvimento LOCAL SingleOne..." -ForegroundColor Green

# Verificar se .NET 6.0 está instalado
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET 6.0 SDK não está instalado. Por favor, instale o .NET 6.0 SDK primeiro." -ForegroundColor Red
    Write-Host "   Download: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# Verificar se Node.js está instalado
try {
    $nodeVersion = node --version
    Write-Host "✅ Node.js: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Node.js não está instalado. Por favor, instale o Node.js primeiro." -ForegroundColor Red
    Write-Host "   Download: https://nodejs.org/" -ForegroundColor Yellow
    exit 1
}

# Verificar se npm está instalado
try {
    $npmVersion = npm --version
    Write-Host "✅ npm: $npmVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ npm não está instalado." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Todas as dependências encontradas!" -ForegroundColor Green

# Configurar backend
Write-Host "🔧 Configurando backend..." -ForegroundColor Yellow
Set-Location "SingleOne_Backend\SingleOneAPI"

# Restaurar dependências
Write-Host "📦 Restaurando dependências do backend..." -ForegroundColor Yellow
dotnet restore

# Verificar se PostgreSQL está rodando
Write-Host "🗄️  Verificando PostgreSQL..." -ForegroundColor Yellow
try {
    $testConnection = Test-NetConnection -ComputerName localhost -Port 5432 -InformationLevel Quiet
    if ($testConnection.TcpTestSucceeded) {
        Write-Host "✅ PostgreSQL está acessível" -ForegroundColor Green
    } else {
        Write-Host "⚠️  PostgreSQL não está rodando na porta 5432" -ForegroundColor Yellow
        Write-Host "   Por favor, instale e configure o PostgreSQL:" -ForegroundColor Yellow
        Write-Host "   1. Download: https://www.postgresql.org/download/" -ForegroundColor Cyan
        Write-Host "   2. Instale com senha: password" -ForegroundColor Cyan
        Write-Host "   3. Crie o banco: CREATE DATABASE singleone;" -ForegroundColor Cyan
        Write-Host "   4. Ou execute: .\setup-postgres.ps1" -ForegroundColor Cyan
    }
} catch {
    Write-Host "⚠️  Não foi possível verificar o PostgreSQL" -ForegroundColor Yellow
}

# Configurar variáveis de ambiente
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

Write-Host "✅ Variáveis de ambiente configuradas" -ForegroundColor Green

# Executar migrations (se existirem)
Write-Host "🔄 Executando migrations..." -ForegroundColor Yellow
try {
    dotnet ef database update
    Write-Host "✅ Migrations executadas com sucesso" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Erro ao executar migrations. Isso é normal se não houver migrations." -ForegroundColor Yellow
}

# Configurar frontend
Write-Host "🔧 Configurando frontend..." -ForegroundColor Yellow
Set-Location "..\..\SingleOne_Frontend"

# Instalar dependências
Write-Host "📦 Instalando dependências do frontend..." -ForegroundColor Yellow
npm install

Write-Host ""
Write-Host "🎉 Configuração local concluída!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Para executar o sistema:" -ForegroundColor Cyan
Write-Host ""
Write-Host "🔧 Backend (Terminal 1):" -ForegroundColor Yellow
Write-Host "   .\run-backend.ps1" -ForegroundColor White
Write-Host ""
Write-Host "🌐 Frontend (Terminal 2):" -ForegroundColor Yellow
Write-Host "   .\run-frontend.ps1" -ForegroundColor White
Write-Host ""
Write-Host "📋 URLs de acesso:" -ForegroundColor Cyan
Write-Host "   🌐 Frontend: http://localhost:4200" -ForegroundColor White
Write-Host "   🔧 Backend API: http://localhost:5000" -ForegroundColor White
Write-Host "   📚 Swagger: http://localhost:5000/swagger" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  Certifique-se de que o PostgreSQL está rodando antes de executar o backend!" -ForegroundColor Yellow 