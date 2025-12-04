-- Corrigir o default da coluna usarpadrao
-- O default deve ser TRUE (padrão) não FALSE

-- Remover o default atual
ALTER TABLE cargosconfianca ALTER COLUMN usarpadrao DROP DEFAULT;

-- Adicionar novo default como TRUE
ALTER TABLE cargosconfianca ALTER COLUMN usarpadrao SET DEFAULT true;

-- Verificar a estrutura
SELECT 
    column_name, 
    data_type, 
    column_default,
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'cargosconfianca' AND column_name = 'usarpadrao';

-- Mostrar todos os cargos
SELECT id, cargo, usarpadrao, nivelcriticidade FROM cargosconfianca ORDER BY id;

