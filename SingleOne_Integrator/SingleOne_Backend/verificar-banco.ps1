# Script PowerShell para verificar a tabela no banco PostgreSQL
# Execute este script para verificar a estrutura da tabela

Write-Host "🔍 Verificando tabela EstoqueMinimoEquipamentos no banco..." -ForegroundColor Cyan

# Configurações de conexão (ajuste conforme necessário)
$server = "localhost"
$port = "5432"
$database = "SingleOne"  # Ajuste o nome do banco se necessário
$username = "postgres"
$password = "Admin@2025"

# Comando psql para verificar a tabela
$sqlCommand = @"
-- Verificar se a tabela existe
SELECT 
    table_name,
    table_type,
    table_schema
FROM information_schema.tables 
WHERE table_name ILIKE '%estoque%minimo%' 
   OR table_name ILIKE '%estoqueminimo%'
ORDER BY table_name;

-- Verificar estrutura da tabela
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name ILIKE '%estoque%minimo%' 
   OR table_name ILIKE '%estoqueminimo%'
ORDER BY table_name, ordinal_position;

-- Verificar dados na tabela
SELECT COUNT(*) as total_registros FROM "EstoqueMinimoEquipamentos";

-- Verificar alguns registros de exemplo
SELECT * FROM "EstoqueMinimoEquipamentos" LIMIT 5;
"@

# Tentar executar psql se estiver disponível
try {
    Write-Host "📊 Executando consultas no banco..." -ForegroundColor Yellow
    
    # Salvar SQL em arquivo temporário
    $tempSqlFile = [System.IO.Path]::GetTempFileName() + ".sql"
    $sqlCommand | Out-File -FilePath $tempSqlFile -Encoding UTF8
    
    # Executar psql
    $env:PGPASSWORD = $password
    $psqlArgs = @(
        "-h", $server,
        "-p", $port,
        "-d", $database,
        "-U", $username,
        "-f", $tempSqlFile,
        "--quiet"
    )
    
    & psql @psqlArgs
    
    # Limpar arquivo temporário
    Remove-Item $tempSqlFile -Force
    
    Write-Host "✅ Verificação concluída!" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Erro ao executar psql: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "💡 Certifique-se de que o PostgreSQL está instalado e o psql está no PATH" -ForegroundColor Yellow
    Write-Host "💡 Ou execute o script SQL manualmente no pgAdmin ou outro cliente PostgreSQL" -ForegroundColor Yellow
    
    Write-Host "`n📝 Script SQL para executar manualmente:" -ForegroundColor Cyan
    Write-Host $sqlCommand -ForegroundColor White
}

Write-Host "`n🎯 Próximos passos:" -ForegroundColor Magenta
Write-Host "1. Verifique se a tabela existe e tem a estrutura correta" -ForegroundColor White
Write-Host "2. Se necessário, execute o script de criação da tabela" -ForegroundColor White
Write-Host "3. Teste o endpoint novamente" -ForegroundColor White
