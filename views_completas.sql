psql : psql:extrair_todas_views.sql:2: erro: views_completas.sql: Permission denied
No C:\Users\Evener\AppData\Local\Temp\ps-script-0ad039cb-bb95-4bce-8a67-dc1b46d4d466.ps1:87 caractere:48
+ ... dmin@2025'; psql -U postgres -d singleone -h localhost -p 5432 -f ext ...
+                 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    + CategoryInfo          : NotSpecified: (psql:extrair_to...rmission denied:String) [], RemoteException
    + FullyQualifiedErrorId : NativeCommandError
 
psql:extrair_todas_views.sql:27: NOTA:  -- View: EquipamentoVM
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.EquipamentoVM CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.EquipamentoVM AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT e.id,
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
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: colaboradoresvm
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.colaboradoresvm CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.colaboradoresvm AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT c.id,
    c.cliente,
    e.nome AS empresa,
    cc.nome AS nomecentrocusto,
    cc.codigo AS codigocentrocusto,
    c.nome,
    c.cpf,
    c.matricula,
    c.email,
    c.tipocolaborador::text AS tipocolaborador,
        CASE
            WHEN COALESCE(NULLIF(c.situacao, ''::bpchar), 'A'::bpchar) = ANY (ARRAY['A'::bpchar, 'D'::bpchar, 
'I'::bpchar, 'F'::bpchar]) THEN COALESCE(NULLIF(c.situacao, ''::bpchar), 'A'::bpchar)
            WHEN c.dtdemissao IS NULL THEN 'A'::bpchar
            WHEN c.dtdemissao < CURRENT_DATE::timestamp without time zone THEN 'D'::bpchar
            ELSE 'A'::bpchar
        END::text AS situacao,
    c.cargo,
    c.setor,
    COALESCE(l.descricao, ''::character varying) AS localidadedescricao,
    COALESCE(l.cidade, ''::character varying) AS localidadecidade,
    COALESCE(l.estado, ''::character varying) AS localidadeestado,
    c.dtadmissao,
    c.dtdemissao,
    c.dtcadastro,
    COALESCE(c.matriculasuperior, ''::character varying) AS matriculasuperior
   FROM colaboradores c
     JOIN empresas e ON e.id = c.empresa
     JOIN centrocusto cc ON cc.id = c.centrocusto
     LEFT JOIN localidades l ON l.id = c.localidade;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: colaboradorhistoricovm
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.colaboradorhistoricovm CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.colaboradorhistoricovm AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT c.id,
    c.nome,
    c.cpf,
    c.matricula,
    c.email,
    c.cargo,
        CASE
            WHEN c.situacao = 'P'::bpchar THEN 'Provisionado'::text
            WHEN c.situacao = 'A'::bpchar THEN 'Ativo'::text
            WHEN c.situacao = 'I'::bpchar THEN 'Inativo'::text
            WHEN c.situacao IS NULL THEN 'N/I'::text
            ELSE NULL::text
        END AS situacao,
        CASE
            WHEN c.situacaoantiga = 'P'::bpchar THEN 'Provisionado'::text
            WHEN c.situacaoantiga = 'A'::bpchar THEN 'Ativo'::text
            WHEN c.situacaoantiga = 'I'::bpchar THEN 'Inativo'::text
            WHEN c.situacaoantiga IS NULL THEN 'N/I'::text
            ELSE NULL::text
        END AS situacaoantiga,
    c.dtatualizacao,
    c.empresa AS empresaatualid,
    e.nome AS empresaatual,
    c.antigaempresa AS empresaantigaid,
    ea.nome AS empresaantiga,
    c.dtatualizacaoempresa,
    c.localidade AS localidadeatualid,
    l.descricao AS localidadeatual,
    c.antigalocalidade AS localidadeantigaid,
    la.descricao AS localidadeantiga,
    c.dtatualizacaolocalidade,
    c.centrocusto AS centrocustoatualid,
    cc.codigo AS codigoccatual,
    cc.nome AS nomeccatual,
    c.antigocentrocusto AS centrocustoantigoid,
    cca.codigo AS codigoccantigo,
    cca.nome AS nomeccantigo,
    c.dtatualizacaocentrocusto,
    e.cliente
   FROM colaboradores c
     JOIN empresas e ON c.empresa = e.id
     LEFT JOIN empresas ea ON c.antigaempresa = ea.id
     JOIN localidades l ON c.localidade = l.id
     LEFT JOIN localidades la ON c.antigalocalidade = la.id
     JOIN centrocusto cc ON c.centrocusto = cc.id
     LEFT JOIN centrocusto cca ON c.antigocentrocusto = cca.id
  WHERE c.dtatualizacao IS NOT NULL OR c.dtatualizacaocentrocusto IS NOT NULL OR c.dtatualizacaoempresa IS NOT NULL OR 
