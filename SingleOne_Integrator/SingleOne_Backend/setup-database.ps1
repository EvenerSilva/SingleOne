Write-Host "🗄️  Configurando banco de dados SingleOne..." -ForegroundColor Green
Write-Host ""

# Configurar senha
$env:PGPASSWORD = "Admin@2025"

Write-Host "🔗 Testando conexão com PostgreSQL..." -ForegroundColor Yellow
try {
    $resultado = psql -h 127.0.0.1 -U postgres -d postgres -c "SELECT 1;" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Conexão estabelecida com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "❌ Erro na conexão com PostgreSQL" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Erro ao testar conexão" -ForegroundColor Red
    exit 1
}

Write-Host "📦 Verificando banco 'singleone'..." -ForegroundColor Yellow
$dbExiste = psql -h 127.0.0.1 -U postgres -d postgres -c "SELECT 1 FROM pg_database WHERE datname='singleone';" 2>$null
if ($dbExiste -like "*1*") {
    Write-Host "✅ Banco 'singleone' já existe" -ForegroundColor Green
} else {
    Write-Host "📦 Criando banco 'singleone'..." -ForegroundColor Yellow
    $criarBanco = psql -h 127.0.0.1 -U postgres -d postgres -c "CREATE DATABASE singleone;" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Banco 'singleone' criado com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "❌ Erro ao criar banco 'singleone'" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "🎉 Banco de dados configurado com sucesso!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Configurações:" -ForegroundColor Cyan
Write-Host "   🗄️  Host: 127.0.0.1" -ForegroundColor White
Write-Host "   👤 Usuário: postgres" -ForegroundColor White
Write-Host "   🔑 Senha: Admin@2025" -ForegroundColor White
Write-Host "   📊 Database: singleone" -ForegroundColor White
Write-Host ""
Write-Host "✅ Agora você pode executar: .\run-backend.ps1" -ForegroundColor Green