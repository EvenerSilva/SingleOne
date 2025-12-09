-- =====================================================
-- CRIAR TABELA HANGFIRE.AGGREGATEDCOUNTER
-- =====================================================

CREATE TABLE IF NOT EXISTS hangfire.aggregatedcounter (
    key VARCHAR(100) NOT NULL,
    value BIGINT NOT NULL,
    expireat TIMESTAMP,
    PRIMARY KEY (key)
);

CREATE INDEX IF NOT EXISTS ix_hangfire_aggregatedcounter_expireat ON hangfire.aggregatedcounter(expireat);

COMMENT ON TABLE hangfire.aggregatedcounter IS 'Contadores agregados do Hangfire para métricas';

SELECT '✅ Tabela hangfire.aggregatedcounter criada!' AS resultado;

