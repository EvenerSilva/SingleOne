#!/bin/bash
# =====================================================
# VERIFICAR E CRIAR TABELA LOCK DO HANGFIRE
# =====================================================

DOCKER_CONTAINER="${DOCKER_CONTAINER:-singleone-postgres}"
DB_NAME="${DB_NAME:-singleone}"

echo "🔍 Verificando se a tabela hangfire.lock existe..."
echo ""

# Verificar se existe
LOCK_EXISTS=$(docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -tAc "SELECT 1 FROM information_schema.tables WHERE table_schema = 'hangfire' AND table_name = 'lock';" 2>/dev/null || echo "0")

if [ "$LOCK_EXISTS" = "1" ]; then
    echo "✅ Tabela hangfire.lock já existe"
    echo ""
    echo "📋 Estrutura da tabela:"
    docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -c "\d hangfire.lock"
else
    echo "❌ Tabela hangfire.lock NÃO existe. Criando..."
    echo ""
    
    docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" << 'EOF'
-- Garantir que o schema existe
CREATE SCHEMA IF NOT EXISTS hangfire;

-- Criar tabela lock
CREATE TABLE hangfire.lock (
    resource VARCHAR(100) NOT NULL PRIMARY KEY,
    acquired TIMESTAMP NOT NULL,
    expireat TIMESTAMP
);

-- Criar índice
CREATE INDEX IF NOT EXISTS ix_hangfire_lock_expireat ON hangfire.lock(expireat);

-- Verificar
SELECT '✅ Tabela hangfire.lock criada!' AS resultado;
EOF
fi

echo ""
echo "📊 Verificando todas as tabelas do Hangfire:"
docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -c "SELECT table_name FROM information_schema.tables WHERE table_schema = 'hangfire' ORDER BY table_name;"

