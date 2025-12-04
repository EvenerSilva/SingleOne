Write-Host "📊 Aplicando SQL diretamente para criar tabela de geolocalização..." -ForegroundColor Green

# Configurar senha do PostgreSQL
$env:PGPASSWORD = "Admin@2025"

# Verificar conexão
Write-Host "🔗 Testando conexão com PostgreSQL..." -ForegroundColor Yellow
$testResult = psql -h 127.0.0.1 -U postgres -d singleone -c "SELECT 1;" 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro na conexão com PostgreSQL" -ForegroundColor Red
    Write-Host "   Verifique se o banco 'singleone' existe e as credenciais estão corretas" -ForegroundColor Yellow
    exit 1
}
Write-Host "✅ Conexão estabelecida com sucesso!" -ForegroundColor Green

# Aplicar SQL simplificado (sem foreign keys)
Write-Host "📝 Criando tabela geolocalizacao_assinatura..." -ForegroundColor Yellow
try {
    psql -h 127.0.0.1 -U postgres -d singleone -f create-geolocalizacao-simple.sql
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Tabela criada com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "⚠️  SQL executado com avisos. Verifique se a tabela já existe." -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Erro ao executar SQL" -ForegroundColor Red
    Write-Host "Erro: $($_.Exception.Message)" -ForegroundColor Red
    
    # Tentar versão ainda mais simples
    Write-Host "🔄 Tentando criar tabela de forma mais simples..." -ForegroundColor Yellow
    $simpleSql = @"
CREATE TABLE IF NOT EXISTS geolocalizacao_assinatura (
    id SERIAL PRIMARY KEY,
    colaborador_id INTEGER NOT NULL,
    colaborador_nome VARCHAR(255) NOT NULL,
    usuario_logado_id INTEGER NOT NULL,
    ip_address VARCHAR(45) NOT NULL,
    country VARCHAR(100),
    city VARCHAR(100),
    region VARCHAR(100),
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8),
    accuracy_meters DECIMAL(10, 2),
    timestamp_captura TIMESTAMP WITH TIME ZONE NOT NULL,
    acao VARCHAR(50) NOT NULL,
    data_criacao TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
"@
    
    echo $simpleSql | psql -h 127.0.0.1 -U postgres -d singleone
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Tabela criada com comando direto!" -ForegroundColor Green
    } else {
        Write-Host "❌ Falha ao criar tabela" -ForegroundColor Red
        exit 1
    }
}

# Verificar se a tabela foi criada
Write-Host "🔍 Verificando se a tabela foi criada..." -ForegroundColor Yellow
$tableCheck = psql -h 127.0.0.1 -U postgres -d singleone -c "SELECT tablename FROM pg_tables WHERE tablename = 'geolocalizacao_assinatura';" 2>$null
if ($tableCheck -like "*geolocalizacao_assinatura*") {
    Write-Host "✅ Tabela 'geolocalizacao_assinatura' encontrada!" -ForegroundColor Green
} else {
    Write-Host "❌ Tabela não foi criada corretamente" -ForegroundColor Red
    exit 1
}

# Verificar estrutura da tabela
Write-Host "📋 Estrutura da tabela:" -ForegroundColor Cyan
psql -h 127.0.0.1 -U postgres -d singleone -c "\d geolocalizacao_assinatura"

Write-Host ""
Write-Host "🎉 Configuração do banco concluída!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Para testar a inserção de dados:" -ForegroundColor Cyan
Write-Host "   Execute o backend e teste o endpoint de assinatura" -ForegroundColor White
