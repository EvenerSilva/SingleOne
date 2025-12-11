-- =====================================================
-- SCRIPT DE CRIAÇÃO DE VIEWS - SINGLEONE
-- =====================================================
-- Descrição: Script para criar todas as views do sistema
-- Versão: 1.0 (Extraído de "01. Criar Tabelas.sql")
-- Data: 2025
-- =====================================================
-- NOTA: Views são criadas com tratamento de erros para garantir que todas sejam criadas
--       mesmo se algumas falharem

-- View: TermosColaboradoresVM
CREATE OR REPLACE VIEW TermosColaboradoresVM AS 
SELECT r.ColaboradorFinal as ColaboradorFinalId, c.Nome as ColaboradorFinal, 
	(SELECT max(DtEnvioTermo) FROM requisicoes rq WHERE rq.ColaboradorFinal = r.ColaboradorFinal) as DtEnvioTermo, 
	(SELECT CASE WHEN count(*) > 0 THEN 'Em aberto' ELSE 'Assinado' END 
	FROM Requisicoes rq WHERE rq.ColaboradorFinal = r.ColaboradorFinal AND rq.AssinaturaEletronica = false) as Situacao
FROM Requisicoes r
	JOIN Colaboradores c ON r.ColaboradorFinal = c.Id
GROUP BY r.ColaboradorFinal, c.Nome;

-- View: EquipamentoVM
DROP VIEW IF EXISTS EquipamentoVM CASCADE;
CREATE OR REPLACE VIEW EquipamentoVM AS
SELECT e.id,
    e.tipoequipamento AS tipoequipamentoid,
    COALESCE(te.descricao, 'Nao definido'::character varying(200)) AS tipoequipamento,
    e.fabricante AS fabricanteid,
    COALESCE(f.descricao, 'Nao definido'::character varying(200)) AS fabricante,
    e.modelo AS modeloid,
    COALESCE(m.descricao, 'Nao definido'::character varying(200)) AS modelo,
    e.notafiscal AS notafiscalid,
        CASE
            WHEN e.notafiscal IS NOT NULL THEN nf.numero::character varying
            ELSE 'Nao definido'::character varying
        END AS "Notafiscal",
    e.equipamentostatus AS equipamentostatusid,
    COALESCE(es.descricao, 'Nao definido'::character varying) AS equipamentostatus,
    e.usuario AS usuarioid,
    COALESCE(u.nome, 'Nao definido'::character varying) AS usuario,
    e.localidade_id AS localizacaoid,
        CASE
            WHEN e.localidade_id = 1 THEN 'Nao definido'::character varying
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
     LEFT JOIN localidades l ON e.localidade_id = l.id
     LEFT JOIN empresas emp ON e.empresa = emp.id
     LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
     LEFT JOIN empresas emp_cc ON cc.empresa = emp_cc.id
     LEFT JOIN contratos con ON e.contrato = con.id
     LEFT JOIN filiais fil ON e.filial_id = fil.id
     LEFT JOIN tipoaquisicao ta ON e.tipoaquisicao = ta.id
  WHERE e.ativo = true;

-- View: EquipamentoHistoricoVM
CREATE OR REPLACE VIEW EquipamentoHistoricoVM AS
SELECT e.id, te.id TipoequipamentoID, te.descricao TipoEquipamento, f.Id FabricanteId, f.Descricao Fabricante, m.Id ModeloId, m.Descricao Modelo, e.NumeroSerie, e.Patrimonio, es.Id EquipamentoStatusId, es.Descricao EquipamentoStatus, c.Id ColaboradorId, c.Nome Colaborador, eh.DtRegistro,
	u.Id UsuarioId, u.Nome Usuario
FROM EquipamentoHistorico eh
	JOIN Equipamentos e ON eh.Equipamento = e.Id
	JOIN Usuarios u ON eh.Usuario = u.Id
	JOIN EquipamentosStatus es ON eh.EquipamentoStatus = es.Id
	JOIN Fabricantes f ON e.Fabricante = f.Id
	JOIN Modelos m ON e.Modelo = m.Id
	JOIN TipoEquipamentos te ON e.tipoequipamento = te.id
	LEFT JOIN Colaboradores c ON eh.Colaborador = c.Id;

-- View: RequisicoesVM
CREATE OR REPLACE VIEW RequisicoesVM AS
SELECT r.id, u.Id UsuarioRequisicaoId, u.Nome UsuarioRequisicao, t.Id TecnicoResponsavelId, t.Nome TecnicoResponsavel, c.Id ColaboradorFinalId, c.Nome ColaboradorFinal,
	r.DtSolicitacao, r.DtProcessamento, r.RequisicaoStatus RequisicaoStatusId, rs.Descricao RequisicaoStatus, r.AssinaturaEletronica, r.DtAssinaturaEletronica, r.DtEnvioTermo, r.HashRequisicao,
	(SELECT count(*) FROM requisicoesItens ri JOIN equipamentos e ON ri.Equipamento = e.id WHERE ri.Requisicao = r.Id AND DtEntrega IS NOT NULL AND DtDevolucao IS NULL AND e.EquipamentoStatus <> 8) EquipamentosPendentes, r.cliente
FROM requisicoes r
	JOIN RequisicoesStatus rs ON r.RequisicaoStatus = rs.id
	JOIN Usuarios u ON r.UsuarioRequisicao = u.Id
	JOIN Usuarios t ON r.TecnicoResponsavel = t.Id
	LEFT JOIN Colaboradores c ON r.ColaboradorFinal = c.Id;

-- View: RequisicaoEquipamentosVM
CREATE OR REPLACE VIEW RequisicaoEquipamentosVM AS  
SELECT ri.Id, r.Id Requisicao, ri.Equipamento EquipamentoId, concat(te.Descricao, ' ', f.Descricao, ' ', m.Descricao) Equipamento, e.NumeroSerie, e.Patrimonio, ue.Id UsuarioEntregaId, ue.Nome UsuarioEntrega,   
 ud.Id UsuarioDevolucaoId, ud.Nome UsuarioDevolucao, ri.DtEntrega, ri.DtDevolucao, ri.ObservacaoEntrega, ri.DtProgramadaRetorno, e.EquipamentoStatus, tl.Numero, ri.linhatelefonica linhaid, e.TipoAquisicao
