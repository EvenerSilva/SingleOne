#!/bin/bash
# =====================================================
# VERIFICAR SE AS TABELAS DO HANGFIRE FORAM CRIADAS
# =====================================================

DOCKER_CONTAINER="${DOCKER_CONTAINER:-singleone-postgres}"
DB_NAME="${DB_NAME:-singleone}"

echo "🔍 Verificando tabelas do Hangfire..."
echo ""

# Verificar se o schema existe
SCHEMA_EXISTS=$(docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -tAc "SELECT 1 FROM information_schema.schemata WHERE schema_name = 'hangfire';" 2>/dev/null || echo "0")

if [ "$SCHEMA_EXISTS" = "1" ]; then
    echo "✅ Schema 'hangfire' existe"
else
    echo "❌ Schema 'hangfire' NÃO existe"
    exit 1
fi

echo ""

# Listar todas as tabelas do Hangfire
echo "📋 Tabelas do Hangfire:"
docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -c "SELECT table_name FROM information_schema.tables WHERE table_schema = 'hangfire' ORDER BY table_name;"

echo ""

# Contar tabelas
TABLE_COUNT=$(docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'hangfire';" 2>/dev/null || echo "0")

echo "📊 Total de tabelas: $TABLE_COUNT"

if [ "$TABLE_COUNT" -ge 11 ]; then
    echo "✅ Todas as tabelas do Hangfire foram criadas!"
else
    echo "⚠️  Faltam tabelas (esperado: 11, encontrado: $TABLE_COUNT)"
fi

echo ""

# Verificar se o backend está rodando
echo "🔍 Verificando status do backend..."
if docker ps --format '{{.Names}}' | grep -q "^singleone-backend$"; then
    echo "✅ Backend está rodando"
    
    # Verificar logs recentes do Hangfire
    echo ""
    echo "📋 Últimas linhas dos logs do backend (Hangfire):"
    docker logs singleone-backend --tail 20 2>&1 | grep -i hangfire || echo "Nenhum log do Hangfire encontrado"
else
    echo "❌ Backend NÃO está rodando"
fi

