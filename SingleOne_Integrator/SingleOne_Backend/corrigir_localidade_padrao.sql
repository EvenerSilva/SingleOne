-- Corrigir localidade padrão na view
-- ================================================

-- 1. Dropar a view atual
DROP VIEW IF EXISTS equipamentovm;

-- 2. Recriar a view sem mostrar "Padrão"
CREATE VIEW equipamentovm AS
SELECT 
    e.id,
    e.tipoequipamento AS tipoequipamentoid,
    COALESCE(te.descricao, 'Não definido') AS tipoequipamento,
    e.fabricante AS fabricanteid,
    COALESCE(f.descricao, 'Não definido') AS fabricante,
    e.modelo AS modeloid,
    COALESCE(m.descricao, 'Não definido') AS modelo,
    e.notafiscal AS notafiscalid,
    e.equipamentostatus AS equipamentostatusid,
    COALESCE(es.descricao, 'Não definido') AS equipamentostatus,
    e.usuario AS usuarioid,
    COALESCE(u.nome, 'Não definido') AS usuario,
    e.localizacao AS localizacaoid,
    CASE 
        WHEN l.descricao = 'Padrão' THEN 'Não definido'
        ELSE COALESCE(l.descricao, 'Não definido')
    END AS localizacao,
    e.possuibo,
    e.descricaobo,
    e.numeroserie,
    e.patrimonio,
    e.dtlimitegarantia,
    e.dtcadastro,
    e.tipoaquisicao,
    e.fornecedor,
    e.cliente,
    NULL::text AS colaboradorid,
    NULL::text AS colaboradornome,
    NULL::text AS requisicaoid,
    e.ativo,
    e.empresa AS empresaid,
    COALESCE(emp.nome, 'Não definido') AS empresa,
    e.centrocusto AS centrocustoid,
    COALESCE(cc.nome, 'Não definido') AS centrocusto,
    e.contrato AS contratoid,
    'Não definido' AS contrato
FROM equipamentos e
LEFT JOIN tipoequipamentos te ON e.tipoequipamento = te.id
LEFT JOIN fabricantes f ON e.fabricante = f.id
LEFT JOIN modelos m ON e.modelo = m.id
LEFT JOIN equipamentosstatus es ON e.equipamentostatus = es.id
LEFT JOIN usuarios u ON e.usuario = u.id
LEFT JOIN localidades l ON e.localizacao = l.id
LEFT JOIN empresas emp ON e.empresa = emp.id
LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
WHERE e.ativo = true;

-- 3. Verificar se a correção funcionou
SELECT 
    id,
    numeroserie,
    patrimonio,
    localizacao,
    localizacaoid,
    empresa,
    centrocusto
FROM equipamentovm 
WHERE id = 1811;
