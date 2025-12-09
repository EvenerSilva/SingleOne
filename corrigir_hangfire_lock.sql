-- =====================================================
-- CORRIGIR TABELA HANGFIRE.LOCK
-- =====================================================
-- A tabela lock do Hangfire precisa de uma estrutura específica

-- Dropar a tabela se existir com estrutura incorreta
DROP TABLE IF EXISTS hangfire.lock CASCADE;

-- Criar a tabela lock com a estrutura correta do Hangfire
CREATE TABLE hangfire.lock (
    resource VARCHAR(100) NOT NULL PRIMARY KEY,
    acquired TIMESTAMP NOT NULL,
    expireat TIMESTAMP
);

-- Índice para performance
CREATE INDEX IF NOT EXISTS ix_hangfire_lock_expireat ON hangfire.lock(expireat);

-- Comentário
COMMENT ON TABLE hangfire.lock IS 'Tabela de locks distribuídos para controle de concorrência do Hangfire';

SELECT '✅ Tabela hangfire.lock corrigida!' AS resultado;

