#!/bin/bash

# 🐳 Script de Configuração Automática do Portainer para Servidor Remoto
# Para executar: bash setup-portainer-remoto.sh

set -e

echo "🚀 Iniciando configuração do Portainer no servidor remoto..."

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Função para log
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"
}

warn() {
    echo -e "${YELLOW}[AVISO]${NC} $1"
}

error() {
    echo -e "${RED}[ERRO]${NC} $1"
}

# Verificar se está executando como root
if [ "$EUID" -ne 0 ]; then
    error "Este script precisa ser executado como root (use sudo)"
    exit 1
fi

# Verificar se Docker está instalado
if ! command -v docker &> /dev/null; then
    error "Docker não está instalado. Instale primeiro."
    exit 1
fi

log "Docker encontrado: $(docker --version)"

# Verificar se Docker está rodando
if ! docker info &> /dev/null; then
    error "Docker não está rodando. Inicie o serviço Docker primeiro."
    exit 1
fi

log "Docker está rodando corretamente"

# Parar e remover Portainer existente (se houver)
if docker ps -a | grep -q portainer; then
    warn "Portainer já existe. Parando e removendo..."
    docker stop portainer 2>/dev/null || true
    docker rm portainer 2>/dev/null || true
fi

# Criar volume para persistir dados
log "Criando volume do Portainer..."
docker volume create portainer_data 2>/dev/null || warn "Volume já existe"

# Criar diretório /opt se não existir
mkdir -p /opt/portainer

# Criar arquivo docker-compose para Portainer
log "Criando configuração do Portainer..."
cat > /opt/portainer/docker-compose.yml << 'EOF'
version: '3.8'

services:
  portainer:
    image: portainer/portainer-ce:latest
    container_name: portainer
    restart: unless-stopped
    ports:
      - "9000:9000"  # Interface web
      - "8000:8000"  # Agentes (opcional)
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
      - portainer_data:/data
    networks:
      - portainer-network

volumes:
  portainer_data:

networks:
  portainer-network:
    driver: bridge
EOF

log "Arquivo docker-compose.yml criado em /opt/portainer/"

# Executar Portainer
log "Iniciando Portainer..."
cd /opt/portainer
docker-compose up -d

# Aguardar Portainer inicializar
log "Aguardando Portainer inicializar..."
sleep 10

# Verificar se Portainer está rodando
if docker ps | grep -q portainer; then
    log "✅ Portainer iniciado com sucesso!"
else
    error "❌ Falha ao iniciar Portainer. Verifique os logs:"
    docker logs portainer
    exit 1
fi

# Verificar status
log "Status dos containers:"
docker ps | grep portainer

# Configurar firewall básico (se ufw estiver disponível)
if command -v ufw &> /dev/null; then
    log "Configurando firewall..."
    ufw allow 9000/tcp comment "Portainer Web UI" 2>/dev/null || warn "Erro ao configurar firewall"
    ufw allow 8000/tcp comment "Portainer Agent" 2>/dev/null || warn "Erro ao configurar firewall"
    log "Firewall configurado (se ativo)"
fi

# Obter IP do servidor
SERVER_IP=$(hostname -I | awk '{print $1}')

echo ""
echo "🎉 Portainer configurado com sucesso!"
echo ""
echo "📋 Informações de acesso:"
echo "   URL: http://${SERVER_IP}:9000"
echo "   URL: http://84.247.128.180:9000"
echo ""
echo "🔧 Próximos passos:"
echo "   1. Acesse http://${SERVER_IP}:9000 no navegador"
echo "   2. Crie um usuário administrador no primeiro acesso"
echo "   3. Selecione 'Docker' como ambiente"
echo "   4. Configure seus containers e stacks"
echo ""
echo "📊 Comandos úteis:"
echo "   Ver logs: docker logs portainer"
echo "   Status:   docker ps | grep portainer"
echo "   Parar:    docker stop portainer"
echo "   Iniciar:  docker start portainer"
echo ""
echo "📁 Arquivos criados:"
echo "   Configuração: /opt/portainer/docker-compose.yml"
echo "   Volume:       portainer_data"
echo ""

log "Configuração finalizada!"












