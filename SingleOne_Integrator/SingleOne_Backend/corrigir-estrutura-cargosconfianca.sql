-- Corrigir estrutura completa da tabela cargosconfianca

-- Adicionar coluna usarpadrao se não existir
ALTER TABLE cargosconfianca ADD COLUMN IF NOT EXISTS usarpadrao BOOLEAN DEFAULT true;

-- Adicionar coluna dataalteracao se não existir
ALTER TABLE cargosconfianca ADD COLUMN IF NOT EXISTS dataalteracao TIMESTAMP;

-- Adicionar coluna usuarioalteracao se não existir
ALTER TABLE cargosconfianca ADD COLUMN IF NOT EXISTS usuarioalteracao INTEGER;

-- Verificar estrutura final
SELECT 
    column_name, 
    data_type, 
    is_nullable, 
    column_default
FROM information_schema.columns 
WHERE table_name = 'cargosconfianca'
ORDER BY ordinal_position;

-- Verificar dados
SELECT id, cargo, usarpadrao, nivelcriticidade, ativo FROM cargosconfianca;

