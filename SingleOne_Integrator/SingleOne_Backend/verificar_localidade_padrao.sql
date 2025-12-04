-- Verificar localidade padrão
-- ================================================

-- 1. Verificar dados atuais
SELECT 
    id,
    numeroserie,
    patrimonio,
    localizacao,
    l.descricao as localizacao_nome
FROM equipamentos e
LEFT JOIN localidades l ON e.localizacao = l.id
WHERE id = 1811;

-- 2. Verificar todas as localidades
SELECT id, descricao FROM localidades ORDER BY id;

-- 3. Verificar na view atual
SELECT 
    id,
    numeroserie,
    patrimonio,
    localizacao,
    localizacaoid
FROM equipamentovm 
WHERE id = 1811;
