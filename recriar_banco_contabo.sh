#!/bin/bash
# =====================================================
# SCRIPT PARA RECRIAR O BANCO SINGLEONE NO CONTABO
# =====================================================
# Este script verifica se o banco existe e o recria se necessário

set -e  # Parar em caso de erro

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}=====================================================${NC}"
echo -e "${YELLOW}  RECRIAÇÃO DO BANCO SINGLEONE - CONTABO${NC}"
echo -e "${YELLOW}=====================================================${NC}"
echo ""

# Configurações do banco (ajuste se necessário)
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-postgres}"
DB_NAME="${DB_NAME:-singleone}"
DOCKER_CONTAINER="${DOCKER_CONTAINER:-singleone-postgres}"

# Detectar se estamos usando Docker
USE_DOCKER=false
if command -v docker > /dev/null 2>&1; then
    # Verificar se o container existe (rodando ou parado)
    if docker ps -a --format '{{.Names}}' | grep -q "^${DOCKER_CONTAINER}$"; then
        # Verificar se está rodando
        if docker ps --format '{{.Names}}' | grep -q "^${DOCKER_CONTAINER}$"; then
            USE_DOCKER=true
            echo -e "${YELLOW}🐳 Detectado container Docker rodando: ${DOCKER_CONTAINER}${NC}"
        else
            echo -e "${YELLOW}⚠️  Container Docker existe mas não está rodando. Tentando iniciar...${NC}"
            docker start "$DOCKER_CONTAINER" > /dev/null 2>&1
            sleep 2
            if docker ps --format '{{.Names}}' | grep -q "^${DOCKER_CONTAINER}$"; then
                USE_DOCKER=true
                echo -e "${GREEN}✅ Container iniciado com sucesso!${NC}"
            fi
        fi
    fi
fi

# Se ainda não detectou Docker, tentar forçar se psql não estiver disponível
if [ "$USE_DOCKER" = false ] && ! command -v psql > /dev/null 2>&1; then
    if command -v docker > /dev/null 2>&1; then
        # Tentar encontrar qualquer container postgres
        POSTGRES_CONTAINER=$(docker ps --format '{{.Names}}' | grep -i postgres | head -n 1)
        if [ -n "$POSTGRES_CONTAINER" ]; then
            DOCKER_CONTAINER="$POSTGRES_CONTAINER"
            USE_DOCKER=true
            echo -e "${YELLOW}🐳 Detectado container PostgreSQL: ${DOCKER_CONTAINER}${NC}"
        fi
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

echo -e "${YELLOW}📋 Configurações:${NC}"
echo "  Host: $DB_HOST"
echo "  Port: $DB_PORT"
echo "  User: $DB_USER"
echo "  Database: $DB_NAME"
echo ""

# Verificar se o banco existe
echo -e "${YELLOW}🔍 Verificando se o banco '$DB_NAME' existe...${NC}"
DB_EXISTS=$(run_psql postgres -tAc "SELECT 1 FROM pg_database WHERE datname='$DB_NAME'" 2>/dev/null || echo "0")

if [ "$DB_EXISTS" = "1" ]; then
    echo -e "${GREEN}✅ Banco '$DB_NAME' já existe!${NC}"
    echo ""
    echo -e "${YELLOW}📊 Verificando estrutura do banco...${NC}"
    
    # Contar tabelas
    TABLE_COUNT=$(run_psql "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';" 2>/dev/null || echo "0")
    
    # Contar views
    VIEW_COUNT=$(run_psql "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public';" 2>/dev/null || echo "0")
    
    echo "  Tabelas encontradas: $TABLE_COUNT"
    echo "  Views encontradas: $VIEW_COUNT"
    echo ""
    
    if [ "$TABLE_COUNT" -lt 50 ]; then
        echo -e "${RED}⚠️  Banco existe mas parece incompleto (menos de 50 tabelas)${NC}"
        read -p "Deseja recriar o banco? (s/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Ss]$ ]]; then
            echo -e "${YELLOW}Operação cancelada.${NC}"
            exit 0
        fi
        RECREATE=true
    else
        echo -e "${GREEN}✅ Banco parece estar completo!${NC}"
        echo ""
        read -p "Deseja mesmo recriar o banco? (s/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Ss]$ ]]; then
            echo -e "${YELLOW}Operação cancelada.${NC}"
            exit 0
        fi
        RECREATE=true
    fi
