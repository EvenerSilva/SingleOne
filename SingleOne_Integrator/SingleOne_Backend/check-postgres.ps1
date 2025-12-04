Write-Host "🗄️  Verificando configuração do PostgreSQL..." -ForegroundColor Green

# Definir variáveis de ambiente
$env:DB_HOST = "localhost"
$env:DB_USER = "postgres"
$env:DB_PASSWORD = "password"

Write-Host "🔧 Configurações do banco:" -ForegroundColor Yellow
Write-Host "   🗄️  Host: $env:DB_HOST" -ForegroundColor White
Write-Host "   👤 Usuário: $env:DB_USER" -ForegroundColor White
Write-Host "   🔑 Senha: $env:DB_PASSWORD" -ForegroundColor White
Write-Host ""

# Verificar se PostgreSQL está rodando
Write-Host "🔄 Verificando PostgreSQL..." -ForegroundColor Yellow
try {
    $testConnection = Test-NetConnection -ComputerName localhost -Port 5432 -InformationLevel Quiet
    if ($testConnection.TcpTestSucceeded) {
        Write-Host "✅ PostgreSQL está acessível na porta 5432" -ForegroundColor Green
    } else {
        Write-Host "❌ PostgreSQL não está acessível na porta 5432" -ForegroundColor Red
        Write-Host "   Verifique se o PostgreSQL está instalado e rodando" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "❌ Erro ao verificar PostgreSQL" -ForegroundColor Red
    exit 1
}

# Testar conexão com o banco
Write-Host "🔗 Testando conexão com o banco..." -ForegroundColor Yellow
try {
    $env:PGPASSWORD = $env:DB_PASSWORD
    $result = psql -h $env:DB_HOST -U $env:DB_USER -d postgres -c "SELECT 1;" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Conexão com PostgreSQL estabelecida" -ForegroundColor Green
        
        # Verificar se o banco singleone existe
        $dbExists = psql -h $env:DB_HOST -U $env:DB_USER -d postgres -c "SELECT 1 FROM pg_database WHERE datname='singleone';" 2>$null
        if ($dbExists -like "*1*") {
            Write-Host "✅ Banco de dados 'singleone' existe" -ForegroundColor Green
        } else {
            Write-Host "📦 Criando banco de dados 'singleone'..." -ForegroundColor Yellow
            psql -h $env:DB_HOST -U $env:DB_USER -d postgres -c "CREATE DATABASE singleone;" 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Banco de dados 'singleone' criado" -ForegroundColor Green
            } else {
                Write-Host "❌ Erro ao criar banco de dados" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "❌ Erro ao conectar com PostgreSQL" -ForegroundColor Red
        Write-Host "   Verifique se a senha está correta: password" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "❌ Erro ao testar conexão" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "🎉 Configuração do PostgreSQL verificada com sucesso!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Para executar o backend:" -ForegroundColor Cyan
Write-Host "   .\run-backend.ps1" -ForegroundColor White 