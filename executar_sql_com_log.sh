#!/bin/bash
# =====================================================
# EXECUTAR SQL COM LOGGING DE ERROS
# =====================================================

set -e

DOCKER_CONTAINER="${DOCKER_CONTAINER:-singleone-postgres}"
DB_NAME="${DB_NAME:-singleone}"
SQL_FILE="${1:-init_db_atualizado.sql}"

echo "📋 Executando $SQL_FILE com logging de erros..."
echo ""

# Criar arquivo de log
LOG_FILE="sql_execution_$(date +%Y%m%d_%H%M%S).log"

# Executar SQL e capturar erros
docker exec -i "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -f - < "$SQL_FILE" 2>&1 | tee "$LOG_FILE"

# Verificar resultado
if [ ${PIPESTATUS[0]} -eq 0 ]; then
    echo ""
    echo "✅ Script executado!"
else
    echo ""
    echo "⚠️  Script executado com erros. Verifique o log: $LOG_FILE"
fi

# Contar tabelas e views
echo ""
echo "📊 Estrutura final:"
TABELAS=$(docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';" 2>/dev/null || echo "0")
VIEWS=$(docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public';" 2>/dev/null || echo "0")

echo "  Tabelas: $TABELAS"
echo "  Views: $VIEWS"

# Mostrar últimos erros do log
echo ""
echo "🔍 Últimos erros encontrados:"
grep -i "error\|fatal\|failed" "$LOG_FILE" | tail -20 || echo "Nenhum erro encontrado"

echo ""
echo "📝 Log completo salvo em: $LOG_FILE"

