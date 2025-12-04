# =====================================================
# Script: Aplicar Políticas de Elegibilidade no Banco
# =====================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Aplicar Políticas de Elegibilidade  " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Parâmetros de conexão (ajuste conforme necessário)
$env:PGPASSWORD = "postgres"
$dbHost = "localhost"
$dbPort = "5432"
$dbName = "singleone"
$dbUser = "postgres"

Write-Host "📋 Configuração:" -ForegroundColor Yellow
Write-Host "   Host: $dbHost"
Write-Host "   Porta: $dbPort"
Write-Host "   Banco: $dbName"
Write-Host "   Usuário: $dbUser"
Write-Host ""

# Verificar se o arquivo SQL existe
$sqlFile = "criar-tabela-politicas-elegibilidade.sql"
if (-not (Test-Path $sqlFile)) {
    Write-Host "❌ Erro: Arquivo $sqlFile não encontrado!" -ForegroundColor Red
    Write-Host "   Certifique-se de estar no diretório correto." -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Arquivo SQL encontrado: $sqlFile" -ForegroundColor Green
Write-Host ""

# Confirmar execução
Write-Host "⚠️  Este script irá:" -ForegroundColor Yellow
Write-Host "   1. Criar a tabela 'politicas_elegibilidade'" -ForegroundColor White
Write-Host "   2. Criar a view 'vw_nao_conformidade_elegibilidade'" -ForegroundColor White
Write-Host "   3. Criar índices e constraints" -ForegroundColor White
Write-Host ""

$confirmation = Read-Host "Deseja continuar? (S/N)"
if ($confirmation -ne 'S' -and $confirmation -ne 's') {
    Write-Host "❌ Operação cancelada pelo usuário." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "🚀 Executando script SQL..." -ForegroundColor Cyan

try {
    # Executar o script SQL usando psql
    $psqlCommand = "psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -f `"$sqlFile`""
    
    Write-Host "   Comando: $psqlCommand" -ForegroundColor Gray
    Write-Host ""
    
    Invoke-Expression $psqlCommand
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "✅ Script executado com sucesso!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "📊 Estruturas criadas:" -ForegroundColor Cyan
        Write-Host "   ✓ Tabela: politicas_elegibilidade" -ForegroundColor Green
        Write-Host "   ✓ View: vw_nao_conformidade_elegibilidade" -ForegroundColor Green
        Write-Host "   ✓ Índices de performance" -ForegroundColor Green
        Write-Host ""
        Write-Host "💡 Próximos passos:" -ForegroundColor Yellow
        Write-Host "   1. Reiniciar a API backend" -ForegroundColor White
        Write-Host "   2. Testar os endpoints de políticas" -ForegroundColor White
        Write-Host "   3. Criar políticas de exemplo via interface" -ForegroundColor White
        Write-Host ""
    } else {
        Write-Host ""
        Write-Host "❌ Erro ao executar o script SQL." -ForegroundColor Red
        Write-Host "   Código de saída: $LASTEXITCODE" -ForegroundColor Red
        Write-Host ""
        Write-Host "💡 Possíveis causas:" -ForegroundColor Yellow
        Write-Host "   - PostgreSQL não está instalado ou não está no PATH" -ForegroundColor White
        Write-Host "   - Credenciais incorretas" -ForegroundColor White
        Write-Host "   - Banco de dados não existe" -ForegroundColor White
        Write-Host "   - Tabela já existe (neste caso, ignore o erro)" -ForegroundColor White
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "❌ Erro ao executar o script:" -ForegroundColor Red
    Write-Host "   $_" -ForegroundColor Red
    exit 1
}

# Limpar senha do ambiente
$env:PGPASSWORD = $null

Write-Host "Pressione qualquer tecla para sair..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

