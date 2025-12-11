-- ========================================
-- Script para corrigir views do dashboard
-- Execute no servidor: sudo -u postgres psql -d singleone -f corrigir_views_dashboard.sql
-- ========================================

-- Primeiro, vamos verificar os IDs reais dos status de equipamentos
DO $$
DECLARE
    status_novo_id INTEGER;
    status_requisitado_id INTEGER;
    status_emestoque_id INTEGER;
    status_entregue_id INTEGER;
    status_roubado_id INTEGER;
    status_devolvido_id INTEGER;
    status_danificado_id INTEGER;
    status_extraviado_id INTEGER;
    status_semconserto_id INTEGER;
    status_descartado_id INTEGER;
    status_migrado_id INTEGER;
BEGIN
    -- Buscar IDs reais dos status
    SELECT id INTO status_novo_id FROM equipamentosstatus WHERE LOWER(descricao) = 'novo' LIMIT 1;
    SELECT id INTO status_requisitado_id FROM equipamentosstatus WHERE LOWER(descricao) = 'requisitado' LIMIT 1;
    SELECT id INTO status_emestoque_id FROM equipamentosstatus WHERE LOWER(descricao) = 'em estoque' LIMIT 1;
    SELECT id INTO status_entregue_id FROM equipamentosstatus WHERE LOWER(descricao) = 'entregue' LIMIT 1;
    SELECT id INTO status_roubado_id FROM equipamentosstatus WHERE LOWER(descricao) = 'roubado' LIMIT 1;
    SELECT id INTO status_devolvido_id FROM equipamentosstatus WHERE LOWER(descricao) = 'devolvido' LIMIT 1;
    SELECT id INTO status_danificado_id FROM equipamentosstatus WHERE LOWER(descricao) = 'danificado' LIMIT 1;
    SELECT id INTO status_extraviado_id FROM equipamentosstatus WHERE LOWER(descricao) = 'extraviado' LIMIT 1;
    SELECT id INTO status_semconserto_id FROM equipamentosstatus WHERE LOWER(descricao) = 'sem conserto' OR LOWER(descricao) = 'semconserto' LIMIT 1;
    SELECT id INTO status_descartado_id FROM equipamentosstatus WHERE LOWER(descricao) = 'descartado' LIMIT 1;
    SELECT id INTO status_migrado_id FROM equipamentosstatus WHERE LOWER(descricao) = 'migrado' LIMIT 1;
    
    RAISE NOTICE 'IDs encontrados: Novo=%, Requisitado=%, EmEstoque=%, Entregue=%, Roubado=%, Devolvido=%, Danificado=%, Extraviado=%, SemConserto=%, Descartado=%, Migrado=%',
        status_novo_id, status_requisitado_id, status_emestoque_id, status_entregue_id, 
        status_roubado_id, status_devolvido_id, status_danificado_id, status_extraviado_id,
        status_semconserto_id, status_descartado_id, status_migrado_id;
END $$;

-- ========================================
-- VIEW VWEQUIPAMENTOSSTATUS (CORRIGIDA)
-- View para agrupar equipamentos por tipo e status (usado no dashboard)
-- Usa os IDs reais dos status em vez de IDs fixos
-- ========================================

DROP VIEW IF EXISTS vwequipamentosstatus CASCADE;

CREATE OR REPLACE VIEW vwequipamentosstatus AS
SELECT 
    e.cliente,
    te.descricao AS tipoequipamento,
    COUNT(CASE WHEN LOWER(es.descricao) = 'novo' THEN 1 END) AS novo,
    COUNT(CASE WHEN LOWER(es.descricao) = 'requisitado' THEN 1 END) AS requisitado,
    COUNT(CASE WHEN LOWER(es.descricao) = 'em estoque' THEN 1 END) AS emestoque,
    COUNT(CASE WHEN LOWER(es.descricao) = 'entregue' THEN 1 END) AS entregue,
    COUNT(CASE WHEN LOWER(es.descricao) = 'roubado' THEN 1 END) AS roubado,
    COUNT(CASE WHEN LOWER(es.descricao) = 'devolvido' THEN 1 END) AS devolvido,
    COUNT(CASE WHEN LOWER(es.descricao) = 'danificado' THEN 1 END) AS danificado,
    COUNT(CASE WHEN LOWER(es.descricao) = 'extraviado' THEN 1 END) AS extraviado,
    COUNT(CASE WHEN LOWER(es.descricao) IN ('sem conserto', 'semconserto') THEN 1 END) AS semconserto,
    COUNT(CASE WHEN LOWER(es.descricao) = 'descartado' THEN 1 END) AS descartado,
    COUNT(CASE WHEN LOWER(es.descricao) = 'migrado' THEN 1 END) AS migrado
