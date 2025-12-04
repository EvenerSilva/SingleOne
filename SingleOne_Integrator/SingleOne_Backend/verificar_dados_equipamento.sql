-- Verificar dados do equipamento específico
-- ================================================

-- 1. Verificar dados na tabela equipamentos
SELECT 
    id,
    numeroserie,
    patrimonio,
    empresa,
    centrocusto,
    localizacao,
    tipoequipamento,
    fabricante,
    modelo
FROM equipamentos 
WHERE id = 1811;

-- 2. Verificar se existem dados nas tabelas relacionadas
SELECT 'empresas' as tabela, id, nome FROM empresas WHERE id IN (SELECT DISTINCT empresa FROM equipamentos WHERE empresa IS NOT NULL) LIMIT 5;
SELECT 'centrocusto' as tabela, id, nome FROM centrocusto WHERE id IN (SELECT DISTINCT centrocusto FROM equipamentos WHERE centrocusto IS NOT NULL) LIMIT 5;
SELECT 'localidades' as tabela, id, descricao FROM localidades WHERE id IN (SELECT DISTINCT localizacao FROM equipamentos WHERE localizacao IS NOT NULL) LIMIT 5;

-- 3. Verificar equipamentos com dados preenchidos
SELECT 
    id,
    numeroserie,
    patrimonio,
    empresa,
    centrocusto,
    localizacao
FROM equipamentos 
WHERE (empresa IS NOT NULL OR centrocusto IS NOT NULL OR localizacao IS NOT NULL)
LIMIT 5;

-- 4. Verificar a view atual
SELECT 
    id,
    numeroserie,
    patrimonio,
    empresa,
    centrocusto,
    localizacao,
    tipoequipamento,
    fabricante,
    modelo
FROM equipamentovm 
WHERE id = 1811;
