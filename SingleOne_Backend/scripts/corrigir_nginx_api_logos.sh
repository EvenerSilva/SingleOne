#!/bin/bash

# Script para garantir que o Nginx está configurado corretamente para /api/logos

echo "=========================================="
echo "🔧 CORRIGINDO NGINX PARA /api/logos"
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
    echo "✅ Certificado SSL encontrado, configurando HTTPS..."
else
    echo "ℹ️  Sem certificado SSL, configurando apenas HTTP..."
fi
echo ""

# Criar backup
BACKUP_FILE="${NGINX_CONFIG}.backup.$(date +%Y%m%d_%H%M%S)"
cp "$NGINX_CONFIG" "$BACKUP_FILE"
echo "✅ Backup criado: $BACKUP_FILE"
echo ""

# Verificar se a configuração atual tem problema
if grep -A 10 "location /api/" "$NGINX_CONFIG" | grep -q "try_files"; then
    echo "⚠️  Problema detectado: /api/ está usando try_files (incorreto)"
    echo "   /api/ deve usar proxy_pass, não try_files"
    NEEDS_FIX=true
else
    NEEDS_FIX=false
fi

# Verificar ordem das location blocks
API_LINE=$(grep -n "^[[:space:]]*location /api/" "$NGINX_CONFIG" | head -1 | cut -d: -f1)
CACHE_LINE=$(grep -n "^[[:space:]]*location ~\* \\.(jpg\|jpeg\|png" "$NGINX_CONFIG" | head -1 | cut -d: -f1)

if [ -n "$API_LINE" ] && [ -n "$CACHE_LINE" ] && [ "$CACHE_LINE" -lt "$API_LINE" ]; then
    echo "⚠️  Ordem incorreta: cache de imagens está antes de /api/"
    NEEDS_FIX=true
fi

if [ "$NEEDS_FIX" = false ]; then
    echo "✅ Configuração parece estar correta"
    echo "   Verificando se há outros problemas..."
    echo ""
fi

# Verificar se precisa recriar a configuração
if [ "$NEEDS_FIX" = true ] || ! grep -q "proxy_pass http://127.0.0.1:5000/api/" "$NGINX_CONFIG"; then
    echo "📝 Recriando configuração do Nginx..."
    
    if [ "$HAS_SSL" = true ]; then
        # Configuração com HTTPS
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

    # ✅ PRIORIDADE 1: Proxy para API (DEVE vir ANTES de qualquer outra regra de arquivos)
    location /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Connection "";
        proxy_buffering off;
        
        # Timeouts para evitar problemas com uploads grandes
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }

    # ✅ PRIORIDADE 2: Cache para assets estáticos do frontend (NÃO /api/)
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        # Excluir /api/ explicitamente
        if ($request_uri ~ ^/api/) {
            return 404;
        }
        expires 1y;
        add_header Cache-Control "public, immutable";
        try_files $uri =404;
    }

    # ✅ PRIORIDADE 3: Angular routing - TODAS as rotas devem retornar index.html
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
        # Configuração apenas HTTP
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

    # ✅ PRIORIDADE 1: Proxy para API (DEVE vir ANTES de qualquer outra regra de arquivos)
    location /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Connection "";
        proxy_buffering off;
        
        # Timeouts para evitar problemas com uploads grandes
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }

    # ✅ PRIORIDADE 2: Cache para assets estáticos do frontend (NÃO /api/)
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        # Excluir /api/ explicitamente
        if ($request_uri ~ ^/api/) {
            return 404;
        }
        expires 1y;
        add_header Cache-Control "public, immutable";
        try_files $uri =404;
    }

    # ✅ PRIORIDADE 3: Angular routing - TODAS as rotas devem retornar index.html
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
    
    echo "✅ Configuração recriada"
else
    echo "✅ Configuração já está correta, apenas verificando..."
fi

# Garantir link simbólico
if [ ! -L "$NGINX_ENABLED" ]; then
    echo "🔗 Criando link simbólico..."
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
RESPONSE=$(curl -s -L -o /dev/null -w "%{http_code}" "http://127.0.0.1/api/logos/$TEST_FILE" 2>/dev/null)

if [ "$RESPONSE" = "200" ]; then
    echo "✅ Nginx está servindo logos corretamente (200 OK)"
elif [ "$RESPONSE" = "404" ]; then
    echo "⚠️  Nginx ainda retorna 404"
    echo "   Verificando se o backend está acessível..."
    BACKEND_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "http://127.0.0.1:5000/api/logos/$TEST_FILE" 2>/dev/null)
    if [ "$BACKEND_RESPONSE" = "200" ]; then
        echo "   ✅ Backend responde corretamente"
        echo "   ⚠️  Problema pode ser no proxy do Nginx"
        echo "   Verifique logs: tail -f /var/log/nginx/error.log"
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

