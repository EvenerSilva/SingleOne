-- =====================================================
-- CRIAR TODAS AS TABELAS DO HANGFIRE MANUALMENTE
-- =====================================================
-- Baseado na estrutura do Hangfire.PostgreSql 1.20.12

-- Garantir que o schema existe
CREATE SCHEMA IF NOT EXISTS hangfire;

-- Dar permissões
GRANT ALL PRIVILEGES ON SCHEMA hangfire TO postgres;
GRANT CREATE ON SCHEMA hangfire TO postgres;

-- Remover todas as tabelas existentes primeiro
DROP TABLE IF EXISTS hangfire.aggregatedcounter CASCADE;
DROP TABLE IF EXISTS hangfire.counter CASCADE;
DROP TABLE IF EXISTS hangfire.set CASCADE;
DROP TABLE IF EXISTS hangfire.list CASCADE;
DROP TABLE IF EXISTS hangfire.hash CASCADE;
DROP TABLE IF EXISTS hangfire.jobqueue CASCADE;
DROP TABLE IF EXISTS hangfire.jobparameter CASCADE;
DROP TABLE IF EXISTS hangfire.state CASCADE;
DROP TABLE IF EXISTS hangfire.job CASCADE;
DROP TABLE IF EXISTS hangfire.server CASCADE;
DROP TABLE IF EXISTS hangfire.lock CASCADE;

-- Criar tabelas na ordem correta (respeitando foreign keys)

-- 1. Tabela lock (sem dependências)
CREATE TABLE hangfire.lock (
    resource VARCHAR(100) NOT NULL PRIMARY KEY,
    acquired TIMESTAMP NOT NULL,
    expireat TIMESTAMP
);

-- 2. Tabela job (sem dependências)
CREATE TABLE hangfire.job (
    id BIGSERIAL PRIMARY KEY,
    stateid BIGINT,
    statename VARCHAR(20),
    invocationdata TEXT NOT NULL,
    arguments TEXT NOT NULL,
    createdat TIMESTAMP NOT NULL,
    expireat TIMESTAMP
);

-- 3. Tabela state (depende de job)
CREATE TABLE hangfire.state (
    id BIGSERIAL PRIMARY KEY,
    jobid BIGINT NOT NULL,
    name VARCHAR(20) NOT NULL,
    reason TEXT,
    createdat TIMESTAMP NOT NULL,
    data TEXT,
    FOREIGN KEY (jobid) REFERENCES hangfire.job(id) ON DELETE CASCADE
);

-- 4. Tabela jobparameter (depende de job)
CREATE TABLE hangfire.jobparameter (
    jobid BIGINT NOT NULL,
    name VARCHAR(40) NOT NULL,
    value TEXT,
    PRIMARY KEY (jobid, name),
    FOREIGN KEY (jobid) REFERENCES hangfire.job(id) ON DELETE CASCADE
);

-- 5. Tabela jobqueue (depende de job)
CREATE TABLE hangfire.jobqueue (
    id BIGSERIAL PRIMARY KEY,
    jobid BIGINT NOT NULL,
    queue VARCHAR(50) NOT NULL,
    fetchedat TIMESTAMP,
    FOREIGN KEY (jobid) REFERENCES hangfire.job(id) ON DELETE CASCADE
);

-- 6. Tabela server (sem dependências)
CREATE TABLE hangfire.server (
    id VARCHAR(100) PRIMARY KEY,
    data TEXT NOT NULL,
    lastheartbeat TIMESTAMP NOT NULL,
    heartbeatinterval INTEGER NOT NULL DEFAULT 15
);

-- 7. Tabela hash
CREATE TABLE hangfire.hash (
    key VARCHAR(100) NOT NULL,
    field VARCHAR(100) NOT NULL,
    value TEXT,
    expireat TIMESTAMP,
    PRIMARY KEY (key, field)
);

-- 8. Tabela list
CREATE TABLE hangfire.list (
    key VARCHAR(100) NOT NULL,
    value TEXT,
    expireat TIMESTAMP,
    PRIMARY KEY (key)
);

-- 9. Tabela set
CREATE TABLE hangfire.set (
    key VARCHAR(100) NOT NULL,
    value VARCHAR(256) NOT NULL,
    score DOUBLE PRECISION,
    expireat TIMESTAMP,
    PRIMARY KEY (key, value)
);

-- 10. Tabela counter
CREATE TABLE hangfire.counter (
    key VARCHAR(100) NOT NULL,
    value INTEGER NOT NULL DEFAULT 1,
    expireat TIMESTAMP,
    PRIMARY KEY (key)
);

-- 11. Tabela aggregatedcounter
CREATE TABLE hangfire.aggregatedcounter (
    key VARCHAR(100) NOT NULL,
    value BIGINT NOT NULL,
    expireat TIMESTAMP,
    PRIMARY KEY (key)
);

-- Criar índices
CREATE INDEX IF NOT EXISTS ix_hangfire_lock_expireat ON hangfire.lock(expireat);
CREATE INDEX IF NOT EXISTS ix_hangfire_job_stateid ON hangfire.job(stateid);
CREATE INDEX IF NOT EXISTS ix_hangfire_job_expireat ON hangfire.job(expireat);
CREATE INDEX IF NOT EXISTS ix_hangfire_state_jobid ON hangfire.state(jobid);
CREATE INDEX IF NOT EXISTS ix_hangfire_jobqueue_queue_fetchedat ON hangfire.jobqueue(queue, fetchedat);
CREATE INDEX IF NOT EXISTS ix_hangfire_hash_expireat ON hangfire.hash(expireat);
CREATE INDEX IF NOT EXISTS ix_hangfire_list_expireat ON hangfire.list(expireat);
CREATE INDEX IF NOT EXISTS ix_hangfire_set_expireat ON hangfire.set(expireat);
CREATE INDEX IF NOT EXISTS ix_hangfire_counter_expireat ON hangfire.counter(expireat);
CREATE INDEX IF NOT EXISTS ix_hangfire_aggregatedcounter_expireat ON hangfire.aggregatedcounter(expireat);

SELECT '✅ Todas as tabelas do Hangfire criadas com sucesso!' AS resultado;

