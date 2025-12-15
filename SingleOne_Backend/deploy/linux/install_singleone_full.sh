#!/usr/bin/env bash

###############################################################################
# Instalação COMPLETA SingleOne sem Docker (Backend + Frontend + Nginx)
#
# Objetivo:
# - Padronizar instalação em novos servidores (ex.: FitBank) a partir do código.
#
# O que este script faz:
#  1) Garante dependências básicas (git, nginx, node, npm)
#  2) Instala e configura PostgreSQL (banco "singleone")
#  3) Publica a API em /opt/singleone-api-publish e cria serviço systemd
#  4) Faz build do frontend Angular e configura Nginx para servir SPA + proxy /api
#
# Uso típico em servidor novo (como root ou via sudo):
#   cd /opt
#   git clone <URL_DO_REPOSITORIO_MONOREPO_OU_BACKEND> SingleOne
#   cd /opt/SingleOne
#   chmod +x deploy/linux/install_singleone_full.sh
#   sudo SITE_DOMAIN="fitbank.singleone.com.br" \
#        SITE_IP="173.249.37.16" \
#        DB_PASSWORD="Admin@2025" \
#        deploy/linux/install_singleone_full.sh
#
# Variáveis de ambiente importantes (todas opcionais, possuem default):
#   DB_NAME       (default: singleone)
#   DB_USER       (default: postgres)
#   DB_PASSWORD   (default: Admin@2025)
#   SITE_DOMAIN   (ex.: fitbank.singleone.com.br)  -> usado no Nginx
#   SITE_IP       (ex.: 173.249.37.16)             -> fallback para server_name
#   USE_SSL       (default: false)                 -> se true, habilita bloco SSL no Nginx
###############################################################################

set -euo pipefail

if [[ "$EUID" -ne 0 ]]; then
  echo "❌ Este script precisa ser executado como root (use: sudo $0)"
  exit 1
fi

REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
# Se estamos em SingleOne_Backend, subir um nível para a raiz do monorepo
if [[ "$(basename "${REPO_DIR}")" == "SingleOne_Backend" ]]; then
  REPO_DIR="$(dirname "${REPO_DIR}")"
fi
PUBLISH_DIR="/opt/singleone-api-publish"
FRONTEND_DIR="${REPO_DIR}/SingleOne_Frontend"
FRONTEND_DIST_DIR="${FRONTEND_DIR}/dist/SingleOne"

DB_NAME="${DB_NAME:-singleone}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-Admin@2025}"

SITE_DOMAIN="${SITE_DOMAIN:-}"
SITE_IP="${SITE_IP:-}"
USE_SSL="${USE_SSL:-false}"

echo "======================================================="
echo " Instalação COMPLETA SingleOne SEM Docker - Linux"
echo "======================================================="
echo "📁 Repositório...............: ${REPO_DIR}"
echo "📁 Publicação API............: ${PUBLISH_DIR}"
echo "📁 Frontend (Angular)........: ${FRONTEND_DIR}"
echo "🗄  Banco de dados...........: ${DB_NAME}"
echo "👤 Usuário do banco..........: ${DB_USER}"
echo "🌐 Domínio (SITE_DOMAIN).....: ${SITE_DOMAIN:-<não definido>}"
echo "🌐 IP (SITE_IP)..............: ${SITE_IP:-<não definido>}"
echo "🔒 USE_SSL...................: ${USE_SSL}"
echo "======================================================="

echo
echo ">>> [0/6] Instalando pacotes básicos (git, curl, nginx)..."
apt update
apt install -y git curl nginx
systemctl enable nginx
systemctl start nginx

echo
echo ">>> [1/6] Instalando PostgreSQL..."
apt install -y postgresql postgresql-contrib
systemctl enable postgresql
systemctl start postgresql

echo
echo ">>> [1.1/6] Configurando usuário e banco..."
sudo -u postgres psql <<SQL
ALTER USER ${DB_USER} WITH PASSWORD '${DB_PASSWORD}';
SQL