c.dtatualizacaolocalidade IS NOT NULL;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: equipamentohistoricovm
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.equipamentohistoricovm CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.equipamentohistoricovm AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT eh.id,
    eh.equipamento AS equipamentoid,
    e.tipoequipamento AS tipoequipamentoid,
    te.descricao AS tipoequipamento,
    e.fabricante AS fabricanteid,
    f.descricao AS fabricante,
    e.modelo AS modeloid,
    m.descricao AS modelo,
    e.numeroserie,
    e.patrimonio,
    eh.equipamentostatus AS equipamentostatusid,
    es.descricao AS equipamentostatus,
    eh.colaborador AS colaboradorid,
    c.nome AS colaborador,
    eh.dtregistro,
    eh.usuario AS usuarioid,
    u.nome AS usuario,
    r.tecnicoresponsavel AS tecnicoresponsavelid,
    tr.nome AS tecnicoresponsavel
   FROM equipamentohistorico eh
     LEFT JOIN equipamentos e ON eh.equipamento = e.id
     LEFT JOIN tipoequipamentos te ON e.tipoequipamento = te.id
     LEFT JOIN fabricantes f ON e.fabricante = f.id
     LEFT JOIN modelos m ON e.modelo = m.id
     LEFT JOIN equipamentosstatus es ON eh.equipamentostatus = es.id
     LEFT JOIN colaboradores c ON eh.colaborador = c.id
     LEFT JOIN usuarios u ON eh.usuario = u.id
     LEFT JOIN requisicoes r ON eh.requisicao = r.id
     LEFT JOIN usuarios tr ON r.tecnicoresponsavel = tr.id
  ORDER BY eh.dtregistro DESC;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: equipamentovm
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.equipamentovm CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.equipamentovm AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT e.id,
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
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: planosvm
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.planosvm CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.planosvm AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT tp.id,
    tp.nome AS plano,
    tp.ativo,
    tp.valor,
    tc.nome AS contrato,
    tc.id AS contratoid,
    tope.nome AS operadora,
    tope.id AS operadoraid,
    count(tl.id) AS contlinhas,
    count(
        CASE
            WHEN tl.emuso = true THEN 1
            ELSE NULL::integer
        END) AS contlinhasemuso,
    count(
        CASE
            WHEN tl.emuso = false THEN 1
            ELSE NULL::integer
        END) AS contlinhaslivres
   FROM telefoniaplanos tp
     JOIN telefoniacontratos tc ON tp.contrato = tc.id
     JOIN telefoniaoperadoras tope ON tc.operadora = tope.id
     LEFT JOIN telefonialinhas tl ON tl.plano = tp.id AND tl.ativo = true
  GROUP BY tp.id, tp.nome, tp.ativo, tp.valor, tc.nome, tc.id, tope.nome, tope.id;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: requisicaoequipamentosvm
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.requisicaoequipamentosvm CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.requisicaoequipamentosvm AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT ri.id,
    r.id AS requisicao,
    ri.equipamento AS equipamentoid,
    concat(te.descricao, ' ', f.descricao, ' ', m.descricao) AS equipamento,
    e.numeroserie,
    e.patrimonio,
    ue.id AS usuarioentregaid,
    ue.nome AS usuarioentrega,
    ud.id AS usuariodevolucaoid,
    ud.nome AS usuariodevolucao,
    ri.dtentrega,
    ri.dtdevolucao,
    ri.observacaoentrega,
    ri.dtprogramadaretorno,
    e.equipamentostatus,
    tl.numero,
    ri.linhatelefonica AS linhaid,
    e.tipoaquisicao
   FROM requisicoes r
     JOIN requisicoesitens ri ON r.id = ri.requisicao
     JOIN equipamentos e ON ri.equipamento = e.id
     JOIN tipoequipamentos te ON e.tipoequipamento = te.id
     JOIN fabricantes f ON e.fabricante = f.id
     JOIN modelos m ON e.modelo = m.id
     LEFT JOIN usuarios ue ON ri.usuarioentrega = ue.id
     LEFT JOIN usuarios ud ON ri.usuariodevolucao = ud.id
     LEFT JOIN telefonialinhas tl ON ri.linhatelefonica = tl.id
  WHERE e.id <> 1
