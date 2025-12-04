# Execute este script para adicionar as colunas necessárias na tabela parametros para 2FA
# Este script deve ser executado APÓS o script apply-brevo-params.ps1

param(
    [string]$DB_HOST = "localhost",
    [string]$DB_USER = "postgres",
    [string]$DB_PASSWORD = "password",
    [string]$DB_NAME = "SingleOne"
)

Write-Host "=== APLICANDO CONFIGURAÇÕES DE 2FA NA TABELA PARAMETROS ===" -ForegroundColor Green
Write-Host ""

# Verificar se o PostgreSQL está rodando
Write-Host "Verificando se o PostgreSQL está rodando..." -ForegroundColor Yellow
try {
    $pgService = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue
    if ($pgService -and $pgService.Status -eq "Running") {
        Write-Host "✓ PostgreSQL está rodando" -ForegroundColor Green
    } else {
        Write-Host "✗ PostgreSQL não está rodando. Tentando iniciar..." -ForegroundColor Red
        Start-Service "postgresql-x64-13" -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 3
        $pgService = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue
        if ($pgService.Status -eq "Running") {
            Write-Host "✓ PostgreSQL iniciado com sucesso" -ForegroundColor Green
        } else {
            Write-Host "✗ Não foi possível iniciar o PostgreSQL" -ForegroundColor Red
            exit 1
        }
    }
} catch {
    Write-Host "✗ Erro ao verificar PostgreSQL: $_" -ForegroundColor Red
    exit 1
}

# Ler o script SQL
$sqlScript = Get-Content -Path "add-2fa-params.sql" -Raw -ErrorAction SilentlyContinue
if (-not $sqlScript) {
    Write-Host "✗ Arquivo add-2fa-params.sql não encontrado!" -ForegroundColor Red
    exit 1
}

Write-Host "Script SQL carregado com sucesso" -ForegroundColor Green

# Executar o script SQL
Write-Host "Executando script SQL..." -ForegroundColor Yellow
try {
    $env:PGPASSWORD = $DB_PASSWORD
    
    $result = & psql -h $DB_HOST -U $DB_USER -d $DB_NAME -c $sqlScript 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Script SQL executado com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "✗ Erro ao executar script SQL:" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Erro ao executar script SQL: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== VERIFICANDO COLUNAS ADICIONADAS ===" -ForegroundColor Green

# Verificar se as colunas foram adicionadas
try {
    $checkQuery = @"
SELECT column_name, data_type, is_nullable, column_default 
FROM information_schema.columns 
WHERE table_name = 'parametros' 
AND column_name LIKE 'two_factor%'
ORDER BY column_name;
"@
    
    $result = & psql -h $DB_HOST -U $DB_USER -d $DB_NAME -c $checkQuery 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Colunas 2FA verificadas:" -ForegroundColor Green
        Write-Host $result -ForegroundColor Cyan
    } else {
        Write-Host "✗ Erro ao verificar colunas: $result" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Erro ao verificar colunas: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== CONFIGURAÇÕES DE 2FA APLICADAS COM SUCESSO! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Próximos passos:" -ForegroundColor Yellow
Write-Host "1. Atualizar o modelo Parametro.cs no backend" -ForegroundColor White
Write-Host "2. Atualizar a tela de parâmetros no frontend" -ForegroundColor White
Write-Host "3. Implementar a lógica de 2FA no sistema de autenticação" -ForegroundColor White
Write-Host ""
Write-Host "Para verificar manualmente, execute:" -ForegroundColor Cyan
Write-Host "psql -h $DB_HOST -U $DB_USER -d $DB_NAME -c `"SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'parametros' AND column_name LIKE 'two_factor%';`"" -ForegroundColor Cyan

