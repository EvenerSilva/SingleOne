-- Verificar se o campo usarpadrao existe
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns 
WHERE table_name = 'cargosconfianca'
ORDER BY ordinal_position;