UNION ALL
 SELECT ri.id,
    r.id AS requisicao,
    ri.equipamento AS equipamentoid,
    concat(f.descricao, ' ', tl.numero::character varying) AS equipamento,
    ''::character varying AS numeroserie,
    e.patrimonio,
    ue.id AS usuarioentregaid,
    ue.nome AS usuarioentrega,
    ud.id AS usuariodevolucaoid,
    ud.nome AS usuariodevolucao,
    ri.dtentrega,
    ri.dtdevolucao,
    ri.observacaoentrega,
    ri.dtprogramadaretorno,
    e.equipamentostatus,
    tl.numero,
    ri.linhatelefonica AS linhaid,
    e.tipoaquisicao
   FROM requisicoes r
     JOIN requisicoesitens ri ON r.id = ri.requisicao
     JOIN equipamentos e ON ri.equipamento = e.id
     JOIN tipoequipamentos te ON e.tipoequipamento = te.id
     JOIN fabricantes f ON e.fabricante = f.id
     JOIN modelos m ON e.modelo = m.id
     LEFT JOIN usuarios ue ON ri.usuarioentrega = ue.id
     LEFT JOIN usuarios ud ON ri.usuariodevolucao = ud.id
     JOIN telefonialinhas tl ON ri.linhatelefonica = tl.id
  WHERE e.id = 1;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: requisicoesvm
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.requisicoesvm CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.requisicoesvm AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT r.id,
    u.id AS usuariorequisicaoid,
    u.nome AS usuariorequisicao,
    t.id AS tecnicoresponsavelid,
    t.nome AS tecnicoresponsavel,
    c.id AS colaboradorfinalid,
    c.nome AS colaboradorfinal,
    r.dtsolicitacao,
    r.dtprocessamento,
    r.requisicaostatus AS requisicaostatusid,
    rs.descricao AS requisicaostatus,
    r.assinaturaeletronica,
    r.dtassinaturaeletronica,
    r.dtenviotermo,
    r.hashrequisicao,
    ( SELECT count(*) AS count
           FROM requisicoesitens ri
             JOIN equipamentos e ON ri.equipamento = e.id
          WHERE ri.requisicao = r.id AND ri.dtentrega IS NOT NULL AND ri.dtdevolucao IS NULL AND e.equipamentostatus 
<> 8) AS equipamentospendentes,
    r.cliente
   FROM requisicoes r
     JOIN requisicoesstatus rs ON r.requisicaostatus = rs.id
     JOIN usuarios u ON r.usuariorequisicao = u.id
     JOIN usuarios t ON r.tecnicoresponsavel = t.id
     LEFT JOIN colaboradores c ON r.colaboradorfinal = c.id;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: termoentregavm
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.termoentregavm CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.termoentregavm AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT te.descricao AS tipoequipamento,
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
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: termoscolaboradoresvm
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.termoscolaboradoresvm CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.termoscolaboradoresvm AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT r.colaboradorfinal AS colaboradorfinalid,
    c.nome AS colaboradorfinal,
    ( SELECT max(rq.dtenviotermo) AS max
           FROM requisicoes rq
          WHERE rq.colaboradorfinal = r.colaboradorfinal) AS dtenviotermo,
    ( SELECT
                CASE
                    WHEN count(*) > 0 THEN 'Em aberto'::text
                    ELSE 'Assinado'::text
                END AS "case"
           FROM requisicoes rq
          WHERE rq.colaboradorfinal = r.colaboradorfinal AND rq.assinaturaeletronica = false) AS situacao
   FROM requisicoes r
     JOIN colaboradores c ON r.colaboradorfinal = c.id
  GROUP BY r.colaboradorfinal, c.nome;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- ERRO ao extrair view vwUltimasRequisicaoBYOD: rela├º├úo 
