-- ============================================================
-- TABELAS DO TinOne - Analytics e Feedback
-- ============================================================
-- Tabelas opcionais para analytics do assistente
-- Totalmente isoladas - podem ser removidas sem impacto
-- ============================================================

-- Tabela de analytics de uso do TinOne
CREATE TABLE IF NOT EXISTS tinone_analytics (
    id SERIAL PRIMARY KEY,
    usuario_id INTEGER REFERENCES usuarios(id),
    cliente_id INTEGER REFERENCES clientes(id),
    sessao_id VARCHAR(100),
    
    -- Contexto da interação
    pagina_url VARCHAR(500),
    pagina_nome VARCHAR(200),
    acao_tipo VARCHAR(100), -- 'pergunta', 'tooltip', 'guia', 'navegacao'
    
    -- Pergunta/Resposta
    pergunta TEXT,
    resposta TEXT,
    tempo_resposta_ms INTEGER,
    
    -- Feedback
    foi_util BOOLEAN,
    feedback_texto TEXT,
    
    -- Metadados
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Índices para performance
CREATE INDEX IF NOT EXISTS idx_tinone_usuario ON tinone_analytics(usuario_id);
CREATE INDEX IF NOT EXISTS idx_tinone_cliente ON tinone_analytics(cliente_id);
CREATE INDEX IF NOT EXISTS idx_tinone_sessao ON tinone_analytics(sessao_id);
CREATE INDEX IF NOT EXISTS idx_tinone_created ON tinone_analytics(created_at);
CREATE INDEX IF NOT EXISTS idx_tinone_acao ON tinone_analytics(acao_tipo);

-- Comentários
COMMENT ON TABLE tinone_analytics IS 'Analytics de uso do assistente TinOne - tabela isolada e opcional';
COMMENT ON COLUMN tinone_analytics.acao_tipo IS 'Tipos: pergunta_chat, tooltip_campo, guia_processo, navegacao_assistida';
COMMENT ON COLUMN tinone_analytics.foi_util IS 'Feedback do usuário se a resposta foi útil (thumbs up/down)';

-- ============================================================
-- Tabela de conversas do TinOne (opcional - para contexto)
-- ============================================================
CREATE TABLE IF NOT EXISTS tinone_conversas (
    id SERIAL PRIMARY KEY,
    usuario_id INTEGER REFERENCES usuarios(id),
    sessao_id VARCHAR(100),
    
    -- Mensagem
    tipo_mensagem VARCHAR(20), -- 'usuario' ou 'assistente'
    mensagem TEXT NOT NULL,
    
    -- Contexto
    pagina_contexto VARCHAR(200),
    metadata JSONB, -- Dados extras em JSON
    
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_tinone_conv_usuario ON tinone_conversas(usuario_id);
CREATE INDEX IF NOT EXISTS idx_tinone_conv_sessao ON tinone_conversas(sessao_id);
CREATE INDEX IF NOT EXISTS idx_tinone_conv_created ON tinone_conversas(created_at);

COMMENT ON TABLE tinone_conversas IS 'Histórico de conversas do TinOne - permite contexto em múltiplas perguntas';

-- ============================================================
-- Tabela de processos guiados (rastreamento de conclusão)
-- ============================================================
CREATE TABLE IF NOT EXISTS tinone_processos_guiados (
    id SERIAL PRIMARY KEY,
    usuario_id INTEGER REFERENCES usuarios(id),
    processo_id VARCHAR(100), -- Ex: 'criar-requisicao'
    processo_nome VARCHAR(200),
    
    -- Status
    iniciado_em TIMESTAMP DEFAULT NOW(),
    concluido_em TIMESTAMP,
    abandonado_em TIMESTAMP,
    status VARCHAR(50), -- 'em_andamento', 'concluido', 'abandonado'
    
    -- Progresso
    passo_atual INTEGER,
    total_passos INTEGER,
    passos_concluidos JSONB, -- Array de IDs dos passos concluídos
    
    -- Metadados
    tempo_total_segundos INTEGER,
    
    CONSTRAINT chk_status CHECK (status IN ('em_andamento', 'concluido', 'abandonado'))
);

CREATE INDEX IF NOT EXISTS idx_tinone_proc_usuario ON tinone_processos_guiados(usuario_id);
CREATE INDEX IF NOT EXISTS idx_tinone_proc_status ON tinone_processos_guiados(status);
CREATE INDEX IF NOT EXISTS idx_tinone_proc_id ON tinone_processos_guiados(processo_id);

COMMENT ON TABLE tinone_processos_guiados IS 'Rastreamento de processos guiados pelo TinOne - útil para analytics';

-- ============================================================
-- View de estatísticas (opcional)
-- ============================================================
CREATE OR REPLACE VIEW vw_tinone_estatisticas AS
SELECT 
    COUNT(*) as total_interacoes,
    COUNT(DISTINCT usuario_id) as usuarios_unicos,
    COUNT(DISTINCT sessao_id) as sessoes_unicas,
    AVG(tempo_resposta_ms) as tempo_medio_resposta,
    COUNT(CASE WHEN foi_util = true THEN 1 END) as feedbacks_positivos,
    COUNT(CASE WHEN foi_util = false THEN 1 END) as feedbacks_negativos,
    DATE(created_at) as data
FROM tinone_analytics
GROUP BY DATE(created_at)
ORDER BY data DESC;

COMMENT ON VIEW vw_tinone_estatisticas IS 'Estatísticas diárias de uso do TinOne';

-- ============================================================
-- REMOVER TABELAS (se necessário fazer rollback completo)
-- ============================================================
-- DROP VIEW IF EXISTS vw_tinone_estatisticas CASCADE;
-- DROP TABLE IF EXISTS tinone_processos_guiados CASCADE;
-- DROP TABLE IF EXISTS tinone_conversas CASCADE;
-- DROP TABLE IF EXISTS tinone_analytics CASCADE;

