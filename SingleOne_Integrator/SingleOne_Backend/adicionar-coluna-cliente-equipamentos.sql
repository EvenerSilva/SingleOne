-- Script para adicionar coluna 'cliente' na tabela equipamentos
-- Execute este comando no PostgreSQL

-- 1. Verificar se a coluna já existe
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
AND column_name = 'cliente';

-- 2. Se não existir, adicionar a coluna
ALTER TABLE equipamentos ADD COLUMN IF NOT EXISTS cliente INTEGER;

-- 3. Verificar se foi adicionada
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'equipamentos' 
AND column_name = 'cliente';

-- 4. Adicionar comentário na coluna
COMMENT ON COLUMN equipamentos.cliente IS 'ID do cliente proprietário do equipamento';

-- 5. Verificar estrutura final da tabela
\d equipamentos
