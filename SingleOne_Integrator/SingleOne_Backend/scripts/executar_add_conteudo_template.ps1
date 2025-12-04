# ========================================================================================================
# Script: Executar adição da coluna conteudo_template_assinado
# Descrição: Script para executar o SQL de adição da coluna no banco de dados
# Data: 2025-10-17
# ========================================================================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Adicionar coluna conteudo_template_assinado" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar qual tipo de banco usar
Write-Host "Qual banco de dados você está usando?" -ForegroundColor Yellow
Write-Host "1 - PostgreSQL" -ForegroundColor White
Write-Host "2 - SQL Server" -ForegroundColor White
$opcao = Read-Host "Escolha (1 ou 2)"

$scriptPath = Join-Path $PSScriptRoot "add_conteudo_template_assinado.sql"

if (-not (Test-Path $scriptPath)) {
    Write-Host "ERRO: Arquivo SQL não encontrado: $scriptPath" -ForegroundColor Red
    exit 1
}

try {
    if ($opcao -eq "1") {
        # PostgreSQL
        Write-Host "`nConfiguração PostgreSQL:" -ForegroundColor Yellow
        $servidor = Read-Host "Servidor (ex: localhost)"
        $porta = Read-Host "Porta (default: 5432)"
        if ([string]::IsNullOrWhiteSpace($porta)) { $porta = "5432" }
        $database = Read-Host "Database (ex: singleone)"
        $usuario = Read-Host "Usuário (ex: postgres)"
        
        Write-Host "`nExecutando script no PostgreSQL..." -ForegroundColor Green
        
        $env:PGPASSWORD = Read-Host "Senha" -AsSecureString | ConvertFrom-SecureString
        
        & psql -h $servidor -p $porta -d $database -U $usuario -f $scriptPath
        
        Write-Host "`nScript executado com sucesso no PostgreSQL!" -ForegroundColor Green
        
    } elseif ($opcao -eq "2") {
        # SQL Server
        Write-Host "`nConfiguração SQL Server:" -ForegroundColor Yellow
        $servidor = Read-Host "Servidor (ex: localhost)"
        $database = Read-Host "Database (ex: SingleOneDB)"
        
        Write-Host "`nExecutando script no SQL Server..." -ForegroundColor Green
        
        & sqlcmd -S $servidor -d $database -i $scriptPath
        
        Write-Host "`nScript executado com sucesso no SQL Server!" -ForegroundColor Green
        
    } else {
        Write-Host "Opção inválida!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "SUCESSO! Coluna adicionada." -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    
} catch {
    Write-Host "`nERRO ao executar script:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host "`nPressione qualquer tecla para sair..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

