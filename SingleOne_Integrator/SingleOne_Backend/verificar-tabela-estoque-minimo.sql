-- Script para verificar a tabela EstoqueMinimoEquipamentos
-- Conecte ao banco PostgreSQL e execute este script

-- Verificar se a tabela existe
SELECT 
    table_name,
    table_type,
    table_schema
FROM information_schema.tables 
WHERE table_name ILIKE '%estoque%minimo%' 
   OR table_name ILIKE '%estoqueminimo%'
ORDER BY table_name;

-- Verificar estrutura da tabela (se existir)
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default,
    character_maximum_length,
    numeric_precision,
    numeric_scale
FROM information_schema.columns 
WHERE table_name ILIKE '%estoque%minimo%' 
   OR table_name ILIKE '%estoqueminimo%'
ORDER BY table_name, ordinal_position;

-- Verificar dados na tabela (se existir)
SELECT COUNT(*) as total_registros FROM "EstoqueMinimoEquipamentos";

-- Verificar alguns registros de exemplo
SELECT * FROM "EstoqueMinimoEquipamentos" LIMIT 5;

-- Verificar constraints e índices
SELECT 
    tc.constraint_name,
    tc.constraint_type,
    tc.table_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc 
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
LEFT JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
    AND ccu.table_schema = tc.table_schema
WHERE tc.table_name ILIKE '%estoque%minimo%' 
   OR tc.table_name ILIKE '%estoqueminimo%'
ORDER BY tc.table_name, tc.constraint_type;