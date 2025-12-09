-- =====================================================
-- CORRIGIR TABELAS HANGFIRE - CONTABO
-- =====================================================
-- Este script remove todas as tabelas do Hangfire e permite
-- que o Hangfire as recrie automaticamente com a estrutura correta
-- =====================================================

-- Conectar ao banco correto
\c singleone;

-- Dropar todas as tabelas do Hangfire na ordem correta (respeitando foreign keys)
DROP TABLE IF EXISTS hangfire.aggregatedcounter CASCADE;
DROP TABLE IF EXISTS hangfire.counter CASCADE;
DROP TABLE IF EXISTS hangfire.hash CASCADE;
DROP TABLE IF EXISTS hangfire.jobparameter CASCADE;
DROP TABLE IF EXISTS hangfire.jobqueue CASCADE;
DROP TABLE IF EXISTS hangfire.list CASCADE;
DROP TABLE IF EXISTS hangfire.set CASCADE;
DROP TABLE IF EXISTS hangfire.state CASCADE;
DROP TABLE IF EXISTS hangfire.job CASCADE;
DROP TABLE IF EXISTS hangfire.server CASCADE;
DROP TABLE IF EXISTS hangfire.lock CASCADE;

-- Dropar o schema se estiver vazio (opcional)
-- DROP SCHEMA IF EXISTS hangfire CASCADE;

-- Recriar o schema
CREATE SCHEMA IF NOT EXISTS hangfire;

-- Garantir permissões
GRANT USAGE ON SCHEMA hangfire TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA hangfire TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA hangfire TO postgres;

-- NOTA: Após executar este script, reinicie o backend.
-- O Hangfire irá criar automaticamente todas as tabelas com a estrutura correta
-- quando PrepareSchemaIfNecessary = true estiver habilitado no Startup.cs

