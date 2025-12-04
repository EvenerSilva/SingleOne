# Script PowerShell para verificar a view vwplanostelefonia
Write-Host "🔍 VERIFICANDO VIEW VWPLANOSTELEFONIA" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

try {
    # Solicitar senha do PostgreSQL
    $Password = Read-Host "Digite a senha do PostgreSQL" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
    $PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    
    Write-Host "📡 Conectando ao banco singleone..." -ForegroundColor Yellow
    
    # Executar script SQL
    $SqlScript = Get-Content ".\verificar-view-vwplanostelefonia.sql" -Raw
    
    Write-Host "📝 Executando script de verificação da view..." -ForegroundColor Yellow
    
    # Usar psql para executar o script
    $Env:PGPASSWORD = $PlainPassword
    $Result = & psql -h localhost -p 5432 -U postgres -d singleone -c $SqlScript 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Script executado com sucesso!" -ForegroundColor Green
        Write-Host "📊 Resultado:" -ForegroundColor Cyan
        Write-Host $Result -ForegroundColor White
    } else {
        Write-Host "❌ Erro ao executar o script:" -ForegroundColor Red
        Write-Host $Result -ForegroundColor Red
    }
    
    # Limpar variável de ambiente
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    
} catch {
    Write-Host "❌ Erro ao executar o script: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🎯 PRÓXIMOS PASSOS:" -ForegroundColor Cyan
Write-Host "1. Verificar se a view vwplanostelefonia existe e tem dados" -ForegroundColor White
Write-Host "2. Testar a API de planos no backend" -ForegroundColor White
Write-Host "3. Verificar se os totalizadores aparecem no frontend" -ForegroundColor White
Write-Host "================================================" -ForegroundColor Cyan
