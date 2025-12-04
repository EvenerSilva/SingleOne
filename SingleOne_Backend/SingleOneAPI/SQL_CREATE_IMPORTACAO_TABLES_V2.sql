-- ============================================
-- SCRIPT: Criar Tabelas de Importação de Linhas (VERSÃO 2 - SEGURA)
-- Data: 2025-10-25
-- Descrição: Tabelas staging e log para importação massiva de linhas telefônicas
-- ============================================

-- ⚠️ ATENÇÃO: Este script pode ser executado múltiplas vezes sem erro
-- Ele verifica se as tabelas existem antes de criar

-- ============================================
-- VERIFICAÇÃO: Quais tabelas de importação já existem?
-- ============================================
SELECT 
    table_name,
    'JÁ EXISTE' as status
FROM information_schema.tables 
WHERE table_schema = 'public' 
  AND table_name IN ('importacao_log', 'importacao_linha_staging')
ORDER BY table_name;

-- ============================================
-- LIMPEZA (OPCIONAL - COMENTE SE NÃO QUISER APAGAR DADOS EXISTENTES)
-- ============================================
-- DROP TABLE IF EXISTS importacao_linha_staging CASCADE;
-- DROP TABLE IF EXISTS importacao_log CASCADE;

-- ============================================
-- 1. Tabela de Log de Importações
-- ============================================
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
);

-- ============================================
-- 2. Tabela Staging para Linhas (dados temporários)
-- ============================================
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
);

-- ============================================
-- 3. Índices para performance (só cria se não existir)
-- ============================================
CREATE INDEX IF NOT EXISTS idx_importacao_log_lote 
    ON importacao_log(lote_id);

CREATE INDEX IF NOT EXISTS idx_importacao_log_cliente 
    ON importacao_log(cliente);

CREATE INDEX IF NOT EXISTS idx_importacao_log_usuario 
    ON importacao_log(usuario);

CREATE INDEX IF NOT EXISTS idx_importacao_log_status 
    ON importacao_log(status);

CREATE INDEX IF NOT EXISTS idx_staging_lote 
    ON importacao_linha_staging(lote_id);

CREATE INDEX IF NOT EXISTS idx_staging_cliente 
    ON importacao_linha_staging(cliente);

CREATE INDEX IF NOT EXISTS idx_staging_status 
    ON importacao_linha_staging(status);

CREATE INDEX IF NOT EXISTS idx_staging_numero 
    ON importacao_linha_staging(numero_linha);

-- ============================================
-- 4. Comentários das tabelas
-- ============================================
COMMENT ON TABLE importacao_log IS 'Histórico de importações de linhas telefônicas';
COMMENT ON TABLE importacao_linha_staging IS 'Dados temporários para validação antes da importação definitiva';

-- ============================================
-- 5. Comentários das colunas principais
-- ============================================
COMMENT ON COLUMN importacao_log.lote_id IS 'GUID único que identifica um lote de importação';
COMMENT ON COLUMN importacao_log.status IS 'PROCESSANDO, CONCLUIDO, ERRO, CANCELADO';
COMMENT ON COLUMN importacao_linha_staging.status IS 'P=Pendente, V=Validado, E=Erro, I=Importado';
COMMENT ON COLUMN importacao_linha_staging.mensagens_validacao IS 'JSON com erros e avisos da validação';

-- ============================================
-- 6. VERIFICAÇÃO FINAL: Confirmar que as tabelas foram criadas
-- ============================================
SELECT 
    table_name,
    '✅ CRIADA COM SUCESSO' as status
FROM information_schema.tables 
WHERE table_schema = 'public' 
  AND table_name IN ('importacao_log', 'importacao_linha_staging')
ORDER BY table_name;

-- ============================================
-- 7. CONTAGEM DE COLUNAS
-- ============================================
SELECT 
    table_name,
    COUNT(*) as total_colunas
FROM information_schema.columns
WHERE table_schema = 'public' 
  AND table_name IN ('importacao_log', 'importacao_linha_staging')
GROUP BY table_name
ORDER BY table_name;

-- ============================================
-- FIM DO SCRIPT
-- ============================================
-- ✅ Se você viu "CRIADA COM SUCESSO" acima, está tudo pronto!
-- 🔄 Agora REINICIE o backend e teste novamente

