#!/usr/bin/env bash

###############################################################################
# Instalação SingleOne sem Docker (PostgreSQL + API .NET diretamente no Linux)
#
# Uso no servidor (como root ou via sudo):
#   cd /opt/SingleOne
#   chmod +x deploy/linux/install_singleone_nodocker.sh
#   sudo deploy/linux/install_singleone_nodocker.sh
#
# Pré-requisito: este repositório já clonado em /opt/SingleOne (ou outro
# diretório equivalente onde o script está sendo executado).
###############################################################################

set -euo pipefail

echo "======================================================="
echo " Instalação SingleOne SEM Docker - Linux (systemd)"
echo "======================================================="

if [[ "$EUID" -ne 0 ]]; then
  echo "❌ Este script precisa ser executado como root (use: sudo $0)"
  exit 1
fi

REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PUBLISH_DIR="/opt/singleone-api-publish"
DB_NAME="singleone"
DB_USER="postgres"
DB_PASSWORD="${DB_PASSWORD:-Admin@2025}"

echo "📁 Diretório do repositório: ${REPO_DIR}"
echo "📁 Diretório de publicação da API: ${PUBLISH_DIR}"
echo "🗄  Banco de dados: ${DB_NAME}"
echo "👤 Usuário do banco: ${DB_USER}"
echo "🔑 Senha do banco: (oculta)"

echo
echo ">>> [1/5] Removendo Docker (se ainda existir)..."
if command -v docker >/dev/null 2>&1; then
  # Tentar parar e limpar docker sem falhar se algo não existir
  systemctl stop docker 2>/dev/null || true

  docker stop $(docker ps -aq) 2>/dev/null || true
  docker rm $(docker ps -aq) 2>/dev/null || true
  docker system prune -a --volumes -f 2>/dev/null || true

  apt remove --purge -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin docker-compose 2>/dev/null || true
  rm -rf /var/lib/docker /var/lib/containerd
  rm -f /usr/local/bin/docker-compose
  apt autoremove -y
else
  echo "   Docker não está instalado ou não está no PATH. Pulando remoção."
fi

echo
echo ">>> [2/5] Instalando PostgreSQL..."
apt update
apt install -y postgresql postgresql-contrib
systemctl enable postgresql
systemctl start postgresql

echo
echo ">>> [2.1/5] Configurando usuário e banco..."
sudo -u postgres psql <<SQL
ALTER USER ${DB_USER} WITH PASSWORD '${DB_PASSWORD}';
DO \$\$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = '${DB_NAME}') THEN
    CREATE DATABASE ${DB_NAME} OWNER ${DB_USER};
  END IF;
END
\$\$;
SQL

echo
echo ">>> [2.2/5] Executando script init_db_atualizado.sql..."
if [[ ! -f "${REPO_DIR}/init_db_atualizado.sql" ]]; then
  echo "❌ Arquivo init_db_atualizado.sql não encontrado em ${REPO_DIR}"
  exit 1
fi

psql -h 127.0.0.1 -U "${DB_USER}" -d "${DB_NAME}" -w -f "${REPO_DIR}/init_db_atualizado.sql"

echo
echo ">>> [3/5] Instalando .NET 6..."
if ! command -v dotnet >/dev/null 2>&1; then
  cd /tmp
  wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
  dpkg -i packages-microsoft-prod.deb
  apt update
  apt install -y dotnet-sdk-6.0 aspnetcore-runtime-6.0
else
  echo "   .NET já está instalado. Pulando instalação."
fi

echo
echo ">>> [4/5] Publicando API SingleOne..."
mkdir -p "${PUBLISH_DIR}"
cd "${REPO_DIR}/SingleOne_Backend/SingleOneAPI"
dotnet publish -c Release -o "${PUBLISH_DIR}"

echo
echo ">>> [4.1/5] Criando arquivo .env para a API..."
cat > "${PUBLISH_DIR}/.env" <<EOF
DB_HOST=127.0.0.1
DB_USER=${DB_USER}
DB_PASSWORD=${DB_PASSWORD}
SITE_URL=http://localhost:5000
EOF

echo
echo ">>> [5/5] Instalando serviço systemd singleone-api..."
cp "${REPO_DIR}/deploy/linux/singleone-api.service" /etc/systemd/system/singleone-api.service
systemctl daemon-reload
systemctl enable --now singleone-api

echo
echo "======================================================="
echo " Instalação concluída."
echo " - Banco: ${DB_NAME} (PostgreSQL nativo)"
echo " - API  : serviço systemd: singleone-api"
echo "======================================================="
echo
echo "Use para verificar o status:"
echo "  systemctl status singleone-api"
echo
echo "Testar API localmente:"
echo "  curl http://localhost:5000/swagger"
echo "ou:"
echo "  curl http://localhost:5000/api/health"
echo