else
    echo -e "${RED}❌ Banco '$DB_NAME' NÃO existe!${NC}"
    RECREATE=true
fi

if [ "$RECREATE" = true ]; then
    echo ""
    echo -e "${YELLOW}🗑️  Removendo banco existente (se houver)...${NC}"
    
    # Terminar conexões ativas
    run_psql postgres -c "
        SELECT pg_terminate_backend(pid)
        FROM pg_stat_activity
        WHERE datname = '$DB_NAME' AND pid <> pg_backend_pid();
    " 2>/dev/null || true
    
    # Dropar banco
    run_psql postgres -c "DROP DATABASE IF EXISTS \"$DB_NAME\";" 2>/dev/null || true
    
    echo -e "${GREEN}✅ Banco removido (se existia)${NC}"
    echo ""
    
    echo -e "${YELLOW}🆕 Criando novo banco '$DB_NAME'...${NC}"
    run_psql postgres -c "CREATE DATABASE \"$DB_NAME\";"
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Banco criado com sucesso!${NC}"
    else
        echo -e "${RED}❌ Erro ao criar banco!${NC}"
        exit 1
    fi
    
    echo ""
    echo -e "${YELLOW}📦 Executando script de inicialização...${NC}"
    
    # Verificar se o script existe
    if [ ! -f "init_db_atualizado.sql" ]; then
        echo -e "${RED}❌ Arquivo 'init_db_atualizado.sql' não encontrado!${NC}"
        echo "   Certifique-se de estar no diretório correto."
        exit 1
    fi
    
    # Executar script de inicialização
    if [ "$USE_DOCKER" = true ]; then
        # Copiar script para o container e executar
        docker cp init_db_atualizado.sql "$DOCKER_CONTAINER":/tmp/init_db_atualizado.sql
        docker exec -e PGPASSWORD="$DB_PASSWORD" "$DOCKER_CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -f /tmp/init_db_atualizado.sql
        docker exec "$DOCKER_CONTAINER" rm -f /tmp/init_db_atualizado.sql
    else
        run_psql "$DB_NAME" -f init_db_atualizado.sql
    fi
    
    if [ $? -eq 0 ]; then
        echo ""
        echo -e "${GREEN}✅ Script de inicialização executado com sucesso!${NC}"
    else
        echo ""
        echo -e "${RED}⚠️  Script executado com alguns erros (isso pode ser normal)${NC}"
        echo "   Verifique os logs acima para detalhes."
    fi
    
    echo ""
    echo -e "${YELLOW}📊 Verificando estrutura final...${NC}"
    
    # Contar tabelas e views novamente
    TABLE_COUNT=$(run_psql "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';" 2>/dev/null || echo "0")
    VIEW_COUNT=$(run_psql "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public';" 2>/dev/null || echo "0")
    
    echo "  Tabelas criadas: $TABLE_COUNT"
    echo "  Views criadas: $VIEW_COUNT"
    echo ""
    
    if [ "$TABLE_COUNT" -ge 60 ] && [ "$VIEW_COUNT" -ge 25 ]; then
        echo -e "${GREEN}✅ Banco recriado com sucesso!${NC}"
        echo ""
        echo -e "${GREEN}=====================================================${NC}"
        echo -e "${GREEN}  ✅ BANCO RECRIADO COM SUCESSO!${NC}"
        echo -e "${GREEN}=====================================================${NC}"
    else
        echo -e "${YELLOW}⚠️  Banco criado mas pode estar incompleto${NC}"
        echo "   Tabelas esperadas: ~64"
        echo "   Views esperadas: ~32"
    fi
fi

echo ""
echo -e "${YELLOW}📝 Credenciais para conexão:${NC}"
echo "  Host: $DB_HOST"
echo "  Port: $DB_PORT"
echo "  User: $DB_USER"
echo "  Password: $DB_PASSWORD"
echo "  Database: $DB_NAME"
echo ""

# Limpar senha do ambiente (apenas se não estiver usando Docker)
if [ "$USE_DOCKER" != true ]; then
    unset PGPASSWORD
fi

echo -e "${GREEN}✅ Processo concluído!${NC}"

