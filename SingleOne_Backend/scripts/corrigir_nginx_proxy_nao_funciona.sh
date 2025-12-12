#!/bin/bash

# Script para corrigir quando o Nginx não está fazendo proxy para /api/

echo "=========================================="
echo "🔧 CORRIGINDO NGINX - PROXY NÃO FUNCIONA"
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
echo "✅ Backup criado: $BACKUP_FILE"
echo ""

# Verificar problema atual
echo "📋 Verificando problema atual..."
if grep -A 3 "location /api/" "$NGINX_CONFIG" | grep -q "try_files"; then
    echo "❌ PROBLEMA ENCONTRADO: /api/ está usando try_files em vez de proxy_pass!"
    echo "   Isso faz o Nginx tentar servir arquivos estáticos em vez de fazer proxy"
fi
echo ""

# Recriar configuração CORRETA
echo "📝 Recriando configuração com proxy_pass correto..."
echo ""

if [ "$HAS_SSL" = true ]; then
    cat > "$NGINX_CONFIG" << 'NGINX_HTTPS_EOF'
# Redirect HTTP to HTTPS
server {
    listen 80 default_server;
    listen [::]:80 default_server;
    server_name demo.singleone.com.br 84.247.128.180 _;
    return 301 https://$server_name$request_uri;
}

# HTTPS Server
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

    # ✅ CRÍTICO: location /api/ DEVE usar proxy_pass, NUNCA try_files
    # Esta é a PRIMEIRA regra e tem prioridade máxima
    location /api/ {
        # NUNCA usar try_files aqui! Sempre proxy_pass
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Connection "";
        proxy_buffering off;
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }

    # Assets estáticos do frontend (NÃO /api/)
    # Esta regra regex NÃO deve capturar /api/ porque já foi processado acima
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        # Proteção extra: se for /api/, retornar 404 para forçar proxy
        if ($request_uri ~ ^/api/) {
            return 404;
        }
        expires 1y;
        add_header Cache-Control "public, immutable";
        try_files $uri =404;
    }

    # Angular routing
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Não fazer cache do index.html
    location = /index.html {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma "no-cache";
        add_header Expires "0";
    }
}
NGINX_HTTPS_EOF
else
    cat > "$NGINX_CONFIG" << 'NGINX_HTTP_EOF'
server {
    listen 80 default_server;
    listen [::]:80 default_server;
    server_name demo.singleone.com.br 84.247.128.180 _;

    root /opt/SingleOne/SingleOne_Frontend/dist/SingleOne;
    index index.html;

    # ✅ CRÍTICO: location /api/ DEVE usar proxy_pass, NUNCA try_files
    # Esta é a PRIMEIRA regra e tem prioridade máxima
    location /api/ {
        # NUNCA usar try_files aqui! Sempre proxy_pass
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Connection "";
        proxy_buffering off;
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }

    # Assets estáticos do frontend (NÃO /api/)
    # Esta regra regex NÃO deve capturar /api/ porque já foi processado acima
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        # Proteção extra: se for /api/, retornar 404 para forçar proxy
        if ($request_uri ~ ^/api/) {
            return 404;
        }
        expires 1y;
        add_header Cache-Control "public, immutable";
        try_files $uri =404;
    }

    # Angular routing
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Não fazer cache do index.html
    location = /index.html {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma "no-cache";
        add_header Expires "0";
    }
}
NGINX_HTTP_EOF
fi

# Garantir link
ln -sf "$NGINX_CONFIG" "$NGINX_ENABLED"
echo "✅ Configuração recriada"
echo ""

# Verificar se está correto
echo "📋 Verificando se a configuração está correta..."
if grep -A 2 "location /api/" "$NGINX_CONFIG" | grep -q "proxy_pass"; then
    echo "✅ Configuração correta: /api/ usa proxy_pass"
else
    echo "❌ ERRO: /api/ NÃO está usando proxy_pass!"
    exit 1
fi

if grep -A 2 "location /api/" "$NGINX_CONFIG" | grep -q "try_files"; then
    echo "❌ ERRO: /api/ está usando try_files (ERRADO!)"
    exit 1
else
    echo "✅ Configuração correta: /api/ NÃO usa try_files"
fi
echo ""

# Testar sintaxe
echo "🧪 Testando sintaxe..."
if nginx -t 2>&1 | grep -q "syntax is ok"; then
    echo "✅ Sintaxe válida"
else
    echo "❌ Erro na sintaxe!"
    nginx -t
    exit 1
fi
echo ""

# Recarregar
echo "🔄 Recarregando Nginx..."
systemctl reload nginx
if [ $? -eq 0 ]; then
    echo "✅ Nginx recarregado"
else
    echo "❌ Erro ao recarregar, tentando reiniciar..."
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
echo "🧪 Testando acesso..."
sleep 2
TEST_FILE="cliente_1_20250815151721.png"
RESPONSE=$(curl -s -L -o /dev/null -w "%{http_code}" "http://127.0.0.1/api/logos/$TEST_FILE" 2>/dev/null)
echo "   URL: http://127.0.0.1/api/logos/$TEST_FILE"
echo "   Resposta: $RESPONSE"

if [ "$RESPONSE" = "200" ]; then
    echo "✅ FUNCIONANDO! Proxy está correto agora"
else
    echo "⚠️  Ainda retorna $RESPONSE"
    echo ""
    echo "📋 Verificando se o backend está respondendo..."
    BACKEND_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "http://127.0.0.1:5000/api/logos/$TEST_FILE" 2>/dev/null)
    echo "   Backend: $BACKEND_RESPONSE"
    if [ "$BACKEND_RESPONSE" = "200" ]; then
        echo "   ✅ Backend OK, problema está no Nginx"
        echo "   Verifique logs: tail -f /var/log/nginx/error.log"
    fi
fi
echo ""

echo "=========================================="
echo "✅ CORREÇÃO CONCLUÍDA"
echo "=========================================="
echo ""