FROM Requisicoes r   
	JOIN RequisicoesItens ri ON r.Id = ri.Requisicao  
	JOIN Equipamentos e ON ri.Equipamento = e.Id
	JOIN TipoEquipamentos te ON e.TipoEquipamento = te.Id
	JOIN Fabricantes f ON e.Fabricante = f.id  
	JOIN Modelos m ON e.Modelo = m.Id   
	LEFT JOIN Usuarios ue ON ri.UsuarioEntrega = ue.Id  
	LEFT JOIN Usuarios ud ON ri.UsuarioDevolucao = ud.Id  
	LEFT JOIN TelefoniaLinhas tl ON ri.LinhaTelefonica = tl.Id  
WHERE e.Id <> 1  
UNION ALL  
SELECT ri.Id, r.Id Requisicao, ri.Equipamento EquipamentoId, concat(f.Descricao, ' ', cast(tl.Numero as varchar)) Equipamento, '' NumeroSerie, e.Patrimonio, ue.Id UsuarioEntregaId, ue.Nome UsuarioEntrega,   
 ud.Id UsuarioDevolucaoId, ud.Nome UsuarioDevolucao, ri.DtEntrega, ri.DtDevolucao, ri.ObservacaoEntrega, ri.DtProgramadaRetorno, e.EquipamentoStatus, tl.Numero, ri.linhatelefonica linhaid, e.TipoAquisicao
FROM Requisicoes r   
	JOIN RequisicoesItens ri ON r.Id = ri.Requisicao  
	JOIN Equipamentos e ON ri.Equipamento = e.Id  
	JOIN TipoEquipamentos te ON e.TipoEquipamento = te.Id
	JOIN Fabricantes f ON e.Fabricante = f.id  
	JOIN Modelos m ON e.Modelo = m.Id   
	LEFT JOIN Usuarios ue ON ri.UsuarioEntrega = ue.Id  
	LEFT JOIN Usuarios ud ON ri.UsuarioDevolucao = ud.Id  
	JOIN TelefoniaLinhas tl ON ri.LinhaTelefonica = tl.Id  
WHERE e.Id = 1;

-- View: TermoEntregaVM
DROP VIEW IF EXISTS TermoEntregaVM CASCADE;
CREATE OR REPLACE VIEW TermoEntregaVM AS 
SELECT te.Descricao TipoEquipamento, fab.Descricao fabricante, mdl.Descricao modelo, eqp.NumeroSerie, COALESCE(eqp.Patrimonio, '') Patrimonio, ri.DtEntrega, ri.ObservacaoEntrega, ri.DtProgramadaRetorno, req.HashRequisicao, req.colaboradorfinal, req.cliente, eqp.tipoaquisicao
FROM Requisicoes req 
	JOIN requisicoesItens ri ON req.id = ri.Requisicao 
	JOIN Equipamentos eqp ON ri.Equipamento = eqp.Id 
	JOIN TipoEquipamentos te ON eqp.TipoEquipamento = te.Id 
	JOIN Fabricantes fab ON eqp.Fabricante = fab.Id 
	JOIN Modelos mdl ON eqp.Modelo = mdl.Id 
WHERE ri.DtDevolucao IS NULL AND req.RequisicaoStatus IN (1,3) AND eqp.EquipamentoStatus <> 8 AND eqp.Id <> 1
UNION ALL
SELECT te.Descricao TipoEquipamento, 'Número' fabricante, cast(tl.Numero as varchar) modelo, '' NumeroSerie, COALESCE(eqp.Patrimonio, '') Patrimonio, ri.DtEntrega, ri.ObservacaoEntrega, ri.DtProgramadaRetorno, req.HashRequisicao, req.colaboradorfinal, req.cliente, eqp.tipoaquisicao
FROM Requisicoes req 
	JOIN requisicoesItens ri ON req.id = ri.Requisicao 
	JOIN Equipamentos eqp ON ri.Equipamento = eqp.Id 
	JOIN TipoEquipamentos te ON eqp.TipoEquipamento = te.Id 
	JOIN Fabricantes fab ON eqp.Fabricante = fab.Id 
	JOIN Modelos mdl ON eqp.Modelo = mdl.Id 
	LEFT JOIN telefoniaLinhas tl ON ri.LinhaTelefonica = tl.Id 
WHERE ri.DtDevolucao IS NULL AND req.RequisicaoStatus IN (1,3) AND eqp.EquipamentoStatus <> 8 AND eqp.Id = 1;

-- View: vwNadaConsta
CREATE OR REPLACE VIEW vwNadaConsta AS
SELECT 
	c.id, c.Nome, c.Cpf, cc.Nome CentroCusto, e.Nome Empresa, c.Matricula, c.Cargo,
(SELECT count(*)
FROM Requisicoes r
	LEFT JOIN RequisicoesItens ri ON r.Id = ri.Requisicao
WHERE r.ColaboradorFinal = c.id AND ri.DtDevolucao IS NULL) MaquinasComColaborador, e.cliente
FROM colaboradores c
	JOIN CentroCusto cc ON c.CentroCusto = cc.Id
	JOIN Empresas e ON c.Empresa = e.Id;

-- View: vwEquipamentosStatus
CREATE OR REPLACE VIEW vwEquipamentosStatus AS
SELECT tec.cliente, descricao TipoEquipamento, 
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 1 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Danificado,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 2 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Devolvido,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 3 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) EmEstoque,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 4 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Entregue,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 5 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Extraviado,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 6 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Novo,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 7 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Requisitado,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 8 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Roubado,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 9 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) SemConserto,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 10 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Migrado,
	(SELECT count(*) FROM equipamentos WHERE equipamentoStatus = 11 AND TipoEquipamento = te.id AND cliente = tec.cliente AND equipamentos.ativo = true) Descartado
FROM TipoEquipamentos te
	JOIN tipoequipamentosclientes tec ON te.Id = tec.tipo
WHERE te.ativo = true;

