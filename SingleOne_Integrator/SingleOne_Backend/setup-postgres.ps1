Write-Host "🗄️  Configurando PostgreSQL para SingleOne (Windows)..." -ForegroundColor Green

# Verificar se PostgreSQL está instalado
try {
    $psqlVersion = psql --version
    Write-Host "✅ PostgreSQL encontrado: $psqlVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ PostgreSQL não está instalado." -ForegroundColor Red
    Write-Host ""
    Write-Host "📦 Para instalar PostgreSQL no Windows:" -ForegroundColor Yellow
    Write-Host "   1. Baixe do site oficial: https://www.postgresql.org/download/windows/" -ForegroundColor Cyan
    Write-Host "   2. Execute o instalador" -ForegroundColor Cyan
    Write-Host "   3. Use a senha: password" -ForegroundColor Cyan
    Write-Host "   4. Mantenha a porta padrão: 5432" -ForegroundColor Cyan
    Write-Host "   5. Instale o pgAdmin (opcional)" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "   Ou use o Chocolatey:" -ForegroundColor Yellow
    Write-Host "   choco install postgresql" -ForegroundColor Cyan
    Write-Host ""
    exit 1
}

# Verificar se o serviço PostgreSQL está rodando
Write-Host "🔄 Verificando serviço PostgreSQL..." -ForegroundColor Yellow
try {
    $service = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue
    if ($service) {
        if ($service.Status -eq "Running") {
            Write-Host "✅ Serviço PostgreSQL está rodando" -ForegroundColor Green
        } else {
            Write-Host "🔄 Iniciando serviço PostgreSQL..." -ForegroundColor Yellow
            Start-Service $service.Name
            Start-Sleep -Seconds 5
            Write-Host "✅ Serviço PostgreSQL iniciado" -ForegroundColor Green
        }
    } else {
        Write-Host "⚠️  Serviço PostgreSQL não encontrado. Verifique se está instalado corretamente." -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️  Não foi possível verificar o serviço PostgreSQL" -ForegroundColor Yellow
}

# Verificar se o banco singleone existe
Write-Host "📦 Verificando banco de dados..." -ForegroundColor Yellow
try {
    $testConnection = Test-NetConnection -ComputerName localhost -Port 5432 -InformationLevel Quiet
    if ($testConnection.TcpTestSucceeded) {
        Write-Host "✅ PostgreSQL está acessível na porta 5432" -ForegroundColor Green
        
        # Tentar conectar e criar banco
        try {
            $env:PGPASSWORD = "password"
            $result = psql -h localhost -U postgres -d postgres -c "SELECT 1;" 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Conexão com PostgreSQL estabelecida" -ForegroundColor Green
                
                # Verificar se o banco singleone existe
                $dbExists = psql -h localhost -U postgres -d postgres -c "SELECT 1 FROM pg_database WHERE datname='singleone';" 2>$null
                if ($dbExists -like "*1*") {
                    Write-Host "✅ Banco de dados 'singleone' já existe" -ForegroundColor Green
                } else {
                    Write-Host "📦 Criando banco de dados 'singleone'..." -ForegroundColor Yellow
                    psql -h localhost -U postgres -d postgres -c "CREATE DATABASE singleone;" 2>$null
                    if ($LASTEXITCODE -eq 0) {
                        Write-Host "✅ Banco de dados 'singleone' criado" -ForegroundColor Green
                    } else {
                        Write-Host "❌ Erro ao criar banco de dados" -ForegroundColor Red
                    }
                }
            } else {
                Write-Host "❌ Erro ao conectar com PostgreSQL" -ForegroundColor Red
            }
        } catch {
            Write-Host "❌ Erro ao conectar com PostgreSQL" -ForegroundColor Red
        }
    } else {
        Write-Host "❌ PostgreSQL não está acessível na porta 5432" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Não foi possível testar PostgreSQL" -ForegroundColor Red
}

Write-Host ""
Write-Host "🎉 Configuração do PostgreSQL concluída!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Configurações do banco:" -ForegroundColor Cyan
Write-Host "   🗄️  Host: localhost" -ForegroundColor White
Write-Host "   📊 Database: singleone" -ForegroundColor White
Write-Host "   👤 Usuário: postgres" -ForegroundColor White
Write-Host "   🔑 Senha: password" -ForegroundColor White
Write-Host ""
Write-Host "🔧 Para conectar via psql:" -ForegroundColor Cyan
Write-Host "   psql -h localhost -U postgres -d singleone" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  Se houver problemas, verifique:" -ForegroundColor Yellow
Write-Host "   1. Se o PostgreSQL está instalado corretamente" -ForegroundColor White
Write-Host "   2. Se a senha está correta (password)" -ForegroundColor White
Write-Host "   3. Se o serviço está rodando" -ForegroundColor White 