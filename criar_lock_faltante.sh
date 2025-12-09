#!/bin/bash
# =====================================================
# CRIAR TABELA LOCK QUE ESTÁ FALTANDO
# =====================================================

DOCKER_CONTAINER="${DOCKER_CONTAINER:-singleone-postgres}"
DB_NAME="${DB_NAME:-singleone}"

echo "🔧 Criando tabela hangfire.lock que está faltando..."
echo ""

docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" << 'EOF'
-- Criar tabela lock se não existir
CREATE TABLE IF NOT EXISTS hangfire.lock (
    resource VARCHAR(100) NOT NULL PRIMARY KEY,
    acquired TIMESTAMP NOT NULL,
    expireat TIMESTAMP
);

-- Criar índice
CREATE INDEX IF NOT EXISTS ix_hangfire_lock_expireat ON hangfire.lock(expireat);

-- Verificar se foi criada
SELECT 
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'hangfire' AND table_name = 'lock')
        THEN '✅ Tabela hangfire.lock criada com sucesso!'
        ELSE '❌ Erro ao criar tabela hangfire.lock'
    END AS resultado;
EOF

echo ""
echo "✅ Processo concluído!"

