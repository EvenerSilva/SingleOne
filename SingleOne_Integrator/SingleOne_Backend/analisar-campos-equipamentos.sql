-- Script para analisar o uso dos campos da tabela equipamentos
-- Este script ajudará a identificar campos duplicados e desnecessários

-- 1. Verificar campos duplicados (com nomes similares)
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%cliente%'
ORDER BY column_name;

-- 2. Verificar campos duplicados para empresa
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%empresa%'
ORDER BY column_name;

-- 3. Verificar campos duplicados para centro de custo
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%centrocusto%'
ORDER BY column_name;

-- 4. Verificar campos duplicados para filial
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%filial%'
ORDER BY column_name;

-- 5. Verificar campos duplicados para localidade
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%local%'
ORDER BY column_name;

-- 6. Verificar campos duplicados para fornecedor
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%fornecedor%'
ORDER BY column_name;

-- 7. Verificar campos duplicados para usuário
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%usuario%'
ORDER BY column_name;

-- 8. Verificar campos duplicados para status
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%status%'
ORDER BY column_name;

-- 9. Verificar campos duplicados para tipo equipamento
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%tipo%'
ORDER BY column_name;

-- 10. Verificar campos duplicados para fabricante
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%fabricante%'
ORDER BY column_name;

-- 11. Verificar campos duplicados para modelo
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%modelo%'
ORDER BY column_name;

-- 12. Verificar campos duplicados para nota fiscal
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%nota%'
ORDER BY column_name;

-- 13. Verificar campos duplicados para contrato
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%contrato%'
ORDER BY column_name;

-- 14. Verificar campos duplicados para tipo aquisição
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
    AND column_name LIKE '%aquisicao%'
ORDER BY column_name;

-- 15. Verificar valores não nulos em campos duplicados
-- Cliente
SELECT 
    'cliente' as campo,
    COUNT(*) as total,
    COUNT(cliente) as nao_nulos,
    COUNT(clienteid) as clienteid_nao_nulos,
    COUNT("ClienteId") as ClienteId_nao_nulos
FROM equipamentos;

-- Empresa
SELECT 
    'empresa' as campo,
    COUNT(*) as total,
    COUNT(empresa) as nao_nulos,
    COUNT("EmpresaId") as EmpresaId_nao_nulos
FROM equipamentos;

-- Centro de custo
SELECT 
    'centrocusto' as campo,
    COUNT(*) as total,
    COUNT(centrocusto) as nao_nulos,
    COUNT("CentrocustoId") as CentrocustoId_nao_nulos,
    COUNT("Centrocusto") as Centrocusto_nao_nulos
FROM equipamentos;

-- Filial
SELECT 
    'filial' as campo,
    COUNT(*) as total,
    COUNT(filial_id) as filial_id_nao_nulos,
    COUNT("Filial") as Filial_nao_nulos,
    COUNT("FilialId") as FilialId_nao_nulos,
    COUNT("FilialId1") as FilialId1_nao_nulos
FROM equipamentos;

-- Localidade
SELECT 
    'localidade' as campo,
    COUNT(*) as total,
    COUNT(localidade_id) as localidade_id_nao_nulos,
    COUNT("Localidade") as Localidade_nao_nulos,
    COUNT("LocalidadeId") as LocalidadeId_nao_nulos
FROM equipamentos;

-- Fornecedor
SELECT 
    'fornecedor' as campo,
    COUNT(*) as total,
    COUNT(fornecedor) as nao_nulos,
    COUNT("Fornecedor") as Fornecedor_nao_nulos,
    COUNT("FornecedorId") as FornecedorId_nao_nulos
FROM equipamentos;

-- Usuario
SELECT 
    'usuario' as campo,
    COUNT(*) as total,
    COUNT(usuario) as nao_nulos,
    COUNT("Usuario") as Usuario_nao_nulos,
    COUNT("UsuarioId") as UsuarioId_nao_nulos
FROM equipamentos;

-- Status
SELECT 
    'status' as campo,
    COUNT(*) as total,
    COUNT(equipamentostatus) as equipamentostatus_nao_nulos,
    COUNT("Equipamentostatus") as Equipamentostatus_nao_nulos,
    COUNT("EquipamentostatusId") as EquipamentostatusId_nao_nulos,
    COUNT("EquipamentosstatusId") as EquipamentosstatusId_nao_nulos
FROM equipamentos;

-- Tipo equipamento
SELECT 
    'tipoequipamento' as campo,
    COUNT(*) as total,
    COUNT(tipoequipamento) as nao_nulos,
    COUNT("Tipoequipamento") as Tipoequipamento_nao_nulos,
    COUNT("TipoequipamentoId") as TipoequipamentoId_nao_nulos
FROM equipamentos;

-- Fabricante
SELECT 
    'fabricante' as campo,
    COUNT(*) as total,
    COUNT(fabricante) as nao_nulos,
    COUNT("Fabricante") as Fabricante_nao_nulos,
    COUNT("FabricanteId") as FabricanteId_nao_nulos
FROM equipamentos;

-- Modelo
SELECT 
    'modelo' as campo,
    COUNT(*) as total,
    COUNT(modelo) as nao_nulos,
    COUNT("Modelo") as Modelo_nao_nulos,
    COUNT("ModeloId") as ModeloId_nao_nulos
FROM equipamentos;

-- Nota fiscal
SELECT 
    'notafiscal' as campo,
    COUNT(*) as total,
    COUNT(notafiscal) as nao_nulos,
    COUNT("Notafiscal") as Notafiscal_nao_nulos,
    COUNT("NotafiscalId") as NotafiscalId_nao_nulos,
    COUNT("NotasfiscaiId") as NotasfiscaiId_nao_nulos
FROM equipamentos;

-- Contrato
SELECT 
    'contrato' as campo,
    COUNT(*) as total,
    COUNT(contrato) as nao_nulos,
    COUNT("Contrato") as Contrato_nao_nulos,
    COUNT("ContratoId") as ContratoId_nao_nulos
FROM equipamentos;

-- Tipo aquisição
SELECT 
    'tipoaquisicao' as campo,
    COUNT(*) as total,
    COUNT(tipoaquisicao) as nao_nulos,
    COUNT("Tipoaquisicao") as Tipoaquisicao_nao_nulos,
    COUNT("TipoaquisicaoId") as TipoaquisicaoId_nao_nulos
FROM equipamentos;
