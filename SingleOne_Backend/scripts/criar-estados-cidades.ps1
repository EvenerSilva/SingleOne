# =====================================================
# SCRIPT POWERSHELL PARA CRIAR TABELAS DE ESTADOS E CIDADES
# Sistema de referência para localidades
# =====================================================

param(
    [string]$ServerHost = "localhost",
    [string]$Port = "5432",
    [string]$Database = "singleonedb",
    [string]$Username = "postgres",
    [string]$ScriptPath = "criar-tabelas-estados-cidades.sql"
)

Write-Host "🚀 CRIANDO TABELAS DE ESTADOS E CIDADES (PostgreSQL)" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
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
$confirma = Read-Host "⚠️  Deseja criar as tabelas de estados e cidades? (S/N)"
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
        Write-Host "✅ Tabelas criadas com sucesso!" -ForegroundColor Green
        Write-Host "🎯 Sistema de referência implementado:" -ForegroundColor Green
        Write-Host "   - Tabela 'estados' com 27 estados brasileiros" -ForegroundColor White
        Write-Host "   - Tabela 'cidades' com cidades principais" -ForegroundColor White
        Write-Host "   - Relacionamento entre estados e cidades" -ForegroundColor White
        Write-Host ""
        Write-Host "📋 Próximos passos:" -ForegroundColor Cyan
        Write-Host "1. Atualizar o backend para usar as novas tabelas" -ForegroundColor White
        Write-Host "2. Modificar o frontend para usar dropdowns" -ForegroundColor White
        Write-Host "3. Testar a funcionalidade de localidades" -ForegroundColor White
    } else {
        throw "psql retornou código de erro: $LASTEXITCODE"
    }
    
    # Limpar senha da memória
    $env:PGPASSWORD = $null
    
}
catch {
    Write-Host ""
    Write-Host "❌ Erro ao executar o script: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Verifique se você tem permissões para criar tabelas." -ForegroundColor Yellow
    Write-Host "Verifique também as configurações de conexão." -ForegroundColor Yellow
    
    # Limpar senha da memória em caso de erro
    $env:PGPASSWORD = $null
    exit 1
}

Write-Host ""
Write-Host "🎉 Processo concluído!" -ForegroundColor Green
Write-Host "💡 Agora você pode implementar dropdowns no frontend!" -ForegroundColor Cyan
