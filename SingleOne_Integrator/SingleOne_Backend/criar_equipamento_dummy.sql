-- 🎯 CRIAR EQUIPAMENTO DUMMY PARA LINHAS TELEFÔNICAS

-- 1. Verificar se existe equipamento com ID 0
SELECT * FROM equipamentos WHERE id = 0;

-- 2. Se não existir, criar equipamento dummy para linhas telefônicas
INSERT INTO equipamentos (
    id,
    cliente,
    numeroserie,
    patrimonio,
    equipamentostatus,
    tipoequipamento,
    tipoaquisicao,
    dtaquisicao,
    observacoes
) 
SELECT 
    0 as id,
    1 as cliente, -- Usar cliente padrão
    'LINHA-TEL-DUMMY' as numeroserie,
    'LINHA-TEL-DUMMY' as patrimonio,
    1 as equipamentostatus, -- Status padrão
    1 as tipoequipamento, -- Tipo padrão
    1 as tipoaquisicao, -- Tipo aquisição padrão
    NOW() as dtaquisicao,
    'Equipamento dummy para registros de histórico de linhas telefônicas' as observacoes
WHERE NOT EXISTS (SELECT 1 FROM equipamentos WHERE id = 0);

-- 3. Verificar se foi criado
SELECT * FROM equipamentos WHERE id = 0;

-- ✅ RESULTADO ESPERADO:
-- Deve existir um equipamento com ID 0 para permitir registros de histórico para linhas telefônicas
