Write-Host "🔄 Criando migration para Geolocalização..." -ForegroundColor Green

# Navegar para o diretório da API
cd SingleOneAPI

# Verificar se Entity Framework tools está instalado
Write-Host "📦 Verificando Entity Framework tools..." -ForegroundColor Yellow
try {
    dotnet ef --version
    Write-Host "✅ Entity Framework tools encontrado" -ForegroundColor Green
} catch {
    Write-Host "❌ Entity Framework tools não encontrado. Instalando..." -ForegroundColor Red
    dotnet tool install --global dotnet-ef
}

# Definir variáveis de ambiente
Write-Host "🔧 Configurando variáveis de ambiente..." -ForegroundColor Yellow
$env:DB_HOST = "127.0.0.1"
$env:DB_USER = "postgres"
$env:DB_PASSWORD = "Admin@2025"
$env:SITE_URL = "http://localhost:4200"

# Criar migration
Write-Host "📝 Criando migration AddGeolocalizacaoAssinatura..." -ForegroundColor Yellow
try {
    dotnet ef migrations add AddGeolocalizacaoAssinatura
    Write-Host "✅ Migration criada com sucesso!" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro ao criar migration" -ForegroundColor Red
    Write-Host "Erro: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Aplicar migration
Write-Host "🚀 Aplicando migration ao banco de dados..." -ForegroundColor Yellow
try {
    dotnet ef database update
    Write-Host "✅ Migration aplicada com sucesso!" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro ao aplicar migration" -ForegroundColor Red
    Write-Host "Erro: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Tentativas alternativas:" -ForegroundColor Cyan
    Write-Host "   1. Execute manualmente: dotnet ef database update" -ForegroundColor White
    Write-Host "   2. Execute o SQL diretamente: .\create-geolocalizacao-table.sql" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "🎉 Configuração do banco concluída!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Nova tabela criada:" -ForegroundColor Cyan
Write-Host "   📊 geolocalizacao_assinatura" -ForegroundColor White
Write-Host ""
Write-Host "🔍 Para verificar se funcionou:" -ForegroundColor Cyan
Write-Host "   SELECT * FROM geolocalizacao_assinatura LIMIT 1;" -ForegroundColor White

cd ..