-- ============================================================
-- SETUP TinOne - Tabela de Configuração (CORRIGIDO)
-- ============================================================
-- Cria tabela específica para configurações do TinOne
-- Não interfere com tabela 'parametros' existente
-- ============================================================

-- Criar tabela de configurações do TinOne
CREATE TABLE IF NOT EXISTS tinone_config (
    id SERIAL PRIMARY KEY,
    cliente INTEGER REFERENCES clientes(id),
    chave VARCHAR(100) NOT NULL,
    valor TEXT,
    descricao TEXT,
    ativo BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    
    -- Garante que cada chave seja única por cliente (ou global se cliente for NULL)
    CONSTRAINT unique_chave_cliente UNIQUE (cliente, chave)
);

-- Índices para performance
CREATE INDEX IF NOT EXISTS idx_tinone_config_cliente ON tinone_config(cliente);
CREATE INDEX IF NOT EXISTS idx_tinone_config_chave ON tinone_config(chave);
CREATE INDEX IF NOT EXISTS idx_tinone_config_ativo ON tinone_config(ativo);

-- Comentários
COMMENT ON TABLE tinone_config IS 'Configurações do assistente TinOne - tabela isolada';
COMMENT ON COLUMN tinone_config.cliente IS 'Cliente específico (NULL = global)';
COMMENT ON COLUMN tinone_config.chave IS 'Chave da configuração (ex: TINONE_HABILITADO)';
COMMENT ON COLUMN tinone_config.valor IS 'Valor da configuração';

-- ============================================================
-- Inserir configurações padrão do TinOne
-- ============================================================

-- Configurações globais (cliente = NULL)
INSERT INTO tinone_config (cliente, chave, valor, descricao, ativo) VALUES 
-- Controle principal
(NULL, 'TINONE_HABILITADO', 'true', 'Habilita/desabilita o assistente TinOne globalmente', true),

-- Funcionalidades específicas
(NULL, 'TINONE_CHAT_HABILITADO', 'true', 'Habilita funcionalidade de chat do TinOne', true),
(NULL, 'TINONE_TOOLTIPS_HABILITADO', 'true', 'Habilita tooltips contextuais nos campos', true),
(NULL, 'TINONE_GUIAS_HABILITADO', 'false', 'Habilita guias passo-a-passo (em desenvolvimento)', true),
(NULL, 'TINONE_SUGESTOES_PROATIVAS', 'false', 'Habilita sugestões proativas (beta)', true),

-- IA/NLP (desabilitado por padrão)
(NULL, 'TINONE_IA_HABILITADA', 'false', 'Habilita processamento com IA/NLP (requer Ollama local)', true),

-- Analytics e Debug
(NULL, 'TINONE_ANALYTICS', 'true', 'Habilita coleta de analytics de uso do TinOne', true),
(NULL, 'TINONE_DEBUG_MODE', 'false', 'Modo debug para desenvolvimento do TinOne', true),

-- Configurações visuais
(NULL, 'TINONE_POSICAO', 'bottom-right', 'Posição do widget: bottom-right, bottom-left', true),
(NULL, 'TINONE_COR_PRIMARIA', '#4a90e2', 'Cor primária do TinOne (hex)', true)

ON CONFLICT (cliente, chave) DO NOTHING;

-- ============================================================
-- Verificar configurações inseridas
-- ============================================================
SELECT 'TinOne configurado com sucesso!' as status;
SELECT COUNT(*) as total_configs FROM tinone_config;

-- ============================================================
-- COMANDOS ÚTEIS
-- ============================================================

-- Ver todas as configurações
-- SELECT * FROM tinone_config WHERE ativo = true ORDER BY chave;

-- Desabilitar TinOne globalmente
-- UPDATE tinone_config SET valor = 'false' WHERE chave = 'TINONE_HABILITADO' AND cliente IS NULL;

-- Desabilitar para um cliente específico
-- INSERT INTO tinone_config (cliente, chave, valor, descricao, ativo) 
-- VALUES (1, 'TINONE_HABILITADO', 'false', 'TinOne desabilitado para este cliente', true)
-- ON CONFLICT (cliente, chave) DO UPDATE SET valor = 'false';

-- Remover completamente o TinOne
-- DROP TABLE IF EXISTS tinone_processos_guiados CASCADE;
-- DROP TABLE IF EXISTS tinone_conversas CASCADE;
-- DROP TABLE IF EXISTS tinone_analytics CASCADE;
-- DROP TABLE IF EXISTS tinone_config CASCADE;

