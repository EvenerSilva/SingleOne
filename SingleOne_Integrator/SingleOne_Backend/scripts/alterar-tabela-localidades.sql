-- =====================================================
-- SCRIPT PARA ALTERAR TABELA LOCALIDADES
-- Adicionar campos cidade e estado
-- =====================================================

USE [SingleOneDB]  -- Substitua pelo nome correto do seu banco
GO

-- Verificar se a tabela existe
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Localidades')
BEGIN
    PRINT '❌ Tabela Localidades não encontrada!'
    PRINT 'Verifique o nome correto da tabela no seu banco de dados.'
    RETURN
END

-- Verificar se os campos já existem
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Localidades' AND COLUMN_NAME = 'cidade')
BEGIN
    PRINT '⚠️ Campo "cidade" já existe na tabela Localidades'
END
ELSE
BEGIN
    -- Adicionar campo cidade
    ALTER TABLE [Localidades] 
    ADD [cidade] NVARCHAR(100) NULL
    
    PRINT '✅ Campo "cidade" adicionado com sucesso!'
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Localidades' AND COLUMN_NAME = 'estado')
BEGIN
    PRINT '⚠️ Campo "estado" já existe na tabela Localidades'
END
ELSE
BEGIN
    -- Adicionar campo estado
    ALTER TABLE [Localidades] 
    ADD [estado] NVARCHAR(50) NULL
    
    PRINT '✅ Campo "estado" adicionado com sucesso!'
END

-- Verificar estrutura final da tabela
PRINT ''
PRINT '📋 ESTRUTURA ATUALIZADA DA TABELA LOCALIDADES:'
PRINT '=============================================='

SELECT 
    COLUMN_NAME as 'Campo',
    DATA_TYPE as 'Tipo',
    IS_NULLABLE as 'Permite Nulo',
    CHARACTER_MAXIMUM_LENGTH as 'Tamanho Máx'
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Localidades'
ORDER BY ORDINAL_POSITION

PRINT ''
PRINT '🎯 CAMPOS ADICIONADOS:'
PRINT '- cidade: NVARCHAR(100) NULL - Nome da cidade (opcional)'
PRINT '- estado: NVARCHAR(50) NULL - Nome do estado (opcional)'
PRINT ''
PRINT '✅ Script executado com sucesso!'
PRINT 'A tabela Localidades agora suporta os campos cidade e estado.'
