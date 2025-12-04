-- Script para limpar duplicatas na tabela tipoequipamentosclientes
-- Data: 2025-09-27

-- 1. Verificar duplicatas antes da limpeza
SELECT 
    cliente, 
    tipo, 
    COUNT(*) as quantidade 
FROM tipoequipamentosclientes 
WHERE cliente = 1 
GROUP BY cliente, tipo 
HAVING COUNT(*) > 1 
ORDER BY tipo;

-- 2. Remover duplicatas mantendo apenas o primeiro registro de cada grupo
WITH duplicatas AS (
    SELECT 
        id,
        ROW_NUMBER() OVER (PARTITION BY cliente, tipo ORDER BY id) as rn
    FROM tipoequipamentosclientes
    WHERE cliente = 1
)
DELETE FROM tipoequipamentosclientes 
WHERE id IN (
    SELECT id 
    FROM duplicatas 
    WHERE rn > 1
);

-- 3. Verificar se as duplicatas foram removidas
SELECT 
    cliente, 
    tipo, 
    COUNT(*) as quantidade 
FROM tipoequipamentosclientes 
WHERE cliente = 1 
GROUP BY cliente, tipo 
HAVING COUNT(*) > 1 
ORDER BY tipo;

-- 4. Mostrar resultado final
SELECT 
    'Duplicatas removidas com sucesso!' as status,
    COUNT(*) as total_registros_restantes
FROM tipoequipamentosclientes 
WHERE cliente = 1;