"public.vwultimasrequisicaobyod" n├úo existe
psql:extrair_todas_views.sql:27: NOTA:  -- ERRO ao extrair view vwUltimasRequisicaoNaoBYOD: rela├º├úo 
"public.vwultimasrequisicaonaobyod" n├úo existe
psql:extrair_todas_views.sql:27: NOTA:  -- View: vw_campanhas_colaboradores_detalhado
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vw_campanhas_colaboradores_detalhado CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vw_campanhas_colaboradores_detalhado AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT c.id AS campanha_id,
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
            WHEN 'P'::bpchar THEN 'Pendente'::text
            WHEN 'E'::bpchar THEN 'Enviado'::text
            WHEN 'A'::bpchar THEN 'Assinado'::text
            WHEN 'R'::bpchar THEN 'Recusado'::text
            ELSE NULL::text
        END AS status_descricao,
    cc.datainclusao,
    cc.dataenvio,
    cc.dataassinatura,
    cc.totalenvios,
    cc.dataultimoenvio,
    cc.ipenvio,
    cc.localizacaoenvio
   FROM campanhasassinaturas c
     JOIN campanhascolaboradores cc ON c.id = cc.campanhaid
     JOIN colaboradores col ON cc.colaboradorid = col.id
     LEFT JOIN empresas e ON col.empresa = e.id
     LEFT JOIN localidades l ON col.localidade = l.id;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vw_campanhas_resumo
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vw_campanhas_resumo CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vw_campanhas_resumo AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT c.id,
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
            WHEN 'A'::bpchar THEN 'Ativa'::text
            WHEN 'I'::bpchar THEN 'Inativa'::text
            WHEN 'C'::bpchar THEN 'Conclu├â┬¡da'::text
            WHEN 'G'::bpchar THEN 'Agendada'::text
            ELSE NULL::text
        END AS status_descricao,
    count(cc.id) AS total_colaboradores_cadastrados,
    count(
        CASE
            WHEN cc.statusassinatura = 'A'::bpchar THEN 1
            ELSE NULL::integer
        END) AS total_assinados_real,
    count(
        CASE
            WHEN cc.statusassinatura = ANY (ARRAY['P'::bpchar, 'E'::bpchar]) THEN 1
            ELSE NULL::integer
        END) AS total_pendentes_real
   FROM campanhasassinaturas c
     LEFT JOIN usuarios u ON c.usuariocriacao = u.id
     LEFT JOIN campanhascolaboradores cc ON c.id = cc.campanhaid
  GROUP BY c.id, c.cliente, c.nome, c.descricao, c.datacriacao, c.datainicio, c.datafim, c.status, 
