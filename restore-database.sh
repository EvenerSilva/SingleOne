#!/bin/bash
# ============================================
# Script de Restauração do Banco de Dados
# SingleOne - PostgreSQL
# ============================================

set -e

# Carregar variáveis de ambiente
if [ -f ".env.production" ]; then
    export $(cat .env.production | grep -v '^#' | xargs)
fi

# Configurações
BACKUP_DIR="./backups"
CONTAINER_NAME="singleone-postgres-prod"

# Cores
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo "🔄 Iniciando restauração do banco de dados..."

# Listar backups disponíveis
echo ""
echo "📋 Backups disponíveis:"
ls -lh ${BACKUP_DIR}/singleone_backup_*.sql.gz 2>/dev/null || {
    echo -e "${RED}❌ Nenhum backup encontrado em ${BACKUP_DIR}${NC}"
    exit 1
}

echo ""
read -p "📝 Digite o nome do arquivo de backup para restaurar: " BACKUP_FILE

# Verificar se arquivo existe
if [ ! -f "${BACKUP_DIR}/${BACKUP_FILE}" ]; then
    echo -e "${RED}❌ Arquivo ${BACKUP_DIR}/${BACKUP_FILE} não encontrado!${NC}"
    exit 1
fi

# Confirmar restauração
echo -e "${YELLOW}⚠️  ATENÇÃO: Esta operação vai SOBRESCREVER todos os dados atuais!${NC}"
read -p "❓ Tem certeza que deseja continuar? (digite 'sim' para confirmar): " CONFIRM

if [ "$CONFIRM" != "sim" ]; then
    echo "❌ Operação cancelada."
    exit 0
fi

# Fazer backup do banco atual antes de restaurar
echo "💾 Fazendo backup de segurança do banco atual..."
./backup-database.sh

# Verificar se o container está rodando
if [ ! "$(docker ps -q -f name=${CONTAINER_NAME})" ]; then
    echo -e "${RED}❌ Container ${CONTAINER_NAME} não está rodando!${NC}"
    exit 1
fi

# Descomprimir backup
echo "🗜️  Descomprimindo backup..."
TEMP_FILE="/tmp/restore_temp.sql"
gunzip -c "${BACKUP_DIR}/${BACKUP_FILE}" > ${TEMP_FILE}

# Restaurar backup
echo "🔄 Restaurando backup..."
cat ${TEMP_FILE} | docker exec -i ${CONTAINER_NAME} psql -U ${POSTGRES_USER:-postgres} ${POSTGRES_DB:-singleone}

# Limpar arquivo temporário
rm ${TEMP_FILE}

echo -e "${GREEN}✅ Restauração concluída com sucesso!${NC}"
echo ""
echo "🔄 Recomenda-se reiniciar o backend:"
echo "   docker-compose -f docker-compose.prod.yml restart backend"

























