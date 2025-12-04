-- ============================================
-- SCRIPT: Criar Tabelas de Importação de Linhas
-- Data: 2025-10-25
-- Descrição: Tabelas staging e log para importação massiva de linhas telefônicas
-- ============================================

-- 1. Tabela de Log de Importações
CREATE TABLE IF NOT EXISTS importacao_log (
    id SERIAL PRIMARY KEY,
    lote_id UUID NOT NULL,
    cliente INTEGER NOT NULL,
    usuario INTEGER NOT NULL,
    tipo_importacao VARCHAR(50) NOT NULL,
    data_inicio TIMESTAMP NOT NULL,
    data_fim TIMESTAMP NULL,
    status VARCHAR(50) NOT NULL,
    total_registros INTEGER NOT NULL DEFAULT 0,
    total_validados INTEGER NOT NULL DEFAULT 0,
    total_erros INTEGER NOT NULL DEFAULT 0,
    total_importados INTEGER NOT NULL DEFAULT 0,
    nome_arquivo VARCHAR(255) NULL,
    observacoes TEXT NULL
    
    -- Nota: Foreign Key será adicionada depois se necessário
    -- CONSTRAINT fk_importacao_log_usuario FOREIGN KEY (usuario) REFERENCES usuario(id)
);

-- 2. Tabela Staging para Linhas (dados temporários)
CREATE TABLE IF NOT EXISTS importacao_linha_staging (
    id SERIAL PRIMARY KEY,
    cliente INTEGER NOT NULL,
    lote_id UUID NOT NULL,
    usuario_importacao INTEGER NOT NULL,
    data_importacao TIMESTAMP NOT NULL,
    
    -- Dados do arquivo
    operadora_nome VARCHAR(255) NULL,
    contrato_nome VARCHAR(255) NULL,
    plano_nome VARCHAR(255) NULL,
    plano_valor DECIMAL(18,2) NOT NULL DEFAULT 0,
    numero_linha DECIMAL(18,0) NOT NULL,
    iccid VARCHAR(50) NULL,
    
    -- Status da validação
    status CHAR(1) NOT NULL DEFAULT 'P', -- P=Pendente, V=Validado, E=Erro, I=Importado
    mensagens_validacao TEXT NULL,
    linha_arquivo INTEGER NOT NULL,
    
    -- IDs encontrados ou a criar
    operadora_id INTEGER NULL,
    contrato_id INTEGER NULL,
    plano_id INTEGER NULL,
    
    -- Flags de criação
    criar_operadora BOOLEAN NOT NULL DEFAULT FALSE,
    criar_contrato BOOLEAN NOT NULL DEFAULT FALSE,
    criar_plano BOOLEAN NOT NULL DEFAULT FALSE
    
    -- Nota: Foreign Key será adicionada depois se necessário
    -- CONSTRAINT fk_staging_usuario FOREIGN KEY (usuario_importacao) REFERENCES usuario(id)
);

-- 3. Índices para performance
CREATE INDEX IF NOT EXISTS idx_importacao_log_lote 
    ON importacao_log(lote_id);

CREATE INDEX IF NOT EXISTS idx_importacao_log_cliente 
    ON importacao_log(cliente);

CREATE INDEX IF NOT EXISTS idx_importacao_log_usuario 
    ON importacao_log(usuario);

CREATE INDEX IF NOT EXISTS idx_staging_lote 
    ON importacao_linha_staging(lote_id);

CREATE INDEX IF NOT EXISTS idx_staging_cliente 
    ON importacao_linha_staging(cliente);

CREATE INDEX IF NOT EXISTS idx_staging_status 
    ON importacao_linha_staging(status);

CREATE INDEX IF NOT EXISTS idx_staging_numero 
    ON importacao_linha_staging(numero_linha);

-- 4. Comentários das tabelas
COMMENT ON TABLE importacao_log IS 'Histórico de importações de linhas telefônicas';
COMMENT ON TABLE importacao_linha_staging IS 'Dados temporários para validação antes da importação definitiva';

-- 5. Comentários das colunas principais
COMMENT ON COLUMN importacao_log.lote_id IS 'GUID único que identifica um lote de importação';
COMMENT ON COLUMN importacao_log.status IS 'PROCESSANDO, CONCLUIDO, ERRO, CANCELADO';
COMMENT ON COLUMN importacao_linha_staging.status IS 'P=Pendente, V=Validado, E=Erro, I=Importado';
COMMENT ON COLUMN importacao_linha_staging.mensagens_validacao IS 'JSON com erros e avisos da validação';

-- ============================================
-- FIM DO SCRIPT
-- ============================================

-- Para verificar se as tabelas foram criadas:
-- SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name LIKE 'importacao%';

