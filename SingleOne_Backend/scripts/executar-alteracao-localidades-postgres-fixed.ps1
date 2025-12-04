# =====================================================
# SCRIPT POWERSHELL PARA ALTERAR TABELA LOCALIDADES (PostgreSQL)
# Executa o script SQL para adicionar campos cidade e estado
# =====================================================

param(
    [string]$ServerHost = "localhost",
    [string]$Port = "5432",
    [string]$Database = "singleone",
    [string]$Username = "postgres",
    [string]$ScriptPath = "alterar-tabela-localidades-postgres.sql"
)

Write-Host "🚀 INICIANDO ALTERAÇÃO DA TABELA LOCALIDADES (PostgreSQL)" -ForegroundColor Cyan
Write-Host "=========================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se o arquivo de script existe
if (-not (Test-Path $ScriptPath)) {
    Write-Host "❌ Arquivo de script não encontrado: $ScriptPath" -ForegroundColor Red
    Write-Host "Verifique se o arquivo existe no diretório correto." -ForegroundColor Yellow
    exit 1
}

Write-Host "📁 Script encontrado: $ScriptPath" -ForegroundColor Green

# Verificar se o psql está disponível
try {
    $psqlVersion = & psql --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ PostgreSQL client (psql) encontrado: $psqlVersion" -ForegroundColor Green
    } else {
        throw "psql não encontrado"
    }
}
catch {
    Write-Host "❌ PostgreSQL client (psql) não encontrado!" -ForegroundColor Red
    Write-Host "Instale o PostgreSQL client ou adicione ao PATH." -ForegroundColor Yellow
    Write-Host "Download: https://www.postgresql.org/download/" -ForegroundColor Cyan
    exit 1
}

Write-Host ""
Write-Host "🗄️  Configurações de conexão:" -ForegroundColor Cyan
Write-Host "   Host: $ServerHost" -ForegroundColor White
Write-Host "   Porta: $Port" -ForegroundColor White
Write-Host "   Banco: $Database" -ForegroundColor White
Write-Host "   Usuário: $Username" -ForegroundColor White
Write-Host ""

# Solicitar senha
$Password = Read-Host "🔐 Digite a senha do usuário $Username" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
$PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

# Confirmar execução
$confirma = Read-Host "⚠️  Deseja executar a alteração da tabela? (S/N)"
if ($confirma -ne "S" -and $confirma -ne "s") {
    Write-Host "❌ Operação cancelada pelo usuário." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "⚡ Executando script SQL..." -ForegroundColor Yellow

try {
    # Executar o script usando psql
    $env:PGPASSWORD = $PlainPassword
    
    $psqlArgs = @(
        "-h", $ServerHost,
        "-p", $Port,
        "-d", $Database,
        "-U", $Username,
        "-f", $ScriptPath,
        "--echo-all"
    )
    
    & psql @psqlArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Script executado com sucesso!" -ForegroundColor Green
        Write-Host "🎯 Campos cidade e estado foram adicionados à tabela localidades." -ForegroundColor Green
        Write-Host ""
        Write-Host "📋 Próximos passos:" -ForegroundColor Cyan
        Write-Host "1. Verificar se os campos foram criados corretamente" -ForegroundColor White
        Write-Host "2. Atualizar o backend para aceitar os novos campos" -ForegroundColor White
        Write-Host "3. Testar o frontend com os novos campos" -ForegroundColor White
    } else {
        throw "psql retornou código de erro: $LASTEXITCODE"
    }
    
    # Limpar senha da memória
    $env:PGPASSWORD = $null
    
}
catch {
    Write-Host ""
    Write-Host "❌ Erro ao executar o script: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Verifique se você tem permissões para alterar a tabela." -ForegroundColor Yellow
    Write-Host "Verifique também as configurações de conexão." -ForegroundColor Yellow
    
    # Limpar senha da memória em caso de erro
    $env:PGPASSWORD = $null
    exit 1
}

Write-Host ""
Write-Host "🎉 Processo concluído!" -ForegroundColor Green
