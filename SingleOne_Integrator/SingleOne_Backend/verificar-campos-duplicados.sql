-- Script para verificar campos duplicados com dados
-- Verificar se campos com nomes similares (maiúsculas vs minúsculas) têm dados

-- 1. Verificar todos os campos com "Id" no final
SELECT 
    'Campos com Id' as categoria,
    column_name,
    COUNT(*) as total_registros
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%Id'
GROUP BY column_name
ORDER BY column_name;

-- 2. Verificar campos com maiúsculas que podem ter dados
SELECT 
    'Campos com maiúsculas' as categoria,
    column_name,
    COUNT(*) as total_registros
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name ~ '[A-Z]'
    AND column_name NOT LIKE '%Id'
GROUP BY column_name
ORDER BY column_name;

-- 3. Verificar dados em campos específicos duplicados
-- Cliente vs ClienteId
SELECT 
    'cliente vs ClienteId' as comparacao,
    COUNT(*) as total,
    COUNT(cliente) as cliente_nao_nulos,
    COUNT("ClienteId") as ClienteId_nao_nulos
FROM equipamentos;

-- Empresa vs EmpresaId
SELECT 
    'empresa vs EmpresaId' as comparacao,
    COUNT(*) as total,
    COUNT(empresa) as empresa_nao_nulos,
    COUNT("EmpresaId") as EmpresaId_nao_nulos
FROM equipamentos;

-- CentroCusto vs CentrocustoId
SELECT 
    'Centrocusto vs CentrocustoId' as comparacao,
    COUNT(*) as total,
    COUNT("Centrocusto") as Centrocusto_nao_nulos,
    COUNT("CentrocustoId") as CentrocustoId_nao_nulos,
    COUNT(centrocusto) as centrocusto_nao_nulos
FROM equipamentos;

-- Filial vs FilialId vs filial_id
SELECT 
    'Filial vs FilialId vs filial_id' as comparacao,
    COUNT(*) as total,
    COUNT("Filial") as Filial_nao_nulos,
    COUNT("FilialId") as FilialId_nao_nulos,
    COUNT("FilialId1") as FilialId1_nao_nulos,
    COUNT(filial_id) as filial_id_nao_nulos
FROM equipamentos;

-- Localidade vs LocalidadeId vs localidade_id
SELECT 
    'Localidade vs LocalidadeId vs localidade_id' as comparacao,
    COUNT(*) as total,
    COUNT("Localidade") as Localidade_nao_nulos,
    COUNT("LocalidadeId") as LocalidadeId_nao_nulos,
    COUNT(localidade_id) as localidade_id_nao_nulos
FROM equipamentos;

-- Fornecedor vs FornecedorId
SELECT 
    'Fornecedor vs FornecedorId' as comparacao,
    COUNT(*) as total,
    COUNT(fornecedor) as fornecedor_nao_nulos,
    COUNT("Fornecedor") as Fornecedor_nao_nulos,
    COUNT("FornecedorId") as FornecedorId_nao_nulos
FROM equipamentos;

-- Usuario vs UsuarioId
SELECT 
    'Usuario vs UsuarioId' as comparacao,
    COUNT(*) as total,
    COUNT(usuario) as usuario_nao_nulos,
    COUNT("Usuario") as Usuario_nao_nulos,
    COUNT("UsuarioId") as UsuarioId_nao_nulos
FROM equipamentos;

-- Equipamentostatus vs EquipamentostatusId
SELECT 
    'Equipamentostatus vs EquipamentostatusId' as comparacao,
    COUNT(*) as total,
    COUNT(equipamentostatus) as equipamentostatus_nao_nulos,
    COUNT("Equipamentostatus") as Equipamentostatus_nao_nulos,
    COUNT("EquipamentostatusId") as EquipamentostatusId_nao_nulos,
    COUNT("EquipamentosstatusId") as EquipamentosstatusId_nao_nulos
FROM equipamentos;

-- Tipoequipamento vs TipoequipamentoId
SELECT 
    'Tipoequipamento vs TipoequipamentoId' as comparacao,
    COUNT(*) as total,
    COUNT(tipoequipamento) as tipoequipamento_nao_nulos,
    COUNT("Tipoequipamento") as Tipoequipamento_nao_nulos,
    COUNT("TipoequipamentoId") as TipoequipamentoId_nao_nulos
FROM equipamentos;

