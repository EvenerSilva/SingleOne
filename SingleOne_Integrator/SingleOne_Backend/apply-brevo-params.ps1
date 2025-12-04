# Script para aplicar as configurações de SMTP no banco de dados
# Execute este script para adicionar as colunas necessárias na tabela parametros

Write-Host "=== Aplicando configurações SMTP no banco de dados ===" -ForegroundColor Green

# Verificar se o arquivo SQL existe
$sqlFile = "add-brevo-params.sql"
if (-not (Test-Path $sqlFile)) {
    Write-Host "Erro: Arquivo $sqlFile não encontrado!" -ForegroundColor Red
    exit 1
}

Write-Host "Arquivo SQL encontrado: $sqlFile" -ForegroundColor Yellow

# Ler o conteúdo do arquivo SQL
$sqlContent = Get-Content $sqlFile -Raw

Write-Host "Conteúdo do arquivo SQL:" -ForegroundColor Cyan
Write-Host $sqlContent -ForegroundColor White

Write-Host "`n=== INSTRUÇÕES PARA APLICAR ===" -ForegroundColor Green
Write-Host "1. Abra o pgAdmin ou outro cliente PostgreSQL" -ForegroundColor Yellow
Write-Host "2. Conecte ao banco de dados SingleOne" -ForegroundColor Yellow
Write-Host "3. Execute o seguinte comando SQL:" -ForegroundColor Yellow
Write-Host "4. Ou copie e cole o conteúdo do arquivo $sqlFile" -ForegroundColor Yellow

Write-Host "`n=== COMANDO ALTERNATIVO ===" -ForegroundColor Green
Write-Host "Se você tiver o psql instalado, pode executar:" -ForegroundColor Yellow
Write-Host "psql -h localhost -U seu_usuario -d seu_banco -f $sqlFile" -ForegroundColor Cyan

Write-Host "`n=== VERIFICAÇÃO ===" -ForegroundColor Green
Write-Host "Após executar o SQL, verifique se as colunas foram criadas:" -ForegroundColor Yellow
Write-Host "SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'parametros' AND column_name LIKE 'smtp%';" -ForegroundColor Cyan

Write-Host "`nPressione qualquer tecla para continuar..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
