-- =========================================================================
-- Script para Atualizar Template de Inventário Forçado
-- =========================================================================
-- Adiciona o marcador @urlSistema para substituir localhost:4200
-- =========================================================================

UPDATE templates 
SET conteudo = REPLACE(
    conteudo,
    'http://localhost:4200/patrimonio',
    '@urlSistema/patrimonio'
)
WHERE tipo = 6 
  AND ativo = true
  AND conteudo LIKE '%localhost:4200%';

-- Verificar a alteração
SELECT 
    id,
    tipo,
    titulo,
    CASE 
        WHEN conteudo LIKE '%@urlSistema%' THEN '✅ Template atualizado com marcador @urlSistema'
        WHEN conteudo LIKE '%localhost%' THEN '⚠️ Ainda contém localhost'
        ELSE '❓ Verificar manualmente'
    END as status
FROM templates
WHERE tipo = 6;