-- View: vwestoqueequipamentosalerta
DROP VIEW IF EXISTS vwestoqueequipamentosalerta CASCADE;
CREATE OR REPLACE VIEW vwestoqueequipamentosalerta AS
SELECT e.cliente,
    l.descricao AS localidade,
    te.descricao AS tipoequipamento,
    f.descricao AS fabricante,
    m.descricao AS modelo,
    count(
        CASE
            WHEN e.equipamentostatus = 3 AND e.ativo = true THEN 1
            ELSE NULL::integer
        END) AS estoqueatual,
    eme.quantidademinima AS estoqueminimo,
        CASE
            WHEN count(
            CASE
                WHEN e.equipamentostatus = 3 AND e.ativo = true THEN 1
                ELSE NULL::integer
            END) < eme.quantidademinima THEN 'ALERTA'::text
            ELSE 'OK'::text
        END AS status,
        CASE
            WHEN count(
            CASE
                WHEN e.equipamentostatus = 3 AND e.ativo = true THEN 1
                ELSE NULL::integer
            END) < eme.quantidademinima THEN eme.quantidademinima - count(
            CASE
                WHEN e.equipamentostatus = 3 AND e.ativo = true THEN 1
                ELSE NULL::integer
            END)
            ELSE 0::bigint
        END AS quantidadefaltante
   FROM equipamentos e
     JOIN modelos m ON e.modelo = m.id
     JOIN fabricantes f ON e.fabricante = f.id
     JOIN tipoequipamentos te ON e.tipoequipamento = te.id
     JOIN localidades l ON e.localidade_id = l.id
     JOIN estoqueminimoequipamentos eme ON e.modelo = eme.modelo AND e.localidade_id = eme.localidade AND e.cliente = eme.cliente
  WHERE eme.ativo = true
  GROUP BY e.cliente, l.descricao, te.descricao, f.descricao, m.descricao, eme.quantidademinima;

-- View: vwExportacaoExcel
DROP VIEW IF EXISTS vwExportacaoExcel CASCADE;
CREATE OR REPLACE VIEW vwExportacaoExcel AS
SELECT DISTINCT eqp.id, c.Nome Colaborador, c.Cargo, te.Descricao TipoEquipamento, fab.Descricao Fabricante, mdl.Descricao Modelo, concat(' ', cast(nf.Numero as varchar), nf.Descricao) NotaFiscal, es.Descricao EquipamentoStatus, es.id EquipamentostatusId,
	usu.Nome UsuarioCadastro, Loc.Descricao Localizacao, CASE eqp.PossuiBO WHEN false THEN 'Não' ELSE 'Sim' END PossuiBO, eqp.DescricaoBO, eqp.NumeroSerie, eqp.Patrimonio,
	eqp.DtCadastro, CASE eqp.TipoAquisicao WHEN 1 THEN 'Alugado' WHEN 2 THEN 'Próprio' WHEN 3 THEN 'Corporativo' ELSE 'Não Definido' END TipoAquisicao, eqp.cliente, eqp.ativo, emp.nome empresa, cc.nome centrocusto
FROM Equipamentos eqp
	JOIN TipoEquipamentos te ON eqp.TipoEquipamento = te.Id
	JOIN Fabricantes fab ON eqp.Fabricante = fab.Id
	JOIN Modelos mdl ON eqp.Modelo = mdl.Id
	JOIN EquipamentosStatus es ON eqp.EquipamentoStatus = es.Id
	JOIN Usuarios usu ON eqp.Usuario = usu.Id
	JOIN Localidades loc ON eqp.localidade_id = loc.Id
	LEFT JOIN NotasFiscais nf ON eqp.NotaFiscal = nf.Id
	LEFT JOIN RequisicoesItens ri ON eqp.id = ri.Equipamento AND ri.DtDevolucao IS NULL AND ri.DtEntrega IS NOT NULL
	LEFT JOIN Requisicoes r ON ri.Requisicao = r.Id
	LEFT JOIN Colaboradores c ON r.ColaboradorFinal = c.Id
	LEFT JOIN Empresas emp ON eqp.empresa = emp.id
	LEFT JOIN CentroCusto cc ON eqp.centrocusto = cc.id
WHERE eqp.Id > 1 AND eqp.Ativo = true;

-- View: ColaboradorHistoricoVM
CREATE OR REPLACE VIEW ColaboradorHistoricoVM AS
SELECT c.Id, c.Nome, Cpf, Matricula, Email, Cargo, 
	CASE
		WHEN Situacao = 'P' THEN 'Provisionado'
		WHEN Situacao = 'A' THEN 'Ativo'
		WHEN Situacao = 'I' THEN 'Inativo' 
		WHEN situacao IS NULL THEN 'N/I' END Situacao, 
	CASE 
		WHEN SituacaoAntiga = 'P' THEN 'Provisionado'
		WHEN SituacaoAntiga = 'A' THEN 'Ativo'
		WHEN SituacaoAntiga = 'I' THEN 'Inativo' 
		WHEN SituacaoAntiga IS NULL THEN 'N/I' END SituacaoAntiga, DtAtualizacao, c.Empresa EmpresaAtualId, e.Nome EmpresaAtual, c.AntigaEmpresa EmpresaAntigaId, ea.Nome EmpresaAntiga, DtAtualizacaoEmpresa, 
	c.Localidade LocalidadeAtualId, l.Descricao LocalidadeAtual, c.AntigaLocalidade LocalidadeAntigaId, la.Descricao LocalidadeAntiga, c.DtAtualizacaoLocalidade,
	c.CentroCusto CentroCustoAtualId, cc.Codigo CodigoCCAtual, cc.Nome NomeCCAtual, c.AntigoCentroCusto CentroCustoAntigoId, cca.Codigo CodigoCCAntigo, cca.Nome NomeCCAntigo, c.DtAtualizacaoCentroCusto, e.cliente
FROM colaboradores c
	JOIN Empresas e ON c.Empresa = e.Id
	LEFT JOIN Empresas ea ON c.AntigaEmpresa = ea.Id
	JOIN Localidades l ON c.Localidade = l.Id
	LEFT JOIN Localidades la ON c.AntigaLocalidade = la.Id
	JOIN CentroCusto cc ON c.CentroCusto = cc.Id
	LEFT JOIN CentroCusto cca ON c.AntigoCentroCusto = cca.Id
