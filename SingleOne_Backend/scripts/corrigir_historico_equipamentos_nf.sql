-- ========================================
-- CORREÇÃO: Criar histórico inicial para equipamentos sem histórico
-- ========================================
-- Problema: Equipamentos cadastrados antes da implementação do histórico automático
--           não aparecem na timeline de recursos
-- Solução: Criar registro inicial na tabela equipamentohistorico

BEGIN;

-- 1. Verificar quantos equipamentos estão sem histórico
SELECT 
    'Equipamentos sem histórico' as descricao,
    COUNT(*) as quantidade
FROM equipamentos e
LEFT JOIN equipamentohistorico eh ON e.id = eh.equipamento
WHERE eh.id IS NULL;

-- 2. Criar histórico inicial para equipamentos sem histórico
-- Usar o status atual do equipamento e a data de cadastro
INSERT INTO equipamentohistorico (
    equipamento,
    equipamentostatus,
    usuario,
    dtregistro,
    colaborador,
    requisicao,
    linhatelefonica,
    linhaemuso
)
SELECT 
    e.id as equipamento,
    COALESCE(e.equipamentostatus, 3) as equipamentostatus, -- 3 = Em estoque (fallback)
    COALESCE(e.usuario, 1) as usuario, -- 1 = Admin (fallback)
    COALESCE(e.dtcadastro, NOW()) as dtregistro,
    NULL as colaborador,
    NULL as requisicao,
    NULL as linhatelefonica,
    false as linhaemuso
FROM equipamentos e
LEFT JOIN equipamentohistorico eh ON e.id = eh.equipamento
WHERE eh.id IS NULL
  AND e.ativo = true; -- Apenas equipamentos ativos

-- 3. Verificar resultado
SELECT 
    'Registros de histórico criados' as descricao,
    COUNT(*) as quantidade
FROM equipamentos e
INNER JOIN equipamentohistorico eh ON e.id = eh.equipamento
WHERE e.dtcadastro >= (NOW() - INTERVAL '1 minute'); -- Criados nos últimos minutos

-- 4. Confirmar estatísticas finais
SELECT 
    'Equipamentos com histórico' as descricao,
    COUNT(DISTINCT e.id) as quantidade
FROM equipamentos e
INNER JOIN equipamentohistorico eh ON e.id = eh.equipamento
WHERE e.ativo = true;

SELECT 
    'Equipamentos ainda sem histórico' as descricao,
    COUNT(*) as quantidade
FROM equipamentos e
LEFT JOIN equipamentohistorico eh ON e.id = eh.equipamento
WHERE eh.id IS NULL
  AND e.ativo = true;

COMMIT;

-- ========================================
-- RESULTADO ESPERADO:
-- - Todos os equipamentos ativos devem ter pelo menos 1 registro de histórico
-- - A timeline de recursos deve exibir o histórico completo
-- ========================================

