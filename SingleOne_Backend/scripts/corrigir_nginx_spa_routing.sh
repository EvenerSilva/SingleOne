#!/bin/bash

echo "=========================================="
echo "🔧 CORRIGINDO NGINX PARA SPA ROUTING"
echo "=========================================="
echo ""

# 1. Verificar configuração atual
echo "📋 [1/4] Verificando configuração atual do Nginx..."
if [ -f /etc/nginx/sites-available/singleone ]; then
    echo "✅ Arquivo encontrado: /etc/nginx/sites-available/singleone"
    echo ""
    echo "Configuração atual do 'location /':"
    grep -A 3 "location / {" /etc/nginx/sites-available/singleone
    echo ""
else
    echo "❌ Arquivo não encontrado!"
    exit 1
fi

# 2. Verificar se o diretório do frontend existe
echo "📋 [2/4] Verificando diretório do frontend..."
if [ -d /var/www/singleone-frontend ]; then
    echo "✅ Diretório encontrado: /var/www/singleone-frontend"
    ls -lh /var/www/singleone-frontend/index.html 2>/dev/null || echo "⚠️ index.html não encontrado!"
else
    echo "❌ Diretório não encontrado: /var/www/singleone-frontend"
    echo "   Usando /opt/SingleOne/SingleOne_Frontend/dist/SingleOne"
fi

# 3. Criar configuração corrigida
echo ""
echo "📋 [3/4] Criando configuração corrigida do Nginx..."

# Determinar o diretório correto do frontend
if [ -d /var/www/singleone-frontend ] && [ -f /var/www/singleone-frontend/index.html ]; then
    FRONTEND_DIR="/var/www/singleone-frontend"
else
    FRONTEND_DIR="/opt/SingleOne/SingleOne_Frontend/dist/SingleOne"
fi

echo "Usando diretório: $FRONTEND_DIR"

cat > /etc/nginx/sites-available/singleone << 'EOF'
server {
    listen 80 default_server;
    listen [::]:80 default_server;
    
    server_name demo.singleone.com.br 84.247.128.180 _;
    
    root FRONTEND_DIR_PLACEHOLDER;
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
    
    # ✅ PRIORIDADE MÁXIMA: Proxy para API (antes de qualquer outra regra)
    location ^~ /api/ {
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
    
    # Cache para assets estáticos
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
        try_files $uri =404;
    }
    
    # ✅ CORREÇÃO CRÍTICA: SPA Routing - TODAS as rotas devem retornar index.html
    # Isso permite que o Angular Router funcione corretamente
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
EOF

# Substituir o placeholder pelo diretório correto
sed -i "s|FRONTEND_DIR_PLACEHOLDER|$FRONTEND_DIR|g" /etc/nginx/sites-available/singleone

echo "✅ Configuração criada"
echo ""

# 4. Testar e aplicar
echo "📋 [4/4] Testando e aplicando configuração..."
nginx -t

if [ $? -eq 0 ]; then
    echo "✅ Configuração válida!"
    echo ""
    echo "🔄 Recarregando Nginx..."
    systemctl reload nginx
    echo "✅ Nginx recarregado!"
    echo ""
    
    echo "=========================================="
    echo "✅ CORREÇÃO APLICADA COM SUCESSO"
    echo "=========================================="
    echo ""
    echo "🧪 Teste agora:"
    echo "   1. Abra: https://demo.singleone.com.br/login"
    echo "   2. Abra o console (F12) e veja se carrega sem erro 404"
    echo "   3. A logo deve aparecer automaticamente"
    echo ""
    echo "📋 Se ainda houver problema, verifique:"
    echo "   curl -I https://demo.singleone.com.br/login"
    echo "   (Deve retornar 200 OK, não 404)"
    echo ""
else
    echo "❌ Erro na configuração do Nginx!"
    echo "   Não foi possível aplicar as mudanças."
    exit 1
fi

