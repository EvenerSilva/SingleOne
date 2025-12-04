-- Script direto para adicionar campos cidade e estado na tabela localidades
-- Execute este script no seu banco PostgreSQL

-- Adicionar campo cidade
ALTER TABLE localidades ADD COLUMN IF NOT EXISTS cidade VARCHAR(100);

-- Adicionar campo estado  
ALTER TABLE localidades ADD COLUMN IF NOT EXISTS estado VARCHAR(50);

-- Verificar se os campos foram criados
SELECT column_name, data_type, is_nullable, character_maximum_length
FROM information_schema.columns 
WHERE table_name = 'localidades' 
  AND column_name IN ('cidade', 'estado')
ORDER BY column_name;
