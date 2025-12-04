-- Script para verificar os status das requisições no banco de dados
-- Execute este script para identificar possíveis inconsistências

-- 1. Verificar se existe uma tabela de status de requisições
SELECT 
    'Verificando tabela de status de requisições' as info,
    schemaname,
    tablename,
    tableowner
FROM pg_tables 
WHERE tablename LIKE '%status%' OR tablename LIKE '%requisicao%';

-- 2. Verificar a view requisicoesvm
SELECT 
    'Verificando view requisicoesvm' as info,
    schemaname,
    viewname,
    viewowner
FROM pg_views 
WHERE viewname = 'requisicoesvm';

-- 3. Verificar a estrutura da view requisicoesvm
SELECT 
    'Estrutura da view requisicoesvm' as info,
    column_name,
    data_type,
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'requisicoesvm'
ORDER BY ordinal_position;

-- 4. Verificar os valores únicos de status na view
SELECT 
    'Valores únicos de status na view' as info,
    requisicaostatusid,
    requisicaostatus,
    COUNT(*) as quantidade
FROM requisicoesvm 
GROUP BY requisicaostatusid, requisicaostatus
ORDER BY requisicaostatusid;

-- 5. Verificar os valores únicos de status na tabela principal
SELECT 
    'Valores únicos de status na tabela principal' as info,
    requisicaostatus,
    COUNT(*) as quantidade
FROM requisicoes 
GROUP BY requisicaostatus
ORDER BY requisicaostatus;

-- 6. Verificar se há inconsistências entre a tabela e a view
SELECT 
    'Verificando inconsistências entre tabela e view' as info,
    r.requisicaostatus as status_tabela,
    rv.requisicaostatusid as status_id_view,
    rv.requisicaostatus as status_desc_view,
    COUNT(*) as quantidade
FROM requisicoes r
LEFT JOIN requisicoesvm rv ON r.id = rv.id
GROUP BY r.requisicaostatus, rv.requisicaostatusid, rv.requisicaostatus
ORDER BY r.requisicaostatus;

-- 7. Verificar requisições com status específicos
SELECT 
    'Requisições com status 3 (supostamente cancelada)' as info,
    id,
    requisicaostatus,
    dtsolicitacao,
    dtenviotermo
FROM requisicoes 
WHERE requisicaostatus = 3
ORDER BY id DESC
LIMIT 10;

-- 8. Verificar requisições com status específicos na view
SELECT 
    'Requisições com status 3 na view' as info,
    id,
    requisicaostatusid,
    requisicaostatus,
    dtsolicitacao,
    dtenviotermo
FROM requisicoesvm 
WHERE requisicaostatusid = 3
ORDER BY id DESC
LIMIT 10;