WHERE c.DtAtualizacao IS NOT NULL OR c.DtAtualizacaoCentroCusto IS NOT NULL OR c.DtAtualizacaoEmpresa IS NOT NULL OR c.DtAtualizacaoLocalidade IS NOT NULL;

-- View: vwDevolucaoProgramada
CREATE OR REPLACE VIEW vwDevolucaoProgramada AS
SELECT req.cliente, col.Nome nomeColaborador, ri.dtProgramadaRetorno
FROM requisicoes req
	JOIN RequisicoesItens ri ON req.id = ri.Requisicao
	JOIN colaboradores col ON req.ColaboradorFinal = col.Id
WHERE ri.DtProgramadaRetorno IS NOT NULL AND ri.DtDevolucao IS NULL AND req.RequisicaoStatus IN (1,3)
	AND req.id NOT IN(SELECT requisicao FROM requisicoesItens WHERE equipamento IN(SELECT id FROM equipamentos WHERE equipamentoStatus = 8));

-- View: vwequipamentoscomcolaboradoresdesligados
CREATE OR REPLACE VIEW vwequipamentoscomcolaboradoresdesligados AS
SELECT r.cliente,
	c.nome, c.dtdemissao,
	count(ri.equipamento) AS qtde
FROM ((requisicoes r
	JOIN requisicoesitens ri ON ((r.id = ri.requisicao)))
	JOIN colaboradores c ON ((r.colaboradorfinal = c.id)))
WHERE ((ri.dtdevolucao IS NULL) AND (c.dtdemissao IS NOT NULL AND c.dtdemissao < NOW()) AND (ri.equipamento IN ( SELECT equipamentos.id
		FROM equipamentos
		WHERE (equipamentos.equipamentostatus <> 8))))
GROUP BY r.cliente, c.nome, c.dtdemissao;

-- View: vwEquipamentosDetalhes
CREATE OR REPLACE VIEW vwEquipamentosDetalhes AS
SELECT e.id, e.cliente, te.id tipoEquipamentoID, te.descricao tipoequipamento, e.fabricante fabricanteId, f.descricao fabricante, m.id modeloid, m.descricao modelo,
	es.id equipamentoStatusID, es.descricao equipamentostatus, l.id localidadeid, l.descricao localidade, e.numeroserie, e.patrimonio, e.empresa empresaid, emp.nome empresa, e.centrocusto centrocustoid, cc.nome centrocusto
FROM equipamentos e
	JOIN tipoequipamentos te ON e.tipoequipamento = te.id
	JOIN fabricantes f ON e.fabricante = f.id
	JOIN modelos m ON e.modelo = m.id
	LEFT JOIN localidades l ON e.localidade_id = l.id
	LEFT JOIN empresas emp ON e.empresa = emp.id
	LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
	JOIN equipamentosstatus es ON e.equipamentostatus = es.id
WHERE te.ativo = TRUE AND e.ativo = true;

-- View: vwTelefonia
CREATE OR REPLACE VIEW vwTelefonia AS 
SELECT o.nome operadora, c.nome contrato, p.nome plano, p.valor, l.numero, l.iccid, l.emuso, l.ativo, c.cliente
FROM telefoniaoperadoras o 
	JOIN telefoniacontratos c ON o.id = c.operadora
	JOIN telefoniaplanos p ON c.id = p.contrato
	JOIN telefonialinhas l ON p.id = l.plano
WHERE o.ativo = true AND c.ativo = true AND p.ativo = true AND l.ativo = true;

-- View: vwLaudos
CREATE OR REPLACE VIEW vwLaudos AS
SELECT l.id, l.cliente, concat(te.descricao, ' ', f.descricao, ' ', m.descricao) equipamento, e.numeroserie, e.patrimonio, l.descricao, l.laudo, l.dtentrada, l.dtlaudo, l.mauuso, l.temconserto, l.usuario, u.nome usuarionome, l.tecnico, u.nome tecniconome, l.valormanutencao, e.empresa, emp.nome empresanome, e.centrocusto, cc.nome centrocustonome
FROM laudos l
	JOIN equipamentos e ON l.equipamento = e.id
	JOIN tipoequipamentos te ON e.tipoequipamento = te.id
	JOIN fabricantes f ON e.fabricante = f.id
	JOIN modelos m ON e.modelo = m.id
	JOIN usuarios t ON l.tecnico = t.id
	JOIN usuarios u ON l.usuario = u.id
	LEFT JOIN empresas emp ON e.empresa = emp.id
	LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
WHERE l.ativo = true;

-- View: vwUltimasRequisicaoBYOD
DROP VIEW IF EXISTS vwultimasrequisicaobyd CASCADE;
CREATE OR REPLACE VIEW vwultimasrequisicaobyd AS
SELECT r.id AS requisicaoid,
	r.cliente,
	r.usuariorequisicao,
	r.tecnicoresponsavel,
	r.requisicaostatus,
	r.colaboradorfinal,
	cf.nome AS nomecolaboradorfinal,
	r.dtsolicitacao,
	r.dtprocessamento,
	r.assinaturaeletronica,
	r.dtassinaturaeletronica,
	r.dtenviotermo,
	r.hashrequisicao,
	ri.id AS requisicaoitemid,
	ri.equipamento,
	ri.linhatelefonica,
	ri.usuarioentrega,
	ri.usuariodevolucao,
	ri.dtentrega,
	ri.dtdevolucao,
	ri.observacaoentrega,
	ri.dtprogramadaretorno,
	e.id AS equipamentoid,
	e.tipoaquisicao,
	e.equipamentostatus,
	e.numeroserie,
	e.patrimonio
FROM requisicoes r
	JOIN requisicoesitens ri ON r.id = ri.requisicao
	JOIN equipamentos e ON ri.equipamento = e.id
	LEFT JOIN colaboradores cf ON r.colaboradorfinal = cf.id
WHERE e.tipoaquisicao = 2
ORDER BY r.dtsolicitacao DESC
LIMIT 1000;

