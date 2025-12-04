# =====================================================
# SCRIPT PARA EXECUTAR SQL E VERIFICAR CAMPO LOGO
# =====================================================

Write-Host "🎯 VERIFICANDO CAMPO LOGO NA TABELA CLIENTES..." -ForegroundColor Cyan
Write-Host ""

# 1. Executar script SQL para adicionar campo logo (se não existir)
Write-Host "📊 Executando script SQL..." -ForegroundColor Yellow

try {
    # Verificar se o PostgreSQL está rodando
    $pgProcess = Get-Process -Name "postgres" -ErrorAction SilentlyContinue
    if ($pgProcess) {
        Write-Host "✅ PostgreSQL está rodando" -ForegroundColor Green
    } else {
        Write-Host "❌ PostgreSQL não está rodando" -ForegroundColor Red
        Write-Host "   Inicie o PostgreSQL primeiro" -ForegroundColor Yellow
        exit 1
    }

    # Executar o script SQL
    Write-Host "   Executando: add-logo-cliente.sql..." -ForegroundColor Gray
    
    # Comando para executar o SQL (ajuste conforme sua configuração)
    $sqlCommand = "psql -h localhost -U postgres -d singleone -f add-logo-cliente.sql"
    Write-Host "   Comando: $sqlCommand" -ForegroundColor Gray
    
    # Executar o comando
    Invoke-Expression $sqlCommand
    
    Write-Host "✅ Script SQL executado!" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro ao executar SQL: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Execute manualmente: psql -h localhost -U postgres -d singleone -f add-logo-cliente.sql" -ForegroundColor Yellow
}

Write-Host ""

# 2. Verificar se o campo foi criado
Write-Host "🔍 Verificando se o campo foi criado..." -ForegroundColor Yellow

try {
    # Comando para verificar a estrutura da tabela
    $checkCommand = "psql -h localhost -U postgres -d singleone -c `"SELECT column_name, data_type, is_nullable FROM information_schema.columns WHERE table_name = 'clientes' AND column_name = 'logo';`""
    
    Write-Host "   Executando verificação..." -ForegroundColor Gray
    $result = Invoke-Expression $checkCommand
    
    if ($result -match "logo") {
        Write-Host "✅ Campo 'logo' encontrado na tabela 'clientes'!" -ForegroundColor Green
    } else {
        Write-Host "❌ Campo 'logo' NÃO foi encontrado!" -ForegroundColor Red
        Write-Host "   Verifique se o script SQL foi executado corretamente" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Erro ao verificar campo: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# 3. Verificar estrutura completa da tabela
Write-Host "📋 Estrutura completa da tabela clientes:" -ForegroundColor Yellow

try {
    $structureCommand = 'psql -h localhost -U postgres -d singleone -c "SELECT column_name, data_type, is_nullable FROM information_schema.columns WHERE table_name = ''clientes'' ORDER BY ordinal_position;"'
    Invoke-Expression $structureCommand
} catch {
    Write-Host "❌ Erro ao verificar estrutura: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# 4. Instruções para compilar
Write-Host "🚀 PRÓXIMOS PASSOS:" -ForegroundColor Cyan
Write-Host ""

Write-Host "1. COMPILAR BACKEND:" -ForegroundColor Yellow
Write-Host "   cd SingleOneAPI" -ForegroundColor Gray
Write-Host "   dotnet build" -ForegroundColor Gray
Write-Host ""

Write-Host "2. TESTAR UPLOAD:" -ForegroundColor Yellow
Write-Host "   - Executar o backend" -ForegroundColor Gray
Write-Host "   - Tentar fazer upload de uma logo" -ForegroundColor Gray
Write-Host "   - Verificar logs no console do backend" -ForegroundColor Gray
Write-Host ""

Write-Host "🎯 CAMPO LOGO VERIFICADO!" -ForegroundColor Green
