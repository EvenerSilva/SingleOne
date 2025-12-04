-- 🔍 Script para verificar a estrutura real da tabela sinalizacoes_suspeitas

-- 1. Verificar se a tabela existe
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_name = 'sinalizacoes_suspeitas';

-- 2. Verificar estrutura completa da tabela
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default,
    character_maximum_length
FROM information_schema.columns 
WHERE table_name = 'sinalizacoes_suspeitas'
ORDER BY ordinal_position;

-- 3. Verificar constraints e chaves
SELECT 
    tc.constraint_name,
    tc.constraint_type,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc 
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
    AND ccu.table_schema = tc.table_schema
WHERE tc.table_name = 'sinalizacoes_suspeitas';

-- 4. Verificar índices
SELECT 
    indexname,
    indexdef
FROM pg_indexes 
WHERE tablename = 'sinalizacoes_suspeitas';

-- 5. Contar registros (se existirem)
SELECT COUNT(*) as total_registros FROM sinalizacoes_suspeitas;
