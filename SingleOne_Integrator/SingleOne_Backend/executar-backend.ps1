# Script para executar o backend SingleOne na porta 5000
Write-Host "🚀 Iniciando Backend SingleOne na porta 5000..." -ForegroundColor Green

# Parar processos dotnet existentes
Write-Host "🛑 Parando processos dotnet existentes..." -ForegroundColor Yellow
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"} | Stop-Process -Force -ErrorAction SilentlyContinue

# Aguardar um pouco
Start-Sleep -Seconds 2

# Verificar se a porta 5000 está livre
$portCheck = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue
if ($portCheck) {
    Write-Host "❌ Porta 5000 já está em uso!" -ForegroundColor Red
    Write-Host "Processos usando a porta 5000:" -ForegroundColor Yellow
    Get-NetTCPConnection -LocalPort 5000 | Format-Table -AutoSize
    exit 1
}

Write-Host "✅ Porta 5000 está livre" -ForegroundColor Green

# Configurar variáveis de ambiente
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5000"

# Executar o backend
Write-Host "🔧 Executando backend..." -ForegroundColor Cyan
Write-Host "📚 Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host "🔧 API: http://localhost:5000/api/" -ForegroundColor Cyan

try {
    dotnet run --project SingleOneAPI --urls "http://localhost:5000"
} catch {
    Write-Host "❌ Erro ao executar o backend: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "🔍 Verifique se há erros de compilação ou configuração" -ForegroundColor Yellow
}
