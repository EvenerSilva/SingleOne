Write-Host "🌐 Iniciando frontend SingleOne..." -ForegroundColor Green

# Verificar se estamos no diretório correto
if (-not (Test-Path "SingleOne_Frontend\package.json")) {
    Write-Host "❌ Execute este script na raiz do projeto SingleOne" -ForegroundColor Red
    exit 1
}

# Navegar para o diretório do frontend
Set-Location "SingleOne_Frontend"

# Verificar se Node.js está instalado
try {
    node --version | Out-Null
    Write-Host "✅ Node.js encontrado" -ForegroundColor Green
} catch {
    Write-Host "❌ Node.js não está instalado" -ForegroundColor Red
    exit 1
}

# Verificar se npm está instalado
try {
    npm --version | Out-Null
    Write-Host "✅ npm encontrado" -ForegroundColor Green
} catch {
    Write-Host "❌ npm não está instalado" -ForegroundColor Red
    exit 1
}

# Verificar se node_modules existe
if (-not (Test-Path "node_modules")) {
    Write-Host "📦 Instalando dependências..." -ForegroundColor Yellow
    npm install
}

# Verificar se o backend está rodando
Write-Host "🔧 Verificando backend..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/" -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ Backend está rodando" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Backend não está respondendo corretamente" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️  Backend não está rodando em http://localhost:5000" -ForegroundColor Yellow
    Write-Host "   Execute o backend primeiro: .\run-backend.ps1" -ForegroundColor Yellow
    Write-Host ""
    $continue = Read-Host "Continuar sem backend? (y/N)"
    if ($continue -ne "y" -and $continue -ne "Y") {
        exit 1
    }
}

# Executar aplicação
Write-Host "🚀 Iniciando frontend..." -ForegroundColor Green
Write-Host "   🌐 Frontend: http://localhost:4200" -ForegroundColor Cyan
Write-Host "   🔧 Backend: http://localhost:5000" -ForegroundColor Cyan
Write-Host ""
Write-Host "Pressione Ctrl+C para parar" -ForegroundColor Yellow
Write-Host ""

npm start 