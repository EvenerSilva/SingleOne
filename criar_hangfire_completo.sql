-- =====================================================
-- SCRIPT COMPLETO PARA CRIAR TODAS AS TABELAS DO HANGFIRE
-- =====================================================

-- Criar schema do Hangfire
CREATE SCHEMA IF NOT EXISTS hangfire;

-- Tabela de locks (distributed locks)
CREATE TABLE IF NOT EXISTS hangfire.lock (
    resource VARCHAR(100) NOT NULL PRIMARY KEY,
    acquired TIMESTAMP NOT NULL,
    expireat TIMESTAMP NOT NULL
);

-- Tabela principal do servidor Hangfire
CREATE TABLE IF NOT EXISTS hangfire.server (
    id VARCHAR(100) PRIMARY KEY,
    data TEXT NOT NULL,
    lastheartbeat TIMESTAMP NOT NULL,
    heartbeatinterval INTEGER NOT NULL DEFAULT 15
);

-- Tabela de jobs
CREATE TABLE IF NOT EXISTS hangfire.job (
    id BIGSERIAL PRIMARY KEY,
    stateid BIGINT,
    statename VARCHAR(20),
    invocationdata TEXT NOT NULL,
    arguments TEXT NOT NULL,
    createdat TIMESTAMP NOT NULL,
    expireat TIMESTAMP
);

-- Tabela de estados dos jobs
CREATE TABLE IF NOT EXISTS hangfire.state (
    id BIGSERIAL PRIMARY KEY,
    jobid BIGINT NOT NULL,
    name VARCHAR(20) NOT NULL,
    reason TEXT,
    createdat TIMESTAMP NOT NULL,
    data TEXT,
    FOREIGN KEY (jobid) REFERENCES hangfire.job(id) ON DELETE CASCADE
);

-- Tabela de parâmetros dos jobs
CREATE TABLE IF NOT EXISTS hangfire.jobparameter (
    jobid BIGINT NOT NULL,
    name VARCHAR(40) NOT NULL,
    value TEXT,
    PRIMARY KEY (jobid, name),
    FOREIGN KEY (jobid) REFERENCES hangfire.job(id) ON DELETE CASCADE
);

-- Tabela de filas
CREATE TABLE IF NOT EXISTS hangfire.jobqueue (
    id BIGSERIAL PRIMARY KEY,
    jobid BIGINT NOT NULL,
    queue VARCHAR(50) NOT NULL,
    fetchedat TIMESTAMP,
    FOREIGN KEY (jobid) REFERENCES hangfire.job(id) ON DELETE CASCADE
);

-- Tabela de hash (para cache)
CREATE TABLE IF NOT EXISTS hangfire.hash (
    key VARCHAR(100) NOT NULL,
    field VARCHAR(100) NOT NULL,
    value TEXT,
    expireat TIMESTAMP,
    PRIMARY KEY (key, field)
);

-- Tabela de listas
CREATE TABLE IF NOT EXISTS hangfire.list (
    key VARCHAR(100) NOT NULL,
    value TEXT,
    expireat TIMESTAMP,
    PRIMARY KEY (key)
);

-- Tabela de sets
CREATE TABLE IF NOT EXISTS hangfire.set (
    key VARCHAR(100) NOT NULL,
    value VARCHAR(256) NOT NULL,
    score DOUBLE PRECISION,
    expireat TIMESTAMP,
    PRIMARY KEY (key, value)
);

-- Tabela de contadores
CREATE TABLE IF NOT EXISTS hangfire.counter (
    key VARCHAR(100) NOT NULL,
    value INTEGER NOT NULL DEFAULT 1,
    expireat TIMESTAMP,
    PRIMARY KEY (key)
);

-- Índices para performance
CREATE INDEX IF NOT EXISTS ix_hangfire_job_stateid ON hangfire.job(stateid);
CREATE INDEX IF NOT EXISTS ix_hangfire_job_expireat ON hangfire.job(expireat);
CREATE INDEX IF NOT EXISTS ix_hangfire_state_jobid ON hangfire.state(jobid);
CREATE INDEX IF NOT EXISTS ix_hangfire_jobqueue_queue_fetchedat ON hangfire.jobqueue(queue, fetchedat);
CREATE INDEX IF NOT EXISTS ix_hangfire_hash_expireat ON hangfire.hash(expireat);
CREATE INDEX IF NOT EXISTS ix_hangfire_list_expireat ON hangfire.list(expireat);
CREATE INDEX IF NOT EXISTS ix_hangfire_set_expireat ON hangfire.set(expireat);
CREATE INDEX IF NOT EXISTS ix_hangfire_counter_expireat ON hangfire.counter(expireat);
CREATE INDEX IF NOT EXISTS ix_hangfire_lock_expireat ON hangfire.lock(expireat);

-- Comentários
COMMENT ON SCHEMA hangfire IS 'Schema para tabelas do Hangfire (sistema de jobs em background)';
COMMENT ON TABLE hangfire.lock IS 'Tabela de locks distribuídos para controle de concorrência';
COMMENT ON TABLE hangfire.server IS 'Registra os servidores Hangfire ativos';
COMMENT ON TABLE hangfire.job IS 'Armazena os jobs agendados e em execução';
COMMENT ON TABLE hangfire.state IS 'Histórico de estados dos jobs';

SELECT '✅ Todas as tabelas do Hangfire criadas com sucesso!' AS resultado;

