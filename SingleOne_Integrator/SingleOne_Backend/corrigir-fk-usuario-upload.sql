-- Corrigir Foreign Key para permitir NULL
-- Remove a constraint antiga e cria uma nova que permite NULL

-- 1. Remover constraint antiga
ALTER TABLE contratos 
DROP CONSTRAINT IF EXISTS fk_contratos_usuarioupload;

-- 2. Criar nova constraint que permite NULL
ALTER TABLE contratos 
ADD CONSTRAINT fk_contratos_usuarioupload 
FOREIGN KEY (usuariouploadarquivo) 
REFERENCES usuarios(id)
ON DELETE SET NULL;

-- Verificar se funcionou
SELECT 
    constraint_name, 
    table_name, 
    column_name,
    is_nullable
FROM information_schema.key_column_usage
WHERE table_name = 'contratos' 
  AND constraint_name = 'fk_contratos_usuarioupload';


