# Script para corrigir tamanho das colunas VARCHAR
Write-Host "Corrigindo tamanho das colunas VARCHAR..." -ForegroundColor Yellow

# Solicitar senha do banco
$password = Read-Host "Digite a senha do PostgreSQL" -AsSecureString
$plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))

try {
    # Executar comando SQL direto
    $env:PGPASSWORD = $plainPassword
    
    Write-Host "Verificando colunas com VARCHAR(14)..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "SELECT column_name, data_type, character_maximum_length FROM information_schema.columns WHERE table_name = 'sinalizacoes_suspeitas' AND character_maximum_length = 14;"
    
    Write-Host "Alterando cpf_consultado para VARCHAR(20)..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "ALTER TABLE sinalizacoes_suspeitas ALTER COLUMN cpf_consultado TYPE VARCHAR(20);"
    
    Write-Host "Verificando alteracao..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "SELECT column_name, data_type, character_maximum_length FROM information_schema.columns WHERE table_name = 'sinalizacoes_suspeitas' AND column_name = 'cpf_consultado';"
    
    Write-Host "Correcao aplicada com sucesso!" -ForegroundColor Green
    
} catch {
    Write-Host "Erro ao executar correcao: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}
