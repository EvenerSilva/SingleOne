-- Script para verificar views e procedures relacionadas
-- 1. Verificar views que referenciam requisicoesitens
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_schema = 'public' 
    AND (table_name LIKE '%requisicao%' OR table_name LIKE '%equipamento%')
ORDER BY table_type, table_name;

-- 2. Verificar procedures/functions que podem referenciar a tabela
SELECT 
    routine_name,
    routine_type,
    routine_definition
FROM information_schema.routines 
WHERE routine_schema = 'public' 
    AND routine_definition LIKE '%requisicoesitens%'
ORDER BY routine_type, routine_name;

-- 3. Verificar se há regras (rules) na tabela
SELECT 
    rule_name,
    definition
FROM pg_rules 
WHERE tablename = 'requisicoesitens';

-- 4. Verificar se há políticas (policies) RLS
SELECT 
    schemaname,
    tablename,
    policyname,
    permissive,
    roles,
    cmd,
    qual,
    with_check
FROM pg_policies 
WHERE tablename = 'requisicoesitens';

-- 5. Verificar se há comentários na tabela ou colunas
SELECT 
    c.table_name,
    c.column_name,
    pgd.description
FROM pg_catalog.pg_statio_all_tables st
INNER JOIN pg_catalog.pg_description pgd ON (pgd.objoid = st.relid)
INNER JOIN information_schema.columns c ON (
    pgd.objsubid = c.ordinal_position AND 
    c.table_schema = st.schemaname AND 
    c.table_name = st.relname
)
WHERE c.table_name = 'requisicoesitens';
