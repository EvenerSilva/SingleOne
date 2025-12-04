# Script para aplicar simplificação dos cadastros
# Implementa herança automática para reduzir campos obrigatórios

Write-Host "🚀 APLICANDO SIMPLIFICAÇÃO DOS CADASTROS..." -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Cyan

# Verificar se o arquivo SQL existe
$sqlFile = "simplificar_cadastros.sql"
if (-not (Test-Path $sqlFile)) {
    Write-Host "❌ Arquivo SQL não encontrado: $sqlFile" -ForegroundColor Red
    exit 1
}

Write-Host "📋 Executando script de simplificação..." -ForegroundColor Yellow

try {
    # Executar o SQL
    $env:PGPASSWORD = "sua_senha_aqui"  # Substitua pela senha real do PostgreSQL
    
    Write-Host "🔧 Aplicando mudanças no banco de dados..." -ForegroundColor Cyan
    
    psql -h localhost -U postgres -d singleone -f $sqlFile
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Simplificação aplicada com sucesso!" -ForegroundColor Green
        Write-Host "" -ForegroundColor White
        Write-Host "🎯 MUDANÇAS IMPLEMENTADAS:" -ForegroundColor Yellow
        Write-Host "   • Campo 'cliente' agora é opcional em colaboradores" -ForegroundColor White
        Write-Host "   • Campo 'cliente' agora é opcional em equipamentos" -ForegroundColor White
        Write-Host "   • Triggers criados para herança automática" -ForegroundColor White
        Write-Host "   • Views simplificadas criadas" -ForegroundColor White
        Write-Host "" -ForegroundColor White
        Write-Host "💡 AGORA O CADASTRO É MAIS SIMPLES:" -ForegroundColor Cyan
        Write-Host "   • Colaborador: apenas Empresa + Centro de Custo (obrigatórios)" -ForegroundColor White
        Write-Host "   • Equipamento: apenas Empresa + Centro de Custo (obrigatórios)" -ForegroundColor White
        Write-Host "   • Cliente é preenchido automaticamente da empresa" -ForegroundColor White
        Write-Host "   • Filial e Localidade são opcionais" -ForegroundColor White
    } else {
        Write-Host "❌ Erro ao aplicar simplificação" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Erro ao executar script: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "" -ForegroundColor White
Write-Host "🏁 Processo concluído!" -ForegroundColor Green
Write-Host "" -ForegroundColor White
Write-Host "📝 PRÓXIMOS PASSOS:" -ForegroundColor Yellow
Write-Host "   1. Recompilar o backend" -ForegroundColor White
Write-Host "   2. Atualizar os modelos C#" -ForegroundColor White
Write-Host "   3. Simplificar os formulários do frontend" -ForegroundColor White
Write-Host "   4. Testar a herança automática" -ForegroundColor White
