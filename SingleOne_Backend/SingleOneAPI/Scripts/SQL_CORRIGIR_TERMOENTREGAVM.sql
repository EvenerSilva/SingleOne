-- ========================================
-- CORRIGIR A VIEW TERMOENTREGAVM
-- Remove referência à coluna e.localizacao que não existe mais
-- ========================================

-- 1. VER A DEFINIÇÃO ATUAL DA VIEW (para diagnóstico)
-- Execute este comando primeiro para ver a definição atual:
-- SELECT pg_get_viewdef('termoentregavm'::regclass, true);

-- 2. CRIAR BACKUP DA VIEW ANTIGA (se existir)
DROP VIEW IF EXISTS termoentregavm_backup CASCADE;
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_views WHERE viewname = 'termoentregavm') THEN
        EXECUTE 'CREATE VIEW termoentregavm_backup AS SELECT * FROM termoentregavm';
    END IF;
END $$;

-- 3. RECRIAR A VIEW SEM A COLUNA localizacao
-- A view original provavelmente tinha uma referência a e.localizacao
-- que foi removida da tabela equipamentos. Esta versão corrigida remove essa referência.

DROP VIEW IF EXISTS termoentregavm CASCADE;

CREATE OR REPLACE VIEW termoentregavm AS
SELECT 
    te.descricao AS tipoequipamento,
    f.descricao AS fabricante,
    m.descricao AS modelo,
    e.numeroserie,
    e.patrimonio,
    ri.dtentrega,
    ri.observacaoentrega,
    ri.dtprogramadaretorno,
    r.hashrequisicao,
    r.colaboradorfinal,
    r.cliente,
    CASE 
        WHEN e.tipoaquisicao = 2 THEN 2  -- BYOD
        ELSE 1  -- Não-BYOD
    END AS tipoaquisicao
FROM equipamentos e
INNER JOIN requisicoesitens ri ON e.id = ri.equipamento
INNER JOIN requisicoes r ON ri.requisicao = r.id
INNER JOIN tipoequipamentos te ON e.tipoequipamento = te.id
LEFT JOIN fabricantes f ON e.fabricante = f.id
LEFT JOIN modelos m ON e.modelo = m.id
WHERE r.requisicaostatus = 3  -- Processada
  AND ri.dtdevolucao IS NULL;

-- 4. VERIFICAR SE A VIEW FOI CRIADA CORRETAMENTE
-- SELECT * FROM termoentregavm LIMIT 5;

-- 5. VERIFICAR SE NÃO HÁ ERROS
-- SELECT COUNT(*) AS total_registros FROM termoentregavm;

