# Script PowerShell para adicionar colunas necessárias ao banco
Write-Host "Adicionando colunas necessárias ao banco de dados..." -ForegroundColor Cyan

# Comandos SQL para adicionar as colunas
$sqlCommands = @(
    "ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS `"ClienteId`" INTEGER;",
    "ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS `"EmpresaId`" INTEGER;",
    "ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS `"ContratoId`" INTEGER;",
    "ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS `"FabricanteId`" INTEGER;",
    "ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS `"ModeloId`" INTEGER;",
    "ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS `"TipoequipamentoId`" INTEGER;",
    "ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS `"TipoaquisicaoId`" INTEGER;",
    "ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS `"EquipamentostatusId`" INTEGER;",
    "ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS `"UsuarioId`" INTEGER;",
    "ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS `"NotafiscalId`" INTEGER;"
)

foreach ($sql in $sqlCommands) {
    Write-Host "Executando: $sql" -ForegroundColor Yellow
    try {
        $result = psql -h localhost -U postgres -d singleone -c $sql
        Write-Host "Sucesso!" -ForegroundColor Green
    }
    catch {
        Write-Host "Erro: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "Verificando colunas adicionadas..." -ForegroundColor Cyan
$verifySql = "SELECT column_name, data_type, is_nullable FROM information_schema.columns WHERE table_name = 'equipamentos' AND column_name LIKE '%Id' ORDER BY column_name;"
psql -h localhost -U postgres -d singleone -c $verifySql

Write-Host "Script concluído!" -ForegroundColor Cyan



