-- =====================================================
-- CORRIGIR HANGFIRE COMPLETO - DEIXAR CRIAR AUTOMATICAMENTE
-- =====================================================

-- Garantir que o schema existe
CREATE SCHEMA IF NOT EXISTS hangfire;

-- Dar permissões ao usuário postgres
GRANT ALL PRIVILEGES ON SCHEMA hangfire TO postgres;
GRANT CREATE ON SCHEMA hangfire TO postgres;

-- Remover TODAS as tabelas do Hangfire que criamos manualmente
-- O Hangfire criará automaticamente com a estrutura correta
DROP TABLE IF EXISTS hangfire.lock CASCADE;
DROP TABLE IF EXISTS hangfire.server CASCADE;
DROP TABLE IF EXISTS hangfire.job CASCADE;
DROP TABLE IF EXISTS hangfire.state CASCADE;
DROP TABLE IF EXISTS hangfire.jobparameter CASCADE;
DROP TABLE IF EXISTS hangfire.jobqueue CASCADE;
DROP TABLE IF EXISTS hangfire.hash CASCADE;
DROP TABLE IF EXISTS hangfire.list CASCADE;
DROP TABLE IF EXISTS hangfire.set CASCADE;
DROP TABLE IF EXISTS hangfire.counter CASCADE;
DROP TABLE IF EXISTS hangfire.aggregatedcounter CASCADE;

-- Remover índices se existirem
DROP INDEX IF EXISTS hangfire.ix_hangfire_lock_expireat CASCADE;
DROP INDEX IF EXISTS hangfire.ix_hangfire_job_stateid CASCADE;
DROP INDEX IF EXISTS hangfire.ix_hangfire_job_expireat CASCADE;
DROP INDEX IF EXISTS hangfire.ix_hangfire_state_jobid CASCADE;
DROP INDEX IF EXISTS hangfire.ix_hangfire_jobqueue_queue_fetchedat CASCADE;
DROP INDEX IF EXISTS hangfire.ix_hangfire_hash_expireat CASCADE;
DROP INDEX IF EXISTS hangfire.ix_hangfire_list_expireat CASCADE;
DROP INDEX IF EXISTS hangfire.ix_hangfire_set_expireat CASCADE;
DROP INDEX IF EXISTS hangfire.ix_hangfire_counter_expireat CASCADE;
DROP INDEX IF EXISTS hangfire.ix_hangfire_aggregatedcounter_expireat CASCADE;

SELECT 'Schema hangfire limpo. O Hangfire criará todas as tabelas automaticamente na próxima inicialização.' AS resultado;

