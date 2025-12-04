-- =====================================================
-- SCRIPT PARA MIGRAR DADOS DE LOCALIZACAO
-- Migra dados da coluna localizacao para localidade_id
-- =====================================================

-- Verificar dados antes da migração
SELECT 'ANTES DA MIGRACAO' as Status;
SELECT COUNT(*) as Total, COUNT(localizacao) as Com_Localizacao, COUNT(localidade_id) as Com_LocalidadeId FROM equipamentos;

-- Migrar dados da coluna localizacao para localidade_id
UPDATE equipamentos 
SET localidade_id = localizacao 
WHERE localizacao IS NOT NULL AND localidade_id IS NULL;

-- Verificar dados após a migração
SELECT 'APOS A MIGRACAO' as Status;
SELECT COUNT(*) as Total, COUNT(localizacao) as Com_Localizacao, COUNT(localidade_id) as Com_LocalidadeId FROM equipamentos;

-- Verificar alguns registros
SELECT id, localizacao, localidade_id FROM equipamentos WHERE localizacao IS NOT NULL OR localidade_id IS NOT NULL LIMIT 10;
