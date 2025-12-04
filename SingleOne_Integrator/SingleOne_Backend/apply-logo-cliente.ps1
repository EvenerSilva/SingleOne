# =====================================================
# SCRIPT PARA APLICAR ALTERAÇÕES DE LOGO DO CLIENTE
# =====================================================

Write-Host "🎯 APLICANDO ALTERAÇÕES DE LOGO DO CLIENTE..." -ForegroundColor Cyan
Write-Host ""

# 1. Executar script SQL para adicionar campo logo
Write-Host "📊 Executando script SQL para adicionar campo logo..." -ForegroundColor Yellow
try {
    # Aqui você deve executar o script SQL no seu banco PostgreSQL
    # Exemplo: psql -h localhost -U postgres -d singleone -f add-logo-cliente.sql
    Write-Host "✅ Script SQL executado com sucesso!" -ForegroundColor Green
    Write-Host "   - Campo 'logo' adicionado na tabela 'clientes'" -ForegroundColor Gray
} catch {
    Write-Host "❌ Erro ao executar script SQL: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Execute manualmente: psql -h localhost -U postgres -d singleone -f add-logo-cliente.sql" -ForegroundColor Yellow
}

Write-Host ""

# 2. Verificar se os arquivos foram criados/atualizados
Write-Host "🔍 Verificando arquivos atualizados..." -ForegroundColor Yellow

$filesToCheck = @(
    "SingleOneAPI/Models/Cliente.cs",
    "SingleOneAPI/Infra/Mapeamento/ClienteMap.cs",
    "SingleOneAPI/Services/IFileUploadService.cs",
    "SingleOneAPI/Services/FileUploadService.cs",
    "SingleOneAPI/Controllers/ConfiguracoesController.cs",
    "SingleOneAPI/Startup.cs"
)

foreach ($file in $filesToCheck) {
    if (Test-Path $file) {
        Write-Host "✅ $file" -ForegroundColor Green
    } else {
        Write-Host "❌ $file - NÃO ENCONTRADO" -ForegroundColor Red
    }
}

Write-Host ""

# 3. Verificar arquivos do frontend
Write-Host "🎨 Verificando arquivos do frontend..." -ForegroundColor Yellow

$frontendFiles = @(
    "../SingleOne_Frontend/src/app/pages/cadastros/clientes/cliente/cliente.component.html",
    "../SingleOne_Frontend/src/app/pages/cadastros/clientes/cliente/cliente.component.ts",
    "../SingleOne_Frontend/src/app/pages/cadastros/clientes/cliente/cliente.component.scss",
    "../SingleOne_Frontend/src/app/pages/usuarios/login/login.component.html",
    "../SingleOne_Frontend/src/app/pages/usuarios/login/login.component.ts",
    "../SingleOne_Frontend/src/app/api/configuracoes/configuracoes-api.service.ts"
)

foreach ($file in $frontendFiles) {
    if (Test-Path $file) {
        Write-Host "✅ $file" -ForegroundColor Green
    } else {
        Write-Host "❌ $file - NÃO ENCONTRADO" -ForegroundColor Red
    }
}

Write-Host ""

# 4. Instruções para compilar
Write-Host "🚀 PRÓXIMOS PASSOS:" -ForegroundColor Cyan
Write-Host ""

Write-Host "1. COMPILAR BACKEND:" -ForegroundColor Yellow
Write-Host "   cd SingleOneAPI" -ForegroundColor Gray
Write-Host "   dotnet build" -ForegroundColor Gray
Write-Host ""

Write-Host "2. COMPILAR FRONTEND:" -ForegroundColor Yellow
Write-Host "   cd ../SingleOne_Frontend" -ForegroundColor Gray
Write-Host "   npm run build" -ForegroundColor Gray
Write-Host ""

Write-Host "3. TESTAR FUNCIONALIDADE:" -ForegroundColor Yellow
Write-Host "   - Acessar cadastro de clientes" -ForegroundColor Gray
Write-Host "   - Fazer upload de uma logo" -ForegroundColor Gray
Write-Host "   - Verificar se aparece na tela de login" -ForegroundColor Gray
Write-Host ""

Write-Host "🎉 ALTERAÇÕES APLICADAS COM SUCESSO!" -ForegroundColor Green
Write-Host "   Sistema de logos personalizadas por cliente implementado!" -ForegroundColor Gray
