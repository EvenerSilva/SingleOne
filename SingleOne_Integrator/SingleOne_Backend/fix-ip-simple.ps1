# Script simples para corrigir tipo de dados da coluna ip_address
Write-Host "Corrigindo tipo de dados da coluna ip_address..." -ForegroundColor Yellow

# Solicitar senha do banco
$password = Read-Host "Digite a senha do PostgreSQL" -AsSecureString
$plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))

try {
    # Executar comando SQL direto
    $env:PGPASSWORD = $plainPassword
    
    Write-Host "Verificando tipo atual da coluna..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'sinalizacoes_suspeitas' AND column_name = 'ip_address';"
    
    Write-Host "Alterando tipo da coluna..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "ALTER TABLE sinalizacoes_suspeitas ALTER COLUMN ip_address TYPE VARCHAR(45) USING ip_address::VARCHAR(45);"
    
    Write-Host "Verificando alteracao..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'sinalizacoes_suspeitas' AND column_name = 'ip_address';"
    
    Write-Host "Correcao aplicada com sucesso!" -ForegroundColor Green
    
} catch {
    Write-Host "Erro ao executar correcao: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}
