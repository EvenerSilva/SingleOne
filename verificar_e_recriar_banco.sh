#!/bin/bash

# =====================================================
# SCRIPT DE VERIFICAÇÃO E RECRIAÇÃO DO BANCO SINGLEONE
# =====================================================
# Este script verifica se o banco 'singleone' existe
# e o recria se necessário, executando o script de inicialização

set -e  # Parar em caso de erro

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "=========================================="
echo "🔍 VERIFICANDO BANCO DE DADOS SINGLEONE"
echo "=========================================="
echo ""

# Variáveis de ambiente (padrões do docker-compose)
DB_HOST="${DB_HOST:-postgres}"
DB_PORT="${DB_PORT:-5432}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-postgres}"
DB_NAME="${DB_NAME:-singleone}"

# Exportar senha para evitar prompt
export PGPASSWORD="$DB_PASSWORD"

echo "📋 Configurações:"
echo "   Host: $DB_HOST"
echo "   Port: $DB_PORT"
echo "   User: $DB_USER"
echo "   Database: $DB_NAME"
echo ""

# Verificar se o banco existe
echo "🔍 Verificando se o banco '$DB_NAME' existe..."
DB_EXISTS=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='$DB_NAME'" 2>/dev/null || echo "0")

if [ "$DB_EXISTS" = "1" ]; then
    echo -e "${GREEN}✅ Banco '$DB_NAME' existe!${NC}"
    echo ""
    echo "📊 Verificando tabelas..."
    TABLE_COUNT=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';" 2>/dev/null || echo "0")
    echo "   Tabelas encontradas: $TABLE_COUNT"
    
    if [ "$TABLE_COUNT" -lt "50" ]; then
        echo -e "${YELLOW}⚠️  Poucas tabelas encontradas. O banco pode estar incompleto.${NC}"
        echo ""
        read -p "Deseja recriar o banco? (s/N): " -n 1 -r
        echo ""
        if [[ $REPLY =~ ^[Ss]$ ]]; then
            echo "🗑️  Removendo banco existente..."
            psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c "DROP DATABASE IF EXISTS $DB_NAME;" 2>/dev/null || true
            DB_EXISTS="0"
        fi
    else
        echo -e "${GREEN}✅ Banco parece estar completo!${NC}"
        exit 0
    fi
else
    echo -e "${RED}❌ Banco '$DB_NAME' NÃO existe!${NC}"
fi

# Criar banco se não existir
if [ "$DB_EXISTS" != "1" ]; then
    echo ""
    echo "🔨 Criando banco '$DB_NAME'..."
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c "CREATE DATABASE $DB_NAME;" 2>/dev/null || {
        echo -e "${RED}❌ Erro ao criar banco!${NC}"
        exit 1
    }
    echo -e "${GREEN}✅ Banco criado com sucesso!${NC}"
fi

# Executar script de inicialização
echo ""
echo "📜 Executando script de inicialização..."
SCRIPT_PATH="$(dirname "$0")/init_db_atualizado.sql"

if [ ! -f "$SCRIPT_PATH" ]; then
    echo -e "${RED}❌ Script não encontrado: $SCRIPT_PATH${NC}"
    exit 1
fi

echo "   Script: $SCRIPT_PATH"
echo ""

# Executar script com tratamento de erros
psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f "$SCRIPT_PATH" 2>&1 | tee /tmp/singleone_init.log

# Verificar resultado
if [ ${PIPESTATUS[0]} -eq 0 ]; then
    echo ""
    echo -e "${GREEN}✅ Script executado com sucesso!${NC}"
    
    # Verificar tabelas criadas
    TABLE_COUNT=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';" 2>/dev/null || echo "0")
    VIEW_COUNT=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -tAc "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public';" 2>/dev/null || echo "0")
    
    echo ""
    echo "📊 Resultado:"
    echo "   Tabelas: $TABLE_COUNT"
    echo "   Views: $VIEW_COUNT"
    echo ""
    echo -e "${GREEN}✅ Banco '$DB_NAME' está pronto para uso!${NC}"
else
    echo ""
    echo -e "${YELLOW}⚠️  Script executado com alguns erros. Verifique o log acima.${NC}"
    echo "   Log salvo em: /tmp/singleone_init.log"
fi

echo ""
echo "=========================================="
echo "✅ PROCESSO CONCLUÍDO"
echo "=========================================="

