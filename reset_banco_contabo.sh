#!/bin/bash

# Script para resetar completamente o banco de dados SingleOne no Contabo
# Execute: bash reset_banco_contabo.sh

echo "=========================================="
echo "RESET COMPLETO DO BANCO SINGLEONE"
echo "=========================================="
echo ""

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Verificar se está no diretório correto
if [ ! -f "init_db_atualizado.sql" ]; then
    echo -e "${RED}ERRO: Arquivo init_db_atualizado.sql não encontrado!${NC}"
    echo "Execute este script no diretório /opt/SingleOne"
    exit 1
fi

echo -e "${YELLOW}1. Desconectando todas as conexões ativas...${NC}"
docker exec -i singleone-postgres psql -U postgres -d postgres <<EOF
SELECT pg_terminate_backend(pg_stat_activity.pid)
FROM pg_stat_activity
WHERE pg_stat_activity.datname = 'singleone'
  AND pid <> pg_backend_pid();
EOF

echo -e "${YELLOW}2. Apagando banco de dados singleone...${NC}"
docker exec -i singleone-postgres psql -U postgres -d postgres -c "DROP DATABASE IF EXISTS singleone;"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Banco apagado com sucesso${NC}"
else
    echo -e "${RED}✗ Erro ao apagar banco${NC}"
    exit 1
fi

echo -e "${YELLOW}3. Criando banco de dados singleone...${NC}"
docker exec -i singleone-postgres psql -U postgres -d postgres -c "CREATE DATABASE singleone;"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Banco criado com sucesso${NC}"
else
    echo -e "${RED}✗ Erro ao criar banco${NC}"
    exit 1
fi

echo -e "${YELLOW}4. Executando script de inicialização...${NC}"
cat init_db_atualizado.sql | docker exec -i singleone-postgres psql -U postgres -d singleone

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Script de inicialização executado${NC}"
else
    echo -e "${RED}✗ Erro ao executar script de inicialização${NC}"
    exit 1
fi

echo -e "${YELLOW}5. Importando templates...${NC}"
if [ -f "import_templates.sql" ]; then
    cat import_templates.sql | docker exec -i singleone-postgres psql -U postgres -d singleone
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ Templates importados com sucesso${NC}"
    else
        echo -e "${YELLOW}⚠ Aviso: Erro ao importar templates (pode ser que já existam)${NC}"
    fi
else
    echo -e "${YELLOW}⚠ Arquivo import_templates.sql não encontrado, pulando importação${NC}"
fi

echo ""
echo -e "${GREEN}=========================================="
echo "RESET CONCLUÍDO COM SUCESSO!"
echo "==========================================${NC}"
echo ""

echo -e "${YELLOW}Verificando dados inseridos...${NC}"
echo ""
echo "Usuários:"
docker exec -it singleone-postgres psql -U postgres -d singleone -c "SELECT id, nome, email, su, adm, ativo FROM usuarios;"

echo ""
echo "Tipos de Aquisição:"
docker exec -it singleone-postgres psql -U postgres -d singleone -c "SELECT Id, Nome FROM TipoAquisicao ORDER BY Id;"

echo ""
echo "Templates:"
docker exec -it singleone-postgres psql -U postgres -d singleone -c "SELECT id, tipo, titulo, ativo FROM templates ORDER BY id;"

echo ""
echo -e "${GREEN}Banco resetado e inicializado com sucesso!${NC}"