# Criar banco se não existir (CREATE DATABASE não pode estar dentro de DO $$)
if ! sudo -u postgres psql -lqt | cut -d \| -f 1 | grep -qw "${DB_NAME}"; then
  echo "   Criando banco ${DB_NAME}..."
  sudo -u postgres createdb -O "${DB_USER}" "${DB_NAME}"
else
  echo "   Banco ${DB_NAME} já existe, pulando criação."
fi

echo
echo ">>> [2/6] Instalando .NET 6 (se necessário)..."
if ! command -v dotnet >/dev/null 2>&1; then
  cd /tmp
  wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
  dpkg -i packages-microsoft-prod.deb
  apt update
  apt install -y dotnet-sdk-6.0 aspnetcore-runtime-6.0
else
  echo "   .NET já instalado, pulando."
fi

echo
echo ">>> [3/6] Preparando banco (init_db_atualizado.sql)..."
# O init_db_atualizado.sql está na raiz do SingleOne_Backend
INIT_SQL="${REPO_DIR}/SingleOne_Backend/init_db_atualizado.sql"
if [[ ! -f "${INIT_SQL}" ]]; then
  # Tentar caminho alternativo se REPO_DIR já é SingleOne_Backend
  INIT_SQL="${REPO_DIR}/init_db_atualizado.sql"
  if [[ ! -f "${INIT_SQL}" ]]; then
    echo "❌ Arquivo init_db_atualizado.sql não encontrado em ${REPO_DIR}/SingleOne_Backend/ nem em ${REPO_DIR}/"
    exit 1
  fi
fi

# Usar PGPASSWORD para autenticação sem prompt
PGPASSWORD="${DB_PASSWORD}" psql -h 127.0.0.1 -U "${DB_USER}" -d "${DB_NAME}" -f "${INIT_SQL}"

echo
echo ">>> [4/6] Publicando API SingleOne..."
mkdir -p "${PUBLISH_DIR}"
# Ajustar caminho da API conforme estrutura do repositório
API_DIR="${REPO_DIR}/SingleOne_Backend/SingleOneAPI"
if [[ ! -d "${API_DIR}" ]]; then
  API_DIR="${REPO_DIR}/SingleOneAPI"
  if [[ ! -d "${API_DIR}" ]]; then
    echo "❌ Diretório SingleOneAPI não encontrado em ${REPO_DIR}/SingleOne_Backend/ nem em ${REPO_DIR}/"
    exit 1
  fi
fi
cd "${API_DIR}"
dotnet publish -c Release -o "${PUBLISH_DIR}"

echo
echo ">>> [4.1/6] Criando arquivo appsettings.json básico (se não existir)..."
if [[ ! -f "${PUBLISH_DIR}/appsettings.json" ]]; then
  cat > "${PUBLISH_DIR}/appsettings.json" <<EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
EOF
fi

echo
echo ">>> [4.2/6] Criando serviço systemd singleone-api..."
cat > /etc/systemd/system/singleone-api.service <<EOF
[Unit]
Description=SingleOne API (.NET 6, sem Docker)
After=network.target postgresql.service

[Service]
WorkingDirectory=${PUBLISH_DIR}
ExecStart=/usr/bin/dotnet ${PUBLISH_DIR}/SingleOneAPI.dll
Restart=always
RestartSec=10
SyslogIdentifier=singleone-api

Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
Environment=SITE_URL=http://${SITE_DOMAIN:-${SITE_IP:-localhost}}

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
systemctl enable --now singleone-api

echo
echo ">>> [5/6] Instalando Node.js + npm para build do frontend..."
if ! command -v node >/dev/null 2>&1; then
  # Instalação simples do Node 18.x (LTS) via NodeSource
  curl -fsSL https://deb.nodesource.com/setup_18.x | bash -
  apt install -y nodejs
else
  echo "   Node.js já instalado, pulando."
fi

echo
echo ">>> [5.1/6] Fazendo build do frontend Angular..."
if [[ ! -d "${FRONTEND_DIR}" ]]; then
  echo "❌ Diretório do frontend não encontrado em ${FRONTEND_DIR}"
  exit 1
