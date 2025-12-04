# Script simples para adicionar campos MTR
Write-Host "Executando script para adicionar campos MTR..." -ForegroundColor Green

$sqlCommands = @"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_obrigatorio')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_obrigatorio BIT NOT NULL DEFAULT 0
    PRINT 'Campo mtr_obrigatorio adicionado'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_numero')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_numero VARCHAR(50) NULL
    PRINT 'Campo mtr_numero adicionado'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_emitido_por')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_emitido_por VARCHAR(20) NULL
    PRINT 'Campo mtr_emitido_por adicionado'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_data_emissao')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_data_emissao DATETIME NULL
    PRINT 'Campo mtr_data_emissao adicionado'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_validade')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_validade DATETIME NULL
    PRINT 'Campo mtr_validade adicionado'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_arquivo')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_arquivo VARCHAR(500) NULL
    PRINT 'Campo mtr_arquivo adicionado'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_empresa_transportadora')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_empresa_transportadora VARCHAR(200) NULL
    PRINT 'Campo mtr_empresa_transportadora adicionado'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_cnpj_transportadora')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_cnpj_transportadora VARCHAR(20) NULL
    PRINT 'Campo mtr_cnpj_transportadora adicionado'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_placa_veiculo')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_placa_veiculo VARCHAR(10) NULL
    PRINT 'Campo mtr_placa_veiculo adicionado'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_motorista')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_motorista VARCHAR(100) NULL
    PRINT 'Campo mtr_motorista adicionado'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_cpf_motorista')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_cpf_motorista VARCHAR(14) NULL
    PRINT 'Campo mtr_cpf_motorista adicionado'
END

PRINT 'Campos MTR adicionados com sucesso!'
"@

try {
    $tempFile = "temp_mtr.sql"
    $sqlCommands | Out-File -FilePath $tempFile -Encoding UTF8
    
    Write-Host "Executando comandos SQL..." -ForegroundColor Yellow
    $result = & sqlcmd -S localhost -d SingleOneDB -i $tempFile
    $result | ForEach-Object { Write-Host $_ }
    
    Remove-Item $tempFile -ErrorAction SilentlyContinue
    Write-Host "Script executado com sucesso!" -ForegroundColor Green
}
catch {
    Write-Host "Erro ao executar script: $($_.Exception.Message)" -ForegroundColor Red
}
