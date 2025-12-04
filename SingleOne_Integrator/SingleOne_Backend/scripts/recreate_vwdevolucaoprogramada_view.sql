-- ========================================
-- Script para recrear a view vwdevolucaoprogramada
-- com informações completas do equipamento
-- PostgreSQL Version
-- ========================================

-- Drop da view existente
DROP VIEW IF EXISTS vwdevolucaoprogramada;

-- Criação da nova view com campos estendidos
CREATE VIEW vwdevolucaoprogramada AS
SELECT 
    r.cliente AS cliente,
    c.nome AS nomecolaborador,
    c.matricula AS matricula,
    ri.dtprogramadaretorno AS dtprogramadaretorno,
    -- Informações do Equipamento
    CASE 
        WHEN e.id IS NOT NULL THEN 
            CONCAT(
                te.descricao, 
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
    -- IDs para navegação
    r.id AS requisicaoid,
    e.id AS equipamentoid,
    c.id AS colaboradorid
FROM 
    requisicoesitens ri
    INNER JOIN requisicoes r ON ri.requisicao = r.id
    INNER JOIN colaboradores c ON r.colaboradorfinal = c.id
    LEFT JOIN equipamentos e ON ri.equipamento = e.id
    LEFT JOIN tipoequipamentos te ON e.tipoequipamento = te.id
    LEFT JOIN fabricantes f ON e.fabricante = f.id
    LEFT JOIN modelos m ON e.modelo = m.id
WHERE 
    ri.dtprogramadaretorno IS NOT NULL
    AND ri.dtdevolucao IS NULL  -- Ainda não foi devolvido
    AND r.colaboradorfinal IS NOT NULL  -- Tem colaborador final
    AND c.situacao = 'A'  -- Colaborador ativo
ORDER BY 
    ri.dtprogramadaretorno ASC;

-- Verificar resultado
SELECT * FROM vwdevolucaoprogramada LIMIT 10;
