DROP VIEW IF EXISTS colaboradoresvm;

CREATE VIEW colaboradoresvm AS
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
            WHEN COALESCE(NULLIF(c.situacao, ''), 'A') IN ('A', 'D', 'I', 'F') THEN COALESCE(NULLIF(c.situacao, ''), 'A')
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
INNER JOIN empresas e ON e.id = c.empresa
INNER JOIN centrocusto cc ON cc.id = c.centrocusto
LEFT JOIN localidades l ON l.id = c.localidade;