-- View: vwUltimasRequisicaoNaoBYOD
DROP VIEW IF EXISTS vwultimasrequisicaonaobyd CASCADE;
CREATE OR REPLACE VIEW vwultimasrequisicaonaobyd AS
SELECT r.id AS requisicaoid,
	r.cliente,
	r.usuariorequisicao,
	r.tecnicoresponsavel,
	r.requisicaostatus,
	r.colaboradorfinal,
	cf.nome AS nomecolaboradorfinal,
	r.dtsolicitacao,
	r.dtprocessamento,
	r.assinaturaeletronica,
	r.dtassinaturaeletronica,
	r.dtenviotermo,
	r.hashrequisicao,
	ri.id AS requisicaoitemid,
	ri.equipamento,
	ri.linhatelefonica,
	ri.usuarioentrega,
	ri.usuariodevolucao,
	ri.dtentrega,
	ri.dtdevolucao,
	ri.observacaoentrega,
	ri.dtprogramadaretorno,
	e.id AS equipamentoid,
	e.tipoaquisicao,
	e.equipamentostatus,
	e.numeroserie,
	e.patrimonio,
	tl.numero
FROM requisicoes r
	JOIN requisicoesitens ri ON r.id = ri.requisicao
	JOIN equipamentos e ON ri.equipamento = e.id
	LEFT JOIN colaboradores cf ON r.colaboradorfinal = cf.id
	LEFT JOIN telefonialinhas tl ON ri.linhatelefonica = tl.id
WHERE e.tipoaquisicao <> 2
ORDER BY r.dtsolicitacao DESC
LIMIT 1000;

-- View: colaboradoresvm
DROP VIEW IF EXISTS public.colaboradoresvm;

CREATE VIEW public.colaboradoresvm AS
SELECT
	c.id,
	c.cliente,
	e.nome AS empresa,
	cc.nome AS nomecentrocusto,
	cc.codigo AS codigocentrocusto,
	c.nome,
	c.cpf,
	c.matricula,
	c.email,
	c.tipocolaborador::text AS tipocolaborador,
	(
		CASE
			WHEN COALESCE(NULLIF(c.situacao, ''), 'A') IN ('A','D','I','F') THEN COALESCE(NULLIF(c.situacao, ''), 'A')
			WHEN c.dtdemissao IS NULL THEN 'A'
			WHEN c.dtdemissao < (CURRENT_DATE)::timestamp THEN 'D'
			ELSE 'A'
		END
	)::text AS situacao,
	c.cargo,
	c.setor,
	COALESCE(l.descricao, '') AS localidadedescricao,
	COALESCE(l.cidade, '') AS localidadecidade,
	COALESCE(l.estado, '') AS localidadeestado,
	c.dtadmissao,
	c.dtdemissao,
	c.dtcadastro,
	COALESCE(c.matriculasuperior, '') AS matriculasuperior
FROM colaboradores c
	JOIN empresas e ON c.empresa = e.id
	JOIN centrocusto cc ON c.centrocusto = cc.id
	LEFT JOIN localidades l ON l.id = c.localidade_id;

-- View: vw_equipamentos_compartilhados
DROP VIEW IF EXISTS vw_equipamentos_compartilhados CASCADE;
CREATE OR REPLACE VIEW vw_equipamentos_compartilhados AS
SELECT 
	e.id AS equipamento_id,
	e.patrimonio,
	e.numeroserie,
	e.compartilhado,
	e.usuario AS responsavel_principal_id,
	u_resp.nome AS responsavel_principal_nome,
	te.descricao AS tipo_equipamento,
	m.descricao AS modelo,
	f.descricao AS fabricante,
	es.descricao AS status,
	l.descricao AS localidade,
	emp.nome AS empresa,
	(SELECT COUNT(*) 
	 FROM equipamento_usuarios_compartilhados euc 
	 WHERE euc.equipamento_id = e.id AND euc.ativo = TRUE
	) AS total_usuarios_compartilhados
FROM equipamentos e
LEFT JOIN usuarios u_resp ON e.usuario = u_resp.id
LEFT JOIN tipoequipamentos te ON e.tipoequipamento = te.id
LEFT JOIN modelos m ON e.modelo = m.id
LEFT JOIN fabricantes f ON e.fabricante = f.id
LEFT JOIN equipamentosstatus es ON e.equipamentostatus = es.id
LEFT JOIN localidades l ON e.localidade_id = l.id
LEFT JOIN empresas emp ON e.empresa = emp.id
WHERE e.ativo = TRUE AND e.compartilhado = TRUE;

-- View: vw_equipamentos_usuarios_compartilhados
CREATE OR REPLACE VIEW vw_equipamentos_usuarios_compartilhados AS
SELECT 
	euc.id,
	euc.equipamento_id,
	e.patrimonio,
	e.numeroserie,
	euc.colaborador_id,
	c.nome AS colaborador_nome,
	c.matricula AS colaborador_matricula,
	c.email AS colaborador_email,
	c.cargo AS colaborador_cargo,
	euc.data_inicio,
	euc.data_fim,
	euc.ativo,
	euc.tipo_acesso,
	euc.observacao,
	euc.criado_por,
	u_criador.nome AS criado_por_nome,
	euc.criado_em,
	CASE 
		WHEN euc.ativo = FALSE THEN 'Inativo'
		WHEN euc.data_fim IS NULL THEN 'Ativo - Indefinido'
		WHEN euc.data_fim < CURRENT_TIMESTAMP THEN 'Expirado'
		ELSE 'Ativo - Temporário'
	END AS status_acesso
FROM equipamento_usuarios_compartilhados euc
INNER JOIN equipamentos e ON euc.equipamento_id = e.id
INNER JOIN colaboradores c ON euc.colaborador_id = c.id
LEFT JOIN usuarios u_criador ON euc.criado_por = u_criador.id
WHERE e.ativo = TRUE;

-- View: planosvm
CREATE OR REPLACE VIEW planosvm AS
SELECT 
    p.id,
    p.nome AS plano,
    p.ativo,
    p.valor,
    c.nome AS contrato,
    c.id AS contratoid,
    o.nome AS operadora,
    o.id AS operadoraid,
    COALESCE(COUNT(l.id), 0) AS contlinhas,
    COALESCE(COUNT(CASE WHEN l.emuso = true THEN l.id END), 0) AS contlinhasemuso,
    COALESCE(COUNT(CASE WHEN l.emuso = false THEN l.id END), 0) AS contlinhaslivres
