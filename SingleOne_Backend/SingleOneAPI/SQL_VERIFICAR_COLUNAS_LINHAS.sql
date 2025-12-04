-- ========================================
-- VERIFICAR COLUNAS DA TABELA TELEFONIALINHAS
-- ========================================

-- 1. LISTAR TODAS AS COLUNAS
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_name = 'telefonialinhas'
ORDER BY ordinal_position;

-- 2. VERIFICAR SE AS COLUNAS cliente, operadora, contrato EXISTEM
SELECT 
    column_name
FROM information_schema.columns
WHERE table_name = 'telefonialinhas'
  AND column_name IN ('cliente', 'operadora', 'contrato');

