# =====================================================
# Script PowerShell: Adicionar campo usarpadrao em politicas_elegibilidade
# Descrição: Executa o script SQL para adicionar o campo
# =====================================================

Write-Host "🚀 Iniciando atualização da tabela politicas_elegibilidade..." -ForegroundColor Cyan
Write-Host ""

# Configuração do banco
$dbHost = "localhost"
$dbPort = "5432"
$dbName = "singleone"
$dbUser = "postgres"
$dbPassword = "postgres"

# Caminho do script SQL
$sqlFile = "adicionar-campo-usarpadrao-politicas.sql"

# Verificar se o arquivo SQL existe
if (-not (Test-Path $sqlFile)) {
    Write-Host "❌ Erro: Arquivo SQL não encontrado: $sqlFile" -ForegroundColor Red
    exit 1
}

Write-Host "📂 Arquivo SQL encontrado: $sqlFile" -ForegroundColor Green
Write-Host "🔍 Conectando ao banco de dados..." -ForegroundColor Yellow
Write-Host "   Host: $dbHost" -ForegroundColor Gray
Write-Host "   Database: $dbName" -ForegroundColor Gray
Write-Host ""

# Configurar variável de ambiente para senha
$env:PGPASSWORD = $dbPassword

try {
    # Executar o script SQL
    Write-Host "⚙️ Executando script SQL..." -ForegroundColor Yellow
    $output = & psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -f $sqlFile 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Script executado com sucesso!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📊 Resultado:" -ForegroundColor Cyan
        Write-Host $output -ForegroundColor White
        Write-Host ""
        Write-Host "🎯 Campo 'usarpadrao' adicionado com sucesso!" -ForegroundColor Green
        Write-Host "   - Default: true (usa padrão LIKE '%cargo%')" -ForegroundColor Gray
        Write-Host "   - false = match exato" -ForegroundColor Gray
    } else {
        Write-Host ""
        Write-Host "❌ Erro ao executar script!" -ForegroundColor Red
        Write-Host $output -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "❌ Erro inesperado: $_" -ForegroundColor Red
    exit 1
} finally {
    # Limpar variável de ambiente
    Remove-Item Env:\PGPASSWORD
}

Write-Host ""
Write-Host "✨ Atualização concluída!" -ForegroundColor Cyan
Write-Host ""

