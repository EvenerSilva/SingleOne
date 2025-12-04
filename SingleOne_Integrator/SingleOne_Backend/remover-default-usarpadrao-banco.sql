-- Remover o default da coluna usarpadrao no banco
-- O Entity Framework vai controlar o valor diretamente

ALTER TABLE cargosconfianca ALTER COLUMN usarpadrao DROP DEFAULT;

-- Tornar a coluna NOT NULL
ALTER TABLE cargosconfianca ALTER COLUMN usarpadrao SET NOT NULL;

-- Verificar
SELECT 
    column_name, 
    data_type, 
    column_default,
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'cargosconfianca' AND column_name = 'usarpadrao';