fi

cd "${FRONTEND_DIR}"
# Tentar npm ci primeiro (mais rápido e determinístico), depois npm install com --legacy-peer-deps se falhar
if [[ -f "package-lock.json" ]]; then
  npm ci || npm install --legacy-peer-deps
else
  npm install --legacy-peer-deps
fi
npm run build -- --configuration production || npm run build --prod

echo
echo ">>> [6/6] Configurando Nginx para servir SPA e proxy da API..."

FRONTEND_DIST_DIR_RESOLVED="${FRONTEND_DIST_DIR}"
if [[ ! -d "${FRONTEND_DIST_DIR_RESOLVED}" ]]; then
  # fallback: tentar achar dist
  if [[ -d "${FRONTEND_DIR}/dist" ]]; then
    FRONTEND_DIST_DIR_RESOLVED="${FRONTEND_DIR}/dist"
  fi
fi

if [[ ! -d "${FRONTEND_DIST_DIR_RESOLVED}" ]]; then
  echo "❌ Diretório dist do frontend não encontrado após build."
  exit 1
fi

SERVER_NAME_VALUE="_"
if [[ -n "${SITE_DOMAIN}" ]]; then
  SERVER_NAME_VALUE="${SITE_DOMAIN}"
elif [[ -n "${SITE_IP}" ]]; then
  SERVER_NAME_VALUE="${SITE_IP}"
fi

NGINX_CONF_PATH="/etc/nginx/sites-available/singleone"

cat > "${NGINX_CONF_PATH}" <<EOF
server {
    listen 80 default_server;
    listen [::]:80 default_server;
    server_name ${SERVER_NAME_VALUE} _;
EOF

if [[ "${USE_SSL}" == "true" ]]; then
  cat >> "${NGINX_CONF_PATH}" <<'EOF'
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2 default_server;
    listen [::]:443 ssl http2 default_server;
    server_name ${SERVER_NAME_VALUE} _;

    # Ajustar caminhos dos certificados conforme Let's Encrypt
    ssl_certificate /etc/letsencrypt/live/${SERVER_NAME_VALUE}/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/${SERVER_NAME_VALUE}/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
EOF
else
  cat >> "${NGINX_CONF_PATH}" <<'EOF'

    # Sem SSL: servir direto em HTTP
EOF
fi

cat >> "${NGINX_CONF_PATH}" <<EOF

    root ${FRONTEND_DIST_DIR_RESOLVED};
    index index.html;

    # Proxy para API
    location /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }

    # Proxy para logos
    location ^~ /api/logos/ {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header Connection "";
        proxy_buffering off;
    }

    # Proxy para Hangfire
    location /hangfire {
        proxy_pass http://127.0.0.1:5000/hangfire;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header Connection "";
        proxy_buffering off;
    }

    # Assets estáticos (apenas para arquivos do frontend, não /api/)
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        if (\$request_uri ~ ^/api/) {
            return 404;
        }
        expires 1y;
        try_files \$uri =404;
    }

    # Angular routing
    location / {
        try_files \$uri \$uri/ /index.html;
    }
}
EOF

ln -sf "${NGINX_CONF_PATH}" /etc/nginx/sites-enabled/singleone
rm -f /etc/nginx/sites-enabled/default || true

nginx -t
systemctl reload nginx

echo
echo "======================================================="
echo " Instalação COMPLETA concluída."
echo " - API  : serviço systemd 'singleone-api' (porta 5000)"
echo " - Frontend: servido via Nginx (site 'singleone')"
echo " - Banco: PostgreSQL banco '${DB_NAME}'"
echo "======================================================="
echo "Teste:"
echo "  systemctl status singleone-api"
echo "  curl http://localhost:5000/swagger"
echo "  Acesse pelo navegador: http://${SITE_DOMAIN:-${SITE_IP:-<IP_DO_SERVIDOR>}}"
echo


