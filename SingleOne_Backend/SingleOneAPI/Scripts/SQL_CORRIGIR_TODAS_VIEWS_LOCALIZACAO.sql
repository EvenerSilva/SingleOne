-- ========================================
-- CORRIGIR TODAS AS VIEWS QUE USAM e.localizacao
-- Remove referência à coluna e.localizacao que não existe mais
-- Substitui por e.localidade_id
-- ========================================

-- 1. CORRIGIR vwequipamentosdetalhes
DROP VIEW IF EXISTS vwequipamentosdetalhes CASCADE;

CREATE OR REPLACE VIEW vwequipamentosdetalhes AS
SELECT 
    e.id,
    e.cliente,
    te.id AS tipoequipamentoid,
    te.descricao AS tipoequipamento,
    e.fabricante AS fabricanteid,
    f.descricao AS fabricante,
    m.id AS modeloid,
    m.descricao AS modelo,
    es.id AS equipamentostatusid,
    es.descricao AS equipamentostatus,
    l.id AS localidadeid,
    l.descricao AS localidade,
    e.numeroserie,
    e.patrimonio,
    e.empresa AS empresaid,
    emp.nome AS empresa,
    e.centrocusto AS centrocustoid,
    cc.nome AS centrocusto
FROM equipamentos e
JOIN tipoequipamentos te ON e.tipoequipamento = te.id
JOIN fabricantes f ON e.fabricante = f.id
JOIN modelos m ON e.modelo = m.id
LEFT JOIN localidades l ON e.localidade_id = l.id  -- ✅ CORRIGIDO: e.localizacao -> e.localidade_id
LEFT JOIN empresas emp ON e.empresa = emp.id
LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
JOIN equipamentosstatus es ON e.equipamentostatus = es.id
WHERE te.ativo = true AND e.ativo = true;

-- 2. CORRIGIR vwestoqueequipamentosalerta
DROP VIEW IF EXISTS vwestoqueequipamentosalerta CASCADE;

CREATE OR REPLACE VIEW vwestoqueequipamentosalerta AS
SELECT 
    e.cliente,
    l.descricao AS localidade,
    te.descricao AS tipoequipamento,
    f.descricao AS fabricante,
    m.descricao AS modelo,
    COUNT(CASE WHEN e.equipamentostatus = 3 AND e.ativo = true THEN 1 END) AS estoqueatual,
    eme.quantidademinima AS estoqueminimo,
    CASE 
        WHEN COUNT(CASE WHEN e.equipamentostatus = 3 AND e.ativo = true THEN 1 END) < eme.quantidademinima THEN 'ALERTA'
        ELSE 'OK'
    END AS status,
    CASE 
        WHEN COUNT(CASE WHEN e.equipamentostatus = 3 AND e.ativo = true THEN 1 END) < eme.quantidademinima 
        THEN eme.quantidademinima - COUNT(CASE WHEN e.equipamentostatus = 3 AND e.ativo = true THEN 1 END)
        ELSE 0
    END AS quantidadefaltante
FROM equipamentos e
JOIN modelos m ON e.modelo = m.id
JOIN fabricantes f ON e.fabricante = f.id
JOIN tipoequipamentos te ON e.tipoequipamento = te.id
JOIN localidades l ON e.localidade_id = l.id  -- ✅ CORRIGIDO: e.localizacao -> e.localidade_id
JOIN estoqueminimoequipamentos eme ON e.modelo = eme.modelo AND e.localidade_id = eme.localidade AND e.cliente = eme.cliente  -- ✅ CORRIGIDO: e.localizacao -> e.localidade_id
WHERE eme.ativo = true
GROUP BY e.cliente, l.descricao, te.descricao, f.descricao, m.descricao, eme.quantidademinima;

-- 3. CORRIGIR equipamentovm
DROP VIEW IF EXISTS equipamentovm CASCADE;

