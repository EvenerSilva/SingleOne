-- ========================================
-- Script para criar views faltantes do dashboard
-- Execute no servidor: sudo -u postgres psql -d singleone -f criar_views_dashboard.sql
-- ========================================

-- ========================================
-- VIEW VWEQUIPAMENTOSSTATUS
-- View para agrupar equipamentos por tipo e status (usado no dashboard)
-- ========================================

DROP VIEW IF EXISTS vwequipamentosstatus CASCADE;

CREATE OR REPLACE VIEW vwequipamentosstatus AS
SELECT 
    e.cliente,
    te.descricao AS tipoequipamento,
    COUNT(CASE WHEN es.id = 1 THEN 1 END) AS novo,
    COUNT(CASE WHEN es.id = 2 THEN 1 END) AS requisitado,
    COUNT(CASE WHEN es.id = 3 THEN 1 END) AS emestoque,
    COUNT(CASE WHEN es.id = 4 THEN 1 END) AS entregue,
    COUNT(CASE WHEN es.id = 5 THEN 1 END) AS roubado,
    COUNT(CASE WHEN es.id = 6 THEN 1 END) AS devolvido,
    COUNT(CASE WHEN es.id = 7 THEN 1 END) AS danificado,
    COUNT(CASE WHEN es.id = 8 THEN 1 END) AS extraviado,
    COUNT(CASE WHEN es.id = 9 THEN 1 END) AS semconserto,
    COUNT(CASE WHEN es.id = 10 THEN 1 END) AS descartado,
    COUNT(CASE WHEN es.id = 11 THEN 1 END) AS migrado
FROM equipamentos e
INNER JOIN tipoequipamentos te ON e.tipoequipamento = te.id
INNER JOIN equipamentosstatus es ON e.equipamentostatus = es.id
WHERE e.ativo = true
GROUP BY e.cliente, te.descricao
ORDER BY e.cliente, te.descricao;

COMMENT ON VIEW vwequipamentosstatus IS 'View para agrupar equipamentos por tipo e status, usado no dashboard';

-- ========================================
-- VIEW VWDEVOLUCAOPROGRAMADUM
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

-- Verificar se as views foram criadas
SELECT 'vwequipamentosstatus' AS view_name, COUNT(*) AS registros FROM vwequipamentosstatus
UNION ALL
SELECT 'vwdevolucaoprogramadum' AS view_name, COUNT(*) AS registros FROM vwdevolucaoprogramadum;

