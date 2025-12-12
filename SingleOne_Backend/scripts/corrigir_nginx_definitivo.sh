#!/bin/bash

# Script definitivo para corrigir o Nginx - força a configuração correta

echo "=========================================="
echo "🔧 CORREÇÃO DEFINITIVA DO NGINX"
echo "=========================================="
echo ""

NGINX_CONFIG="/etc/nginx/sites-available/singleone"
NGINX_ENABLED="/etc/nginx/sites-enabled/singleone"

# Verificar SSL
DOMAIN="demo.singleone.com.br"
CERT_PATH="/etc/letsencrypt/live/$DOMAIN"
HAS_SSL=false

if [ -f "$CERT_PATH/fullchain.pem" ] && [ -f "$CERT_PATH/privkey.pem" ]; then
    HAS_SSL=true
    echo "✅ Certificado SSL encontrado"
else
    echo "ℹ️  Sem certificado SSL"
fi
echo ""

# Backup
BACKUP_FILE="${NGINX_CONFIG}.backup.$(date +%Y%m%d_%H%M%S)"
cp "$NGINX_CONFIG" "$BACKUP_FILE"
echo "✅ Backup: $BACKUP_FILE"
echo ""

# Remover arquivo default se existir
if [ -L "/etc/nginx/sites-enabled/default" ] || [ -f "/etc/nginx/sites-enabled/default" ]; then
    rm -f /etc/nginx/sites-enabled/default
    echo "✅ Arquivo default removido"
fi

# Recriar configuração do zero - VERSÃO SIMPLIFICADA E GARANTIDA
echo "📝 Criando configuração garantida..."
echo ""

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

    # PROXY PARA API - DEVE SER A PRIMEIRA REGRA
    location /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Assets estáticos (apenas para arquivos do frontend, não /api/)
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

    # PROXY PARA API - DEVE SER A PRIMEIRA REGRA
    location /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Assets estáticos (apenas para arquivos do frontend, não /api/)
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
echo "✅ Configuração criada"
echo ""

# Verificar sintaxe
echo "🧪 Testando sintaxe..."
if nginx -t 2>&1 | grep -q "syntax is ok"; then
    echo "✅ Sintaxe OK"
else
    echo "❌ Erro na sintaxe!"
    nginx -t
    exit 1
fi
echo ""

# Parar e reiniciar Nginx completamente
echo "🔄 Reiniciando Nginx completamente..."
systemctl stop nginx
sleep 2
systemctl start nginx
sleep 2

if systemctl is-active --quiet nginx; then
    echo "✅ Nginx reiniciado"
else
    echo "❌ Erro ao reiniciar Nginx!"
    systemctl status nginx --no-pager | head -10
    exit 1
fi
echo ""

# Testar
echo "🧪 Testando..."
sleep 2
TEST_FILE="cliente_1_20250815151721.png"

# Testar backend primeiro
echo "   Testando backend diretamente..."
BACKEND_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "http://127.0.0.1:5000/api/logos/$TEST_FILE" 2>/dev/null)
echo "   Backend: $BACKEND_RESPONSE"

if [ "$BACKEND_RESPONSE" != "200" ]; then
    echo "   ⚠️  Backend não está respondendo corretamente"
    echo "   Verifique: systemctl status singleone-api"
fi
echo ""

# Testar via Nginx
echo "   Testando via Nginx (HTTPS)..."
NGINX_RESPONSE=$(curl -s -k -o /dev/null -w "%{http_code}" "https://127.0.0.1/api/logos/$TEST_FILE" 2>/dev/null)
echo "   Nginx HTTPS: $NGINX_RESPONSE"

if [ "$NGINX_RESPONSE" = "200" ]; then
    echo "✅ FUNCIONANDO!"
elif [ "$NGINX_RESPONSE" = "404" ]; then
    echo "❌ Ainda retorna 404"
    echo ""
    echo "📋 Verificando configuração atual..."
    echo "   Bloco /api/:"
    sed -n '/location \/api\/ {/,/^[[:space:]]*}/p' "$NGINX_CONFIG"
    echo ""
    echo "📋 Verificando logs..."
    tail -5 /var/log/nginx/error.log | grep -i "api\|logo" || echo "   Nenhum erro relacionado"
else
    echo "⚠️  Resposta: $NGINX_RESPONSE"
fi
echo ""

echo "=========================================="
echo "✅ CORREÇÃO CONCLUÍDA"
echo "=========================================="
echo ""

