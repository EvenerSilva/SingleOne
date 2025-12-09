#!/bin/bash
# =====================================================
# SCRIPT PARA VERIFICAR E RECRIAR BANCO FORÇADAMENTE
# =====================================================

set -e

# Cores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

DOCKER_CONTAINER="${DOCKER_CONTAINER:-singleone-postgres}"
DB_NAME="${DB_NAME:-singleone}"

echo -e "${YELLOW}=====================================================${NC}"
echo -e "${YELLOW}  VERIFICAÇÃO E RECRIAÇÃO DO BANCO${NC}"
echo -e "${YELLOW}=====================================================${NC}"
echo ""

# Verificar se o container está rodando
if ! docker ps --format '{{.Names}}' | grep -q "^${DOCKER_CONTAINER}$"; then
    echo -e "${YELLOW}🔄 Iniciando container...${NC}"
    docker start "$DOCKER_CONTAINER"
    sleep 3
fi

echo -e "${YELLOW}📊 Verificando estado do banco '$DB_NAME'...${NC}"
echo ""

# Verificar se o banco existe
DB_EXISTS=$(docker exec "$DOCKER_CONTAINER" psql -U postgres -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='$DB_NAME'" 2>/dev/null || echo "0")

if [ "$DB_EXISTS" = "1" ]; then
    echo -e "${GREEN}✅ Banco '$DB_NAME' existe${NC}"
    
    # Contar tabelas
    TABLE_COUNT=$(docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';" 2>/dev/null || echo "0")
    
    # Contar views
    VIEW_COUNT=$(docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public';" 2>/dev/null || echo "0")
    
    echo "  📋 Tabelas: $TABLE_COUNT"
    echo "  👁️  Views: $VIEW_COUNT"
    echo ""
    
    if [ "$TABLE_COUNT" -ge 60 ] && [ "$VIEW_COUNT" -ge 25 ]; then
        echo -e "${GREEN}✅ Banco está completo!${NC}"
        echo ""
        read -p "Deseja mesmo recriar o banco? (s/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Ss]$ ]]; then
            echo -e "${YELLOW}Operação cancelada.${NC}"
            exit 0
        fi
    else
        echo -e "${RED}⚠️  Banco existe mas está incompleto!${NC}"
        echo "   Esperado: ~64 tabelas e ~32 views"
        echo "   Encontrado: $TABLE_COUNT tabelas e $VIEW_COUNT views"
        echo ""
        read -p "Deseja recriar o banco? (S/n): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Nn]$ ]]; then
            echo -e "${YELLOW}Operação cancelada.${NC}"
            exit 0
        fi
    fi
else
    echo -e "${RED}❌ Banco '$DB_NAME' não existe${NC}"
fi

echo ""
echo -e "${YELLOW}🗑️  Removendo banco existente...${NC}"

# Terminar conexões ativas
docker exec "$DOCKER_CONTAINER" psql -U postgres -d postgres -c "
    SELECT pg_terminate_backend(pid)
    FROM pg_stat_activity
    WHERE datname = '$DB_NAME' AND pid <> pg_backend_pid();
" 2>/dev/null || true

# Dropar banco
docker exec "$DOCKER_CONTAINER" psql -U postgres -d postgres -c "DROP DATABASE IF EXISTS \"$DB_NAME\";" 2>/dev/null || true

echo -e "${GREEN}✅ Banco removido${NC}"
echo ""

# Criar novo banco
echo -e "${YELLOW}🆕 Criando novo banco '$DB_NAME'...${NC}"
docker exec "$DOCKER_CONTAINER" psql -U postgres -d postgres -c "CREATE DATABASE \"$DB_NAME\";"

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
    exit 1
fi

# Copiar script para o container e executar
docker cp init_db_atualizado.sql "$DOCKER_CONTAINER":/tmp/init_db_atualizado.sql
docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -f /tmp/init_db_atualizado.sql
docker exec "$DOCKER_CONTAINER" rm -f /tmp/init_db_atualizado.sql

if [ $? -eq 0 ]; then
    echo ""
    echo -e "${GREEN}✅ Script de inicialização executado!${NC}"
else
    echo ""
    echo -e "${YELLOW}⚠️  Script executado com alguns erros (pode ser normal)${NC}"
fi

echo ""
echo -e "${YELLOW}📊 Verificando estrutura final...${NC}"

# Contar tabelas e views novamente
TABLE_COUNT=$(docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';" 2>/dev/null || echo "0")
VIEW_COUNT=$(docker exec "$DOCKER_CONTAINER" psql -U postgres -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public';" 2>/dev/null || echo "0")

echo "  📋 Tabelas criadas: $TABLE_COUNT"
echo "  👁️  Views criadas: $VIEW_COUNT"
echo ""

if [ "$TABLE_COUNT" -ge 60 ] && [ "$VIEW_COUNT" -ge 25 ]; then
    echo -e "${GREEN}=====================================================${NC}"
    echo -e "${GREEN}  ✅ BANCO RECRIADO COM SUCESSO!${NC}"
    echo -e "${GREEN}=====================================================${NC}"
else
    echo -e "${YELLOW}⚠️  Banco criado mas pode estar incompleto${NC}"
    echo "   Tabelas esperadas: ~64 (encontrado: $TABLE_COUNT)"
    echo "   Views esperadas: ~32 (encontrado: $VIEW_COUNT)"
fi

echo ""
echo -e "${GREEN}✅ Processo concluído!${NC}"

