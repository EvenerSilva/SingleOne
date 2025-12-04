-- Script para verificar os nomes corretos das tabelas
-- Execute este script para confirmar os nomes das tabelas no seu banco

-- 1. Verificar todas as tabelas que contêm 'equipamento'
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_name ILIKE '%equipamento%'
ORDER BY table_name;

-- 2. Verificar todas as tabelas que contêm 'requisicao'
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_name ILIKE '%requisicao%'
ORDER BY table_name;

-- 3. Verificar todas as tabelas que contêm 'usuario'
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_name ILIKE '%usuario%'
ORDER BY table_name;

-- 4. Verificar todas as tabelas que contêm 'colaborador'
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_name ILIKE '%colaborador%'
ORDER BY table_name;

-- 5. Verificar todas as tabelas que contêm 'fabricante'
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_name ILIKE '%fabricante%'
ORDER BY table_name;

-- 6. Verificar todas as tabelas que contêm 'modelo'
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_name ILIKE '%modelo%'
ORDER BY table_name;

-- 7. Verificar todas as tabelas que contêm 'tipoequipamento'
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_name ILIKE '%tipoequipamento%'
ORDER BY table_name;

-- 8. Verificar todas as tabelas que contêm 'equipamentosstatus'
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_name ILIKE '%equipamentosstatus%'
ORDER BY table_name;

-- 9. Verificar todas as tabelas que contêm 'requisicao'
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_name ILIKE '%requisicao%'
ORDER BY table_name;

-- 10. Verificar todas as tabelas que contêm 'equipamentohistorico'
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_name ILIKE '%equipamentohistorico%'
ORDER BY table_name;
