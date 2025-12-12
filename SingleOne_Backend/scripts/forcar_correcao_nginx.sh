#!/bin/bash

# Script para FORÇAR correção do Nginx, removendo interferências

echo "=========================================="
echo "🔧 FORÇANDO CORREÇÃO DO NGINX"
echo "=========================================="
echo ""

# 1. Desabilitar arquivo default se existir
echo "📋 [1/5] Desabilitando arquivo default do Nginx..."
if [ -L "/etc/nginx/sites-enabled/default" ]; then
    rm /etc/nginx/sites-enabled/default
    echo "✅ Arquivo default desabilitado"
elif [ -f "/etc/nginx/sites-enabled/default" ]; then
    rm /etc/nginx/sites-enabled/default
    echo "✅ Arquivo default removido"
else
    echo "✅ Nenhum arquivo default encontrado"
fi
echo ""

# 2. Garantir que apenas singleone está habilitado
echo "📋 [2/5] Verificando arquivos habilitados..."
echo "   Arquivos atualmente habilitados:"
ls -la /etc/nginx/sites-enabled/ 2>/dev/null
echo ""

# 3. Recriar configuração do zero
echo "📋 [3/5] Recriando configuração do zero..."
NGINX_CONFIG="/etc/nginx/sites-available/singleone"
NGINX_ENABLED="/etc/nginx/sites-enabled/singleone"

# Verificar SSL
DOMAIN="demo.singleone.com.br"
CERT_PATH="/etc/letsencrypt/live/$DOMAIN"
HAS_SSL=false

if [ -f "$CERT_PATH/fullchain.pem" ] && [ -f "$CERT_PATH/privkey.pem" ]; then
    HAS_SSL=true
fi

# Backup
BACKUP_FILE="${NGINX_CONFIG}.backup.$(date +%Y%m%d_%H%M%S)"
cp "$NGINX_CONFIG" "$BACKUP_FILE" 2>/dev/null || true
echo "✅ Backup criado (se existir)"

# Criar configuração mínima e correta
if [ "$HAS_SSL" = true ]; then
    cat > "$NGINX_CONFIG" << 'EOF'
server {
    listen 80 default_server;
    listen [::]:80 default_server;
    server_name demo.singleone.com.br 84.247.128.180 _;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2 default_server;
    listen [::]:443 ssl http2 default_server;
    server_name demo.singleone.com.br 84.247.128.180 _;

    ssl_certificate /etc/letsencrypt/live/demo.singleone.com.br/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/demo.singleone.com.br/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    root /opt/SingleOne/SingleOne_Frontend/dist/SingleOne;
    index index.html;

    # PROXY PARA API - PRIMEIRO, ANTES DE TUDO
    location /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Assets estáticos (NÃO /api/)
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        if ($request_uri ~ ^/api/) {
            return 404;
        }
        expires 1y;
        try_files $uri =404;
    }

    # Angular routing
    location / {
        try_files $uri $uri/ /index.html;
    }
}
EOF
else
    cat > "$NGINX_CONFIG" << 'EOF'
server {
    listen 80 default_server;
    listen [::]:80 default_server;
    server_name demo.singleone.com.br 84.247.128.180 _;

    root /opt/SingleOne/SingleOne_Frontend/dist/SingleOne;
    index index.html;

    # PROXY PARA API - PRIMEIRO, ANTES DE TUDO
    location /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Assets estáticos (NÃO /api/)
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        if ($request_uri ~ ^/api/) {
            return 404;
        }
        expires 1y;
        try_files $uri =404;
    }

    # Angular routing
    location / {
        try_files $uri $uri/ /index.html;
    }
}
EOF
fi

# Garantir link
ln -sf "$NGINX_CONFIG" "$NGINX_ENABLED"
echo "✅ Configuração recriada"
echo ""

# 4. Testar e recarregar
echo "📋 [4/5] Testando configuração..."
if nginx -t; then
    echo "✅ Configuração válida"
else
    echo "❌ Erro na configuração!"
    exit 1
fi
echo ""

# 5. Recarregar Nginx
echo "📋 [5/5] Recarregando Nginx..."
systemctl reload nginx
if [ $? -eq 0 ]; then
    echo "✅ Nginx recarregado"
else
    systemctl restart nginx
    if [ $? -eq 0 ]; then
        echo "✅ Nginx reiniciado"
    else
        echo "❌ Erro ao reiniciar Nginx!"
        exit 1
    fi
fi
echo ""

# Testar
echo "🧪 Testando..."
TEST_FILE="cliente_1_20250815151721.png"
RESPONSE=$(curl -s -L -o /dev/null -w "%{http_code}" "http://127.0.0.1/api/logos/$TEST_FILE" 2>/dev/null)
echo "   Resposta: $RESPONSE"

if [ "$RESPONSE" = "200" ]; then
    echo "✅ FUNCIONANDO!"
else
    echo "❌ Ainda com problema"
    echo "   Execute o diagnóstico: sudo bash /opt/SingleOne/SingleOne_Backend/scripts/diagnosticar_nginx_404.sh"
fi
echo ""

