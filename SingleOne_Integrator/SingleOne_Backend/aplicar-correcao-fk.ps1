# Script para corrigir Foreign Key de usuário upload em contratos

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CORRIGIR FK USUARIO UPLOAD - CONTRATOS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se o arquivo SQL existe
if (-not (Test-Path "corrigir-fk-usuario-upload.sql")) {
    Write-Host "❌ Erro: Arquivo corrigir-fk-usuario-upload.sql não encontrado!" -ForegroundColor Red
    exit 1
}

Write-Host "📄 Arquivo SQL encontrado!" -ForegroundColor Green
Write-Host ""

# Solicitar dados de conexão
Write-Host "Digite os dados de conexão do banco PostgreSQL:" -ForegroundColor Yellow
Write-Host ""

$dbHost = Read-Host "Host (padrão: localhost)"
if ([string]::IsNullOrWhiteSpace($dbHost)) { $dbHost = "localhost" }

$dbPort = Read-Host "Porta (padrão: 5432)"
if ([string]::IsNullOrWhiteSpace($dbPort)) { $dbPort = "5432" }

$dbName = Read-Host "Nome do banco"
if ([string]::IsNullOrWhiteSpace($dbName)) {
    Write-Host "❌ Nome do banco é obrigatório!" -ForegroundColor Red
    exit 1
}

$dbUser = Read-Host "Usuário"
if ([string]::IsNullOrWhiteSpace($dbUser)) {
    Write-Host "❌ Usuário é obrigatório!" -ForegroundColor Red
    exit 1
}

$dbPassword = Read-Host "Senha" -AsSecureString
$dbPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($dbPassword)
)

Write-Host ""
Write-Host "📊 Configuração:" -ForegroundColor Cyan
Write-Host "   Host: $dbHost" -ForegroundColor White
Write-Host "   Porta: $dbPort" -ForegroundColor White
Write-Host "   Banco: $dbName" -ForegroundColor White
Write-Host "   Usuário: $dbUser" -ForegroundColor White
Write-Host ""

# Confirmar execução
$confirm = Read-Host "Deseja executar a correção? (S/N)"
if ($confirm -ne "S" -and $confirm -ne "s") {
    Write-Host "❌ Operação cancelada pelo usuário." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "⚙️  Executando correção no banco de dados..." -ForegroundColor Yellow

# Definir variável de ambiente para senha
$env:PGPASSWORD = $dbPasswordPlain

try {
    # Executar o script SQL
    psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -f "corrigir-fk-usuario-upload.sql" 2>&1 | Out-String | Write-Host
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Correção aplicada com sucesso!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📋 Foreign Key corrigida:" -ForegroundColor Cyan
        Write-Host "   - fk_contratos_usuarioupload agora permite NULL" -ForegroundColor White
        Write-Host "   - Upload funcionará mesmo sem identificação do usuário" -ForegroundColor White
        Write-Host ""
        Write-Host "🎯 Próximo passo: Testar o upload novamente" -ForegroundColor Yellow
    } else {
        Write-Host ""
        Write-Host "❌ Erro ao executar o script SQL!" -ForegroundColor Red
        Write-Host "Código de saída: $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host ""
    Write-Host "❌ Erro ao executar comando psql: $_" -ForegroundColor Red
    exit 1
} finally {
    # Limpar variável de senha
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CONCLUÍDO" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan


