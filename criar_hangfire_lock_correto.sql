-- =====================================================
-- CRIAR TABELA HANGFIRE.LOCK COM ESTRUTURA CORRETA
-- =====================================================
-- Baseado na estrutura esperada pelo Hangfire.PostgreSql 1.20.12

-- Garantir que o schema existe
CREATE SCHEMA IF NOT EXISTS hangfire;

-- Dar permissões
GRANT ALL PRIVILEGES ON SCHEMA hangfire TO postgres;
GRANT CREATE ON SCHEMA hangfire TO postgres;

-- Dropar se existir com estrutura incorreta
DROP TABLE IF EXISTS hangfire.lock CASCADE;

-- Criar tabela lock com a estrutura EXATA que o Hangfire espera
CREATE TABLE hangfire.lock (
    resource VARCHAR(100) NOT NULL PRIMARY KEY,
    acquired TIMESTAMP NOT NULL,
    expireat TIMESTAMP
);

-- Índice para performance
CREATE INDEX IF NOT EXISTS ix_hangfire_lock_expireat ON hangfire.lock(expireat);

-- Comentário
COMMENT ON TABLE hangfire.lock IS 'Tabela de locks distribuídos do Hangfire';

SELECT '✅ Tabela hangfire.lock criada com estrutura correta!' AS resultado;

