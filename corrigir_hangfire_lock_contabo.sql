-- =====================================================
-- CORRIGIR TABELA HANGFIRE.LOCK
-- =====================================================
-- O Hangfire está tentando criar colunas que já existem
-- Vamos dropar a tabela e deixar o Hangfire criar automaticamente

-- Dropar a tabela lock se existir
DROP TABLE IF EXISTS hangfire.lock CASCADE;

-- O Hangfire criará a tabela automaticamente na próxima inicialização
SELECT 'Tabela hangfire.lock removida. O Hangfire criará automaticamente na próxima inicialização.' AS resultado;

