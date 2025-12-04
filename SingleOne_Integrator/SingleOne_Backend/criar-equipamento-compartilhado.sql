-- ============================================================================
-- Script: Implementação de Equipamento Compartilhado
-- Descrição: Adiciona funcionalidade de múltiplos usuários para equipamentos
-- Data: 03/10/2025
-- ============================================================================

-- ============================================================================
-- PARTE 1: Adicionar campo 'compartilhado' na tabela equipamentos
-- ============================================================================

-- Verificar se a coluna já existe antes de adicionar
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'equipamentos' 
        AND column_name = 'compartilhado'
    ) THEN
        ALTER TABLE equipamentos 
        ADD COLUMN compartilhado BOOLEAN DEFAULT FALSE NOT NULL;
        
        RAISE NOTICE 'Coluna "compartilhado" adicionada com sucesso na tabela equipamentos';
    ELSE
        RAISE NOTICE 'Coluna "compartilhado" já existe na tabela equipamentos';
    END IF;
END $$;

-- Criar índice para performance
CREATE INDEX IF NOT EXISTS idx_equipamentos_compartilhado 
ON equipamentos(compartilhado) 
WHERE compartilhado = TRUE;

-- Adicionar comentário na coluna
COMMENT ON COLUMN equipamentos.compartilhado IS 'Indica se o equipamento permite múltiplos usuários';

-- ============================================================================
-- PARTE 2: Criar tabela equipamento_usuarios_compartilhados
-- ============================================================================

-- Verificar se a tabela já existe antes de criar
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.tables 
        WHERE table_name = 'equipamento_usuarios_compartilhados'
    ) THEN
        CREATE TABLE equipamento_usuarios_compartilhados (
            id SERIAL PRIMARY KEY,
            equipamento_id INTEGER NOT NULL,
            colaborador_id INTEGER NOT NULL,
            data_inicio TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
            data_fim TIMESTAMP NULL,
            ativo BOOLEAN DEFAULT TRUE NOT NULL,
            tipo_acesso VARCHAR(50) DEFAULT 'usuario_compartilhado' NOT NULL,
            observacao TEXT NULL,
            criado_por INTEGER NOT NULL,
            criado_em TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
            
            -- Foreign Keys
            CONSTRAINT fk_equip_usuarios_comp_equipamento 
                FOREIGN KEY (equipamento_id) 
                REFERENCES equipamentos(id) 
                ON DELETE CASCADE,
            
            CONSTRAINT fk_equip_usuarios_comp_colaborador 
                FOREIGN KEY (colaborador_id) 
                REFERENCES colaboradores(id) 
                ON DELETE CASCADE,
            
            CONSTRAINT fk_equip_usuarios_comp_criado_por 
                FOREIGN KEY (criado_por) 
                REFERENCES usuarios(id) 
                ON DELETE RESTRICT,
            
            -- Garantir que tipo_acesso tenha valor válido
            CONSTRAINT chk_tipo_acesso 
                CHECK (tipo_acesso IN ('usuario_compartilhado', 'temporario', 'turno')),
            
            -- Evitar duplicatas: mesma combinação ativa de equipamento + colaborador
            CONSTRAINT uk_equipamento_colaborador_ativo 
                UNIQUE NULLS NOT DISTINCT (equipamento_id, colaborador_id, 
                    CASE WHEN ativo = TRUE THEN TRUE ELSE NULL END)
        );
        
        RAISE NOTICE 'Tabela "equipamento_usuarios_compartilhados" criada com sucesso';
    ELSE
        RAISE NOTICE 'Tabela "equipamento_usuarios_compartilhados" já existe';
    END IF;
END $$;

-- ============================================================================
-- PARTE 3: Criar índices para performance
-- ============================================================================

CREATE INDEX IF NOT EXISTS idx_equip_usuarios_comp_equipamento 
ON equipamento_usuarios_compartilhados(equipamento_id);

CREATE INDEX IF NOT EXISTS idx_equip_usuarios_comp_colaborador 
ON equipamento_usuarios_compartilhados(colaborador_id);

CREATE INDEX IF NOT EXISTS idx_equip_usuarios_comp_ativo 
ON equipamento_usuarios_compartilhados(ativo) 
WHERE ativo = TRUE;

CREATE INDEX IF NOT EXISTS idx_equip_usuarios_comp_tipo 
ON equipamento_usuarios_compartilhados(tipo_acesso);

CREATE INDEX IF NOT EXISTS idx_equip_usuarios_comp_data_fim 
ON equipamento_usuarios_compartilhados(data_fim) 
WHERE data_fim IS NOT NULL;

-- Índice composto para consultas comuns
CREATE INDEX IF NOT EXISTS idx_equip_usuarios_comp_eq_ativo 
ON equipamento_usuarios_compartilhados(equipamento_id, ativo);

-- ============================================================================
-- PARTE 4: Adicionar comentários nas colunas para documentação
-- ============================================================================

COMMENT ON TABLE equipamento_usuarios_compartilhados IS 
'Gerencia múltiplos usuários para equipamentos compartilhados. Permite que um equipamento tenha um responsável principal e vários usuários secundários.';

