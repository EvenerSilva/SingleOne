# ============================================================
# Script de Instalação do TinOne
# ============================================================
# Aplica as configurações necessárias para o assistente TinOne
# Totalmente reversível e não-invasivo
# ============================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Instalação do Assistente TinOne" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configurações do PostgreSQL
$DB_HOST = "localhost"
$DB_PORT = "5432"
$DB_NAME = "singleone"
$DB_USER = "postgres"
$DB_PASSWORD = "password"

Write-Host "Conectando ao banco de dados..." -ForegroundColor Yellow
Write-Host "Database: $DB_NAME" -ForegroundColor Gray
Write-Host ""

# Função para executar SQL
function Execute-SqlFile {
    param (
        [string]$FilePath,
        [string]$Description
    )
    
    Write-Host "► $Description" -ForegroundColor Cyan
    
    if (-not (Test-Path $FilePath)) {
        Write-Host "  ✗ Arquivo não encontrado: $FilePath" -ForegroundColor Red
        return $false
    }
    
    try {
        $env:PGPASSWORD = $DB_PASSWORD
        $result = psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f $FilePath 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ Concluído com sucesso!" -ForegroundColor Green
            return $true
        } else {
            Write-Host "  ✗ Erro ao executar: $result" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "  ✗ Erro: $_" -ForegroundColor Red
        return $false
    }
    finally {
        $env:PGPASSWORD = $null
    }
}

# Etapa 1: Adicionar parâmetros
Write-Host "Etapa 1/2: Configurando parâmetros do TinOne..." -ForegroundColor Yellow
$params_ok = Execute-SqlFile -FilePath ".\setup-tinone-params.sql" -Description "Inserindo parâmetros de configuração"

if (-not $params_ok) {
    Write-Host ""
    Write-Host "⚠️  Erro ao adicionar parâmetros. Verifique se a tabela 'parametros' existe." -ForegroundColor Red
    Write-Host ""
    Read-Host "Pressione ENTER para continuar mesmo assim ou CTRL+C para cancelar"
}

Write-Host ""

# Etapa 2: Criar tabelas de analytics (opcional)
Write-Host "Etapa 2/2: Criando tabelas de analytics (opcional)..." -ForegroundColor Yellow
$tables_ok = Execute-SqlFile -FilePath ".\create-tinone-tables.sql" -Description "Criando tabelas tinone_analytics, tinone_conversas, etc"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

if ($params_ok) {
    Write-Host "✓ Instalação concluída!" -ForegroundColor Green
    Write-Host ""
    Write-Host "O assistente TinOne está habilitado." -ForegroundColor Green
    Write-Host ""
    Write-Host "📌 Próximos passos:" -ForegroundColor Yellow
    Write-Host "   1. Execute o backend: .\run-backend.ps1" -ForegroundColor Gray
    Write-Host "   2. Execute o frontend: .\run-frontend.ps1" -ForegroundColor Gray
    Write-Host "   3. O TinOne aparecerá no canto inferior direito" -ForegroundColor Gray
    Write-Host ""
    Write-Host "🔧 Para desabilitar o TinOne:" -ForegroundColor Yellow
    Write-Host "   Vá em Configurações > Parâmetros > TINONE_HABILITADO = false" -ForegroundColor Gray
} else {
    Write-Host "⚠️  Instalação parcial" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Alguns passos falharam. Verifique:" -ForegroundColor Yellow
    Write-Host "  • PostgreSQL está rodando?" -ForegroundColor Gray
    Write-Host "  • Credenciais estão corretas?" -ForegroundColor Gray
    Write-Host "  • Banco 'singleone' existe?" -ForegroundColor Gray
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar parâmetros inseridos
Write-Host "Verificando parâmetros inseridos..." -ForegroundColor Yellow
$env:PGPASSWORD = $DB_PASSWORD
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "SELECT chave, valor, descricao FROM parametros WHERE chave LIKE 'TINONE_%' ORDER BY chave;"
$env:PGPASSWORD = $null

Write-Host ""
Write-Host "Pressione ENTER para sair..." -ForegroundColor Gray
Read-Host

