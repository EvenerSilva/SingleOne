#!/bin/bash

# ==========================================
# 🚀 OTIMIZAÇÃO DE PERFORMANCE DO NGINX
# ==========================================
# Adiciona: Gzip, Cache, Timeouts, Buffers
# ==========================================

echo "=========================================="
echo "🚀 OTIMIZANDO NGINX PARA PERFORMANCE"
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

# Criar configuração OTIMIZADA
echo "📝 Criando configuração otimizada..."
echo ""

if [ "$HAS_SSL" = true ]; then
    cat > "$NGINX_CONFIG" << 'EOF'
# ==========================================
# NGINX OTIMIZADO - SingleOne
# ==========================================

# Redirecionar HTTP para HTTPS
server {
    listen 80 default_server;
    listen [::]:80 default_server;
    server_name demo.singleone.com.br 84.247.128.180 _;
    return 301 https://$server_name$request_uri;
}

# Servidor HTTPS Principal
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
    
    # SSL Performance
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;
    ssl_stapling on;
    ssl_stapling_verify on;

    root /opt/SingleOne/SingleOne_Frontend/dist/SingleOne;
    index index.html;
    
    charset utf-8;

    # ==========================================
    # 🚀 GZIP COMPRESSION (Reduz 70-90% do tamanho)
    # ==========================================
    gzip on;
    gzip_vary on;
    gzip_min_length 1000;
    gzip_comp_level 6;
    gzip_proxied any;
    gzip_types
        text/plain
        text/css
        text/xml
        text/javascript
        application/json
        application/javascript
        application/xml+rss
        application/x-javascript
        image/svg+xml
        font/truetype
        font/opentype
        application/vnd.ms-fontobject;
    gzip_disable "msie6";

    # ==========================================
    # 🔄 PROXY PARA API (OTIMIZADO)
    # ==========================================
    location /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        
        # Headers
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Connection "";
        
        # Timeouts (evitar requisições travadas)
        proxy_connect_timeout 30s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
        
        # Buffers (melhor performance)
        proxy_buffering on;
        proxy_buffer_size 4k;
        proxy_buffers 8 4k;
        proxy_busy_buffers_size 8k;
        
        # Cache de resposta da API (opcional - apenas para GET)
        proxy_cache_bypass $http_upgrade;
        
        # Não fazer cache de respostas da API por padrão
        add_header Cache-Control "no-cache, no-store, must-revalidate";
    }

    # ==========================================
    # 📦 CACHE DE ASSETS ESTÁTICOS (1 ano)
    # ==========================================
    location ~* ^(?!/api/).*\.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot|webp)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
        add_header Vary "Accept-Encoding";
        access_log off;
        
        # Compressão adicional para assets
        gzip_static on;
    }

    # ==========================================
    # 🚫 SEM CACHE PARA INDEX.HTML
    # ==========================================
    location = /index.html {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma "no-cache";
        add_header Expires "0";
        add_header Content-Type "text/html; charset=utf-8";
    }

    # ==========================================
    # 📄 ANGULAR ROUTING (SPA)
    # ==========================================
    location / {
        try_files $uri $uri/ /index.html;
        add_header Content-Type "text/html; charset=utf-8";
    }

    # ==========================================
    # 🔒 SECURITY HEADERS
    # ==========================================
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;

    # ==========================================
    # 📊 LOGGING (Opcional - desabilitar para melhor performance)
    # ==========================================
    access_log /var/log/nginx/singleone-access.log;
    error_log /var/log/nginx/singleone-error.log warn;
}
EOF
else
    cat > "$NGINX_CONFIG" << 'EOF'
# ==========================================
# NGINX OTIMIZADO - SingleOne (HTTP)
# ==========================================

server {
    listen 80 default_server;
    listen [::]:80 default_server;
    server_name demo.singleone.com.br 84.247.128.180 _;

    root /opt/SingleOne/SingleOne_Frontend/dist/SingleOne;
    index index.html;
    
    charset utf-8;

    # ==========================================
    # 🚀 GZIP COMPRESSION
    # ==========================================
    gzip on;
    gzip_vary on;
    gzip_min_length 1000;
    gzip_comp_level 6;
    gzip_proxied any;
    gzip_types
        text/plain
        text/css
        text/xml
        text/javascript
        application/json
        application/javascript
        application/xml+rss
        application/x-javascript
        image/svg+xml
        font/truetype
        font/opentype
        application/vnd.ms-fontobject;
    gzip_disable "msie6";

    # ==========================================
    # 🔄 PROXY PARA API (OTIMIZADO)
    # ==========================================
    location /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Connection "";
        
        proxy_connect_timeout 30s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
        
        proxy_buffering on;
        proxy_buffer_size 4k;
        proxy_buffers 8 4k;
        proxy_busy_buffers_size 8k;
        
        add_header Cache-Control "no-cache, no-store, must-revalidate";
    }

    # ==========================================
    # 📦 CACHE DE ASSETS ESTÁTICOS
    # ==========================================
    location ~* ^(?!/api/).*\.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot|webp)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
        add_header Vary "Accept-Encoding";
        access_log off;
    }

    # ==========================================
    # 🚫 SEM CACHE PARA INDEX.HTML
    # ==========================================
    location = /index.html {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma "no-cache";
        add_header Expires "0";
        add_header Content-Type "text/html; charset=utf-8";
    }

    # ==========================================
    # 📄 ANGULAR ROUTING
    # ==========================================
    location / {
        try_files $uri $uri/ /index.html;
        add_header Content-Type "text/html; charset=utf-8";
    }

    # ==========================================
    # 🔒 SECURITY HEADERS
    # ==========================================
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;

    access_log /var/log/nginx/singleone-access.log;
    error_log /var/log/nginx/singleone-error.log warn;
}
EOF
fi

# Garantir link
ln -sf "$NGINX_CONFIG" "$NGINX_ENABLED"
echo "✅ Configuração otimizada criada"
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

# Reiniciar Nginx
echo "🔄 Reiniciando Nginx..."
systemctl reload nginx
sleep 2

if systemctl is-active --quiet nginx; then
    echo "✅ Nginx reiniciado com sucesso"
else
    echo "❌ Erro ao reiniciar Nginx!"
    systemctl status nginx --no-pager | head -10
    exit 1
fi
echo ""

# Testar compressão
echo "🧪 Testando compressão Gzip..."
TEST_URL="https://demo.singleone.com.br"
if [ "$HAS_SSL" != true ]; then
    TEST_URL="http://demo.singleone.com.br"
fi

GZIP_TEST=$(curl -s -H "Accept-Encoding: gzip" -I "$TEST_URL/main.js" 2>/dev/null | grep -i "content-encoding" || echo "")
if [ -n "$GZIP_TEST" ]; then
    echo "✅ Gzip está funcionando!"
else
    echo "⚠️  Gzip pode não estar ativo (normal se o arquivo não existir)"
fi
echo ""

echo "=========================================="
echo "✅ OTIMIZAÇÃO CONCLUÍDA!"
echo "=========================================="
echo ""
echo "📊 Melhorias aplicadas:"
echo "   ✅ Gzip compression (reduz 70-90% do tamanho)"
echo "   ✅ Cache de assets estáticos (1 ano)"
echo "   ✅ Timeouts otimizados no proxy"
echo "   ✅ Buffers ajustados para melhor performance"
echo "   ✅ SSL session cache"
echo "   ✅ Security headers"
echo ""
echo "🚀 Teste agora: https://demo.singleone.com.br"
echo "   O sistema deve carregar MUITO mais rápido!"
echo ""

