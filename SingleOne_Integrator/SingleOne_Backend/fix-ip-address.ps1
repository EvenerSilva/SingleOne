# =====================================================
# SCRIPT PARA CORRIGIR TIPO DE DADOS DA COLUNA IP_ADDRESS
# =====================================================

Write-Host "🔧 Corrigindo tipo de dados da coluna ip_address..." -ForegroundColor Yellow

# Configurações do banco
$hostname = "localhost"
$database = "singleone"
$username = "postgres"

# Solicitar senha do banco
$password = Read-Host "Digite a senha do PostgreSQL" -AsSecureString
$plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))

# Comando SQL para corrigir o tipo
$sqlCommand = @"
SELECT 
    column_name, 
    data_type, 
    character_maximum_length
FROM information_schema.columns 
WHERE table_name = 'sinalizacoes_suspeitas' 
AND column_name = 'ip_address';

ALTER TABLE sinalizacoes_suspeitas 
ALTER COLUMN ip_address TYPE VARCHAR(45) USING ip_address::VARCHAR(45);

SELECT 
    column_name, 
    data_type, 
    character_maximum_length
FROM information_schema.columns 
WHERE table_name = 'sinalizacoes_suspeitas' 
AND column_name = 'ip_address';

SELECT COUNT(*) as total_registros FROM sinalizacoes_suspeitas;
"@

try {
    # Executar o comando SQL
    $env:PGPASSWORD = $plainPassword
    $result = psql -h $hostname -U $username -d $database -c $sqlCommand
    
    Write-Host "✅ Correção aplicada com sucesso!" -ForegroundColor Green
    Write-Host "Resultado:" -ForegroundColor Cyan
    Write-Host $result
    
} catch {
    Write-Host "❌ Erro ao executar correção: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    # Limpar variável de ambiente
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host "`n🎯 Próximos passos:" -ForegroundColor Yellow
Write-Host "1. Teste a funcionalidade de sinalização de suspeitas" -ForegroundColor White
Write-Host "2. Verifique se os dados estão sendo salvos corretamente" -ForegroundColor White
Write-Host "3. Monitore os logs do backend para erros" -ForegroundColor White
