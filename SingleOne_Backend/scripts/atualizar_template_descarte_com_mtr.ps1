# =====================================================
# Script: Atualizar Template de Descarte com MTR
# Data: 2025-10-09
# Objetivo: Executar o script SQL para atualizar template ID 5 com campos MTR
# =====================================================

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  Atualizar Template de Descarte (ID 5)" -ForegroundColor Cyan
Write-Host "  Adicionar secao MTR ao template" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Solicitar credenciais do banco
$servidor = Read-Host "Servidor do banco de dados (ex: localhost)"
$banco = Read-Host "Nome do banco de dados (ex: singleone)"
$usuario = Read-Host "Usuario do banco de dados"
$senha = Read-Host "Senha do banco de dados" -AsSecureString
$senhaPlainText = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($senha))

# Definir caminho do script SQL
$scriptSQL = Join-Path $PSScriptRoot "insert_template_descarte.sql"

if (-not (Test-Path $scriptSQL)) {
    Write-Host "[ERRO] Arquivo SQL nao encontrado: $scriptSQL" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] Script SQL encontrado: $scriptSQL" -ForegroundColor Green
Write-Host ""
Write-Host "[EXECUTANDO] Executando script SQL..." -ForegroundColor Yellow
Write-Host ""

# Executar o script SQL usando mysql client
try {
    # Verificar se mysql client esta disponivel
    $mysqlCmd = Get-Command mysql -ErrorAction SilentlyContinue
    
    if ($null -eq $mysqlCmd) {
        Write-Host "[ERRO] Cliente mysql nao encontrado no PATH." -ForegroundColor Red
        Write-Host "[DICA] Instale o MySQL Client ou adicione o caminho do mysql.exe ao PATH do sistema." -ForegroundColor Yellow
        Write-Host "       Exemplo de caminho: C:\Program Files\MySQL\MySQL Server 8.0\bin" -ForegroundColor Gray
        exit 1
    }

    # Executar o script
    $env:MYSQL_PWD = $senhaPlainText
    $output = & mysql -h $servidor -u $usuario -D $banco -e "source $scriptSQL" 2>&1
    $exitCode = $LASTEXITCODE
    
    # Limpar senha do ambiente
    Remove-Item Env:\MYSQL_PWD
    
    if ($exitCode -eq 0) {
        Write-Host "[SUCESSO] Script executado com sucesso!" -ForegroundColor Green
        Write-Host ""
        Write-Host "[VERIFICANDO] Verificando template atualizado..." -ForegroundColor Cyan
        
        # Verificar se template contem MTR
        $env:MYSQL_PWD = $senhaPlainText
        $verificacao = & mysql -h $servidor -u $usuario -D $banco -e "SELECT id, titulo, CASE WHEN conteudo LIKE '%MTR_NUMERO%' THEN 'SIM' ELSE 'NAO' END as tem_mtr FROM templates WHERE id = 5;" 2>&1
        Remove-Item Env:\MYSQL_PWD
        
        Write-Host $verificacao
        Write-Host ""
        Write-Host "[OK] Template ID 5 atualizado com sucesso!" -ForegroundColor Green
        Write-Host "[INFO] O PDF de protocolo de descartes agora ira mostrar as informacoes do MTR." -ForegroundColor Cyan
    } else {
        Write-Host "[ERRO] Erro ao executar script SQL:" -ForegroundColor Red
        Write-Host $output -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "[ERRO] $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  [CONCLUIDO] Processo concluido!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan
