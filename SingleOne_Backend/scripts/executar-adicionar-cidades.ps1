# =====================================================
# SCRIPT POWERSHELL PARA ADICIONAR CAPITAIS E CIDADES
# Complementar as tabelas de estados e cidades
# =====================================================

param(
    [string]$ServerHost = "localhost",
    [string]$Port = "5432",
    [string]$Database = "singleonedb",
    [string]$Username = "postgres",
    [string]$ScriptPath = "adicionar-capitais-principais-cidades.sql"
)

Write-Host "🚀 ADICIONANDO CAPITAIS E PRINCIPAIS CIDADES (PostgreSQL)" -ForegroundColor Cyan
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
$confirma = Read-Host "⚠️  Deseja adicionar capitais e principais cidades? (S/N)"
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
        Write-Host "✅ Cidades adicionadas com sucesso!" -ForegroundColor Green
        Write-Host "🎯 Agora você tem:" -ForegroundColor Green
        Write-Host "   - 27 estados brasileiros" -ForegroundColor White
        Write-Host "   - Todas as capitais" -ForegroundColor White
        Write-Host "   - Principais cidades de cada estado" -ForegroundColor White
        Write-Host "   - ~150+ cidades no total" -ForegroundColor White
        Write-Host ""
        Write-Host "📋 Próximos passos:" -ForegroundColor Cyan
        Write-Host "1. Testar os dropdowns no frontend" -ForegroundColor White
        Write-Host "2. Verificar se as cidades aparecem corretamente" -ForegroundColor White
        Write-Host "3. Implementar busca por estado → cidade" -ForegroundColor White
    } else {
        throw "psql retornou código de erro: $LASTEXITCODE"
    }
    
    # Limpar senha da memória
    $env:PGPASSWORD = $null
    
}
catch {
    Write-Host ""
    Write-Host "❌ Erro ao executar o script: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Verifique se você tem permissões para inserir dados." -ForegroundColor Yellow
    Write-Host "Verifique também as configurações de conexão." -ForegroundColor Yellow
    
    # Limpar senha da memória em caso de erro
    $env:PGPASSWORD = $null
    exit 1
}

Write-Host ""
Write-Host "🎉 Processo concluído!" -ForegroundColor Green
Write-Host "💡 Agora você tem um sistema completo de estados e cidades!" -ForegroundColor Cyan
