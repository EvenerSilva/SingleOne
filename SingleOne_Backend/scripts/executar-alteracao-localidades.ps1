# =====================================================
# SCRIPT POWERSHELL PARA ALTERAR TABELA LOCALIDADES
# Executa o script SQL para adicionar campos cidade e estado
# =====================================================

param(
    [string]$ServerInstance = "localhost",
    [string]$Database = "SingleOneDB",
    [string]$ScriptPath = "alterar-tabela-localidades.sql"
)

Write-Host "🚀 INICIANDO ALTERAÇÃO DA TABELA LOCALIDADES" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se o SQL Server está rodando
Write-Host "🔍 Verificando conexão com SQL Server..." -ForegroundColor Yellow
try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = "Server=$ServerInstance;Database=$Database;Integrated Security=true;"
    $connection.Open()
    $connection.Close()
    Write-Host "✅ Conexão com SQL Server estabelecida com sucesso!" -ForegroundColor Green
}
catch {
    Write-Host "❌ Erro ao conectar com SQL Server: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Verifique se o SQL Server está rodando e se as credenciais estão corretas." -ForegroundColor Yellow
    exit 1
}

# Verificar se o arquivo de script existe
if (-not (Test-Path $ScriptPath)) {
    Write-Host "❌ Arquivo de script não encontrado: $ScriptPath" -ForegroundColor Red
    Write-Host "Verifique se o arquivo existe no diretório correto." -ForegroundColor Yellow
    exit 1
}

Write-Host "📁 Script encontrado: $ScriptPath" -ForegroundColor Green

# Ler o conteúdo do script
$scriptContent = Get-Content $ScriptPath -Raw

# Substituir o nome do banco no script
$scriptContent = $scriptContent -replace "USE \[SingleOneDB\]", "USE [$Database]"

Write-Host "🗄️  Banco de dados: $Database" -ForegroundColor Cyan
Write-Host "🖥️  Servidor: $ServerInstance" -ForegroundColor Cyan
Write-Host ""

# Confirmar execução
$confirma = Read-Host "⚠️  Deseja executar a alteração da tabela? (S/N)"
if ($confirma -ne "S" -and $confirma -ne "s") {
    Write-Host "❌ Operação cancelada pelo usuário." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "⚡ Executando script SQL..." -ForegroundColor Yellow

try {
    # Executar o script
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = "Server=$ServerInstance;Database=$Database;Integrated Security=true;"
    $connection.Open()
    
    $command = New-Object System.Data.SqlClient.SqlCommand($scriptContent, $connection)
    $command.CommandTimeout = 300  # 5 minutos de timeout
    
    $result = $command.ExecuteNonQuery()
    
    $connection.Close()
    
    Write-Host ""
    Write-Host "✅ Script executado com sucesso!" -ForegroundColor Green
    Write-Host "🎯 Campos cidade e estado foram adicionados à tabela Localidades." -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Próximos passos:" -ForegroundColor Cyan
    Write-Host "1. Verificar se os campos foram criados corretamente" -ForegroundColor White
    Write-Host "2. Atualizar o backend para aceitar os novos campos" -ForegroundColor White
    Write-Host "3. Testar o frontend com os novos campos" -ForegroundColor White
    
}
catch {
    Write-Host ""
    Write-Host "❌ Erro ao executar o script: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Verifique se você tem permissões para alterar a tabela." -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "🎉 Processo concluído!" -ForegroundColor Green
