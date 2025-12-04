Write-Host "🚀 Configurando ambiente de desenvolvimento SingleOne..." -ForegroundColor Green

# Verificar se Docker está instalado
try {
    docker --version | Out-Null
    Write-Host "✅ Docker encontrado" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker não está instalado. Por favor, instale o Docker Desktop primeiro." -ForegroundColor Red
    exit 1
}

# Verificar se Docker Compose está instalado
try {
    docker-compose --version | Out-Null
    Write-Host "✅ Docker Compose encontrado" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker Compose não está instalado." -ForegroundColor Red
    exit 1
}

# Parar containers existentes
Write-Host "🛑 Parando containers existentes..." -ForegroundColor Yellow
docker-compose down

# Construir e iniciar containers
Write-Host "🔨 Construindo e iniciando containers..." -ForegroundColor Yellow
docker-compose up --build -d

# Aguardar PostgreSQL estar pronto
Write-Host "⏳ Aguardando PostgreSQL estar pronto..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Verificar se os containers estão rodando
Write-Host "🔍 Verificando status dos containers..." -ForegroundColor Yellow
docker-compose ps

Write-Host ""
Write-Host "🎉 Ambiente de desenvolvimento configurado!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 URLs de acesso:" -ForegroundColor Cyan
Write-Host "   🌐 Frontend: http://localhost" -ForegroundColor White
Write-Host "   🔧 Backend API: http://localhost:5000" -ForegroundColor White
Write-Host "   📚 Swagger: http://localhost:5000/swagger" -ForegroundColor White
Write-Host "   🗄️  PostgreSQL: localhost:5432" -ForegroundColor White
Write-Host ""
Write-Host "📝 Comandos úteis:" -ForegroundColor Cyan
Write-Host "   - Ver logs: docker-compose logs -f" -ForegroundColor White
Write-Host "   - Parar: docker-compose down" -ForegroundColor White
Write-Host "   - Reiniciar: docker-compose restart" -ForegroundColor White
Write-Host ""
Write-Host "🔧 Para desenvolvimento local sem Docker:" -ForegroundColor Cyan
Write-Host "   1. Instale .NET 6.0 SDK" -ForegroundColor White
Write-Host "   2. Instale Node.js 10+" -ForegroundColor White
Write-Host "   3. Configure PostgreSQL localmente" -ForegroundColor White
Write-Host "   4. Execute os scripts de setup manual" -ForegroundColor White 