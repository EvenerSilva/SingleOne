# Script PowerShell para executar SQL de campos MTR
# Data: 08/10/2025

Write-Host "🚛 Executando script para adicionar campos MTR..." -ForegroundColor Green

# Definir conexão
$server = "localhost"
$database = "SingleOneDB"

# Comandos SQL
$sqlCommands = @"
-- Verificar se a tabela existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'protocolos_descarte')
BEGIN
    PRINT 'ERRO: Tabela protocolos_descarte não encontrada!'
    RETURN
END

-- Adicionar campos MTR
PRINT 'Adicionando campos MTR...'

-- MTR Obrigatório
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_obrigatorio')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_obrigatorio BIT NOT NULL DEFAULT 0
    PRINT '✓ Campo mtr_obrigatorio adicionado'
END

-- Número do MTR
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_numero')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_numero VARCHAR(50) NULL
    PRINT '✓ Campo mtr_numero adicionado'
END

-- Quem emitiu o MTR
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_emitido_por')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_emitido_por VARCHAR(20) NULL
    PRINT '✓ Campo mtr_emitido_por adicionado'
END

-- Data de emissão do MTR
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_data_emissao')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_data_emissao DATETIME NULL
    PRINT '✓ Campo mtr_data_emissao adicionado'
END

-- Data de validade do MTR
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_validade')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_validade DATETIME NULL
    PRINT '✓ Campo mtr_validade adicionado'
END

-- Arquivo MTR
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_arquivo')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_arquivo VARCHAR(500) NULL
    PRINT '✓ Campo mtr_arquivo adicionado'
END

-- Empresa transportadora
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_empresa_transportadora')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_empresa_transportadora VARCHAR(200) NULL
    PRINT '✓ Campo mtr_empresa_transportadora adicionado'
END

-- CNPJ da transportadora
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_cnpj_transportadora')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_cnpj_transportadora VARCHAR(20) NULL
    PRINT '✓ Campo mtr_cnpj_transportadora adicionado'
END

-- Placa do veículo
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_placa_veiculo')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_placa_veiculo VARCHAR(10) NULL
    PRINT '✓ Campo mtr_placa_veiculo adicionado'
END

-- Nome do motorista
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_motorista')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_motorista VARCHAR(100) NULL
    PRINT '✓ Campo mtr_motorista adicionado'
END

-- CPF do motorista
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_cpf_motorista')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_cpf_motorista VARCHAR(14) NULL
    PRINT '✓ Campo mtr_cpf_motorista adicionado'
END

PRINT '✅ Campos MTR adicionados com sucesso!'
"@

try {
    # Tentar conectar e executar
    Write-Host "📡 Conectando ao SQL Server..." -ForegroundColor Yellow
    
    # Usar o módulo SqlServer se disponível
    if (Get-Module -ListAvailable -Name SqlServer) {
        Import-Module SqlServer
        Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $sqlCommands
        Write-Host "✅ Script executado com sucesso via módulo SqlServer!" -ForegroundColor Green
    }
    else {
        Write-Host "⚠️ Módulo SqlServer não encontrado. Tentando alternativa..." -ForegroundColor Yellow
        
        # Tentar com sqlcmd direto
        $tempFile = "temp_mtr_script.sql"
        $sqlCommands | Out-File -FilePath $tempFile -Encoding UTF8
        
        $result = & sqlcmd -S $server -d $database -i $tempFile 2>&1
        $result | ForEach-Object { Write-Host $_ }
        
        Remove-Item $tempFile -ErrorAction SilentlyContinue
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Script executado com sucesso via sqlcmd!" -ForegroundColor Green
        } else {
            Write-Host "❌ Erro ao executar script via sqlcmd" -ForegroundColor Red
        }
    }
}
catch {
    Write-Host "❌ Erro ao executar script: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "💡 Verifique se o SQL Server está rodando e acessível" -ForegroundColor Yellow
}

Write-Host "🏁 Script finalizado." -ForegroundColor Blue
