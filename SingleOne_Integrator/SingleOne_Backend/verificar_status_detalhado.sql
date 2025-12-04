-- Script para verificar detalhadamente os status das requisições
-- Foco na inconsistência identificada: ID 3 = "Processada" em vez de "Cancelada"

-- 1. Verificar TODOS os status únicos na view
SELECT 
    'TODOS os status únicos na view' as info,
    requisicaostatusid,
    requisicaostatus,
    COUNT(*) as quantidade
FROM requisicoesvm 
GROUP BY requisicaostatusid, requisicaostatus
ORDER BY requisicaostatusid;

-- 2. Verificar TODOS os status únicos na tabela principal
SELECT 
    'TODOS os status únicos na tabela principal' as info,
    requisicaostatus,
    COUNT(*) as quantidade
FROM requisicoes 
GROUP BY requisicaostatus
ORDER BY requisicaostatus;

-- 3. Verificar se existe uma tabela de status de requisições
SELECT 
    'Tabelas relacionadas a status' as info,
    schemaname,
    tablename,
    tableowner
FROM pg_tables 
WHERE tablename LIKE '%status%' OR tablename LIKE '%requisicao%';

-- 4. Verificar se existe uma tabela específica de status de requisições
SELECT 
    'Tabelas de status de requisições' as info,
    schemaname,
    tablename,
    tableowner
FROM pg_tables 
WHERE tablename LIKE '%requisicao%status%' OR tablename LIKE '%status%requisicao%';

-- 5. Verificar a definição da view requisicoesvm
SELECT 
    'Definição da view requisicoesvm' as info,
    viewdefinition
FROM pg_views 
WHERE viewname = 'requisicoesvm';

-- 6. Verificar se há uma tabela de status que define os valores
SELECT 
    'Verificando se existe tabela de status' as info,
    schemaname,
    tablename
FROM pg_tables 
WHERE tablename = 'requisicaostatus' OR tablename = 'requisicoesstatus';

-- 7. Se existir tabela de status, verificar os valores
-- (Descomente se a consulta 6 retornar resultados)
-- SELECT * FROM requisicaostatus ORDER BY id;
-- SELECT * FROM requisicoesstatus ORDER BY id;
