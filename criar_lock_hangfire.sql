-- =====================================================
-- CRIAR TABELA LOCK DO HANGFIRE
-- =====================================================
-- Esta tabela é necessária para controle de concorrência do Hangfire

-- Garantir que o schema existe
CREATE SCHEMA IF NOT EXISTS hangfire;

-- Criar a tabela lock
CREATE TABLE IF NOT EXISTS hangfire.lock (
    resource VARCHAR(100) NOT NULL PRIMARY KEY,
    acquired TIMESTAMP NOT NULL,
    expireat TIMESTAMP
);

-- Criar índice para performance
CREATE INDEX IF NOT EXISTS ix_hangfire_lock_expireat ON hangfire.lock(expireat);

-- Garantir permissões
GRANT USAGE ON SCHEMA hangfire TO postgres;
GRANT ALL PRIVILEGES ON TABLE hangfire.lock TO postgres;

