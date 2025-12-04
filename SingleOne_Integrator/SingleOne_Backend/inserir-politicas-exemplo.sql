-- =====================================================
-- Políticas de Elegibilidade - Exemplos
-- =====================================================

-- Obter o ID do cliente (assumindo cliente 2)
-- Se você tiver outro ID de cliente, ajuste o valor abaixo

-- POLÍTICA 1: Estagiários NÃO podem ter Smartphones
INSERT INTO politicas_elegibilidade 
    (cliente, tipo_colaborador, tipo_equipamento_id, permite_acesso, observacoes, usuario_cadastro, ativo)
VALUES 
    (1, 'E', 4, false, 'Estagiários não são elegíveis para smartphones corporativos conforme política da empresa', 1, true);

-- POLÍTICA 2: Estagiários NÃO podem ter Notebooks
INSERT INTO politicas_elegibilidade 
    (cliente, tipo_colaborador, tipo_equipamento_id, permite_acesso, observacoes, usuario_cadastro, ativo)
VALUES 
    (1, 'E', 2, false, 'Estagiários não são elegíveis para notebooks corporativos conforme política da empresa', 1, true);

-- POLÍTICA 3: CLT pode ter no máximo 1 Smartphone
INSERT INTO politicas_elegibilidade 
    (cliente, tipo_colaborador, tipo_equipamento_id, permite_acesso, quantidade_maxima, observacoes, usuario_cadastro, ativo)
VALUES 
    (1, 'C', 4, true, 1, 'Colaboradores CLT podem ter no máximo 1 smartphone', 1, true);

-- POLÍTICA 4: CLT pode ter no máximo 1 Notebook
INSERT INTO politicas_elegibilidade 
    (cliente, tipo_colaborador, tipo_equipamento_id, permite_acesso, quantidade_maxima, observacoes, usuario_cadastro, ativo)
VALUES 
    (1, 'C', 2, true, 1, 'Colaboradores CLT podem ter no máximo 1 notebook', 1, true);

-- POLÍTICA 5: Gerentes podem ter no máximo 2 Smartphones
INSERT INTO politicas_elegibilidade 
    (cliente, tipo_colaborador, tipo_equipamento_id, permite_acesso, quantidade_maxima, observacoes, usuario_cadastro, ativo)
VALUES 
    (1, 'G', 4, true, 2, 'Gerentes podem ter no máximo 2 smartphones', 1, true);

-- POLÍTICA 6: Gerentes podem ter no máximo 2 Notebooks
INSERT INTO politicas_elegibilidade 
    (cliente, tipo_colaborador, tipo_equipamento_id, permite_acesso, quantidade_maxima, observacoes, usuario_cadastro, ativo)
VALUES 
    (1, 'G', 2, true, 2, 'Gerentes podem ter no máximo 2 notebooks', 1, true);

-- Ver políticas criadas
SELECT 
    pe.id,
    pe.tipo_colaborador,
    CASE pe.tipo_colaborador
        WHEN 'E' THEN 'Estagiário'
        WHEN 'C' THEN 'CLT'
        WHEN 'G' THEN 'Gerente'
        ELSE pe.tipo_colaborador
    END as tipo_descricao,
    te.descricao as equipamento,
    pe.permite_acesso,
    pe.quantidade_maxima,
    pe.observacoes
FROM politicas_elegibilidade pe
LEFT JOIN tipoequipamentos te ON pe.tipo_equipamento_id = te.id
WHERE pe.ativo = true
ORDER BY pe.tipo_colaborador, te.descricao;