-- Fabricante vs FabricanteId
SELECT 
    'Fabricante vs FabricanteId' as comparacao,
    COUNT(*) as total,
    COUNT(fabricante) as fabricante_nao_nulos,
    COUNT("Fabricante") as Fabricante_nao_nulos,
    COUNT("FabricanteId") as FabricanteId_nao_nulos
FROM equipamentos;

-- Modelo vs ModeloId
SELECT 
    'Modelo vs ModeloId' as comparacao,
    COUNT(*) as total,
    COUNT(modelo) as modelo_nao_nulos,
    COUNT("Modelo") as Modelo_nao_nulos,
    COUNT("ModeloId") as ModeloId_nao_nulos
FROM equipamentos;

-- Notafiscal vs NotafiscalId
SELECT 
    'Notafiscal vs NotafiscalId' as comparacao,
    COUNT(*) as total,
    COUNT(notafiscal) as notafiscal_nao_nulos,
    COUNT("Notafiscal") as Notafiscal_nao_nulos,
    COUNT("NotafiscalId") as NotafiscalId_nao_nulos,
    COUNT("NotasfiscaiId") as NotasfiscaiId_nao_nulos
FROM equipamentos;

-- Contrato vs ContratoId
SELECT 
    'Contrato vs ContratoId' as comparacao,
    COUNT(*) as total,
    COUNT(contrato) as contrato_nao_nulos,
    COUNT("Contrato") as Contrato_nao_nulos,
    COUNT("ContratoId") as ContratoId_nao_nulos
FROM equipamentos;

-- Tipoaquisicao vs TipoaquisicaoId
SELECT 
    'Tipoaquisicao vs TipoaquisicaoId' as comparacao,
    COUNT(*) as total,
    COUNT(tipoaquisicao) as tipoaquisicao_nao_nulos,
    COUNT("Tipoaquisicao") as Tipoaquisicao_nao_nulos,
    COUNT("TipoaquisicaoId") as TipoaquisicaoId_nao_nulos
FROM equipamentos;

-- 4. Verificar se há dados em campos que começam com maiúscula
SELECT 
    'Campos com maiúscula - dados não nulos' as categoria,
    column_name,
    CASE 
        WHEN column_name = 'ClienteId' THEN (SELECT COUNT(*) FROM equipamentos WHERE "ClienteId" IS NOT NULL)
        WHEN column_name = 'EmpresaId' THEN (SELECT COUNT(*) FROM equipamentos WHERE "EmpresaId" IS NOT NULL)
        WHEN column_name = 'CentrocustoId' THEN (SELECT COUNT(*) FROM equipamentos WHERE "CentrocustoId" IS NOT NULL)
        WHEN column_name = 'FilialId' THEN (SELECT COUNT(*) FROM equipamentos WHERE "FilialId" IS NOT NULL)
        WHEN column_name = 'LocalidadeId' THEN (SELECT COUNT(*) FROM equipamentos WHERE "LocalidadeId" IS NOT NULL)
        WHEN column_name = 'FornecedorId' THEN (SELECT COUNT(*) FROM equipamentos WHERE "FornecedorId" IS NOT NULL)
        WHEN column_name = 'UsuarioId' THEN (SELECT COUNT(*) FROM equipamentos WHERE "UsuarioId" IS NOT NULL)
        WHEN column_name = 'EquipamentostatusId' THEN (SELECT COUNT(*) FROM equipamentos WHERE "EquipamentostatusId" IS NOT NULL)
        WHEN column_name = 'TipoequipamentoId' THEN (SELECT COUNT(*) FROM equipamentos WHERE "TipoequipamentoId" IS NOT NULL)
        WHEN column_name = 'FabricanteId' THEN (SELECT COUNT(*) FROM equipamentos WHERE "FabricanteId" IS NOT NULL)
        WHEN column_name = 'ModeloId' THEN (SELECT COUNT(*) FROM equipamentos WHERE "ModeloId" IS NOT NULL)
        WHEN column_name = 'NotafiscalId' THEN (SELECT COUNT(*) FROM equipamentos WHERE "NotafiscalId" IS NOT NULL)
        WHEN column_name = 'ContratoId' THEN (SELECT COUNT(*) FROM equipamentos WHERE "ContratoId" IS NOT NULL)
        WHEN column_name = 'TipoaquisicaoId' THEN (SELECT COUNT(*) FROM equipamentos WHERE "TipoaquisicaoId" IS NOT NULL)
        ELSE 0
    END as registros_nao_nulos
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%Id'
ORDER BY column_name;