FROM telefoniaplanos p
LEFT JOIN telefoniacontratos c ON p.contrato = c.id
LEFT JOIN telefoniaoperadoras o ON c.operadora = o.id
LEFT JOIN telefonialinhas l ON l.plano = p.id
WHERE p.ativo = true
GROUP BY p.id, p.nome, p.ativo, p.valor, c.nome, c.id, o.nome, o.id;

COMMENT ON VIEW planosvm IS 'View para listar planos de telefonia com informações agregadas de linhas';

-- View: vwplanostelefonia
CREATE OR REPLACE VIEW vwplanostelefonia AS
SELECT 
    p.id,
    p.nome AS plano,
    p.ativo,
    p.valor,
    c.nome AS contrato,
    c.id AS contratoid,
    o.nome AS operadora,
    o.id AS operadoraid,
    COALESCE(COUNT(l.id), 0) AS contlinhas,
    COALESCE(COUNT(CASE WHEN l.emuso = true THEN l.id END), 0) AS contlinhasemuso,
    COALESCE(COUNT(CASE WHEN l.emuso = false THEN l.id END), 0) AS contlinhaslivres
FROM telefoniaplanos p
LEFT JOIN telefoniacontratos c ON p.contrato = c.id
LEFT JOIN telefoniaoperadoras o ON c.operadora = o.id
LEFT JOIN telefonialinhas l ON l.plano = p.id
WHERE p.ativo = true
GROUP BY p.id, p.nome, p.ativo, p.valor, c.nome, c.id, o.nome, o.id;

COMMENT ON VIEW vwplanostelefonia IS 'View para listar planos de telefonia com informações agregadas de linhas';

-- View: vw_tinone_estatisticas
DROP VIEW IF EXISTS vw_tinone_estatisticas CASCADE;
CREATE OR REPLACE VIEW vw_tinone_estatisticas AS
SELECT count(*) AS total_interacoes,
    count(DISTINCT usuario_id) AS usuarios_unicos,
    count(DISTINCT sessao_id) AS sessoes_unicas,
    avg(tempo_resposta_ms) AS tempo_medio_resposta,
    count(
        CASE
            WHEN foi_util = true THEN 1
            ELSE NULL::integer
        END) AS feedbacks_positivos,
    count(
        CASE
            WHEN foi_util = false THEN 1
            ELSE NULL::integer
        END) AS feedbacks_negativos,
    DATE(created_at) AS data
   FROM tinone_analytics
  GROUP BY DATE(created_at)
  ORDER BY DATE(created_at) DESC;

COMMENT ON VIEW vw_tinone_estatisticas IS 'Estatísticas diárias de uso do TinOne';

-- View: vw_campanhas_resumo
CREATE OR REPLACE VIEW vw_campanhas_resumo AS
SELECT 
    c.id,
    c.cliente,
    c.nome,
    c.descricao,
    c.datacriacao,
    c.datainicio,
    c.datafim,
    c.status,
    c.totalcolaboradores,
    c.totalenviados,
    c.totalassinados,
    c.totalpendentes,
    c.percentualadesao,
    c.dataultimoenvio,
    c.dataconclusao,
    u.nome AS usuariocriacao_nome,
    CASE c.status
        WHEN 'A' THEN 'Ativa'
        WHEN 'I' THEN 'Inativa'
        WHEN 'C' THEN 'Concluída'
        WHEN 'G' THEN 'Agendada'
    END AS status_descricao,
    COUNT(cc.id) AS total_colaboradores_cadastrados,
    COUNT(CASE WHEN cc.statusassinatura = 'A' THEN 1 END) AS total_assinados_real,
    COUNT(CASE WHEN cc.statusassinatura IN ('P', 'E') THEN 1 END) AS total_pendentes_real
FROM campanhasassinaturas c
LEFT JOIN usuarios u ON c.usuariocriacao = u.id
LEFT JOIN campanhascolaboradores cc ON c.id = cc.campanhaid
GROUP BY c.id, c.cliente, c.nome, c.descricao, c.datacriacao, c.datainicio, c.datafim, 
         c.status, c.totalcolaboradores, c.totalenviados, c.totalassinados, 
         c.totalpendentes, c.percentualadesao, c.dataultimoenvio, c.dataconclusao, u.nome;

COMMENT ON VIEW vw_campanhas_resumo IS 'Visão resumida das campanhas com estatísticas atualizadas';

-- View: vw_campanhas_colaboradores_detalhado
CREATE OR REPLACE VIEW vw_campanhas_colaboradores_detalhado AS
SELECT 
    c.id AS campanha_id,
    c.nome AS campanha_nome,
    c.status AS campanha_status,
    cc.id AS associacao_id,
    cc.colaboradorid,
    col.nome AS colaborador_nome,
    col.cpf AS colaborador_cpf,
    col.email AS colaborador_email,
    col.cargo AS colaborador_cargo,
    e.nome AS empresa_nome,
    l.descricao AS localidade_nome,
    cc.statusassinatura,
    CASE cc.statusassinatura
        WHEN 'P' THEN 'Pendente'
        WHEN 'E' THEN 'Enviado'
        WHEN 'A' THEN 'Assinado'
        WHEN 'R' THEN 'Recusado'
    END AS status_descricao,
    cc.datainclusao,
    cc.dataenvio,
    cc.dataassinatura,
    cc.totalenvios,
    cc.dataultimoenvio,
    cc.ipenvio,
    cc.localizacaoenvio
FROM campanhasassinaturas c
INNER JOIN campanhascolaboradores cc ON c.id = cc.campanhaid
INNER JOIN colaboradores col ON cc.colaboradorid = col.id
LEFT JOIN empresas e ON col.empresa = e.id
LEFT JOIN localidades l ON col.localidade = l.id;

COMMENT ON VIEW vw_campanhas_colaboradores_detalhado IS 'Visão detalhada de colaboradores por campanha';

