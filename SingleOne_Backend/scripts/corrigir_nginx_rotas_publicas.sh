#!/bin/bash

# Script para garantir que o Nginx está configurado corretamente
# para servir rotas públicas do Angular (SPA routing)

echo "=========================================="
echo "🔧 CORRIGINDO NGINX PARA ROTAS PÚBLICAS"
echo "=========================================="
echo ""

NGINX_CONFIG="/etc/nginx/sites-available/singleone"
NGINX_ENABLED="/etc/nginx/sites-enabled/singleone"

# Verificar se o arquivo existe
if [ ! -f "$NGINX_CONFIG" ]; then
    echo "❌ Arquivo de configuração não encontrado: $NGINX_CONFIG"
    echo "📝 Criando configuração..."
    
    # Criar diretório se não existir
    mkdir -p /etc/nginx/sites-available
    mkdir -p /etc/nginx/sites-enabled
fi

# Backup do arquivo atual
if [ -f "$NGINX_CONFIG" ]; then
    echo "📦 Fazendo backup do arquivo atual..."
    cp "$NGINX_CONFIG" "${NGINX_CONFIG}.backup.$(date +%Y%m%d_%H%M%S)"
fi

# Criar/atualizar configuração
echo "📝 Criando configuração do Nginx..."
cat > "$NGINX_CONFIG" << 'EOF'
server {
    listen 80 default_server;
    listen [::]:80 default_server;

    server_name _;

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

    # ✅ CORREÇÃO: Rotas públicas do Angular (SPA routing)
    # Todas as rotas devem cair no index.html para o Angular rotear
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Cache para assets estáticos
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
EOF

# Garantir que está habilitado
if [ ! -L "$NGINX_ENABLED" ]; then
    echo "🔗 Criando link simbólico..."
    ln -sf "$NGINX_CONFIG" "$NGINX_ENABLED"
fi

# Testar configuração
echo ""
echo "🧪 Testando configuração do Nginx..."
if nginx -t; then
    echo "✅ Configuração válida!"
    echo ""
    echo "🔄 Recarregando Nginx..."
    systemctl reload nginx
    if [ $? -eq 0 ]; then
        echo "✅ Nginx recarregado com sucesso!"
    else
        echo "❌ Erro ao recarregar Nginx. Verifique os logs: journalctl -u nginx -n 50"
        exit 1
    fi
else
    echo "❌ Erro na configuração do Nginx!"
    exit 1
fi

echo ""
echo "=========================================="
echo "✅ CONFIGURAÇÃO CONCLUÍDA!"
echo "=========================================="
echo ""
echo "📋 Rotas públicas que devem funcionar:"
echo "   - /termos/:hash/:isByod"
echo "   - /verificar-termo/:hash"
echo "   - /patrimonio"
echo "   - /portaria"
echo "   - /login"
echo ""
echo "🧪 Teste acessando:"
echo "   http://84.247.128.180/verificar-termo/SEU_HASH_AQUI"
echo ""

