#!/bin/bash
# =====================================================
# SCRIPT PARA VERIFICAR STATUS DO BANCO SINGLEONE
# =====================================================

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configurações do banco
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-postgres}"
DB_NAME="${DB_NAME:-singleone}"
DOCKER_CONTAINER="${DOCKER_CONTAINER:-singleone-postgres}"

# Detectar se estamos usando Docker
USE_DOCKER=false
if command -v docker > /dev/null 2>&1; then
    if docker ps --format '{{.Names}}' | grep -q "^${DOCKER_CONTAINER}$"; then
        USE_DOCKER=true
        echo -e "${YELLOW}🐳 Detectado container Docker: ${DOCKER_CONTAINER}${NC}"
    fi
fi

# Função para executar psql
run_psql() {
    local db=$1
    shift
    if [ "$USE_DOCKER" = true ]; then
        docker exec -e PGPASSWORD="$DB_PASSWORD" "$DOCKER_CONTAINER" psql -U "$DB_USER" -d "$db" "$@"
    else
        export PGPASSWORD="$DB_PASSWORD"
        psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$db" "$@"
    fi
}

echo -e "${BLUE}=====================================================${NC}"
echo -e "${BLUE}  VERIFICAÇÃO DO BANCO SINGLEONE${NC}"
echo -e "${BLUE}=====================================================${NC}"
echo ""

echo -e "${YELLOW}📋 Configurações:${NC}"
echo "  Host: $DB_HOST"
echo "  Port: $DB_PORT"
echo "  User: $DB_USER"
echo "  Database: $DB_NAME"
echo ""

# Verificar conexão com PostgreSQL
echo -e "${YELLOW}🔍 Verificando conexão com PostgreSQL...${NC}"
if run_psql postgres -c "SELECT version();" > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Conexão com PostgreSQL OK${NC}"
else
    echo -e "${RED}❌ Não foi possível conectar ao PostgreSQL!${NC}"
    echo "   Verifique:"
    echo "   - Se o PostgreSQL está rodando"
    if [ "$USE_DOCKER" = true ]; then
        echo "   - Se o container Docker '$DOCKER_CONTAINER' está rodando"
    else
        echo "   - Se o host/porta estão acessíveis"
    fi
    echo "   - Se as credenciais estão corretas"
    exit 1
fi

echo ""

# Verificar se o banco existe
echo -e "${YELLOW}🔍 Verificando se o banco '$DB_NAME' existe...${NC}"
DB_EXISTS=$(run_psql postgres -tAc "SELECT 1 FROM pg_database WHERE datname='$DB_NAME'" 2>/dev/null || echo "0")

if [ "$DB_EXISTS" = "1" ]; then
    echo -e "${GREEN}✅ Banco '$DB_NAME' existe!${NC}"
    echo ""
    
    # Verificar estrutura
    echo -e "${YELLOW}📊 Estrutura do banco:${NC}"
    
    TABLE_COUNT=$(run_psql "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';" 2>/dev/null || echo "0")
    VIEW_COUNT=$(run_psql "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public';" 2>/dev/null || echo "0")
    SEQUENCE_COUNT=$(run_psql "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.sequences WHERE sequence_schema = 'public';" 2>/dev/null || echo "0")
    
    echo "  📋 Tabelas: $TABLE_COUNT"
    echo "  👁️  Views: $VIEW_COUNT"
    echo "  🔢 Sequences: $SEQUENCE_COUNT"
    echo ""
    
    # Verificar tabelas críticas
    echo -e "${YELLOW}🔍 Verificando tabelas críticas:${NC}"
    
    CRITICAL_TABLES=("clientes" "fornecedores" "usuarios" "equipamentos" "requisicoes" "localidades")
    MISSING_TABLES=()
    
    for table in "${CRITICAL_TABLES[@]}"; do
        EXISTS=$(run_psql "$DB_NAME" -tAc "SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '$table';" 2>/dev/null || echo "0")
        if [ "$EXISTS" = "1" ]; then
            echo -e "  ${GREEN}✅${NC} $table"
        else
            echo -e "  ${RED}❌${NC} $table (FALTANDO)"
            MISSING_TABLES+=("$table")
        fi
    done
    
    echo ""
    
    # Verificar dados básicos
    echo -e "${YELLOW}📊 Dados básicos:${NC}"
    
    CLIENT_COUNT=$(run_psql "$DB_NAME" -tAc "SELECT COUNT(*) FROM clientes;" 2>/dev/null || echo "0")
    USER_COUNT=$(run_psql "$DB_NAME" -tAc "SELECT COUNT(*) FROM usuarios;" 2>/dev/null || echo "0")
    
    echo "  👥 Clientes: $CLIENT_COUNT"
    echo "  👤 Usuários: $USER_COUNT"
    echo ""
    
    # Status geral
    if [ "$TABLE_COUNT" -ge 60 ] && [ "$VIEW_COUNT" -ge 25 ] && [ ${#MISSING_TABLES[@]} -eq 0 ]; then
        echo -e "${GREEN}✅ Banco está completo e funcionando!${NC}"
    else
        echo -e "${YELLOW}⚠️  Banco pode estar incompleto:${NC}"
        if [ "$TABLE_COUNT" -lt 60 ]; then
            echo "   - Poucas tabelas (esperado: ~64, encontrado: $TABLE_COUNT)"
        fi
        if [ "$VIEW_COUNT" -lt 25 ]; then
            echo "   - Poucas views (esperado: ~32, encontrado: $VIEW_COUNT)"
        fi
        if [ ${#MISSING_TABLES[@]} -gt 0 ]; then
            echo "   - Tabelas críticas faltando: ${MISSING_TABLES[*]}"
        fi
    fi
    
else
    echo -e "${RED}❌ Banco '$DB_NAME' NÃO existe!${NC}"
    echo ""
    echo -e "${YELLOW}💡 Para recriar o banco, execute:${NC}"
    echo "   ./recriar_banco_contabo.sh"
fi

echo ""
echo -e "${BLUE}=====================================================${NC}"

# Limpar senha do ambiente (apenas se não estiver usando Docker)
if [ "$USE_DOCKER" != true ]; then
    unset PGPASSWORD
fi

