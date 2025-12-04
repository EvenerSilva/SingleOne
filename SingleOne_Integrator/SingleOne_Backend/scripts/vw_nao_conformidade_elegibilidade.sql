-- ========================================================================================================
-- VIEW: vw_nao_conformidade_elegibilidade
-- Descrição: Identifica colaboradores que possuem equipamentos mas não são elegíveis conforme políticas
-- ========================================================================================================

CREATE OR REPLACE VIEW vw_nao_conformidade_elegibilidade AS
WITH equipamentos_alocados AS (
    -- Buscar todos os equipamentos atualmente alocados a colaboradores
    SELECT DISTINCT ON (c.id, e.id)
        c.id AS colaborador_id,
        c.nome AS colaborador_nome,
        c.cpf AS colaborador_cpf,
        c.email AS colaborador_email,
        c.cargo AS colaborador_cargo,
        c.tipocolaborador AS tipo_colaborador,
        CASE c.tipocolaborador
            WHEN 'F' THEN 'Funcionário'
            WHEN 'T' THEN 'Terceirizado'
            WHEN 'C' THEN 'Consultor'
            ELSE 'Desconhecido'
        END AS tipo_colaborador_descricao,
        emp.nome AS empresa_nome,
        COALESCE(cc.codigo || ' - ' || cc.nome, '') AS centro_custo,
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
    INNER JOIN requisicoes r ON r.colaboradorfinal = c.id
    INNER JOIN requisicoesitens ri ON ri.requisicao = r.id
    INNER JOIN equipamentos e ON e.id = ri.equipamento
    INNER JOIN tipoequipamentos te ON te.id = e.tipoequipamento
    LEFT JOIN fabricantes f ON f.id = e.fabricante
    LEFT JOIN modelos m ON m.id = e.modelo
    LEFT JOIN empresas emp ON emp.id = c.empresa
    LEFT JOIN centrocusto cc ON cc.id = c.centrocusto
    LEFT JOIN localidades loc ON loc.id = c.localidade
    WHERE ri.dtdevolucao IS NULL
      AND ri.equipamento IS NOT NULL
      AND (c.dtdemissao IS NULL OR c.dtdemissao > NOW())
      AND (e.tipoaquisicao IS NULL OR e.tipoaquisicao != 2)  -- Excluir BYOD (Particular)
      AND e.equipamentostatus = 4  -- Apenas equipamentos com status "Entregue"
),
contagem_equipamentos AS (
    -- Contar quantos equipamentos de cada tipo cada colaborador possui
    SELECT 
        colaborador_id,
        tipo_equipamento_id,
        COUNT(*) AS quantidade_atual
    FROM equipamentos_alocados
    GROUP BY colaborador_id, tipo_equipamento_id
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
SELECT 
    ea.colaborador_id,
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
    NOW() AS dt_geracao_relatorio
FROM equipamentos_alocados ea
LEFT JOIN politicas_aplicaveis pa ON 
    pa.colaborador_id = ea.colaborador_id 
    AND pa.tipo_equipamento_id = ea.tipo_equipamento_id
LEFT JOIN contagem_equipamentos ce ON 
    ce.colaborador_id = ea.colaborador_id 
    AND ce.tipo_equipamento_id = ea.tipo_equipamento_id
WHERE 
    (pa.politica_id IS NOT NULL AND pa.permite_acesso = false)
    OR (pa.politica_id IS NOT NULL 
        AND pa.permite_acesso = true 
        AND pa.quantidade_maxima IS NOT NULL 
        AND ce.quantidade_atual > pa.quantidade_maxima)
ORDER BY ea.colaborador_nome, ea.tipo_equipamento_descricao;

