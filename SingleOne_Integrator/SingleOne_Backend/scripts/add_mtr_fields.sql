-- =====================================================
-- Script: Adicionar campos MTR ao protocolo de descarte
-- Data: 08/10/2025
-- Descrição: Adiciona campos para Manifesto de Transporte de Resíduos (MTR)
-- =====================================================

USE [SingleOneDB]
GO

PRINT 'Iniciando adição de campos MTR...'

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
ELSE
    PRINT '⚠ Campo mtr_obrigatorio já existe'

-- Número do MTR
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_numero')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_numero VARCHAR(50) NULL
    PRINT '✓ Campo mtr_numero adicionado'
END
ELSE
    PRINT '⚠ Campo mtr_numero já existe'

-- Quem emitiu o MTR
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_emitido_por')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_emitido_por VARCHAR(20) NULL
    PRINT '✓ Campo mtr_emitido_por adicionado'
END
ELSE
    PRINT '⚠ Campo mtr_emitido_por já existe'

-- Data de emissão do MTR
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_data_emissao')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_data_emissao DATETIME NULL
    PRINT '✓ Campo mtr_data_emissao adicionado'
END
ELSE
    PRINT '⚠ Campo mtr_data_emissao já existe'

-- Data de validade do MTR
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_validade')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_validade DATETIME NULL
    PRINT '✓ Campo mtr_validade adicionado'
END
ELSE
    PRINT '⚠ Campo mtr_validade já existe'

-- Arquivo MTR
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_arquivo')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_arquivo VARCHAR(500) NULL
    PRINT '✓ Campo mtr_arquivo adicionado'
END
ELSE
    PRINT '⚠ Campo mtr_arquivo já existe'

-- Empresa transportadora
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_empresa_transportadora')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_empresa_transportadora VARCHAR(200) NULL
    PRINT '✓ Campo mtr_empresa_transportadora adicionado'
END
ELSE
    PRINT '⚠ Campo mtr_empresa_transportadora já existe'

-- CNPJ da transportadora
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_cnpj_transportadora')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_cnpj_transportadora VARCHAR(20) NULL
    PRINT '✓ Campo mtr_cnpj_transportadora adicionado'
END
ELSE
    PRINT '⚠ Campo mtr_cnpj_transportadora já existe'

-- Placa do veículo
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_placa_veiculo')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_placa_veiculo VARCHAR(10) NULL
    PRINT '✓ Campo mtr_placa_veiculo adicionado'
END
ELSE
    PRINT '⚠ Campo mtr_placa_veiculo já existe'

-- Nome do motorista
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_motorista')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_motorista VARCHAR(100) NULL
    PRINT '✓ Campo mtr_motorista adicionado'
END
ELSE
    PRINT '⚠ Campo mtr_motorista já existe'

-- CPF do motorista
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('protocolos_descarte') AND name = 'mtr_cpf_motorista')
BEGIN
    ALTER TABLE protocolos_descarte ADD mtr_cpf_motorista VARCHAR(14) NULL
    PRINT '✓ Campo mtr_cpf_motorista adicionado'
END
ELSE
    PRINT '⚠ Campo mtr_cpf_motorista já existe'

-- Verificar estrutura final
PRINT ''
PRINT 'Verificando estrutura final da tabela protocolos_descarte:'
SELECT 
    COLUMN_NAME as 'Campo',
    DATA_TYPE as 'Tipo',
    IS_NULLABLE as 'Permite_Null',
    COLUMN_DEFAULT as 'Valor_Padrao'
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'protocolos_descarte' 
    AND COLUMN_NAME LIKE 'mtr_%'
ORDER BY ORDINAL_POSITION

PRINT ''
PRINT '✅ Campos MTR adicionados com sucesso!'
PRINT '📋 Próximos passos:'
PRINT '   1. Compilar o backend'
PRINT '   2. Atualizar interface TypeScript'
PRINT '   3. Modificar modal de protocolo'
PRINT '   4. Atualizar template PDF'
