-- ========================================
-- VIEWS CORRIGIDAS - REMOÇÃO DA COLUNA localizacao
-- Todas as views foram atualizadas para usar localidade_id em vez de localizacao
-- Data: 2025
-- ========================================

-- ========================================
-- 1. VIEW TERMOENTREGAVM
-- View para listar equipamentos do termo de entrega
-- ========================================

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

COMMENT ON VIEW termoentregavm IS 'View para listar equipamentos do termo de entrega, sem referência à coluna localizacao (removida)';

-- ========================================
-- 2. VIEW VWEQUIPAMENTOSDETALHES
-- View para listar detalhes de equipamentos
-- ========================================

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

COMMENT ON VIEW vwequipamentosdetalhes IS 'View para listar detalhes de equipamentos, usando localidade_id em vez de localizacao';

-- ========================================
-- 3. VIEW VWESTOQUEEQUIPAMENTOSALERTA
-- View para alertas de estoque de equipamentos
-- ========================================

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

COMMENT ON VIEW vwestoqueequipamentosalerta IS 'View para alertas de estoque, usando localidade_id em vez de localizacao';

-- ========================================
-- 4. VIEW EQUIPAMENTOVM
-- View para equipamentos (minúscula)
-- ========================================

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

COMMENT ON VIEW equipamentovm IS 'View para equipamentos, usando localidade_id em vez de localizacao';

-- ========================================
-- 5. VIEW EQUIPAMENTOVM (maiúscula)
-- View para equipamentos (maiúscula - caso diferente)
-- ========================================

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

COMMENT ON VIEW "EquipamentoVM" IS 'View para equipamentos (maiúscula), usando localidade_id em vez de localizacao';

-- ========================================
-- 6. VIEW VWEXPORTACAOEXCEL
-- View para exportação de equipamentos para Excel
-- ========================================

DROP VIEW IF EXISTS vwexportacaoexcel CASCADE;

CREATE OR REPLACE VIEW vwexportacaoexcel AS
SELECT DISTINCT 
    eqp.id,
    c.nome AS colaborador,
    c.cargo,
    te.descricao AS tipoequipamento,
    fab.descricao AS fabricante,
    mdl.descricao AS modelo,
    concat(' ', nf.numero::character varying, nf.descricao) AS notafiscal,
    es.descricao AS equipamentostatus,
    es.id AS equipamentostatusid,
    usu.nome AS usuariocadastro,
    loc.descricao AS localizacao,
    CASE eqp.possuibo
        WHEN false THEN 'Não'::text
        ELSE 'Sim'::text
    END AS possuibo,
    eqp.descricaobo,
    eqp.numeroserie,
    eqp.patrimonio,
    eqp.dtcadastro,
    CASE eqp.tipoaquisicao
        WHEN 1 THEN 'Alugado'::text
        WHEN 2 THEN 'Próprio'::text
        WHEN 3 THEN 'Corporativo'::text
        ELSE 'Não Definido'::text
    END AS tipoaquisicao,
    eqp.cliente,
    eqp.ativo,
    emp.nome AS empresa,
    cc.nome AS centrocusto
FROM equipamentos eqp
JOIN tipoequipamentos te ON eqp.tipoequipamento = te.id
JOIN fabricantes fab ON eqp.fabricante = fab.id
JOIN modelos mdl ON eqp.modelo = mdl.id
JOIN equipamentosstatus es ON eqp.equipamentostatus = es.id
JOIN usuarios usu ON eqp.usuario = usu.id
JOIN localidades loc ON eqp.localidade_id = loc.id  -- ✅ CORRIGIDO: eqp.localizacao -> eqp.localidade_id
LEFT JOIN notasfiscais nf ON eqp.notafiscal = nf.id
LEFT JOIN requisicoesitens ri ON eqp.id = ri.equipamento AND ri.dtdevolucao IS NULL AND ri.dtentrega IS NOT NULL
LEFT JOIN requisicoes r ON ri.requisicao = r.id
LEFT JOIN colaboradores c ON r.colaboradorfinal = c.id
LEFT JOIN empresas emp ON eqp.empresa = emp.id
LEFT JOIN centrocusto cc ON eqp.centrocusto = cc.id
WHERE eqp.id > 1 AND eqp.ativo = true;

COMMENT ON VIEW vwexportacaoexcel IS 'View para exportação de equipamentos para Excel, usando localidade_id em vez de localizacao';
