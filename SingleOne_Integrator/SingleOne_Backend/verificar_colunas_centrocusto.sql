-- Script para verificar e limpar colunas desnecessárias na tabela centrocusto
-- Verificar todas as colunas existentes
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'centrocusto' 
ORDER BY ordinal_position;

-- Verificar se existe alguma coluna FilialId1 ou similar
SELECT column_name 
FROM information_schema.columns 
WHERE table_name = 'centrocusto' 
AND column_name LIKE '%FilialId%';

-- Se existir FilialId1, removê-la (descomente a linha abaixo se necessário)
-- ALTER TABLE centrocusto DROP COLUMN IF EXISTS "FilialId1";

-- Verificar constraints
SELECT conname, contype, pg_get_constraintdef(oid) as definition
FROM pg_constraint 
WHERE conrelid = 'centrocusto'::regclass;
