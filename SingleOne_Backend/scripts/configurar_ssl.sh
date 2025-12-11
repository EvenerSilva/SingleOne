#!/bin/bash

# Script para configurar SSL/HTTPS com Let's Encrypt

echo "=========================================="
echo "🔒 CONFIGURANDO SSL/HTTPS"
echo "=========================================="
echo ""

DOMAIN="demo.singleone.com.br"
NGINX_CONFIG="/etc/nginx/sites-available/singleone"
NGINX_ENABLED="/etc/nginx/sites-enabled/singleone"
CERT_PATH="/etc/letsencrypt/live/$DOMAIN"

# 1. Verificar/Instalar Certbot
echo "📋 [1/6] Verificando Certbot..."
if ! command -v certbot > /dev/null 2>&1; then
    echo "   📦 Instalando Certbot..."
    apt update
    apt install -y certbot python3-certbot-nginx
    if [ $? -eq 0 ]; then
        echo "   ✅ Certbot instalado"
    else
        echo "   ❌ Erro ao instalar Certbot!"
        exit 1
    fi
else
    echo "   ✅ Certbot já está instalado"
fi
echo ""

# 2. Verificar se há certificado existente
echo "📋 [2/6] Verificando certificado existente..."
if [ -d "$CERT_PATH" ] && [ -f "$CERT_PATH/fullchain.pem" ]; then
    echo "   ✅ Certificado já existe"
    
    # Verificar expiração
    if command -v openssl > /dev/null 2>&1; then
        EXPIRY_DATE=$(openssl x509 -enddate -noout -in "$CERT_PATH/fullchain.pem" 2>/dev/null | cut -d= -f2)
        EXPIRY_EPOCH=$(date -d "$EXPIRY_DATE" +%s 2>/dev/null)
        CURRENT_EPOCH=$(date +%s)
        DAYS_LEFT=$(( ($EXPIRY_EPOCH - $CURRENT_EPOCH) / 86400 ))
        
        if [ "$DAYS_LEFT" -lt 30 ]; then
            echo "   ⚠️  Certificado expira em $DAYS_LEFT dias, renovando..."
            certbot renew --nginx --non-interactive
        else
            echo "   ✅ Certificado válido por mais $DAYS_LEFT dias"
        fi
    fi
else
    echo "   ⚠️  Certificado não encontrado, será gerado"
fi
echo ""

# 3. Garantir que Nginx está configurado para HTTP primeiro (se não tiver HTTPS)
echo "📋 [3/6] Garantindo configuração HTTP básica..."
if [ ! -f "$NGINX_CONFIG" ] || (! grep -q "listen 80" "$NGINX_CONFIG" && ! grep -q "listen 443" "$NGINX_CONFIG"); then
    echo "   📝 Criando configuração HTTP básica..."
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
    
    # Garantir link simbólico
    if [ ! -L "$NGINX_ENABLED" ]; then
        ln -sf "$NGINX_CONFIG" "$NGINX_ENABLED"
    fi
    
    # Testar e recarregar
    if nginx -t; then
        systemctl reload nginx
        echo "   ✅ Configuração HTTP criada"
    else
        echo "   ❌ Erro na configuração!"
        exit 1
    fi
else
    echo "   ✅ Configuração já existe"
fi
echo ""

# 4. Gerar/Renovar certificado
echo "📋 [4/6] Gerando/Renovando certificado SSL..."
if [ ! -d "$CERT_PATH" ] || [ ! -f "$CERT_PATH/fullchain.pem" ]; then
    echo "   🔐 Gerando novo certificado..."
    certbot --nginx -d $DOMAIN --non-interactive --agree-tos --email admin@singleone.com.br --redirect
    if [ $? -eq 0 ]; then
        echo "   ✅ Certificado gerado com sucesso"
    else
        echo "   ❌ Erro ao gerar certificado!"
        echo "   Tente manualmente: sudo certbot --nginx -d $DOMAIN"
        exit 1
    fi
