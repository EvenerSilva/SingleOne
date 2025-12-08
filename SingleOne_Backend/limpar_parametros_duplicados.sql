-- Script para limpar registros duplicados de parâmetros
-- Mantém apenas o registro mais recente (maior ID) para cada cliente
-- Execute: psql -h localhost -U postgres -d singleone -f limpar_parametros_duplicados.sql

-- Verificar duplicados antes de limpar
SELECT 
    cliente,
    COUNT(*) as total_registros,
    STRING_AGG(id::text, ', ' ORDER BY id) as ids
FROM parametros
GROUP BY cliente
HAVING COUNT(*) > 1
ORDER BY cliente;

-- Limpar duplicados: manter apenas o registro com maior ID para cada cliente
WITH duplicados AS (
    SELECT 
        id,
        cliente,
        ROW_NUMBER() OVER (PARTITION BY cliente ORDER BY id DESC) as rn
    FROM parametros
)
DELETE FROM parametros
WHERE id IN (
    SELECT id 
    FROM duplicados 
    WHERE rn > 1
);

-- Verificar resultado após limpeza
SELECT 
    cliente,
    COUNT(*) as total_registros,
    MAX(id) as id_restante
FROM parametros
GROUP BY cliente
ORDER BY cliente;

