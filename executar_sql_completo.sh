#!/bin/bash
# =====================================================
# EXECUTAR SQL COMPLETO COM CAPTURA DE ERROS
# =====================================================

DOCKER_CONTAINER="${DOCKER_CONTAINER:-singleone-postgres}"
DB_NAME="${DB_NAME:-singleone}"
SQL_FILE="${1:-init_db_atualizado.sql}"

echo "📋 Executando $SQL_FILE..."
echo ""

# Criar arquivo de log
LOG_FILE="sql_execution_$(date +%Y%m%d_%H%M%S).log"

# Executar SQL sem parar em erros e capturar tudo
docker exec -i "$DOCKER_CONTAINER" bash -c "PGOPTIONS='--client-min-messages=warning' psql -U postgres -d $DB_NAME -v ON_ERROR_STOP=0 -f -" < "$SQL_FILE" > "$LOG_FILE" 2>&1

# Verificar quantas tabelas e views foram criadas
echo ""
echo "📊 Estrutura criada:"
TABELAS=$(docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';" 2>/dev/null || echo "0")
VIEWS=$(docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public';" 2>/dev/null || echo "0")

echo "  ✅ Tabelas: $TABELAS"
echo "  ✅ Views: $VIEWS"
echo ""

# Mostrar erros encontrados
echo "🔍 Erros encontrados no log:"
ERROR_COUNT=$(grep -iE "error|fatal|failed" "$LOG_FILE" | grep -v "NOTICE" | wc -l)
if [ "$ERROR_COUNT" -gt 0 ]; then
    echo "  ⚠️  Total de erros: $ERROR_COUNT"
    echo ""
    echo "  Primeiros 30 erros:"
    grep -iE "error|fatal|failed" "$LOG_FILE" | grep -v "NOTICE" | head -30 | sed 's/^/    /'
else
    echo "  ✅ Nenhum erro crítico encontrado"
fi

echo ""
echo "📝 Log completo salvo em: $LOG_FILE"
echo ""
echo "💡 Para ver o log completo: cat $LOG_FILE"

