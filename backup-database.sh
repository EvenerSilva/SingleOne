#!/bin/bash
# ============================================
# Script de Backup do Banco de Dados
# SingleOne - PostgreSQL
# ============================================

set -e

# Carregar variáveis de ambiente
if [ -f ".env.production" ]; then
    export $(cat .env.production | grep -v '^#' | xargs)
fi

# Configurações
BACKUP_DIR="./backups"
DATE=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILE="${BACKUP_DIR}/singleone_backup_${DATE}.sql"
CONTAINER_NAME="singleone-postgres-prod"

# Cores
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

echo "📦 Iniciando backup do banco de dados..."

# Criar diretório de backup se não existir
mkdir -p ${BACKUP_DIR}

# Verificar se o container está rodando
if [ ! "$(docker ps -q -f name=${CONTAINER_NAME})" ]; then
    echo -e "${RED}❌ Container ${CONTAINER_NAME} não está rodando!${NC}"
    exit 1
fi

# Fazer backup
echo "💾 Criando backup em ${BACKUP_FILE}..."
docker exec ${CONTAINER_NAME} pg_dump -U ${POSTGRES_USER:-postgres} ${POSTGRES_DB:-singleone} > ${BACKUP_FILE}

# Comprimir backup
echo "🗜️  Comprimindo backup..."
gzip ${BACKUP_FILE}

COMPRESSED_FILE="${BACKUP_FILE}.gz"
FILE_SIZE=$(du -h ${COMPRESSED_FILE} | cut -f1)

echo -e "${GREEN}✅ Backup criado com sucesso!${NC}"
echo "📄 Arquivo: ${COMPRESSED_FILE}"
echo "💾 Tamanho: ${FILE_SIZE}"

# Manter apenas os últimos 7 backups
echo "🧹 Limpando backups antigos (mantendo últimos 7)..."
ls -t ${BACKUP_DIR}/singleone_backup_*.sql.gz | tail -n +8 | xargs -r rm

echo -e "${GREEN}✅ Processo de backup concluído!${NC}"

























