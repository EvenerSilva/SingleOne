-- Script simples para adicionar o campo usarpadrao

ALTER TABLE cargosconfianca ADD COLUMN IF NOT EXISTS usarpadrao BOOLEAN DEFAULT true;

-- Comentário no campo
COMMENT ON COLUMN cargosconfianca.usarpadrao IS 'Indica se deve usar match por padrão (LIKE) ao invés de match exato';

-- Atualizar registros existentes para usar padrão (comportamento novo)
UPDATE cargosconfianca SET usarpadrao = true WHERE usarpadrao IS NULL;

-- Verificar resultado
SELECT id, cargo, usarpadrao, nivelcriticidade, ativo FROM cargosconfianca;

