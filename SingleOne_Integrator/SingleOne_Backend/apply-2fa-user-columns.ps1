# Execute este script para adicionar as colunas necessárias na tabela usuarios para 2FA
# Este script deve ser executado APÓS o script apply-2fa-params.ps1

param(
    [string]$DB_HOST = "localhost",
    [string]$DB_USER = "postgres",
    [string]$DB_PASSWORD = "password",
    [string]$DB_NAME = "SingleOne"
)

Write-Host "=== APLICANDO CONFIGURAÇÕES DE 2FA NA TABELA USUARIOS ===" -ForegroundColor Green
Write-Host ""

# Verificar se o serviço PostgreSQL está rodando
$postgresService = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue
if ($postgresService -and $postgresService.Status -eq "Running") {
    Write-Host "✓ Serviço PostgreSQL está rodando" -ForegroundColor Green
} else {
    Write-Host "⚠ Serviço PostgreSQL não encontrado ou não está rodando" -ForegroundColor Yellow
    Write-Host "  Certifique-se de que o PostgreSQL está instalado e rodando" -ForegroundColor White
}

Write-Host ""
Write-Host "=== EXECUTANDO SCRIPT SQL ===" -ForegroundColor Green

# Executar o script SQL
try {
    $sqlScript = Get-Content "add-2fa-user-columns.sql" -Raw
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
    $checkColumns = @"
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns 
WHERE table_name = 'usuarios' 
AND column_name IN ('two_factor_enabled', 'two_factor_secret', 'two_factor_backup_codes', 'two_factor_last_used')
ORDER BY column_name;
"@

    $columnsResult = & psql -h $DB_HOST -U $DB_USER -d $DB_NAME -c $checkColumns 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Colunas verificadas com sucesso:" -ForegroundColor Green
        Write-Host $columnsResult -ForegroundColor White
    } else {
        Write-Host "⚠ Erro ao verificar colunas: $columnsResult" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠ Erro ao verificar colunas: $_" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== CONFIGURAÇÕES DE 2FA APLICADAS COM SUCESSO! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Próximos passos:" -ForegroundColor Yellow
Write-Host "1. Atualizar o modelo Usuario.cs no backend" -ForegroundColor White
Write-Host "2. Implementar a lógica de 2FA no sistema de autenticação" -ForegroundColor White
Write-Host "3. Criar endpoints para gerenciar 2FA dos usuários" -ForegroundColor White
Write-Host "4. Atualizar o frontend para incluir configurações de 2FA por usuário" -ForegroundColor White
Write-Host ""
Write-Host "As colunas foram adicionadas na tabela 'usuarios':" -ForegroundColor Cyan
Write-Host "  • two_factor_enabled (BOOLEAN)" -ForegroundColor White
Write-Host "  • two_factor_secret (VARCHAR)" -ForegroundColor White
Write-Host "  • two_factor_backup_codes (TEXT)" -ForegroundColor White
Write-Host "  • two_factor_last_used (TIMESTAMP)" -ForegroundColor White
