# ============================================
# Script para Recalcular Estatísticas das Campanhas
# ============================================

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Recalcular Campanhas - SingleOne" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Configurações
$baseUrl = "http://localhost:5000"
$clienteId = 1

Write-Host "📋 Cliente ID: $clienteId" -ForegroundColor Yellow
Write-Host "🌐 URL Base: $baseUrl" -ForegroundColor Yellow
Write-Host ""

# Pedir o token
Write-Host "🔑 Cole o token JWT (você pode pegar no localStorage do navegador):" -ForegroundColor Green
Write-Host "   1. Abra o Developer Tools (F12)" -ForegroundColor Gray
Write-Host "   2. Vá em Application > Local Storage" -ForegroundColor Gray
Write-Host "   3. Procure por 'token' ou 'authToken'" -ForegroundColor Gray
Write-Host "   4. Copie o valor e cole aqui" -ForegroundColor Gray
Write-Host ""

$token = Read-Host "Token"

if ([string]::IsNullOrWhiteSpace($token)) {
    Write-Host "❌ Token não fornecido. Tentando sem autenticação..." -ForegroundColor Red
    Write-Host ""
}

# Montar URL
$url = "$baseUrl/api/CampanhaAssinatura/RecalcularEstatisticas?clienteId=$clienteId"

Write-Host "🚀 Executando requisição..." -ForegroundColor Cyan
Write-Host "URL: $url" -ForegroundColor Gray
Write-Host ""

try {
    # Preparar headers
    $headers = @{
        "Content-Type" = "application/json"
    }
    
    if (![string]::IsNullOrWhiteSpace($token)) {
        $headers["Authorization"] = "Bearer $token"
    }
    
    # Fazer requisição
    $response = Invoke-RestMethod -Uri $url -Method POST -Headers $headers -ErrorAction Stop
    
    Write-Host "✅ SUCESSO!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Resultado:" -ForegroundColor Cyan
    Write-Host "   Mensagem: $($response.mensagem)" -ForegroundColor White
    Write-Host "   Total de Campanhas: $($response.totalCampanhas)" -ForegroundColor White
    Write-Host "   Recalculadas: $($response.recalculadas)" -ForegroundColor White
    Write-Host ""
    Write-Host "🎉 Agora recarregue o dashboard (F5) para ver os novos valores!" -ForegroundColor Green
}
catch {
    Write-Host "❌ ERRO ao executar requisição!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Detalhes do erro:" -ForegroundColor Yellow
    Write-Host $_.Exception.Message -ForegroundColor Red
    
    if ($_.Exception.Response) {
        Write-Host ""
        Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Yellow
        
        # Tentar ler o corpo da resposta
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $reader.BaseStream.Position = 0
            $responseBody = $reader.ReadToEnd()
            Write-Host "Resposta do servidor:" -ForegroundColor Yellow
            Write-Host $responseBody -ForegroundColor Red
        }
        catch {}
    }
    
    Write-Host ""
    Write-Host "💡 Possíveis soluções:" -ForegroundColor Yellow
    Write-Host "   1. Verifique se o backend está rodando (http://localhost:5000)" -ForegroundColor Gray
    Write-Host "   2. Verifique se o token está correto" -ForegroundColor Gray
    Write-Host "   3. Verifique os logs do backend para mais detalhes" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Pressione qualquer tecla para sair..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

