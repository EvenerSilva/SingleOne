-- =====================================================
-- Script: Criar Tabelas de Políticas de Elegibilidade
-- Descrição: Sistema para controlar quais perfis de
--            colaboradores podem ter acesso a recursos
-- =====================================================

-- Tabela: politicas_elegibilidade
-- Armazena as regras de elegibilidade por tipo de colaborador e tipo de equipamento
CREATE TABLE IF NOT EXISTS politicas_elegibilidade (
    id SERIAL PRIMARY KEY,
    cliente INTEGER NOT NULL,
    tipo_colaborador VARCHAR(50) NOT NULL, -- 'Estagiário', 'CLT', 'Gerente', 'Diretor', etc
    cargo VARCHAR(100), -- Cargo específico (opcional, para filtro mais refinado)
    tipo_equipamento_id INTEGER NOT NULL, -- FK para tipoequipamentos
    permite_acesso BOOLEAN NOT NULL DEFAULT true, -- true = pode ter, false = não pode ter
    quantidade_maxima INTEGER, -- Quantidade máxima permitida (NULL = ilimitado)
    observacoes TEXT, -- Observações sobre a política
    ativo BOOLEAN NOT NULL DEFAULT true,
    dt_cadastro TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    dt_atualizacao TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    usuario_cadastro INTEGER, -- ID do usuário que cadastrou
    
    -- Foreign Keys
    CONSTRAINT fk_politica_cliente FOREIGN KEY (cliente) REFERENCES clientes(id) ON DELETE CASCADE,
    CONSTRAINT fk_politica_tipo_equipamento FOREIGN KEY (tipo_equipamento_id) REFERENCES tipoequipamentos(id) ON DELETE CASCADE,
    CONSTRAINT fk_politica_usuario FOREIGN KEY (usuario_cadastro) REFERENCES usuarios(id) ON DELETE SET NULL,
    
    -- Constraints
    CONSTRAINT uk_politica_elegibilidade UNIQUE (cliente, tipo_colaborador, cargo, tipo_equipamento_id)
);

-- Índices para melhor performance
CREATE INDEX idx_politica_cliente ON politicas_elegibilidade(cliente);
CREATE INDEX idx_politica_tipo_colaborador ON politicas_elegibilidade(tipo_colaborador);
CREATE INDEX idx_politica_tipo_equipamento ON politicas_elegibilidade(tipo_equipamento_id);
CREATE INDEX idx_politica_ativo ON politicas_elegibilidade(ativo);

-- Comentários nas colunas
COMMENT ON TABLE politicas_elegibilidade IS 'Armazena as políticas de elegibilidade de recursos por perfil de colaborador';
COMMENT ON COLUMN politicas_elegibilidade.tipo_colaborador IS 'Tipo do colaborador conforme campo tipocolaborador da tabela colaboradores (ex: E=Estagiário, C=CLT, etc)';
COMMENT ON COLUMN politicas_elegibilidade.cargo IS 'Cargo específico para refinamento da política (opcional)';
COMMENT ON COLUMN politicas_elegibilidade.permite_acesso IS 'Define se o perfil pode (true) ou não (false) ter acesso ao recurso';
COMMENT ON COLUMN politicas_elegibilidade.quantidade_maxima IS 'Quantidade máxima de recursos deste tipo que o colaborador pode ter (NULL = ilimitado)';

-- =====================================================
-- View: Relatório de Não Conformidade
-- Colaboradores que possuem recursos mas não são elegíveis
-- =====================================================

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
        ELSE c.tipocolaborador
    END AS tipo_colaborador_descricao,
    emp.razaosocial AS empresa_nome,
    cc.descricao AS centro_custo,
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
    -- Contagem de equipamentos do mesmo tipo que o colaborador possui
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
    c.situacao = 'A' -- Apenas colaboradores ativos
    AND e.ativo = true
    AND eh.dtretorno IS NULL -- Equipamento ainda está com o colaborador
    AND (
        -- Colaborador não tem permissão para este tipo de equipamento
        (pe.permite_acesso = false)
        OR
        -- Ou excedeu a quantidade máxima permitida
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

-- =====================================================
-- Dados Iniciais (Exemplos)
-- =====================================================

-- Inserir políticas exemplo para o cliente 2
-- Nota: Ajuste o cliente_id conforme seu ambiente

-- Exemplo 1: Estagiários NÃO podem ter Smartphones
-- INSERT INTO politicas_elegibilidade (cliente, tipo_colaborador, tipo_equipamento_id, permite_acesso, observacoes, usuario_cadastro)
-- SELECT 2, 'E', te.id, false, 'Estagiários não são elegíveis para smartphones corporativos', 1
-- FROM tipoequipamentos te
-- WHERE LOWER(te.descricao) LIKE '%smartphone%' OR LOWER(te.descricao) LIKE '%celular%'
-- AND te.ativo = true
-- LIMIT 1;

-- Exemplo 2: Estagiários NÃO podem ter Notebooks
-- INSERT INTO politicas_elegibilidade (cliente, tipo_colaborador, tipo_equipamento_id, permite_acesso, observacoes, usuario_cadastro)
-- SELECT 2, 'E', te.id, false, 'Estagiários não são elegíveis para notebooks corporativos', 1
-- FROM tipoequipamentos te
-- WHERE LOWER(te.descricao) LIKE '%notebook%' OR LOWER(te.descricao) LIKE '%laptop%'
-- AND te.ativo = true
-- LIMIT 1;

-- Exemplo 3: CLT pode ter no máximo 1 smartphone
-- INSERT INTO politicas_elegibilidade (cliente, tipo_colaborador, tipo_equipamento_id, permite_acesso, quantidade_maxima, observacoes, usuario_cadastro)
-- SELECT 2, 'C', te.id, true, 1, 'Colaboradores CLT podem ter no máximo 1 smartphone', 1
-- FROM tipoequipamentos te
-- WHERE LOWER(te.descricao) LIKE '%smartphone%' OR LOWER(te.descricao) LIKE '%celular%'
-- AND te.ativo = true
-- LIMIT 1;

-- Exemplo 4: Gerentes podem ter no máximo 2 notebooks
-- INSERT INTO politicas_elegibilidade (cliente, tipo_colaborador, tipo_equipamento_id, permite_acesso, quantidade_maxima, observacoes, usuario_cadastro)
-- SELECT 2, 'G', te.id, true, 2, 'Gerentes podem ter no máximo 2 notebooks', 1
-- FROM tipoequipamentos te
-- WHERE LOWER(te.descricao) LIKE '%notebook%' OR LOWER(te.descricao) LIKE '%laptop%'
-- AND te.ativo = true
-- LIMIT 1;

COMMENT ON TABLE politicas_elegibilidade IS 'Políticas de Elegibilidade - Define quais perfis de colaboradores podem ter acesso a determinados tipos de recursos';

-- =====================================================
-- FIM DO SCRIPT
-- =====================================================

