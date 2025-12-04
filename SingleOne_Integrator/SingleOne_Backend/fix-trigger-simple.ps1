# Script para corrigir trigger de forma simples
Write-Host "Corrigindo trigger para nao depender de usuario_id..." -ForegroundColor Yellow

# Solicitar senha do banco
$password = Read-Host "Digite a senha do PostgreSQL" -AsSecureString
$plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))

try {
    # Executar comando SQL direto
    $env:PGPASSWORD = $plainPassword
    
    Write-Host "Modificando trigger para nao inserir no historico automaticamente..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "CREATE OR REPLACE FUNCTION criar_historico_sinalizacao() RETURNS TRIGGER AS \$\$ BEGIN RETURN NEW; END; \$\$ language 'plpgsql';"
    
    Write-Host "Adicionando coluna nome_vigilante na tabela..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "ALTER TABLE sinalizacoes_suspeitas ADD COLUMN IF NOT EXISTS nome_vigilante VARCHAR(100);"
    
    Write-Host "Verificando alteracoes..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "SELECT column_name FROM information_schema.columns WHERE table_name = 'sinalizacoes_suspeitas' AND column_name = 'nome_vigilante';"
    
    Write-Host "Correcao aplicada com sucesso!" -ForegroundColor Green
    Write-Host "Agora o vigilante pode colocar o nome dele diretamente na tela de consulta." -ForegroundColor Cyan
    
} catch {
    Write-Host "Erro ao executar correcao: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}
