# ========================================
# Script: Criar tabela descarteevidencias
# Descrição: Executa o script SQL para criar a tabela de evidências de descarte
# Data: 03/10/2025
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CRIAÇÃO DA TABELA descarteevidencias" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Solicitar senha do PostgreSQL
$password = Read-Host "Digite a senha do PostgreSQL" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
$plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

# Definir variável de ambiente
$env:PGPASSWORD = $plainPassword

Write-Host "Executando script SQL..." -ForegroundColor Yellow
Write-Host ""

# Executar script
psql -h localhost -U postgres -d singleone -f criar-tabela-descarteevidencias.sql

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  ✓ Script executado com sucesso!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Pressione qualquer tecla para sair..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

