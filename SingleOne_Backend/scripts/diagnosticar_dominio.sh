#!/bin/bash

echo "=========================================="
echo "🔍 DIAGNÓSTICO: Domínio vs IP"
echo "=========================================="
echo ""

# 1. Verificar DNS
echo "📋 [1/5] Verificando resolução DNS..."
DNS_IP=$(dig +short demo.singleone.com.br | head -1)
SERVER_IP=$(hostname -I | awk '{print $1}')

echo "DNS aponta para: $DNS_IP"
echo "IP do servidor: $SERVER_IP"

if [ "$DNS_IP" = "$SERVER_IP" ]; then
    echo "✅ DNS aponta para o IP correto"
else
    echo "❌ DNS não aponta para o IP do servidor!"
fi
echo ""

# 2. Testar acesso pelo domínio
echo "📋 [2/5] Testando acesso HTTP pelo domínio..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://demo.singleone.com.br)
echo "HTTP Status: $HTTP_CODE"
if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "301" ] || [ "$HTTP_CODE" = "302" ]; then
    echo "✅ HTTP acessível pelo domínio"
else
    echo "❌ HTTP não acessível pelo domínio (código $HTTP_CODE)"
fi
echo ""

# 3. Testar HTTPS
echo "📋 [3/5] Testando acesso HTTPS pelo domínio..."
HTTPS_CODE=$(curl -s -o /dev/null -w "%{http_code}" https://demo.singleone.com.br 2>/dev/null || echo "FALHOU")
echo "HTTPS Status: $HTTPS_CODE"
if [ "$HTTPS_CODE" = "200" ]; then
    echo "✅ HTTPS acessível pelo domínio"
else
    echo "❌ HTTPS não acessível pelo domínio"
    
    # Verificar se há certificado SSL
    if [ -d /etc/letsencrypt/live/demo.singleone.com.br ]; then
        echo "⚠️  Certificado SSL existe, mas pode não estar configurado"
    else
        echo "⚠️  Certificado SSL não existe"
    fi
fi
echo ""

# 4. Verificar configuração do Nginx
echo "📋 [4/5] Verificando server_name no Nginx..."
if [ -f /etc/nginx/sites-available/singleone ]; then
    SERVER_NAME=$(grep "server_name" /etc/nginx/sites-available/singleone | head -1)
    echo "Configuração atual:"
    echo "  $SERVER_NAME"
    
    if echo "$SERVER_NAME" | grep -q "demo.singleone.com.br"; then
        echo "✅ server_name inclui demo.singleone.com.br"
    else
        echo "❌ server_name NÃO inclui demo.singleone.com.br"
    fi
else
    echo "❌ Arquivo de configuração não encontrado"
fi
echo ""

# 5. Verificar se há múltiplos blocos server
echo "📋 [5/5] Verificando blocos server no Nginx..."
NUM_SERVERS=$(grep -c "^server {" /etc/nginx/sites-enabled/* 2>/dev/null)
echo "Número de blocos server ativos: $NUM_SERVERS"

if [ "$NUM_SERVERS" -gt 1 ]; then
    echo "⚠️  Há múltiplos blocos server, pode haver conflito"
    echo ""
    echo "Blocos encontrados:"
    grep -H "server_name" /etc/nginx/sites-enabled/* 2>/dev/null
fi
echo ""

echo "=========================================="
echo "📊 RESUMO E CORREÇÃO"
echo "=========================================="

if [ "$HTTP_CODE" = "200" ] || [ "$HTTPS_CODE" = "200" ]; then
    echo "✅ Domínio acessível"
else
    echo "❌ Domínio não acessível - aplicando correção..."
    echo ""
    
    # Corrigir configuração do Nginx
    cat > /etc/nginx/sites-available/singleone << 'NGINXCONF'
server {
    listen 80;
    listen [::]:80;
    
    server_name demo.singleone.com.br 84.247.128.180 _;
    
    # Redirecionar HTTP para HTTPS se houver certificado
    if (-f /etc/letsencrypt/live/demo.singleone.com.br/fullchain.pem) {
        return 301 https://$server_name$request_uri;
    }
    
    root /var/www/singleone-frontend;
    index index.html;
    
    # Gzip
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css text/xml text/javascript application/x-javascript application/xml+rss application/json application/javascript;
    
    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    
    # API proxy
    location ^~ /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Connection "";
        proxy_buffering off;
    }
    
    # Cache para assets
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
    
    # SPA routing
    location / {
        try_files $uri $uri/ /index.html;
    }
    
    # Sem cache para index.html
    location = /index.html {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma "no-cache";
        add_header Expires "0";
    }
}

# HTTPS (se houver certificado)
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    
    server_name demo.singleone.com.br 84.247.128.180;
    
    ssl_certificate /etc/letsencrypt/live/demo.singleone.com.br/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/demo.singleone.com.br/privkey.pem;
    
    root /var/www/singleone-frontend;
    index index.html;
    
    # Gzip
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css text/xml text/javascript application/x-javascript application/xml+rss application/json application/javascript;
    
    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    
    # API proxy
    location ^~ /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Connection "";
        proxy_buffering off;
    }
    
    # Cache para assets
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
    
    # SPA routing
    location / {
        try_files $uri $uri/ /index.html;
    }
    
    # Sem cache para index.html
    location = /index.html {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma "no-cache";
        add_header Expires "0";
    }
}
NGINXCONF
    
    # Remover if condicional se não houver certificado
    if [ ! -f /etc/letsencrypt/live/demo.singleone.com.br/fullchain.pem ]; then
        sed -i '/if (-f \/etc\/letsencrypt/,/}/d' /etc/nginx/sites-available/singleone
        # Remover bloco HTTPS também
        sed -i '/# HTTPS (se houver certificado)/,$d' /etc/nginx/sites-available/singleone
    fi
    
    # Garantir link simbólico
    ln -sf /etc/nginx/sites-available/singleone /etc/nginx/sites-enabled/singleone
    
    # Testar e recarregar
    nginx -t && systemctl reload nginx
    
    echo "✅ Configuração corrigida e Nginx recarregado"
    echo ""
    echo "🧪 Teste novamente:"
    echo "   curl -I http://demo.singleone.com.br"
    echo "   curl -I https://demo.singleone.com.br"
fi
echo ""