COMMENT ON COLUMN equipamento_usuarios_compartilhados.equipamento_id IS 
'ID do equipamento compartilhado';

COMMENT ON COLUMN equipamento_usuarios_compartilhados.colaborador_id IS 
'ID do colaborador que tem acesso ao equipamento';

COMMENT ON COLUMN equipamento_usuarios_compartilhados.data_inicio IS 
'Data de início do acesso do colaborador ao equipamento';

COMMENT ON COLUMN equipamento_usuarios_compartilhados.data_fim IS 
'Data de fim do acesso. NULL = acesso indefinido; Preenchido = acesso temporário';

COMMENT ON COLUMN equipamento_usuarios_compartilhados.ativo IS 
'Indica se o acesso está ativo. Seguindo o padrão do sistema, não deletamos registros, apenas inativamos.';

COMMENT ON COLUMN equipamento_usuarios_compartilhados.tipo_acesso IS 
'Tipo de acesso do colaborador: usuario_compartilhado (padrão), temporario (acesso por período), turno (uso por turno de trabalho)';

COMMENT ON COLUMN equipamento_usuarios_compartilhados.observacao IS 
'Campo livre para observações sobre o compartilhamento (motivo, restrições, etc)';

COMMENT ON COLUMN equipamento_usuarios_compartilhados.criado_por IS 
'ID do usuário que criou o registro de compartilhamento';

COMMENT ON COLUMN equipamento_usuarios_compartilhados.criado_em IS 
'Data e hora de criação do registro';

-- ============================================================================
-- PARTE 5: Criar view para facilitar consultas
-- ============================================================================

CREATE OR REPLACE VIEW vw_equipamentos_compartilhados AS
SELECT 
    e.id AS equipamento_id,
    e.patrimonio,
    e.numeroserie,
    e.compartilhado,
    
    -- Responsável principal
    e.usuario AS responsavel_principal_id,
    u_resp.nome AS responsavel_principal_nome,
    
    -- Informações do tipo e modelo
    te.descricao AS tipo_equipamento,
    m.descricao AS modelo,
    f.descricao AS fabricante,
    
    -- Status
    es.descricao AS status,
    
    -- Localização
    l.nome AS localidade,
    emp.razaosocial AS empresa,
    
    -- Quantidade de usuários compartilhados ativos
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

COMMENT ON VIEW vw_equipamentos_compartilhados IS 
'View que lista todos os equipamentos compartilhados com informações resumidas';

-- ============================================================================
-- PARTE 6: Criar view detalhada com usuários
-- ============================================================================

CREATE OR REPLACE VIEW vw_equipamentos_usuarios_compartilhados AS
SELECT 
    euc.id,
    euc.equipamento_id,
    e.patrimonio,
    e.numeroserie,
    
    -- Usuário compartilhado
    euc.colaborador_id,
    c.nome AS colaborador_nome,
    c.matricula AS colaborador_matricula,
    c.email AS colaborador_email,
    c.cargo AS colaborador_cargo,
    
    -- Informações do compartilhamento
    euc.data_inicio,
    euc.data_fim,
    euc.ativo,
    euc.tipo_acesso,
    euc.observacao,
    
    -- Auditoria
    euc.criado_por,
    u_criador.nome AS criado_por_nome,
    euc.criado_em,
    
    -- Status do acesso
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

COMMENT ON VIEW vw_equipamentos_usuarios_compartilhados IS 
'View detalhada que lista todos os usuários compartilhados de equipamentos com informações completas';

-- ============================================================================
-- PARTE 7: Criar função para adicionar usuário compartilhado
-- ============================================================================

CREATE OR REPLACE FUNCTION fn_adicionar_usuario_compartilhado(
    p_equipamento_id INTEGER,
    p_colaborador_id INTEGER,
    p_data_inicio TIMESTAMP,
    p_data_fim TIMESTAMP,
    p_tipo_acesso VARCHAR(50),
    p_observacao TEXT,
    p_criado_por INTEGER
)
RETURNS INTEGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_novo_id INTEGER;
    v_equipamento_compartilhado BOOLEAN;
BEGIN
    -- Verificar se equipamento existe e está marcado como compartilhado
    SELECT compartilhado INTO v_equipamento_compartilhado
    FROM equipamentos
    WHERE id = p_equipamento_id AND ativo = TRUE;
    
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Equipamento não encontrado ou está inativo';
    END IF;
    
    IF v_equipamento_compartilhado = FALSE THEN
        RAISE EXCEPTION 'Equipamento não está marcado como compartilhado';
    END IF;
    
    -- Verificar se colaborador existe e está ativo
    IF NOT EXISTS (
        SELECT 1 FROM colaboradores 
        WHERE id = p_colaborador_id AND situacao = 'A'
    ) THEN
        RAISE EXCEPTION 'Colaborador não encontrado ou está inativo';
    END IF;
    
    -- Verificar se já não existe registro ativo
    IF EXISTS (
        SELECT 1 FROM equipamento_usuarios_compartilhados
        WHERE equipamento_id = p_equipamento_id 
        AND colaborador_id = p_colaborador_id 
        AND ativo = TRUE
    ) THEN
        RAISE EXCEPTION 'Colaborador já está cadastrado como usuário ativo deste equipamento';
    END IF;
    
    -- Inserir registro
    INSERT INTO equipamento_usuarios_compartilhados (
        equipamento_id, colaborador_id, data_inicio, data_fim,
        tipo_acesso, observacao, criado_por, ativo
    ) VALUES (
        p_equipamento_id, p_colaborador_id, p_data_inicio, p_data_fim,
        p_tipo_acesso, p_observacao, p_criado_por, TRUE
    )
    RETURNING id INTO v_novo_id;
    
    RETURN v_novo_id;
