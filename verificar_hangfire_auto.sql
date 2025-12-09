-- =====================================================
-- VERIFICAR SE O HANGFIRE PODE CRIAR TABELAS AUTOMATICAMENTE
-- =====================================================

-- Verificar se o schema existe
SELECT 
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'hangfire') 
        THEN '✅ Schema hangfire existe'
        ELSE '❌ Schema hangfire NÃO existe'
    END AS status_schema;

-- Verificar permissões do usuário postgres
SELECT 
    has_schema_privilege('postgres', 'hangfire', 'CREATE') AS pode_criar_schema,
    has_schema_privilege('postgres', 'hangfire', 'USAGE') AS pode_usar_schema;

-- Verificar se há tabelas no schema hangfire
SELECT 
    COUNT(*) AS total_tabelas,
    string_agg(table_name, ', ') AS tabelas
FROM information_schema.tables 
WHERE table_schema = 'hangfire';

-- Verificar permissões do usuário no banco
SELECT 
    datname,
    datacl
FROM pg_database 
WHERE datname = 'singleone';

