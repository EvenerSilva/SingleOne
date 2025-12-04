-- =====================================================
-- Script: Adicionar campo usarpadrao em politicas_elegibilidade
-- Descrição: Adiciona campo para controlar se usa busca por padrão (LIKE) ou match exato
-- Similar à implementação de cargos_confianca
-- =====================================================

-- Adicionar coluna usarpadrao (default = true, usa padrão)
ALTER TABLE politicas_elegibilidade 
ADD COLUMN IF NOT EXISTS usarpadrao BOOLEAN NOT NULL DEFAULT true;

-- Comentário explicativo
COMMENT ON COLUMN politicas_elegibilidade.usarpadrao IS 'Define se usa busca por padrão (true = LIKE ''%cargo%'') ou match exato (false = cargo exato). Default: true';

-- Atualizar registros existentes para usar padrão (se campo cargo estiver preenchido)
UPDATE politicas_elegibilidade 
SET usarpadrao = true 
WHERE cargo IS NOT NULL AND cargo != '';

-- Criar índice para melhorar performance
CREATE INDEX IF NOT EXISTS idx_politica_usarpadrao ON politicas_elegibilidade(usarpadrao);

-- Verificar resultado
SELECT 
    id,
    cliente,
    tipo_colaborador,
    cargo,
    usarpadrao,
    CASE 
        WHEN usarpadrao = true THEN 'Usa Padrão (LIKE)'
        ELSE 'Match Exato'
    END as tipo_busca
FROM politicas_elegibilidade
WHERE ativo = true
ORDER BY id DESC
LIMIT 10;

-- =====================================================
-- FIM DO SCRIPT
-- =====================================================

