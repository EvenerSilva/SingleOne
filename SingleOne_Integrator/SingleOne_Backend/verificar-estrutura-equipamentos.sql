-- Script para verificar a estrutura da tabela equipamentos
-- Execute este comando no PostgreSQL

-- 1. Verificar todas as colunas da tabela equipamentos
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
ORDER BY ordinal_position;

-- 2. Verificar especificamente a coluna cliente
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
AND column_name = 'cliente';

-- 3. Verificar se existe coluna com C maiúsculo
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
AND column_name = 'Cliente';

-- 4. Mostrar estrutura completa da tabela
\d equipamentos