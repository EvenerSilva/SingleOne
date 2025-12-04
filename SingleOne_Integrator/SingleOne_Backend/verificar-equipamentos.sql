-- Script para verificar se a tabela Equipamentos existe
-- 1. Verificar se a tabela existe
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_name = 'Equipamentos' 
   OR table_name = 'equipamentos'
   OR table_name = 'equipamento';

-- 2. Verificar todas as tabelas que contêm 'equipamento' no nome
SELECT 
    table_name,
    table_type
FROM information_schema.tables 
WHERE table_name ILIKE '%equipamento%'
ORDER BY table_name;

-- 3. Verificar estrutura da tabela Equipamento (se existir)
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'Equipamento'
ORDER BY ordinal_position;

-- 4. Verificar se há dados na tabela
SELECT COUNT(*) as total_equipamentos FROM "Equipamento";
