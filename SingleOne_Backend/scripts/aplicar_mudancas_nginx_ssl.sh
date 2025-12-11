#!/bin/bash

# Script para aplicar todas as mudanças de Nginx e SSL agora

echo "=========================================="
echo "🔧 APLICANDO MUDANÇAS NGINX E SSL"
echo "=========================================="
echo ""

NGINX_CONFIG="/etc/nginx/sites-available/singleone"
NGINX_ENABLED="/etc/nginx/sites-enabled/singleone"
DOMAIN="demo.singleone.com.br"
CERT_PATH="/etc/letsencrypt/live/$DOMAIN"

# 1. Verificar se há certificado SSL
echo "📋 [1/4] Verificando certificado SSL..."
HAS_SSL=false

if [ -f "$CERT_PATH/fullchain.pem" ] && [ -f "$CERT_PATH/privkey.pem" ]; then
    HAS_SSL=true
    echo "   ✅ Certificado SSL encontrado"
    
    # Verificar expiração
    if command -v openssl > /dev/null 2>&1; then
        EXPIRY_DATE=$(openssl x509 -enddate -noout -in "$CERT_PATH/fullchain.pem" 2>/dev/null | cut -d= -f2)
        EXPIRY_EPOCH=$(date -d "$EXPIRY_DATE" +%s 2>/dev/null)
        CURRENT_EPOCH=$(date +%s)
        DAYS_LEFT=$(( ($EXPIRY_EPOCH - $CURRENT_EPOCH) / 86400 ))
        echo "   📅 Certificado válido por mais $DAYS_LEFT dias"
    fi
else
    echo "   ⚠️  Certificado SSL não encontrado, usando apenas HTTP"
fi
echo ""

# 2. Criar/atualizar configuração do Nginx
echo "📋 [2/4] Criando/atualizando configuração do Nginx..."

# Criar backup
if [ -f "$NGINX_CONFIG" ]; then
    cp "$NGINX_CONFIG" "$NGINX_CONFIG.backup.$(date +%Y%m%d_%H%M%S)"
    echo "   💾 Backup criado"
fi

if [ "$HAS_SSL" = true ]; then
    echo "   🔒 Configurando com HTTPS..."
    cat > "$NGINX_CONFIG" << NGINX_HTTPS_EOF
# Redirect HTTP to HTTPS
server {
    listen 80 default_server;
    listen [::]:80 default_server;
    server_name demo.singleone.com.br 84.247.128.180 _;
    return 301 https://\$server_name\$request_uri;
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

    # Proxy para API
    location /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header Connection "";
        proxy_buffering off;
    }

    # Angular routing - TODAS as rotas devem retornar index.html
    location / {
        try_files \$uri \$uri/ /index.html;
    }

    # Cache para assets estáticos
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
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
    echo "   🌐 Configurando apenas HTTP..."
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

    # Proxy para API
    location /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Connection "";
        proxy_buffering off;
    }

    # Angular routing - TODAS as rotas devem retornar index.html
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Cache para assets estáticos
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
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

echo "   ✅ Configuração criada"
echo ""

# 3. Garantir link simbólico
echo "📋 [3/4] Garantindo link simbólico..."
if [ ! -L "$NGINX_ENABLED" ]; then
    ln -sf "$NGINX_CONFIG" "$NGINX_ENABLED"
    echo "   ✅ Link simbólico criado"
else
    echo "   ✅ Link simbólico já existe"
fi
echo ""

# 4. Testar e aplicar configuração
echo "📋 [4/4] Testando e aplicando configuração..."
if nginx -t; then
    echo "   ✅ Configuração válida"
    
    # Recarregar Nginx
    systemctl reload nginx
    if [ $? -eq 0 ]; then
        echo "   ✅ Nginx recarregado com sucesso"
    else
        echo "   ⚠️  Erro ao recarregar, tentando reiniciar..."
        systemctl restart nginx
        if [ $? -eq 0 ]; then
            echo "   ✅ Nginx reiniciado com sucesso"
        else
            echo "   ❌ Erro ao reiniciar Nginx!"
            exit 1
        fi
    fi
else
    echo "   ❌ Erro na configuração!"
    echo "   Restaurando backup..."
    if ls "$NGINX_CONFIG.backup."* > /dev/null 2>&1; then
        mv "$NGINX_CONFIG.backup."* "$NGINX_CONFIG" 2>/dev/null
        echo "   ✅ Backup restaurado"
    fi
    exit 1
fi
echo ""

# 5. Verificar status
echo "📋 Verificando status..."
sleep 2

if [ "$HAS_SSL" = true ]; then
    # Testar HTTPS
    HTTPS_CODE=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 5 https://$DOMAIN/ 2>/dev/null)
    if [ "$HTTPS_CODE" = "200" ]; then
        echo "   ✅ HTTPS funcionando (HTTP $HTTPS_CODE)"
    else
        echo "   ⚠️  HTTPS retornou HTTP $HTTPS_CODE"
    fi
    
    # Verificar porta 443
    if ss -tunlp | grep -q ":443"; then
        echo "   ✅ Porta 443 está em uso"
    else
        echo "   ⚠️  Porta 443 não está em uso"
    fi
fi

# Testar HTTP
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 5 http://$DOMAIN/ 2>/dev/null)
if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "301" ]; then
    echo "   ✅ HTTP funcionando (HTTP $HTTP_CODE)"
else
    echo "   ⚠️  HTTP retornou HTTP $HTTP_CODE"
fi
echo ""

# Resumo
echo "=========================================="
echo "✅ MUDANÇAS APLICADAS COM SUCESSO"
echo "=========================================="
echo ""
echo "📊 Configuração:"
if [ "$HAS_SSL" = true ]; then
    echo "   ✅ HTTPS configurado e ativo"
    echo "   🌐 Acesse: https://$DOMAIN"
else
    echo "   ⚠️  Apenas HTTP configurado"
    echo "   🌐 Acesse: http://$DOMAIN"
    echo "   💡 Para configurar HTTPS: sudo bash /opt/SingleOne/SingleOne_Backend/scripts/configurar_ssl.sh"
fi
echo ""
echo "📋 Status dos serviços:"
echo "   Nginx: $(systemctl is-active nginx)"
echo ""
echo "💾 Backup salvo em: $NGINX_CONFIG.backup.*"
echo ""

