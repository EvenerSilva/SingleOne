#!/bin/bash
# Execute este script no servidor Contabo para criar os scripts de banco

cd /opt/SingleOne

# Criar script de verificação
cat > verificar_banco_contabo.sh << 'EOF'
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

# Exportar senha para psql
export PGPASSWORD="$DB_PASSWORD"

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
if psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c "SELECT version();" > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Conexão com PostgreSQL OK${NC}"
else
    echo -e "${RED}❌ Não foi possível conectar ao PostgreSQL!${NC}"
    echo "   Verifique:"
    echo "   - Se o PostgreSQL está rodando"
    echo "   - Se as credenciais estão corretas"
    echo "   - Se o host/porta estão acessíveis"
    exit 1
fi

echo ""

# Verificar se o banco existe
echo -e "${YELLOW}🔍 Verificando se o banco '$DB_NAME' existe...${NC}"
DB_EXISTS=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='$DB_NAME'" 2>/dev/null || echo "0")

if [ "$DB_EXISTS" = "1" ]; then
    echo -e "${GREEN}✅ Banco '$DB_NAME' existe!${NC}"
    echo ""
    
    # Verificar estrutura
    echo -e "${YELLOW}📊 Estrutura do banco:${NC}"
    
    TABLE_COUNT=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';" 2>/dev/null || echo "0")
    VIEW_COUNT=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public';" 2>/dev/null || echo "0")
    SEQUENCE_COUNT=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.sequences WHERE sequence_schema = 'public';" 2>/dev/null || echo "0")
    
    echo "  📋 Tabelas: $TABLE_COUNT"
    echo "  👁️  Views: $VIEW_COUNT"
    echo "  🔢 Sequences: $SEQUENCE_COUNT"
    echo ""
    
    # Verificar tabelas críticas
    echo -e "${YELLOW}🔍 Verificando tabelas críticas:${NC}"
    
    CRITICAL_TABLES=("clientes" "fornecedores" "usuarios" "equipamentos" "requisicoes" "localidades")
    MISSING_TABLES=()
    
    for table in "${CRITICAL_TABLES[@]}"; do
        EXISTS=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -tAc "SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '$table';" 2>/dev/null || echo "0")
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
    
    CLIENT_COUNT=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM clientes;" 2>/dev/null || echo "0")
    USER_COUNT=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM usuarios;" 2>/dev/null || echo "0")
    
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

# Limpar senha do ambiente
unset PGPASSWORD
EOF

# Criar script de recriação
cat > recriar_banco_contabo.sh << 'EOF'
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

# Exportar senha para psql
export PGPASSWORD="$DB_PASSWORD"

echo -e "${YELLOW}📋 Configurações:${NC}"
echo "  Host: $DB_HOST"
echo "  Port: $DB_PORT"
echo "  User: $DB_USER"
echo "  Database: $DB_NAME"
echo ""

# Verificar se o banco existe
echo -e "${YELLOW}🔍 Verificando se o banco '$DB_NAME' existe...${NC}"
DB_EXISTS=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='$DB_NAME'" 2>/dev/null || echo "0")

if [ "$DB_EXISTS" = "1" ]; then
    echo -e "${GREEN}✅ Banco '$DB_NAME' já existe!${NC}"
    echo ""
    echo -e "${YELLOW}📊 Verificando estrutura do banco...${NC}"
    
    # Contar tabelas
    TABLE_COUNT=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';" 2>/dev/null || echo "0")
    
    # Contar views
    VIEW_COUNT=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public';" 2>/dev/null || echo "0")
    
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
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c "
        SELECT pg_terminate_backend(pid)
        FROM pg_stat_activity
        WHERE datname = '$DB_NAME' AND pid <> pg_backend_pid();
    " 2>/dev/null || true
    
    # Dropar banco
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c "DROP DATABASE IF EXISTS \"$DB_NAME\";" 2>/dev/null || true
    
    echo -e "${GREEN}✅ Banco removido (se existia)${NC}"
    echo ""
    
    echo -e "${YELLOW}🆕 Criando novo banco '$DB_NAME'...${NC}"
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c "CREATE DATABASE \"$DB_NAME\";"
    
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
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f init_db_atualizado.sql
    
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
    TABLE_COUNT=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';" 2>/dev/null || echo "0")
    VIEW_COUNT=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public';" 2>/dev/null || echo "0")
    
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

# Limpar senha do ambiente
unset PGPASSWORD

echo -e "${GREEN}✅ Processo concluído!${NC}"
EOF

# Dar permissão de execução
chmod +x verificar_banco_contabo.sh
chmod +x recriar_banco_contabo.sh

echo "✅ Scripts criados com sucesso!"
echo ""
echo "Agora você pode executar:"
echo "  ./verificar_banco_contabo.sh"
echo "  ./recriar_banco_contabo.sh"

