# =====================================================
# Script: Atualizar Template de Descarte com MTR (PostgreSQL)
# Data: 2025-10-09
# Objetivo: Executar o script SQL para atualizar template ID 5 com campos MTR
# =====================================================

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  Atualizar Template de Descarte (ID 5)" -ForegroundColor Cyan
Write-Host "  Adicionar secao MTR ao template" -ForegroundColor Cyan
Write-Host "  PostgreSQL" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Solicitar credenciais do banco
$servidor = Read-Host "Servidor do banco de dados (ex: 127.0.0.1)"
$banco = Read-Host "Nome do banco de dados (ex: singleone)"
$usuario = Read-Host "Usuario do banco de dados (ex: postgres)"
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
Write-Host "[EXECUTANDO] Executando script SQL no PostgreSQL..." -ForegroundColor Yellow
Write-Host ""

# Executar o script SQL usando psql
try {
    # Verificar se psql esta disponivel
    $psqlCmd = Get-Command psql -ErrorAction SilentlyContinue
    
    if ($null -eq $psqlCmd) {
        Write-Host "[ERRO] Cliente psql nao encontrado no PATH." -ForegroundColor Red
        Write-Host "[DICA] Instale o PostgreSQL Client ou adicione o caminho do psql.exe ao PATH do sistema." -ForegroundColor Yellow
        Write-Host "       Exemplo de caminho: C:\Program Files\PostgreSQL\16\bin" -ForegroundColor Gray
        exit 1
    }

    # Configurar senha no ambiente
    $env:PGPASSWORD = $senhaPlainText
    
    # Executar o script
    $output = & psql -h $servidor -U $usuario -d $banco -f $scriptSQL 2>&1
    $exitCode = $LASTEXITCODE
    
    # Limpar senha do ambiente
    Remove-Item Env:\PGPASSWORD
    
    if ($exitCode -eq 0) {
        Write-Host "[SUCESSO] Script executado com sucesso!" -ForegroundColor Green
        Write-Host ""
        Write-Host "[VERIFICANDO] Verificando template atualizado..." -ForegroundColor Cyan
        Write-Host ""
        
        # Verificar se template contem MTR
        $env:PGPASSWORD = $senhaPlainText
        $verificacao = & psql -h $servidor -U $usuario -d $banco -c "SELECT id, titulo, CASE WHEN conteudo LIKE '%MTR_NUMERO%' THEN 'SIM' ELSE 'NAO' END as tem_mtr FROM templates WHERE id = 5;" 2>&1
        Remove-Item Env:\PGPASSWORD
        
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

