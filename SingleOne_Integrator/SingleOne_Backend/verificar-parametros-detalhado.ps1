# VERIFICAÇÃO DETALHADA DA TABELA PARAMETROS
Write-Host "=== VERIFICAÇÃO DETALHADA DA TABELA PARAMETROS ===" -ForegroundColor Green

# URL da API
$baseUrl = "http://localhost:5000"

# 1. Fazer login
Write-Host "1. Fazendo login..." -ForegroundColor Cyan
$loginData = @{
    email = "administrador@singleone.tech"
    senha = "MTQyNTM2QEFkbWlu"
}

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/Usuario/Login" -Method POST -Body ($loginData | ConvertTo-Json) -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "Login OK - Token obtido" -ForegroundColor Green
} catch {
    Write-Host "Erro no login: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# 2. Verificar configuração para Cliente 1
Write-Host "`n2. Verificando configuração Cliente 1..." -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/Usuario/GetGlobalTwoFactorStatus/1" -Method GET -Headers $headers
    Write-Host "Cliente 1 - 2FA Global: $($response.twoFactorEnabledGlobally)" -ForegroundColor Yellow
} catch {
    Write-Host "Erro ao verificar Cliente 1: $($_.Exception.Message)" -ForegroundColor Red
}

# 3. Verificar configuração para Cliente 2
Write-Host "`n3. Verificando configuração Cliente 2..." -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/Usuario/GetGlobalTwoFactorStatus/2" -Method GET -Headers $headers
    Write-Host "Cliente 2 - 2FA Global: $($response.twoFactorEnabledGlobally)" -ForegroundColor Yellow
} catch {
    Write-Host "Erro ao verificar Cliente 2: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== PROBLEMA IDENTIFICADO ===" -ForegroundColor Red
Write-Host "A tabela parametros está com problemas!" -ForegroundColor Yellow
Write-Host "Execute no banco para INVESTIGAR:" -ForegroundColor Cyan
Write-Host "SELECT * FROM parametros ORDER BY id;" -ForegroundColor White
Write-Host "SELECT * FROM parametros ORDER BY cliente;" -ForegroundColor White

Write-Host "`n=== SOLUÇÃO CORRETA ===" -ForegroundColor Green
Write-Host "1. Verifique se existe entrada para Cliente 2" -ForegroundColor Cyan
Write-Host "2. Se não existir, crie uma entrada" -ForegroundColor Cyan
Write-Host "3. Se existir múltiplas, mantenha apenas uma por cliente" -ForegroundColor Cyan

Write-Host "`n=== INVESTIGAÇÃO CONCLUÍDA ===" -ForegroundColor Green
