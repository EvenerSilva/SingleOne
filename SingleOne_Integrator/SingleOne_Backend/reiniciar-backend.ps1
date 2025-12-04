# Script para reiniciar o backend com as novas alterações
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "  REINICIANDO BACKEND COM ALTERACOES" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

# Limpar build anterior
Write-Host "[1/3] Limpando build anterior..." -ForegroundColor Yellow
if (Test-Path "SingleOneAPI\bin") {
    Remove-Item "SingleOneAPI\bin" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "   OK: Pasta bin removida" -ForegroundColor Green
}
if (Test-Path "SingleOneAPI\obj") {
    Remove-Item "SingleOneAPI\obj" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "   OK: Pasta obj removida" -ForegroundColor Green
}

# Compilar projeto
Write-Host ""
Write-Host "[2/3] Compilando projeto..." -ForegroundColor Yellow
dotnet build SingleOneAPI/SingleOneAPI.csproj

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "   ERRO na compilacao!" -ForegroundColor Red
    Write-Host "   Verifique os erros acima" -ForegroundColor Red
    exit 1
}

Write-Host "   OK: Projeto compilado com sucesso" -ForegroundColor Green

# Iniciar servidor
Write-Host ""
Write-Host "[3/3] Iniciando servidor..." -ForegroundColor Yellow
Write-Host ""
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "  SERVIDOR INICIANDO..." -ForegroundColor Green
Write-Host "  Aguarde 'Now listening on: http://localhost:5000'" -ForegroundColor Yellow
Write-Host "  Pressione Ctrl+C para parar" -ForegroundColor Yellow
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

dotnet run --project SingleOneAPI/SingleOneAPI.csproj
