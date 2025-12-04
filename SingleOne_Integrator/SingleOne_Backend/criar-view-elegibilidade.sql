CREATE OR REPLACE VIEW vw_nao_conformidade_elegibilidade AS
SELECT 
    c.id AS colaborador_id,
    c.nome AS colaborador_nome,
    c.cpf AS colaborador_cpf,
    c.email AS colaborador_email,
    c.cargo AS colaborador_cargo,
    c.tipocolaborador AS tipo_colaborador,
    CASE c.tipocolaborador
        WHEN 'E' THEN 'Estagiário'
        WHEN 'C' THEN 'CLT'
        WHEN 'G' THEN 'Gerente'
        WHEN 'D' THEN 'Diretor'
        WHEN 'T' THEN 'Terceirizado'
        ELSE c.tipocolaborador::text
    END AS tipo_colaborador_descricao,
    emp.nome AS empresa_nome,
    cc.nome AS centro_custo,
    loc.descricao AS localidade,
    e.id AS equipamento_id,
    e.patrimonio AS equipamento_patrimonio,
    e.numeroserie AS equipamento_serie,
    te.id AS tipo_equipamento_id,
    te.descricao AS tipo_equipamento_descricao,
    cat.descricao AS categoria_equipamento,
    fab.descricao AS fabricante,
    mod.descricao AS modelo,
    es.descricao AS equipamento_status,
    pe.id AS politica_id,
    pe.permite_acesso,
    pe.quantidade_maxima,
    pe.observacoes AS politica_observacoes,
    (
        SELECT COUNT(*)
        FROM equipamentos e2
        INNER JOIN equipamentohistorico eh2 ON e2.id = eh2.equipamento
        WHERE eh2.colaborador = c.id
        AND e2.tipoequipamento = te.id
        AND e2.ativo = true
        AND eh2.dtretorno IS NULL
    ) AS quantidade_atual,
    CURRENT_TIMESTAMP AS dt_geracao_relatorio
FROM 
    colaboradores c
    INNER JOIN equipamentohistorico eh ON c.id = eh.colaborador
    INNER JOIN equipamentos e ON eh.equipamento = e.id
    INNER JOIN tipoequipamentos te ON e.tipoequipamento = te.id
    LEFT JOIN categorias cat ON te.categoria_id = cat.id
    LEFT JOIN fabricantes fab ON e.fabricante = fab.id
    LEFT JOIN modelos mod ON e.modelo = mod.id
    LEFT JOIN equipamentosstatus es ON e.equipamentostatus = es.id
    LEFT JOIN empresas emp ON c.empresa = emp.id
    LEFT JOIN centrocusto cc ON c.centrocusto = cc.id
    LEFT JOIN localidades loc ON c.localidade_id = loc.id
    LEFT JOIN politicas_elegibilidade pe ON 
        pe.cliente = c.cliente
        AND pe.tipo_colaborador = c.tipocolaborador::text
        AND pe.tipo_equipamento_id = te.id
        AND pe.ativo = true
        AND (pe.cargo IS NULL OR pe.cargo = c.cargo)
WHERE 
    c.situacao = 'A'
    AND e.ativo = true
    AND eh.dtretorno IS NULL
    AND (
        (pe.permite_acesso = false)
        OR
        (
            pe.quantidade_maxima IS NOT NULL 
            AND (
                SELECT COUNT(*)
                FROM equipamentos e3
                INNER JOIN equipamentohistorico eh3 ON e3.id = eh3.equipamento
                WHERE eh3.colaborador = c.id
                AND e3.tipoequipamento = te.id
                AND e3.ativo = true
                AND eh3.dtretorno IS NULL
            ) > pe.quantidade_maxima
        )
    )
ORDER BY 
    c.nome, te.descricao;

COMMENT ON VIEW vw_nao_conformidade_elegibilidade IS 'View que identifica colaboradores que possuem recursos mas não são elegíveis conforme as políticas definidas';