-- View: vw_nao_conformidade_elegibilidade
DROP VIEW IF EXISTS vw_nao_conformidade_elegibilidade CASCADE;
CREATE OR REPLACE VIEW vw_nao_conformidade_elegibilidade AS
WITH equipamentos_alocados AS (
    SELECT DISTINCT ON (c.id, e.id)
        c.id AS colaborador_id,
        c.nome AS colaborador_nome,
        c.cpf AS colaborador_cpf,
        c.email AS colaborador_email,
        c.cargo AS colaborador_cargo,
        c.tipocolaborador AS tipo_colaborador,
        CASE c.tipocolaborador
            WHEN 'F'::bpchar THEN 'Funcionário'::text
            WHEN 'T'::bpchar THEN 'Terceirizado'::text
            WHEN 'C'::bpchar THEN 'Consultor'::text
            ELSE 'Desconhecido'::text
        END AS tipo_colaborador_descricao,
        emp.nome AS empresa_nome,
        COALESCE((cc.codigo::text || ' - '::text) || cc.nome::text, ''::text) AS centro_custo,
        loc.descricao AS localidade,
        e.id AS equipamento_id,
        e.patrimonio AS equipamento_patrimonio,
        e.numeroserie AS equipamento_serie,
        te.id AS tipo_equipamento_id,
        te.descricao AS tipo_equipamento_descricao,
        te.categoria_id AS categoria_equipamento,
        e.fabricante AS fabricante_id,
        f.descricao AS fabricante,
        e.modelo AS modelo_id,
        m.descricao AS modelo,
        e.equipamentostatus AS equipamento_status,
        c.cliente
    FROM colaboradores c
    JOIN requisicoes r ON r.colaboradorfinal = c.id
    JOIN requisicoesitens ri ON ri.requisicao = r.id
    JOIN equipamentos e ON e.id = ri.equipamento
    JOIN tipoequipamentos te ON te.id = e.tipoequipamento
    LEFT JOIN fabricantes f ON f.id = e.fabricante
    LEFT JOIN modelos m ON m.id = e.modelo
    LEFT JOIN empresas emp ON emp.id = c.empresa
    LEFT JOIN centrocusto cc ON cc.id = c.centrocusto
    LEFT JOIN localidades loc ON loc.id = c.localidade_id
    WHERE ri.dtdevolucao IS NULL AND ri.equipamento IS NOT NULL AND (c.dtdemissao IS NULL OR c.dtdemissao > now()) AND (e.tipoaquisicao IS NULL OR e.tipoaquisicao <> 2) AND e.equipamentostatus = 4
),
contagem_equipamentos AS (
    SELECT equipamentos_alocados.colaborador_id,
        equipamentos_alocados.tipo_equipamento_id,
        count(*) AS quantidade_atual
       FROM equipamentos_alocados
      GROUP BY equipamentos_alocados.colaborador_id, equipamentos_alocados.tipo_equipamento_id
),
politicas_aplicaveis AS (
    SELECT DISTINCT ON (ea.colaborador_id, ea.tipo_equipamento_id)
        ea.colaborador_id,
        ea.tipo_colaborador,
        ea.colaborador_cargo,
        ea.tipo_equipamento_id,
        pe.id AS politica_id,
        pe.permite_acesso AS permite_acesso,
        pe.quantidade_maxima AS quantidade_maxima,
        pe.observacoes AS politica_observacoes,
        pe.usarpadrao,
        pe.cargo AS politica_cargo
    FROM equipamentos_alocados ea
    LEFT JOIN politicas_elegibilidade pe ON 
        pe.tipo_colaborador = ea.tipo_colaborador
        AND pe.tipo_equipamento_id = ea.tipo_equipamento_id
        AND pe.cliente = ea.cliente
        AND pe.ativo = true
        AND (
            pe.cargo IS NULL 
            OR pe.cargo = ''
            OR (
                pe.cargo IS NOT NULL 
                AND pe.cargo <> ''
                AND ea.colaborador_cargo IS NOT NULL
                AND (
                    (pe.usarpadrao = false AND UPPER(TRIM(ea.colaborador_cargo)) = UPPER(TRIM(pe.cargo)))
                    OR (pe.usarpadrao = true AND UPPER(ea.colaborador_cargo) LIKE '%' || UPPER(TRIM(pe.cargo)) || '%')
                )
            )
        )
)
SELECT ea.colaborador_id,
    ea.colaborador_nome,
    ea.colaborador_cpf,
    ea.colaborador_email,
    ea.colaborador_cargo,
    ea.tipo_colaborador,
    ea.tipo_colaborador_descricao,
    ea.empresa_nome,
    ea.centro_custo,
    ea.localidade,
    ea.equipamento_id,
    ea.equipamento_patrimonio,
    ea.equipamento_serie,
    ea.tipo_equipamento_id,
    ea.tipo_equipamento_descricao,
    ea.categoria_equipamento,
    ea.fabricante_id,
    ea.fabricante,
    ea.modelo_id,
    ea.modelo,
    ea.equipamento_status,
    ea.cliente,
    pa.politica_id,
    COALESCE(pa.permite_acesso, true) AS permite_acesso,
    pa.quantidade_maxima,
    pa.politica_observacoes,
    ce.quantidade_atual,
    now() AS dt_geracao_relatorio
   FROM equipamentos_alocados ea
     LEFT JOIN politicas_aplicaveis pa ON pa.colaborador_id = ea.colaborador_id AND pa.tipo_equipamento_id = ea.tipo_equipamento_id
     LEFT JOIN contagem_equipamentos ce ON ce.colaborador_id = ea.colaborador_id AND ce.tipo_equipamento_id = ea.tipo_equipamento_id
  WHERE pa.politica_id IS NOT NULL AND pa.permite_acesso = false OR pa.politica_id IS NOT NULL AND pa.permite_acesso = true AND pa.quantidade_maxima IS NOT NULL AND ce.quantidade_atual > pa.quantidade_maxima
  ORDER BY ea.colaborador_nome, ea.tipo_equipamento_descricao;

COMMENT ON VIEW vw_nao_conformidade_elegibilidade IS 'Identifica colaboradores que possuem equipamentos mas não são elegíveis conforme políticas';

