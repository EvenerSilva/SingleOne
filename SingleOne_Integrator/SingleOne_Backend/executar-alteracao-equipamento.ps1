# Script para executar a alteração da tabela requisicoesitens
# Permite que a coluna equipamento aceite valores NULL para linhas telefônicas

Write-Host "🔧 Alterando estrutura da tabela requisicoesitens..." -ForegroundColor Yellow

# Verificar se o PostgreSQL está rodando
try {
    $pgProcess = Get-Process -Name "postgres" -ErrorAction SilentlyContinue
    if ($pgProcess) {
        Write-Host "✅ PostgreSQL está rodando" -ForegroundColor Green
    } else {
        Write-Host "❌ PostgreSQL não está rodando. Inicie o serviço primeiro." -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Erro ao verificar PostgreSQL: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Configurações do banco (ajuste conforme necessário)
$DB_HOST = "localhost"
$DB_PORT = "5432"
$DB_NAME = "SingleOne"
$DB_USER = "postgres"

# Solicitar senha do banco
$DB_PASSWORD = Read-Host "Digite a senha do PostgreSQL" -AsSecureString
$DB_PASSWORD_PLAIN = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($DB_PASSWORD))

Write-Host "📊 Conectando ao banco de dados..." -ForegroundColor Cyan

# Executar o script SQL principal
try {
    # Executar via psql usando arquivo
    $env:PGPASSWORD = $DB_PASSWORD_PLAIN
    Write-Host "🔧 Executando alteração na tabela requisicoesitens..." -ForegroundColor Yellow
    $result = psql -h $DB_HOST -p $DB_PORT -d $DB_NAME -U $DB_USER -f "alterar_equipamento_nullable.sql" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Alteração em requisicoesitens executada com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "❌ Erro ao executar alteração em requisicoesitens:" -ForegroundColor Red
        Write-Host $result
        exit 1
    }
} catch {
    Write-Host "❌ Erro ao executar script: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Executar o script SQL para equipamentohistorico
try {
    Write-Host "🔧 Executando alteração na tabela equipamentohistorico..." -ForegroundColor Yellow
    $result = psql -h $DB_HOST -p $DB_PORT -d $DB_NAME -U $DB_USER -f "corrigir-equipamentohistorico.sql" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Alteração em equipamentohistorico executada com sucesso!" -ForegroundColor Green
        Write-Host "📋 Resultado final:" -ForegroundColor Cyan
        Write-Host $result
    } else {
        Write-Host "❌ Erro ao executar alteração em equipamentohistorico:" -ForegroundColor Red
        Write-Host $result
    }
} catch {
    Write-Host "❌ Erro ao executar script: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    # Limpar variável de ambiente
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host "🎉 Processo concluído!" -ForegroundColor Green
Write-Host "💡 Agora você pode testar o salvamento de requisições com linhas telefônicas" -ForegroundColor Cyan