c.totalcolaboradores, c.totalenviados, c.totalassinados, c.totalpendentes, c.percentualadesao, c.dataultimoenvio, 
c.dataconclusao, u.nome;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vw_colaboradores_simples
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vw_colaboradores_simples CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vw_colaboradores_simples AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT c.id,
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
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vw_equipamentos_simples
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vw_equipamentos_simples CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vw_equipamentos_simples AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT e.id,
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
    e.localidade_id,
    l.descricao AS localidade_nome,
    COALESCE(e.cliente, emp.cliente) AS cliente,
    cl.razaosocial AS cliente_nome
   FROM equipamentos e
     LEFT JOIN empresas emp ON e.empresa = emp.id
     LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
     LEFT JOIN filiais f ON e.filial_id = f.id
     LEFT JOIN localidades l ON e.localidade_id = l.id
     LEFT JOIN clientes cl ON COALESCE(e.cliente, emp.cliente) = cl.id;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vw_nao_conformidade_elegibilidade
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vw_nao_conformidade_elegibilidade CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vw_nao_conformidade_elegibilidade AS
psql:extrair_todas_views.sql:27: NOTA:   WITH equipamentos_alocados AS (
         SELECT DISTINCT ON (c.id, e.id) c.id AS colaborador_id,
            c.nome AS colaborador_nome,
            c.cpf AS colaborador_cpf,
            c.email AS colaborador_email,
            c.cargo AS colaborador_cargo,
            c.tipocolaborador AS tipo_colaborador,
                CASE c.tipocolaborador
                    WHEN 'F'::bpchar THEN 'Funcion├â┬írio'::text
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
             LEFT JOIN localidades loc ON loc.id = c.localidade
          WHERE ri.dtdevolucao IS NULL AND ri.equipamento IS NOT NULL AND (c.dtdemissao IS NULL OR c.dtdemissao > 
now()) AND (e.tipoaquisicao IS NULL OR e.tipoaquisicao <> 2) AND e.equipamentostatus = 4
        ), contagem_equipamentos AS (
         SELECT equipamentos_alocados.colaborador_id,
            equipamentos_alocados.tipo_equipamento_id,
            count(*) AS quantidade_atual
           FROM equipamentos_alocados
          GROUP BY equipamentos_alocados.colaborador_id, equipamentos_alocados.tipo_equipamento_id
        ), politicas_aplicaveis AS (
         SELECT DISTINCT ON (ea_1.colaborador_id, ea_1.tipo_equipamento_id) ea_1.colaborador_id,
            ea_1.tipo_colaborador,
            ea_1.colaborador_cargo,
            ea_1.tipo_equipamento_id,
            pe.id AS politica_id,
            pe.permite_acesso,
            pe.quantidade_maxima,
            pe.observacoes AS politica_observacoes,
            pe.usarpadrao,
            pe.cargo AS politica_cargo
           FROM equipamentos_alocados ea_1
             LEFT JOIN politicas_elegibilidade pe ON pe.tipo_colaborador::bpchar = ea_1.tipo_colaborador AND 
pe.tipo_equipamento_id = ea_1.tipo_equipamento_id AND pe.cliente = ea_1.cliente AND pe.ativo = true AND (pe.cargo IS 
NULL OR pe.cargo::text = ''::text OR pe.cargo IS NOT NULL AND pe.cargo::text <> ''::text AND ea_1.colaborador_cargo IS 
NOT NULL AND (pe.usarpadrao = false AND upper(TRIM(BOTH FROM ea_1.colaborador_cargo)) = upper(TRIM(BOTH FROM 
pe.cargo)) OR pe.usarpadrao = true AND upper(ea_1.colaborador_cargo::text) ~~ (('%'::text || upper(TRIM(BOTH FROM 
pe.cargo))) || '%'::text)))
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
     LEFT JOIN politicas_aplicaveis pa ON pa.colaborador_id = ea.colaborador_id AND pa.tipo_equipamento_id = 
ea.tipo_equipamento_id
     LEFT JOIN contagem_equipamentos ce ON ce.colaborador_id = ea.colaborador_id AND ce.tipo_equipamento_id = 
ea.tipo_equipamento_id
  WHERE pa.politica_id IS NOT NULL AND pa.permite_acesso = false OR pa.politica_id IS NOT NULL AND pa.permite_acesso = 
true AND pa.quantidade_maxima IS NOT NULL AND ce.quantidade_atual > pa.quantidade_maxima
  ORDER BY ea.colaborador_nome, ea.tipo_equipamento_descricao;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vw_tinone_estatisticas
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vw_tinone_estatisticas CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vw_tinone_estatisticas AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT count(*) AS total_interacoes,
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
    date(created_at) AS data
   FROM tinone_analytics
  GROUP BY (date(created_at))
  ORDER BY (date(created_at)) DESC;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vwdevolucaoprogramada
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vwdevolucaoprogramada CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vwdevolucaoprogramada AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT r.cliente,
    c.nome AS nomecolaborador,
    c.matricula,
    ri.dtprogramadaretorno,
        CASE
            WHEN e.id IS NOT NULL THEN concat(te.descricao,
            CASE
                WHEN f.descricao IS NOT NULL AND m.descricao IS NOT NULL THEN concat(' - ', f.descricao, ' ', 
m.descricao)
                WHEN f.descricao IS NOT NULL THEN concat(' - ', f.descricao)
                WHEN m.descricao IS NOT NULL THEN concat(' - ', m.descricao)
                ELSE ''::text
            END,
            CASE
                WHEN e.numeroserie IS NOT NULL AND e.numeroserie::text <> ''::text THEN concat(' (SN: ', 
e.numeroserie, ')')
                ELSE ''::text
            END,
            CASE
                WHEN e.patrimonio IS NOT NULL AND e.patrimonio::text <> ''::text THEN concat(' [Pat: ', e.patrimonio, 
']')
                ELSE ''::text
            END)
            ELSE 'Equipamento n├úo identificado'::text
        END AS equipamento,
    te.descricao AS tipoequipamento,
    e.numeroserie AS serial,
    e.patrimonio,
    r.id AS requisicaoid,
    e.id AS equipamentoid,
    c.id AS colaboradorid
   FROM requisicoesitens ri
     JOIN requisicoes r ON ri.requisicao = r.id
     JOIN colaboradores c ON r.colaboradorfinal = c.id
     LEFT JOIN equipamentos e ON ri.equipamento = e.id
     LEFT JOIN tipoequipamentos te ON e.tipoequipamento = te.id
     LEFT JOIN fabricantes f ON e.fabricante = f.id
     LEFT JOIN modelos m ON e.modelo = m.id
  WHERE ri.dtprogramadaretorno IS NOT NULL AND ri.dtdevolucao IS NULL AND r.colaboradorfinal IS NOT NULL AND 
c.situacao = 'A'::bpchar AND (r.requisicaostatus = ANY (ARRAY[1, 3]))
  ORDER BY ri.dtprogramadaretorno;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vwequipamentoscomcolaboradoresdesligados
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vwequipamentoscomcolaboradoresdesligados CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vwequipamentoscomcolaboradoresdesligados AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT r.cliente,
    c.nome,
    c.dtdemissao,
    count(ri.equipamento) AS qtde
   FROM requisicoes r
     JOIN requisicoesitens ri ON r.id = ri.requisicao
     JOIN colaboradores c ON r.colaboradorfinal = c.id
  WHERE ri.dtdevolucao IS NULL AND c.dtdemissao IS NOT NULL AND c.dtdemissao < now() AND (ri.equipamento IN ( SELECT 
equipamentos.id
           FROM equipamentos
          WHERE equipamentos.equipamentostatus <> 8))
  GROUP BY r.cliente, c.nome, c.dtdemissao;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vwequipamentosdetalhes
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vwequipamentosdetalhes CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vwequipamentosdetalhes AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT e.id,
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
     LEFT JOIN localidades l ON e.localidade_id = l.id
     LEFT JOIN empresas emp ON e.empresa = emp.id
     LEFT JOIN centrocusto cc ON e.centrocusto = cc.id
     JOIN equipamentosstatus es ON e.equipamentostatus = es.id
  WHERE te.ativo = true AND e.ativo = true;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vwequipamentosstatus
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vwequipamentosstatus CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vwequipamentosstatus AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT e.cliente,
    te.descricao AS tipoequipamento,
    count(
        CASE
            WHEN es.descricao::text ~~* 'Danificado'::text THEN 1
            ELSE NULL::integer
        END) AS danificado,
    count(
        CASE
            WHEN es.descricao::text ~~* 'Devolvido'::text THEN 1
            ELSE NULL::integer
        END) AS devolvido,
    count(
        CASE
            WHEN es.descricao::text ~~* 'Em estoque'::text THEN 1
            ELSE NULL::integer
        END) AS emestoque,
    count(
        CASE
            WHEN es.descricao::text ~~* 'Entregue'::text THEN 1
            ELSE NULL::integer
        END) AS entregue,
    count(
        CASE
            WHEN es.descricao::text ~~* 'Extraviado'::text THEN 1
            ELSE NULL::integer
        END) AS extraviado,
    count(
        CASE
            WHEN es.descricao::text ~~* 'Novo'::text OR es.descricao::text ~~* 'Lan├ºado'::text THEN 1
            ELSE NULL::integer
        END) AS novo,
    count(
        CASE
            WHEN es.descricao::text ~~* 'Requisitado'::text THEN 1
            ELSE NULL::integer
        END) AS requisitado,
    count(
        CASE
            WHEN es.descricao::text ~~* 'Roubado'::text THEN 1
            ELSE NULL::integer
        END) AS roubado,
    count(
        CASE
            WHEN es.descricao::text ~~* 'Sem conserto'::text THEN 1
            ELSE NULL::integer
        END) AS semconserto,
    count(
        CASE
            WHEN es.descricao::text ~~* 'Migrado'::text THEN 1
            ELSE NULL::integer
        END) AS migrado,
    count(
        CASE
            WHEN es.descricao::text ~~* 'Descartado'::text THEN 1
            ELSE NULL::integer
        END) AS descartado
   FROM equipamentos e
     JOIN tipoequipamentos te ON e.tipoequipamento = te.id
     LEFT JOIN equipamentosstatus es ON e.equipamentostatus = es.id
  WHERE te.ativo = true AND te.descricao::text !~~* '%telefon%'::text
  GROUP BY e.cliente, te.descricao
 HAVING count(*) > 0
  ORDER BY te.descricao;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vwestoqueequipamentosalerta
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vwestoqueequipamentosalerta CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vwestoqueequipamentosalerta AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT e.cliente,
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
     JOIN estoqueminimoequipamentos eme ON e.modelo = eme.modelo AND e.localidade_id = eme.localidade AND e.cliente = 
eme.cliente
  WHERE eme.ativo = true
  GROUP BY e.cliente, l.descricao, te.descricao, f.descricao, m.descricao, eme.quantidademinima;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vwestoquelinhasalerta
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vwestoquelinhasalerta CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vwestoquelinhasalerta AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT c.cliente,
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
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vwexportacaoexcel
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vwexportacaoexcel CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vwexportacaoexcel AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT DISTINCT eqp.id,
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
            WHEN false THEN 'N├úo'::text
            ELSE 'Sim'::text
        END AS possuibo,
    eqp.descricaobo,
    eqp.numeroserie,
    eqp.patrimonio,
    eqp.dtcadastro,
        CASE eqp.tipoaquisicao
            WHEN 1 THEN 'Alugado'::text
            WHEN 2 THEN 'Pr├│prio'::text
            WHEN 3 THEN 'Corporativo'::text
            ELSE 'N├úo Definido'::text
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
DO
     JOIN usuarios usu ON eqp.usuario = usu.id
     JOIN localidades loc ON eqp.localidade_id = loc.id
     LEFT JOIN notasfiscais nf ON eqp.notafiscal = nf.id
     LEFT JOIN requisicoesitens ri ON eqp.id = ri.equipamento AND ri.dtdevolucao IS NULL AND ri.dtentrega IS NOT NULL
     LEFT JOIN requisicoes r ON ri.requisicao = r.id
     LEFT JOIN colaboradores c ON r.colaboradorfinal = c.id
     LEFT JOIN empresas emp ON eqp.empresa = emp.id
     LEFT JOIN centrocusto cc ON eqp.centrocusto = cc.id
  WHERE eqp.id > 1 AND eqp.ativo = true;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vwlaudos
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vwlaudos CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vwlaudos AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT l.id,
    l.cliente,
    concat(te.descricao, ' ', f.descricao, ' ', m.descricao) AS equipamento,
    e.numeroserie,
    e.patrimonio,
    l.descricao,
    l.laudo,
    l.dtentrada,
    l.dtlaudo,
    l.mauuso,
    l.temconserto,
    l.usuario,
    u.nome AS usuarionome,
    l.tecnico,
    t.nome AS tecniconome,
    l.valormanutencao,
    e.empresa,
    emp.nome AS empresanome,
    e.centrocusto,
    cc.nome AS centrocustonome
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
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vwnadaconsta
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vwnadaconsta CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vwnadaconsta AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT c.id,
    c.nome,
    c.cpf,
    cc.nome AS centrocusto,
    e.nome AS empresa,
    c.matricula,
    c.cargo,
    ( SELECT count(*) AS count
           FROM requisicoes r
             LEFT JOIN requisicoesitens ri ON r.id = ri.requisicao
          WHERE r.colaboradorfinal = c.id AND ri.dtdevolucao IS NULL) AS maquinascomcolaborador,
    e.cliente
   FROM colaboradores c
     JOIN centrocusto cc ON c.centrocusto = cc.id
     JOIN empresas e ON c.empresa = e.id;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vwplanostelefonia
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vwplanostelefonia CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vwplanostelefonia AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT p.id,
    p.nome AS plano,
    p.ativo,
    p.valor,
    c.nome AS contrato,
    c.id AS contratoid,
    o.nome AS operadora,
    o.id AS operadoraid,
    count(l.id) AS contlinhas,
    count(
        CASE
            WHEN l.emuso = true THEN 1
            ELSE NULL::integer
        END) AS contlinhasemuso,
    count(
        CASE
            WHEN l.emuso = false THEN 1
            ELSE NULL::integer
        END) AS contlinhaslivres
   FROM telefoniaplanos p
     JOIN telefoniacontratos c ON p.contrato = c.id
     JOIN telefoniaoperadoras o ON c.operadora = o.id
     LEFT JOIN telefonialinhas l ON l.plano = p.id
  WHERE p.ativo = true
  GROUP BY p.id, p.nome, p.ativo, p.valor, c.nome, c.id, o.nome, o.id;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
psql:extrair_todas_views.sql:27: NOTA:  -- View: vwtelefonia
psql:extrair_todas_views.sql:27: NOTA:  DROP VIEW IF EXISTS public.vwtelefonia CASCADE;
psql:extrair_todas_views.sql:27: NOTA:  CREATE OR REPLACE VIEW public.vwtelefonia AS
psql:extrair_todas_views.sql:27: NOTA:   SELECT o.nome AS operadora,
    c.nome AS contrato,
    p.nome AS plano,
    p.valor,
    l.numero,
    l.iccid,
    l.emuso,
    l.ativo,
    c.cliente
   FROM telefoniaoperadoras o
     JOIN telefoniacontratos c ON o.id = c.operadora
     JOIN telefoniaplanos p ON c.id = p.contrato
     JOIN telefonialinhas l ON p.id = l.plano
  WHERE o.ativo = true AND c.ativo = true AND p.ativo = true AND l.ativo = true;
psql:extrair_todas_views.sql:27: NOTA:  ;
psql:extrair_todas_views.sql:27: NOTA:  
