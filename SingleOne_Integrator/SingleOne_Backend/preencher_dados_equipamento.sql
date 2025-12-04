-- Preencher dados faltantes do equipamento 1811
-- ================================================

-- 1. Verificar dados disponíveis
SELECT 'empresas' as tipo, id, nome FROM empresas WHERE cliente = 1 LIMIT 3;
SELECT 'centrocusto' as tipo, id, nome FROM centrocusto WHERE cliente = 1 LIMIT 3;
SELECT 'localidades' as tipo, id, descricao FROM localidades WHERE cliente = 1 LIMIT 3;

-- 2. Atualizar equipamento 1811 com dados padrão
UPDATE equipamentos 
SET 
    empresa = (SELECT id FROM empresas WHERE cliente = 1 LIMIT 1),
    localizacao = (SELECT id FROM localidades WHERE cliente = 1 LIMIT 1)
WHERE id = 1811;

-- 3. Verificar se a atualização funcionou
SELECT 
    id,
    numeroserie,
    patrimonio,
    empresa,
    centrocusto,
    localizacao
FROM equipamentos 
WHERE id = 1811;

-- 4. Verificar na view
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
