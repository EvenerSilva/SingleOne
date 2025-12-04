Write-Host "🔍 Verificando status das requisições no banco de dados..." -ForegroundColor Green

# Configurar variáveis de ambiente
$env:DB_HOST = "127.0.0.1"
$env:DB_USER = "postgres"
$env:DB_PASSWORD = "Admin@2025"
$env:PGPASSWORD = $env:DB_PASSWORD

Write-Host "📊 Executando verificações no banco 'singleone'..." -ForegroundColor Yellow

# Executar o script SQL
$sqlScript = Get-Content "verificar_status_requisicoes.sql" -Raw

Write-Host ""
Write-Host "=== RESULTADO DA VERIFICAÇÃO ===" -ForegroundColor Cyan
Write-Host ""

# Executar cada consulta separadamente para melhor visualização
$queries = @(
    "SELECT 'Verificando tabela de status de requisições' as info, schemaname, tablename, tableowner FROM pg_tables WHERE tablename LIKE '%status%' OR tablename LIKE '%requisicao%';",
    
    "SELECT 'Verificando view requisicoesvm' as info, schemaname, viewname, viewowner FROM pg_views WHERE viewname = 'requisicoesvm';",
    
    "SELECT 'Estrutura da view requisicoesvm' as info, column_name, data_type, is_nullable FROM information_schema.columns WHERE table_name = 'requisicoesvm' ORDER BY ordinal_position;",
    
    "SELECT 'Valores únicos de status na view' as info, requisicaostatusid, requisicaostatus, COUNT(*) as quantidade FROM requisicoesvm GROUP BY requisicaostatusid, requisicaostatus ORDER BY requisicaostatusid;",
    
    "SELECT 'Valores únicos de status na tabela principal' as info, requisicaostatus, COUNT(*) as quantidade FROM requisico GROUP BY requisicaostatus ORDER BY requisicaostatus;",
    
    "SELECT 'Verificando inconsistências entre tabela e view' as info, r.requisicaostatus as status_tabela, rv.requisicaostatusid as status_id_view, rv.requisicaostatus as status_desc_view, COUNT(*) as quantidade FROM requisico r LEFT JOIN requisicoesvm rv ON r.id = rv.id GROUP BY r.requisicaostatus, rv.requisicaostatusid, rv.requisicaostatus ORDER BY r.requisicaostatus;",
    
    "SELECT 'Requisições com status 3 (supostamente cancelada)' as info, id, requisicaostatus, dtsolicitacao, dtenviotermo FROM requisico WHERE requisicaostatus = 3 ORDER BY id DESC LIMIT 5;",
    
    "SELECT 'Requisições com status 3 na view' as info, id, requisicaostatusid, requisicaostatus, dtsolicitacao, dtenviotermo FROM requisicoesvm WHERE requisicaostatusid = 3 ORDER BY id DESC LIMIT 5;"
)

foreach ($query in $queries) {
    Write-Host "Executando: $($query.Substring(0, [Math]::Min(50, $query.Length)))..." -ForegroundColor Gray
    
    try {
        $result = psql -h $env:DB_HOST -U $env:DB_USER -d singleone -c $query 2>$null
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host $result -ForegroundColor White
        } else {
            Write-Host "❌ Erro ao executar consulta" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "❌ Erro: $_" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "---" -ForegroundColor DarkGray
    Write-Host ""
}

Write-Host "✅ Verificação concluída!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 ANÁLISE DOS RESULTADOS:" -ForegroundColor Yellow
Write-Host "1. Verifique se os IDs dos status estão corretos" -ForegroundColor White
Write-Host "2. Compare os status entre a tabela e a view" -ForegroundColor White
Write-Host "3. Identifique possíveis inconsistências" -ForegroundColor White
Write-Host ""
Write-Host "🔧 PRÓXIMOS PASSOS:" -ForegroundColor Cyan
Write-Host "- Se houver inconsistências, corrija as constantes no código" -ForegroundColor White
Write-Host "- Se os IDs estiverem corretos, verifique a lógica de negócio" -ForegroundColor White