else
    echo "   🔄 Renovando certificado existente..."
    certbot renew --nginx --non-interactive
    if [ $? -eq 0 ]; then
        echo "   ✅ Certificado renovado"
    else
        echo "   ⚠️  Certificado não precisava ser renovado ou houve erro"
    fi
fi
echo ""

# 5. Verificar e garantir configuração HTTPS no Nginx
echo "📋 [5/6] Verificando e garantindo configuração HTTPS..."
if [ -f "$CERT_PATH/fullchain.pem" ] && [ -f "$CERT_PATH/privkey.pem" ]; then
    if ! grep -q "listen 443" "$NGINX_CONFIG"; then
        echo "   📝 Certificado existe mas HTTPS não está configurado, configurando agora..."
        
        # Criar backup
        cp "$NGINX_CONFIG" "$NGINX_CONFIG.backup.$(date +%Y%m%d_%H%M%S)"
        
        # Criar configuração completa com HTTPS
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
        
        # Testar configuração
        if nginx -t; then
            systemctl reload nginx
            echo "   ✅ HTTPS configurado no Nginx"
        else
            echo "   ❌ Erro na configuração, restaurando backup..."
            mv "$NGINX_CONFIG.backup."* "$NGINX_CONFIG" 2>/dev/null
            exit 1
        fi
    else
        echo "   ✅ HTTPS já está configurado no Nginx"
    fi
    
    # Verificar se há redirecionamento HTTP -> HTTPS
    if grep -q "return 301 https" "$NGINX_CONFIG" || grep -q "rewrite.*https" "$NGINX_CONFIG"; then
        echo "   ✅ Redirecionamento HTTP -> HTTPS configurado"
    else
        echo "   ⚠️  Redirecionamento HTTP -> HTTPS não encontrado"
    fi
else
    echo "   ⚠️  Certificado não encontrado, HTTPS não pode ser configurado"
fi
echo ""

# 6. Configurar renovação automática
echo "📋 [6/6] Configurando renovação automática..."
if systemctl list-unit-files | grep -q certbot.timer; then
    systemctl enable certbot.timer
    systemctl start certbot.timer
    echo "   ✅ Timer do Certbot habilitado"
    
    # Verificar próximo agendamento
    NEXT_RUN=$(systemctl list-timers certbot.timer --no-pager | grep certbot | awk '{print $1, $2, $3}')
    if [ ! -z "$NEXT_RUN" ]; then
        echo "   📅 Próxima renovação: $NEXT_RUN"
    fi
else
    echo "   ⚠️  Timer do Certbot não encontrado, configurando crontab..."
    
    # Adicionar ao crontab se não existir
    if ! crontab -l 2>/dev/null | grep -q "certbot renew"; then
        (crontab -l 2>/dev/null; echo "0 3 * * * certbot renew --quiet --nginx") | crontab -
        echo "   ✅ Tarefa adicionada ao crontab (renovação diária às 3h)"
    else
        echo "   ✅ Tarefa já existe no crontab"
    fi
fi
echo ""

# Testar HTTPS
echo "🧪 Testando HTTPS..."
sleep 2
HTTPS_CODE=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 5 https://$DOMAIN/ 2>/dev/null)
if [ "$HTTPS_CODE" = "200" ]; then
    echo "   ✅ HTTPS funcionando (HTTP $HTTPS_CODE)"
else
    echo "   ⚠️  HTTPS retornou HTTP $HTTPS_CODE"
fi
echo ""

# Resumo
echo "=========================================="
echo "✅ CONFIGURAÇÃO SSL CONCLUÍDA"
echo "=========================================="
echo ""
echo "🌐 Acesse:"
echo "   - HTTP:  http://$DOMAIN"
echo "   - HTTPS: https://$DOMAIN"
echo ""
echo "📋 Comandos úteis:"
echo "   - Ver certificados: sudo certbot certificates"
echo "   - Renovar manualmente: sudo certbot renew"
echo "   - Ver status do timer: sudo systemctl status certbot.timer"
echo "   - Ver logs: sudo tail -f /var/log/letsencrypt/letsencrypt.log"
echo ""