END;
$$;

COMMENT ON FUNCTION fn_adicionar_usuario_compartilhado IS 
'Função para adicionar usuário compartilhado com validações';

-- ============================================================================
-- PARTE 8: Criar função para remover (inativar) usuário compartilhado
-- ============================================================================

CREATE OR REPLACE FUNCTION fn_remover_usuario_compartilhado(
    p_id INTEGER,
    p_usuario_id INTEGER
)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
BEGIN
    -- Atualizar registro para inativo
    UPDATE equipamento_usuarios_compartilhados
    SET ativo = FALSE,
        data_fim = CURRENT_TIMESTAMP
    WHERE id = p_id;
    
    IF NOT FOUND THEN
        RETURN FALSE;
    END IF;
    
    RETURN TRUE;
END;
$$;

COMMENT ON FUNCTION fn_remover_usuario_compartilhado IS 
'Função para inativar usuário compartilhado (não deleta, apenas marca como inativo)';

-- ============================================================================
-- PARTE 9: Criar trigger para validações automáticas
-- ============================================================================

CREATE OR REPLACE FUNCTION fn_validar_equipamento_compartilhado()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    -- Validar tipo de acesso
    IF NEW.tipo_acesso NOT IN ('usuario_compartilhado', 'temporario', 'turno') THEN
        RAISE EXCEPTION 'Tipo de acesso inválido: %', NEW.tipo_acesso;
    END IF;
    
    -- Se for temporário, data_fim deve ser preenchida
    IF NEW.tipo_acesso = 'temporario' AND NEW.data_fim IS NULL THEN
        RAISE EXCEPTION 'Para tipo de acesso temporário, a data de fim deve ser informada';
    END IF;
    
    -- Data fim deve ser maior que data início
    IF NEW.data_fim IS NOT NULL AND NEW.data_fim <= NEW.data_inicio THEN
        RAISE EXCEPTION 'Data de fim deve ser maior que data de início';
    END IF;
    
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_validar_equipamento_compartilhado 
ON equipamento_usuarios_compartilhados;

CREATE TRIGGER trg_validar_equipamento_compartilhado
    BEFORE INSERT OR UPDATE ON equipamento_usuarios_compartilhados
    FOR EACH ROW
    EXECUTE FUNCTION fn_validar_equipamento_compartilhado();

-- ============================================================================
-- PARTE 10: Consultas de verificação
-- ============================================================================

-- Verificar estrutura criada
SELECT 
    'Coluna compartilhado existe' AS verificacao,
    CASE WHEN EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'equipamentos' AND column_name = 'compartilhado'
    ) THEN '✓ OK' ELSE '✗ FALHA' END AS status
UNION ALL
SELECT 
    'Tabela equipamento_usuarios_compartilhados existe',
    CASE WHEN EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_name = 'equipamento_usuarios_compartilhados'
    ) THEN '✓ OK' ELSE '✗ FALHA' END
UNION ALL
SELECT 
    'View vw_equipamentos_compartilhados existe',
    CASE WHEN EXISTS (
        SELECT 1 FROM information_schema.views 
        WHERE table_name = 'vw_equipamentos_compartilhados'
    ) THEN '✓ OK' ELSE '✗ FALHA' END
UNION ALL
SELECT 
    'View vw_equipamentos_usuarios_compartilhados existe',
    CASE WHEN EXISTS (
        SELECT 1 FROM information_schema.views 
        WHERE table_name = 'vw_equipamentos_usuarios_compartilhados'
    ) THEN '✓ OK' ELSE '✗ FALHA' END;

-- Contar índices criados
SELECT 
    'Total de índices criados' AS verificacao,
    COUNT(*)::TEXT || ' índices' AS status
FROM pg_indexes 
WHERE tablename = 'equipamento_usuarios_compartilhados';

-- ============================================================================
-- Script executado com sucesso!
-- ============================================================================

\echo ''
\echo '============================================================================'
\echo 'Script de criação de equipamento compartilhado executado com sucesso!'
\echo '============================================================================'
\echo 'Estruturas criadas:'
\echo '  - Coluna "compartilhado" na tabela equipamentos'
\echo '  - Tabela equipamento_usuarios_compartilhados'
\echo '  - Índices de performance'
\echo '  - Views vw_equipamentos_compartilhados e vw_equipamentos_usuarios_compartilhados'
\echo '  - Funções fn_adicionar_usuario_compartilhado e fn_remover_usuario_compartilhado'
\echo '  - Trigger de validação'
\echo '============================================================================'
\echo ''

