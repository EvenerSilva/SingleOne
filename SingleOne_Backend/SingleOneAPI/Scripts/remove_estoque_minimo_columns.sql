-- Script para remover colunas de estoque mínimo da tabela modelos
-- Execute este script no banco de dados PostgreSQL

-- Remover a coluna setestoqueminimo
ALTER TABLE modelos DROP COLUMN IF EXISTS setestoqueminimo;

-- Remover a coluna quantidadeestoqueminimo
ALTER TABLE modelos DROP COLUMN IF EXISTS quantidadeestoqueminimo;

-- Verificar se as colunas foram removidas
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'modelos' 
ORDER BY ordinal_position;
