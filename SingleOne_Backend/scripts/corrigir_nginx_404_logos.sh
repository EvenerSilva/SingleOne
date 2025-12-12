#!/bin/bash

# Script para corrigir definitivamente o problema de 404 no Nginx para /api/logos

echo "=========================================="
echo "🔧 CORRIGINDO 404 DO NGINX PARA /api/logos"
echo "=========================================="
echo ""

NGINX_CONFIG="/etc/nginx/sites-available/singleone"
NGINX_ENABLED="/etc/nginx/sites-enabled/singleone"

# Verificar se há certificado SSL
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

# Criar backup
BACKUP_FILE="${NGINX_CONFIG}.backup.$(date +%Y%m%d_%H%M%S)"
cp "$NGINX_CONFIG" "$BACKUP_FILE"
echo "✅ Backup criado: $BACKUP_FILE"
echo ""

# Verificar configuração atual
echo "📋 Verificando configuração atual..."
if grep -A 5 "location /api/" "$NGINX_CONFIG" | grep -q "proxy_pass"; then
    echo "✅ Proxy para /api/ encontrado"
else
    echo "❌ Proxy para /api/ NÃO encontrado!"
fi

# Verificar se há regra de cache de imagens interferindo
if grep -A 3 "location ~\* \\.(jpg\|jpeg\|png" "$NGINX_CONFIG" | grep -q "try_files"; then
    echo "⚠️  Regra de cache de imagens pode estar interferindo"
fi
echo ""

# Recriar configuração correta
echo "📝 Recriando configuração do Nginx com prioridade correta..."
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

    # SSL Configuration
    ssl_certificate /etc/letsencrypt/live/demo.singleone.com.br/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/demo.singleone.com.br/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;

    root /opt/SingleOne/SingleOne_Frontend/dist/SingleOne;
    index index.html;

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css text/xml text/javascript application/x-javascript application/xml+rss application/json application/javascript;

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;

    # ✅ CRÍTICO: Proxy para API - DEVE vir ANTES de qualquer outra regra
    # Esta regra tem prioridade sobre todas as outras porque é mais específica
    location /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Connection "";
        proxy_buffering off;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }

    # Cache para assets estáticos do frontend (NÃO /api/)
    # Esta regra NÃO deve capturar /api/ porque /api/ já foi processado acima
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        # Garantir que não processa /api/
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

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css text/xml text/javascript application/x-javascript application/xml+rss application/json application/javascript;

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;

    # ✅ CRÍTICO: Proxy para API - DEVE vir ANTES de qualquer outra regra
    # Esta regra tem prioridade sobre todas as outras porque é mais específica
    location /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Connection "";
        proxy_buffering off;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }

    # Cache para assets estáticos do frontend (NÃO /api/)
    # Esta regra NÃO deve capturar /api/ porque /api/ já foi processado acima
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        # Garantir que não processa /api/
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

# Garantir link simbólico
if [ ! -L "$NGINX_ENABLED" ]; then
    ln -sf "$NGINX_CONFIG" "$NGINX_ENABLED"
    echo "✅ Link simbólico criado"
fi

# Testar configuração
echo "🧪 Testando configuração do Nginx..."
if nginx -t 2>&1 | grep -q "syntax is ok"; then
    echo "✅ Configuração válida!"
else
    echo "❌ Erro na configuração!"
    nginx -t
    echo ""
    echo "💡 Restaurando backup..."
    cp "$BACKUP_FILE" "$NGINX_CONFIG"
    exit 1
fi
echo ""

# Recarregar Nginx
echo "🔄 Recarregando Nginx..."
systemctl reload nginx
if [ $? -eq 0 ]; then
    echo "✅ Nginx recarregado com sucesso!"
else
    echo "❌ Erro ao recarregar Nginx!"
    systemctl restart nginx
    if [ $? -eq 0 ]; then
        echo "✅ Nginx reiniciado com sucesso!"
    else
        echo "❌ Erro ao reiniciar Nginx!"
        exit 1
    fi
fi
echo ""

# Testar acesso
echo "🧪 Testando acesso via Nginx..."
TEST_FILE="cliente_1_20250815151721.png"
echo "   Testando: http://127.0.0.1/api/logos/$TEST_FILE"
RESPONSE=$(curl -s -L -o /dev/null -w "%{http_code}" "http://127.0.0.1/api/logos/$TEST_FILE" 2>/dev/null)

if [ "$RESPONSE" = "200" ]; then
    echo "✅ Nginx está servindo logos corretamente (200 OK)"
elif [ "$RESPONSE" = "404" ]; then
    echo "⚠️  Nginx ainda retorna 404"
    echo ""
    echo "📋 Verificando se o backend está acessível..."
    BACKEND_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "http://127.0.0.1:5000/api/logos/$TEST_FILE" 2>/dev/null)
    if [ "$BACKEND_RESPONSE" = "200" ]; then
        echo "   ✅ Backend responde corretamente (200 OK)"
        echo ""
        echo "   ⚠️  Problema está no proxy do Nginx"
        echo "   Verificando logs do Nginx..."
        tail -20 /var/log/nginx/error.log | grep -i "api\|logo\|proxy" || echo "   Nenhum erro relacionado encontrado"
        echo ""
        echo "   💡 Verificando se há outros arquivos de configuração interferindo..."
        ls -la /etc/nginx/sites-enabled/ | grep -v singleone
        echo ""
        echo "   💡 Verificando configuração principal do Nginx..."
        if grep -q "include /etc/nginx/sites-enabled/\*" /etc/nginx/nginx.conf; then
            echo "   ✅ sites-enabled está incluído"
        else
            echo "   ⚠️  sites-enabled pode não estar incluído"
        fi
    else
        echo "   ❌ Backend também retorna $BACKEND_RESPONSE"
    fi
else
    echo "⚠️  Nginx respondeu com código: $RESPONSE"
fi
echo ""

echo "=========================================="
echo "✅ CORREÇÃO CONCLUÍDA"
echo "=========================================="
echo ""
echo "📋 Se ainda houver 404, execute:"
echo "   1. Ver logs: tail -f /var/log/nginx/error.log"
echo "   2. Verificar backend: curl -I http://127.0.0.1:5000/api/logos/$TEST_FILE"
echo "   3. Verificar configuração: cat $NGINX_CONFIG | grep -A 10 'location /api/'"
echo ""

