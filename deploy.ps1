# ============================================
# Script de Deploy - SingleOne Docker (Windows)
# ============================================

$ErrorActionPreference = "Stop"

Write-Host "🚀 Iniciando deploy do SingleOne..." -ForegroundColor Cyan

# Verificar se .env existe
if (-not (Test-Path ".env")) {
    Write-Host "❌ Arquivo .env não encontrado!" -ForegroundColor Red
    Write-Host "Copie o env.example para .env e configure as variáveis" -ForegroundColor Yellow
    Write-Host "Comando: Copy-Item env.example .env" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ Arquivo .env encontrado" -ForegroundColor Green

# Verificar se Docker está instalado
try {
    docker --version | Out-Null
    Write-Host "✓ Docker instalado" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker não está instalado!" -ForegroundColor Red
    exit 1
}

# Verificar se Docker Compose está instalado
try {
    docker-compose --version | Out-Null
    Write-Host "✓ Docker Compose instalado" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker Compose não está instalado!" -ForegroundColor Red
    exit 1
}

# Parar containers antigos se existirem
Write-Host "⏹️  Parando containers antigos..." -ForegroundColor Yellow
docker-compose down

# Build das imagens
Write-Host "🔨 Construindo imagens Docker..." -ForegroundColor Yellow
docker-compose build --no-cache

# Iniciar containers
Write-Host "🚀 Iniciando containers..." -ForegroundColor Yellow
docker-compose up -d

# Aguardar serviços ficarem prontos
Write-Host "⏳ Aguardando serviços iniciarem..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Verificar status
Write-Host "📊 Status dos serviços:" -ForegroundColor Yellow
docker-compose ps

# Testar saúde dos serviços
Write-Host ""
Write-Host "🏥 Testando saúde dos serviços..." -ForegroundColor Yellow

# Testar banco
try {
    docker-compose exec -T database pg_isready -U postgres | Out-Null
    Write-Host "✓ Banco de dados: OK" -ForegroundColor Green
} catch {
    Write-Host "✗ Banco de dados: ERRO" -ForegroundColor Red
}

# Aguardar um pouco mais para o backend
Start-Sleep -Seconds 5

# Testar backend
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/test/health" -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Backend API: OK" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠ Backend API: Ainda não respondeu (normal na primeira vez)" -ForegroundColor Yellow
}

# Testar frontend
try {
    $response = Invoke-WebRequest -Uri "http://localhost:4200/health" -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Frontend: OK" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠ Frontend: Ainda não respondeu" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "🎉 Deploy concluído!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "📱 Acesse a aplicação:"
Write-Host "   Frontend: " -NoNewline
Write-Host "http://localhost:4200" -ForegroundColor Green
Write-Host "   Backend:  " -NoNewline
Write-Host "http://localhost:5000" -ForegroundColor Green
Write-Host ""
Write-Host "📝 Para ver os logs:"
Write-Host "   docker-compose logs -f" -ForegroundColor Yellow
Write-Host ""
Write-Host "🛑 Para parar tudo:"
Write-Host "   docker-compose down" -ForegroundColor Yellow
Write-Host ""


