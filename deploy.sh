#!/bin/bash
# ============================================
# Script de Deploy - SingleOne
# Para uso no servidor Contabo Ubuntu
# ============================================

set -e  # Para em caso de erro

echo "🚀 Iniciando deploy do SingleOne..."

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Verificar se .env.production existe
if [ ! -f ".env.production" ]; then
    echo -e "${RED}❌ Erro: Arquivo .env.production não encontrado!${NC}"
    echo -e "${YELLOW}📝 Por favor, copie env.production.example para .env.production e configure as variáveis.${NC}"
    exit 1
fi

# Carregar variáveis de ambiente
export $(cat .env.production | grep -v '^#' | xargs)

echo -e "${GREEN}✅ Variáveis de ambiente carregadas${NC}"

# Verificar se Docker está instalado
if ! command -v docker &> /dev/null; then
    echo -e "${RED}❌ Docker não está instalado!${NC}"
    exit 1
fi

# Verificar se Docker Compose está instalado
if ! command -v docker-compose &> /dev/null; then
    echo -e "${RED}❌ Docker Compose não está instalado!${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Docker e Docker Compose encontrados${NC}"

# Perguntar se quer fazer backup do banco de dados
read -p "📦 Deseja fazer backup do banco de dados antes de continuar? (s/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Ss]$ ]]; then
    echo "📦 Criando backup..."
    ./backup-database.sh
    echo -e "${GREEN}✅ Backup criado com sucesso${NC}"
fi

# Parar containers antigos
echo "🛑 Parando containers antigos..."
docker-compose -f docker-compose.prod.yml down

# Fazer pull das imagens base
echo "📥 Atualizando imagens base..."
docker-compose -f docker-compose.prod.yml pull postgres

# Build das imagens
echo "🔨 Fazendo build das imagens..."
docker-compose -f docker-compose.prod.yml build --no-cache

# Subir os containers
echo "🚀 Iniciando containers..."
docker-compose -f docker-compose.prod.yml up -d

# Aguardar backend estar pronto
echo "⏳ Aguardando backend inicializar..."
sleep 10

# Verificar status dos containers
echo ""
echo "📊 Status dos containers:"
docker-compose -f docker-compose.prod.yml ps

# Verificar logs
echo ""
echo "📝 Últimos logs:"
docker-compose -f docker-compose.prod.yml logs --tail=20

# Verificar health
echo ""
echo "🏥 Verificando saúde dos serviços..."
sleep 5

BACKEND_HEALTH=$(docker inspect --format='{{.State.Health.Status}}' singleone-backend-prod 2>/dev/null || echo "unknown")
FRONTEND_HEALTH=$(docker inspect --format='{{.State.Health.Status}}' singleone-frontend-prod 2>/dev/null || echo "unknown")

echo "Backend: $BACKEND_HEALTH"
echo "Frontend: $FRONTEND_HEALTH"

echo ""
echo -e "${GREEN}✅ Deploy concluído!${NC}"
echo ""
echo "🌐 URLs de acesso:"
echo "   Frontend: http://SEU_IP:${FRONTEND_PORT:-3000}"
echo "   Backend API: http://SEU_IP:${BACKEND_PORT:-5000}"
echo ""
echo "📊 Para ver logs em tempo real:"
echo "   docker-compose -f docker-compose.prod.yml logs -f"
echo ""
echo "🛑 Para parar os containers:"
echo "   docker-compose -f docker-compose.prod.yml down"
echo ""