CREATE OR REPLACE VIEW equipamentovm AS
SELECT 
    e.id,
    e.tipoequipamento AS tipoequipamentoid,
    COALESCE(te.descricao, 'Nao definido'::character varying) AS tipoequipamento,
    e.fabricante AS fabricanteid,
    COALESCE(f.descricao, 'Nao definido'::character varying) AS fabricante,
    e.modelo AS modeloid,
    COALESCE(m.descricao, 'Nao definido'::character varying) AS modelo,
    e.notafiscal AS notafiscalid,
    CASE 
        WHEN e.notafiscal IS NOT NULL THEN nf.numero::character varying
        ELSE 'Nao definido'::character varying
    END AS "Notafiscal",
    e.equipamentostatus AS equipamentostatusid,
    COALESCE(es.descricao, 'Nao definido'::character varying) AS equipamentostatus,
    e.usuario AS usuarioid,
    COALESCE(u.nome, 'Nao definido'::character varying) AS usuario,
    e.localidade_id AS localizacaoid,  -- ✅ CORRIGIDO: Removido COALESCE com e.localizacao
    CASE 
        WHEN e.localidade_id = 1 THEN 'Nao definido'::character varying  -- ✅ CORRIGIDO: Removido COALESCE
        ELSE COALESCE(l.descricao, 'Nao definido'::character varying)
    END AS localizacao,
    e.possuibo,
    e.descricaobo,
    e.numeroserie,
    e.patrimonio,
    e.dtlimitegarantia,
    e.dtcadastro,
    e.tipoaquisicao,
    COALESCE(ta.nome, 'Nao definido'::character varying) AS "TipoAquisicao",
    e.fornecedor,
    CASE 
        WHEN e.fornecedor IS NOT NULL THEN forn.nome
        ELSE 'Nao definido'::character varying
    END AS "FornecedorNome",
    e.cliente,
    NULL::text AS colaboradorid,
    NULL::text AS colaboradornome,
    NULL::text AS requisicaoid,
    e.ativo,
    COALESCE(e.empresa, cc.empresa) AS empresaid,
    COALESCE(emp.nome, emp_cc.nome, 'Nao definido'::character varying) AS empresa,
    e.centrocusto AS centrocustoid,
    COALESCE(cc.nome, 'Nao definido'::character varying) AS centrocusto,
    e.contrato AS contratoid,
    COALESCE(con.descricao, 'Nao definido'::character varying) AS contrato,
    e.filial_id AS "Filialid",
    COALESCE(fil.nome, 'Nao definido'::character varying) AS "Filial"
FROM equipamentos e
LEFT JOIN tipoequipamentos te ON e.tipoequipamento = te.id
LEFT JOIN fabricantes f ON e.fabricante = f.id
LEFT JOIN modelos m ON e.modelo = m.id
LEFT JOIN notasfiscais nf ON e.notafiscal = nf.id
LEFT JOIN fornecedores forn ON e.fornecedor = forn.id
LEFT JOIN equipamentosstatus es ON e.equipamentostatus = es.id
LEFT JOIN usuarios u ON e.usuario = u.id
LEFT JOIN localidades l ON e.localidade_id = l.id  -- ✅ CORRIGIDO: Removido COALESCE com e.localizacao
LEFT JOIN empresas emp ON e.empresa = emp.id
LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
LEFT JOIN empresas emp_cc ON cc.empresa = emp_cc.id
LEFT JOIN contratos con ON e.contrato = con.id
LEFT JOIN filiais fil ON e.filial_id = fil.id
LEFT JOIN tipoaquisicao ta ON e.tipoaquisicao = ta.id
WHERE e.ativo = true;

-- 4. CORRIGIR EquipamentoVM (se for diferente de equipamentovm)
-- Nota: Verifique se EquipamentoVM é uma view separada ou apenas um alias

-- Verificar se as views foram criadas corretamente
SELECT 'vwequipamentosdetalhes' AS viewname, COUNT(*) AS total FROM vwequipamentosdetalhes
UNION ALL
SELECT 'vwestoqueequipamentosalerta' AS viewname, COUNT(*) AS total FROM vwestoqueequipamentosalerta;