-- View: equipamentovm (minúsculo)
DROP VIEW IF EXISTS equipamentovm CASCADE;
CREATE OR REPLACE VIEW equipamentovm AS
SELECT e.id,
    e.tipoequipamento AS tipoequipamentoid,
    COALESCE(te.descricao, 'Nao definido'::character varying(200)) AS tipoequipamento,
    e.fabricante AS fabricanteid,
    COALESCE(f.descricao, 'Nao definido'::character varying(200)) AS fabricante,
    e.modelo AS modeloid,
    COALESCE(m.descricao, 'Nao definido'::character varying(200)) AS modelo,
    e.notafiscal AS notafiscalid,
        CASE
            WHEN e.notafiscal IS NOT NULL THEN nf.numero::character varying
            ELSE 'Nao definido'::character varying
        END AS "Notafiscal",
    e.equipamentostatus AS equipamentostatusid,
    COALESCE(es.descricao, 'Nao definido'::character varying) AS equipamentostatus,
    e.usuario AS usuarioid,
    COALESCE(u.nome, 'Nao definido'::character varying) AS usuario,
    e.localidade_id AS localizacaoid,
        CASE
            WHEN e.localidade_id = 1 THEN 'Nao definido'::character varying
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
     LEFT JOIN localidades l ON e.localidade_id = l.id
     LEFT JOIN empresas emp ON e.empresa = emp.id
     LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
     LEFT JOIN empresas emp_cc ON cc.empresa = emp_cc.id
     LEFT JOIN contratos con ON e.contrato = con.id
     LEFT JOIN filiais fil ON e.filial_id = fil.id
     LEFT JOIN tipoaquisicao ta ON e.tipoaquisicao = ta.id
  WHERE e.ativo = true;

-- View: termoentregavm (minúsculo)
DROP VIEW IF EXISTS termoentregavm CASCADE;
CREATE OR REPLACE VIEW termoentregavm AS
SELECT te.descricao AS tipoequipamento,
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
            WHEN e.tipoaquisicao = 2 THEN 2
            ELSE 1
        END AS tipoaquisicao
   FROM equipamentos e
     JOIN requisicoesitens ri ON e.id = ri.equipamento
     JOIN requisicoes r ON ri.requisicao = r.id
     JOIN tipoequipamentos te ON e.tipoequipamento = te.id
     LEFT JOIN fabricantes f ON e.fabricante = f.id
     LEFT JOIN modelos m ON e.modelo = m.id
  WHERE r.requisicaostatus = 3 AND ri.dtdevolucao IS NULL;

-- View: vw_colaboradores_simples
CREATE OR REPLACE VIEW vw_colaboradores_simples AS
SELECT c.id,
    c.nome,
    c.cpf,
    c.matricula,
    c.email,
    c.cargo,
    c.setor,
    c.dtadmissao,
    c.situacao,
    c.empresa,
    e.nome AS empresa_nome,
    c.centrocusto,
    cc.nome AS centro_custo_nome,
    c.filial_id,
    f.nome AS filial_nome,
    c.localidade_id,
    l.descricao AS localidade_nome,
    COALESCE(c.cliente, e.cliente) AS cliente,
    cl.razaosocial AS cliente_nome
   FROM colaboradores c
     JOIN empresas e ON c.empresa = e.id
     JOIN centrocusto cc ON c.centrocusto = cc.id
     LEFT JOIN filiais f ON c.filial_id = f.id
     LEFT JOIN localidades l ON c.localidade_id = l.id
     JOIN clientes cl ON COALESCE(c.cliente, e.cliente) = cl.id;

-- View: vw_equipamentos_simples
CREATE OR REPLACE VIEW vw_equipamentos_simples AS
SELECT e.id,
    e.numeroserie,
    e.patrimonio,
    e.dtcadastro,
    e.ativo,
    e.empresa,
    emp.nome AS empresa_nome,
    e.centrocusto,
    cc.nome AS centro_custo_nome,
    e.filial_id,
    f.nome AS filial_nome,
    e.localidade_id AS localidade_id,
    l.descricao AS localidade_nome,
    COALESCE(e.cliente, emp.cliente) AS cliente,
    cl.razaosocial AS cliente_nome
   FROM equipamentos e
     LEFT JOIN empresas emp ON e.empresa = emp.id
     LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
     LEFT JOIN filiais f ON e.filial_id = f.id
     LEFT JOIN localidades l ON e.localidade_id = l.id
     LEFT JOIN clientes cl ON COALESCE(e.cliente, emp.cliente) = cl.id;

-- View: vwestoquelinhasalerta
CREATE OR REPLACE VIEW vwestoquelinhasalerta AS
SELECT c.cliente,
    l.descricao AS localidade,
    o.nome AS operadora,
    c.nome AS contrato,
    p.nome AS plano,
    eml.perfiluso,
    count(
        CASE
            WHEN tl.emuso = false AND tl.ativo = true THEN 1
            ELSE NULL::integer
        END) AS estoqueatual,
    eml.quantidademinima AS estoqueminimo,
        CASE
            WHEN count(
            CASE
                WHEN tl.emuso = false AND tl.ativo = true THEN 1
                ELSE NULL::integer
            END) < eml.quantidademinima THEN 'ALERTA'::text
            ELSE 'OK'::text
        END AS status,
        CASE
            WHEN count(
            CASE
                WHEN tl.emuso = false AND tl.ativo = true THEN 1
                ELSE NULL::integer
            END) < eml.quantidademinima THEN eml.quantidademinima - count(
            CASE
                WHEN tl.emuso = false AND tl.ativo = true THEN 1
                ELSE NULL::integer
            END)
            ELSE 0::bigint
        END AS quantidadefaltante
   FROM telefonialinhas tl
     JOIN telefoniaplanos p ON tl.plano = p.id
     JOIN telefoniacontratos c ON p.contrato = c.id
     JOIN telefoniaoperadoras o ON c.operadora = o.id
     JOIN estoqueminimolinhas eml ON c.operadora = eml.operadora AND p.id = eml.plano AND c.cliente = eml.cliente
     JOIN localidades l ON eml.localidade = l.id
  WHERE eml.ativo = true
  GROUP BY c.cliente, l.descricao, o.nome, c.nome, p.nome, eml.perfiluso, eml.quantidademinima;
