-- ========================================
-- CORRIGIR VIEWS equipamentovm e EquipamentoVM
-- Remove referência à coluna e.localizacao que não existe mais
-- ========================================

-- Corrigir equipamentovm
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
    e.localidade_id AS localizacaoid,  -- ✅ CORRIGIDO
    CASE 
        WHEN e.localidade_id = 1 THEN 'Nao definido'::character varying  -- ✅ CORRIGIDO
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
LEFT JOIN localidades l ON e.localidade_id = l.id  -- ✅ CORRIGIDO
LEFT JOIN empresas emp ON e.empresa = emp.id
LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
LEFT JOIN empresas emp_cc ON cc.empresa = emp_cc.id
LEFT JOIN contratos con ON e.contrato = con.id
LEFT JOIN filiais fil ON e.filial_id = fil.id
LEFT JOIN tipoaquisicao ta ON e.tipoaquisicao = ta.id
WHERE e.ativo = true;

-- Corrigir EquipamentoVM (mesma estrutura, apenas nome diferente)
DROP VIEW IF EXISTS "EquipamentoVM" CASCADE;

CREATE OR REPLACE VIEW "EquipamentoVM" AS
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
    e.localidade_id AS localizacaoid,  -- ✅ CORRIGIDO
    CASE 
        WHEN e.localidade_id = 1 THEN 'Nao definido'::character varying  -- ✅ CORRIGIDO
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
LEFT JOIN localidades l ON e.localidade_id = l.id  -- ✅ CORRIGIDO
LEFT JOIN empresas emp ON e.empresa = emp.id
LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
LEFT JOIN empresas emp_cc ON cc.empresa = emp_cc.id
LEFT JOIN contratos con ON e.contrato = con.id
LEFT JOIN filiais fil ON e.filial_id = fil.id
LEFT JOIN tipoaquisicao ta ON e.tipoaquisicao = ta.id
WHERE e.ativo = true;