FROM equipamentos e
INNER JOIN tipoequipamentos te ON e.tipoequipamento = te.id
INNER JOIN equipamentosstatus es ON e.equipamentostatus = es.id
WHERE e.ativo = true
GROUP BY e.cliente, te.descricao
ORDER BY e.cliente, te.descricao;

COMMENT ON VIEW vwequipamentosstatus IS 'View para agrupar equipamentos por tipo e status, usado no dashboard';

-- ========================================
-- VIEW VWDEVOLUCAOPROGRAMADUM (CORRIGIDA)
-- View para listar devoluções programadas com informações completas
-- ========================================

DROP VIEW IF EXISTS vwdevolucaoprogramadum CASCADE;

CREATE OR REPLACE VIEW vwdevolucaoprogramadum AS
SELECT 
    r.cliente AS cliente,
    c.nome AS nomecolaborador,
    c.matricula AS matricula,
    ri.dtprogramadaretorno AS dtprogramadaretorno,
    CASE 
        WHEN e.id IS NOT NULL THEN 
            CONCAT(
                COALESCE(te.descricao, ''), 
                CASE 
                    WHEN f.descricao IS NOT NULL AND m.descricao IS NOT NULL THEN CONCAT(' - ', f.descricao, ' ', m.descricao)
                    WHEN f.descricao IS NOT NULL THEN CONCAT(' - ', f.descricao)
                    WHEN m.descricao IS NOT NULL THEN CONCAT(' - ', m.descricao)
                    ELSE ''
                END,
                CASE WHEN e.numeroserie IS NOT NULL AND e.numeroserie <> '' THEN CONCAT(' (SN: ', e.numeroserie, ')') ELSE '' END,
                CASE WHEN e.patrimonio IS NOT NULL AND e.patrimonio <> '' THEN CONCAT(' [Pat: ', e.patrimonio, ']') ELSE '' END
            )
        ELSE 'Equipamento não identificado'
    END AS equipamento,
    te.descricao AS tipoequipamento,
    e.numeroserie AS serial,
    e.patrimonio AS patrimonio,
    r.id AS requisicaoid,
    e.id AS equipamentoid,
    c.id AS colaboradorid,
    ri.id AS requisicoesitemid
FROM requisicoesitens ri
INNER JOIN requisicoes r ON ri.requisicao = r.id
INNER JOIN colaboradores c ON r.colaboradorfinal = c.id
LEFT JOIN equipamentos e ON ri.equipamento = e.id
LEFT JOIN tipoequipamentos te ON e.tipoequipamento = te.id
LEFT JOIN fabricantes f ON e.fabricante = f.id
LEFT JOIN modelos m ON e.modelo = m.id
WHERE ri.dtprogramadaretorno IS NOT NULL
    AND ri.dtdevolucao IS NULL
    AND r.colaboradorfinal IS NOT NULL;

COMMENT ON VIEW vwdevolucaoprogramadum IS 'View para listar devoluções programadas com informações completas do equipamento e colaborador';

-- Verificar se as views foram criadas e testar
SELECT 'vwequipamentosstatus' AS view_name, COUNT(*) AS registros FROM vwequipamentosstatus
UNION ALL
SELECT 'vwdevolucaoprogramadum' AS view_name, COUNT(*) AS registros FROM vwdevolucaoprogramadum;

-- Mostrar alguns registros de exemplo
SELECT 'Exemplo vwequipamentosstatus:' AS info;
SELECT * FROM vwequipamentosstatus LIMIT 5;

SELECT 'Exemplo vwdevolucaoprogramadum:' AS info;
SELECT * FROM vwdevolucaoprogramadum LIMIT 5;

