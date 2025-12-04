# ============================================
# Script SIMPLIFICADO - Recalcular Campanhas
# (Sem necessidade de token se API permitir)
# ============================================

Write-Host "🚀 Recalculando campanhas..." -ForegroundColor Cyan

$url = "http://localhost:5000/api/CampanhaAssinatura/RecalcularEstatisticas?clienteId=1"

try {
    $response = Invoke-RestMethod -Uri $url -Method POST -ContentType "application/json"
    
    Write-Host "✅ SUCESSO!" -ForegroundColor Green
    Write-Host "   Mensagem: $($response.mensagem)" -ForegroundColor White
    Write-Host "   Campanhas recalculadas: $($response.recalculadas)/$($response.totalCampanhas)" -ForegroundColor White
    Write-Host ""
    Write-Host "🎉 Recarregue o dashboard (F5)!" -ForegroundColor Green
}
catch {
    Write-Host "❌ ERRO: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Tente o script completo: recalcular-campanhas.ps1" -ForegroundColor Yellow
}

Write-Host ""
Read-Host "Pressione Enter para sair"

