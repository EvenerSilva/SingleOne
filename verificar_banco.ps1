# =====================================================
# SCRIPT DE VERIFICAÇÃO DO BANCO SINGLEONE (PowerShell)
# =====================================================

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "🔍 VERIFICANDO BANCO DE DADOS SINGLEONE" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Configurações (padrões do docker-compose)
$DB_HOST = $env:DB_HOST
if ([string]::IsNullOrEmpty($DB_HOST)) { $DB_HOST = "localhost" }

$DB_PORT = $env:DB_PORT
if ([string]::IsNullOrEmpty($DB_PORT)) { $DB_PORT = "5432" }

$DB_USER = $env:DB_USER
if ([string]::IsNullOrEmpty($DB_USER)) { $DB_USER = "postgres" }

$DB_PASSWORD = $env:DB_PASSWORD
if ([string]::IsNullOrEmpty($DB_PASSWORD)) { $DB_PASSWORD = "postgres" }

$DB_NAME = $env:DB_NAME
if ([string]::IsNullOrEmpty($DB_NAME)) { $DB_NAME = "singleone" }

Write-Host "📋 Configurações:" -ForegroundColor Yellow
Write-Host "   Host: $DB_HOST" -ForegroundColor White
Write-Host "   Port: $DB_PORT" -ForegroundColor White
Write-Host "   User: $DB_USER" -ForegroundColor White
Write-Host "   Database: $DB_NAME" -ForegroundColor White
Write-Host "   Password: $(if ($DB_PASSWORD) { '***' } else { 'NÃO DEFINIDA' })" -ForegroundColor White
Write-Host ""

# Verificar se psql está disponível
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psqlPath) {
    Write-Host "❌ psql não encontrado no PATH!" -ForegroundColor Red
    Write-Host "   Instale o PostgreSQL Client ou adicione ao PATH" -ForegroundColor Yellow
    Write-Host "   Download: https://www.postgresql.org/download/" -ForegroundColor Yellow
    exit 1
}

# Verificar se o banco existe
Write-Host "🔍 Verificando se o banco '$DB_NAME' existe..." -ForegroundColor Yellow

$env:PGPASSWORD = $DB_PASSWORD
$checkDb = & psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='$DB_NAME';" 2>&1

if ($LASTEXITCODE -eq 0 -and $checkDb -eq "1") {
    Write-Host "✅ Banco '$DB_NAME' existe!" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "📊 Verificando tabelas..." -ForegroundColor Yellow
    $tableCount = & psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -tAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   Tabelas encontradas: $tableCount" -ForegroundColor White
        
        if ([int]$tableCount -lt 50) {
            Write-Host "⚠️  Poucas tabelas encontradas. O banco pode estar incompleto." -ForegroundColor Yellow
        } else {
            Write-Host "✅ Banco parece estar completo!" -ForegroundColor Green
        }
    }
} else {
    Write-Host "❌ Banco '$DB_NAME' NÃO existe!" -ForegroundColor Red
    Write-Host ""
    Write-Host "🔨 Para criar o banco, execute:" -ForegroundColor Yellow
    Write-Host "   docker exec -it singleone-postgres psql -U postgres -c 'CREATE DATABASE singleone;'" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "   E depois execute o script de inicialização:" -ForegroundColor Yellow
    Write-Host "   docker exec -i singleone-postgres psql -U postgres -d singleone < init_db_atualizado.sql" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "✅ VERIFICAÇÃO CONCLUÍDA" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

