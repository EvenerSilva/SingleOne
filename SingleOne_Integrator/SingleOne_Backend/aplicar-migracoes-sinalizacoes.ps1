# 🗄️ SCRIPT PARA APLICAR MIGRAÇÕES DAS SINALIZAÇÕES DE SUSPEITAS
# Este script aplica as migrações necessárias para as tabelas de sinalizações

Write-Host "🗄️ APLICANDO MIGRAÇÕES DAS SINALIZAÇÕES DE SUSPEITAS" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan

# Verificar se o arquivo SQL existe
$sqlFile = "sinalizacao_suspeitas_final.sql"
if (-not (Test-Path $sqlFile)) {
    Write-Host "❌ Arquivo $sqlFile não encontrado!" -ForegroundColor Red
    Write-Host "Certifique-se de que o arquivo está no diretório atual." -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Arquivo $sqlFile encontrado" -ForegroundColor Green

# Configurações do banco (ajuste conforme necessário)
$dbHost = "localhost"
$dbPort = "5432"
$dbName = "singleone"  # Ajuste conforme seu banco
$dbUser = "postgres"   # Ajuste conforme sua configuração
$dbPassword = "sua_senha_aqui"  # ⚠️ SUBSTITUA pela senha real

Write-Host "`n📋 Configurações do banco:" -ForegroundColor Yellow
Write-Host "   Host: $dbHost" -ForegroundColor Gray
Write-Host "   Porta: $dbPort" -ForegroundColor Gray
Write-Host "   Banco: $dbName" -ForegroundColor Gray
Write-Host "   Usuário: $dbUser" -ForegroundColor Gray

# Verificar se o psql está disponível
try {
    $psqlVersion = psql --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ PostgreSQL client (psql) encontrado: $psqlVersion" -ForegroundColor Green
    } else {
        throw "psql não encontrado"
    }
} catch {
    Write-Host "❌ PostgreSQL client (psql) não encontrado!" -ForegroundColor Red
    Write-Host "Instale o PostgreSQL ou adicione o psql ao PATH." -ForegroundColor Yellow
    exit 1
}

# Definir variável de ambiente para senha
$env:PGPASSWORD = $dbPassword

Write-Host "`n🚀 Executando migrações..." -ForegroundColor Yellow

try {
    # Executar o script SQL
    $result = psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -f $sqlFile 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Migrações aplicadas com sucesso!" -ForegroundColor Green
        
        # Verificar se as tabelas foram criadas
        Write-Host "`n🔍 Verificando tabelas criadas..." -ForegroundColor Yellow
        
        $verificacaoQuery = @"
SELECT 
    'sinalizacoes_suspeitas' as tabela,
    COUNT(*) as colunas
FROM information_schema.columns 
WHERE table_name = 'sinalizacoes_suspeitas'

UNION ALL

SELECT 
    'historico_investigacoes' as tabela,
    COUNT(*) as colunas
FROM information_schema.columns 
WHERE table_name = 'historico_investigacoes'

UNION ALL

SELECT 
    'motivos_suspeita' as tabela,
    COUNT(*) as colunas
FROM information_schema.columns 
WHERE table_name = 'motivos_suspeita';
"@

        $verificacao = psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -c $verificacaoQuery -t 2>$null
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Verificação das tabelas:" -ForegroundColor Green
            $verificacao | ForEach-Object {
                if ($_.Trim() -ne "") {
                    $parts = $_.Trim() -split '\|'
                    if ($parts.Length -eq 2) {
                        $tabela = $parts[0].Trim()
                        $colunas = $parts[1].Trim()
                        Write-Host "   - $tabela : $colunas colunas" -ForegroundColor Gray
                    }
                }
            }
        }
        
        # Verificar dados inseridos
        Write-Host "`n📊 Verificando dados inseridos..." -ForegroundColor Yellow
        
        $dadosQuery = "SELECT COUNT(*) as total FROM motivos_suspeita;"
        $totalMotivos = psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -c $dadosQuery -t 2>$null
        
        if ($LASTEXITCODE -eq 0) {
            $totalMotivos = $totalMotivos.Trim()
            Write-Host "✅ Motivos de suspeita inseridos: $totalMotivos" -ForegroundColor Green
        }
        
        Write-Host "`n🎉 MIGRAÇÕES CONCLUÍDAS COM SUCESSO!" -ForegroundColor Green
        Write-Host "=====================================" -ForegroundColor Green
        Write-Host "As tabelas de sinalizações de suspeitas foram criadas e configuradas." -ForegroundColor White
        
    } else {
        Write-Host "❌ Erro ao aplicar migrações!" -ForegroundColor Red
        Write-Host "Detalhes do erro:" -ForegroundColor Yellow
        Write-Host $result -ForegroundColor Red
        exit 1
    }
    
} catch {
    Write-Host "❌ Erro inesperado: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    # Limpar variável de ambiente
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host "`n📝 PRÓXIMOS PASSOS:" -ForegroundColor Yellow
Write-Host "1. Inicie o backend da aplicação" -ForegroundColor Gray
Write-Host "2. Teste os endpoints usando o script teste-sinalizacoes-api.ps1" -ForegroundColor Gray
Write-Host "3. Acesse o frontend em /relatorios/sinalizacoes-suspeitas" -ForegroundColor Gray
Write-Host "4. Configure usuários com permissões adequadas" -ForegroundColor Gray
