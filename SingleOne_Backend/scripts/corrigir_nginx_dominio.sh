#!/bin/bash

# Script para corrigir Nginx para aceitar domínio e IP

echo "=========================================="
echo "🔧 CORRIGINDO NGINX PARA DOMÍNIO E IP"
echo "=========================================="
echo ""

NGINX_CONFIG="/etc/nginx/sites-available/singleone"
NGINX_ENABLED="/etc/nginx/sites-enabled/singleone"
DOMAIN="demo.singleone.com.br"
SERVER_IP=$(hostname -I | awk '{print $1}')

echo "📋 Informações:"
echo "   Domínio: $DOMAIN"
echo "   IP do servidor: $SERVER_IP"
echo ""

# Criar/atualizar configuração do Nginx
echo "📝 Criando/atualizando configuração do Nginx..."

# Verificar se há múltiplos blocos server e remover configurações antigas
if [ -f "$NGINX_CONFIG" ]; then
    SERVER_COUNT=$(grep -c "^server {" "$NGINX_CONFIG" 2>/dev/null || echo "0")
    if [ "$SERVER_COUNT" -gt 1 ]; then
        echo "   ⚠️  Múltiplos blocos server encontrados, limpando..."
    fi
fi

cat > "$NGINX_CONFIG" << NGINX_EOF
server {
    listen 80 default_server;
    listen [::]:80 default_server;

    # Aceitar domínio, IP e qualquer host (para garantir acesso)
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
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header Connection "";
        proxy_buffering off;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
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
NGINX_EOF

echo "✅ Configuração criada/atualizada"
echo ""

# Garantir link simbólico
echo "🔗 Garantindo link simbólico..."
if [ ! -L "$NGINX_ENABLED" ]; then
    ln -sf "$NGINX_CONFIG" "$NGINX_ENABLED"
    echo "✅ Link simbólico criado"
else
    echo "✅ Link simbólico já existe"
fi
echo ""

# Testar configuração
echo "🧪 Testando configuração do Nginx..."
if nginx -t; then
    echo "✅ Configuração válida!"
else
    echo "❌ Erro na configuração!"
    exit 1
fi
echo ""

# Recarregar Nginx
echo "🔄 Recarregando Nginx..."
systemctl reload nginx
if [ $? -eq 0 ]; then
    echo "✅ Nginx recarregado com sucesso!"
else
    echo "❌ Erro ao recarregar Nginx, tentando reiniciar..."
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
echo "🧪 Testando acesso..."
sleep 2

echo "   Teste 1: Acesso por IP..."
HTTP_CODE1=$(curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1/ 2>/dev/null)
if [ "$HTTP_CODE1" = "200" ]; then
    echo "   ✅ HTTP $HTTP_CODE1 - Acesso por IP OK"
else
    echo "   ⚠️  HTTP $HTTP_CODE1 - Acesso por IP"
fi

echo "   Teste 2: Acesso com domínio no header..."
HTTP_CODE2=$(curl -s -o /dev/null -w "%{http_code}" -H "Host: $DOMAIN" http://127.0.0.1/ 2>/dev/null)
if [ "$HTTP_CODE2" = "200" ]; then
    echo "   ✅ HTTP $HTTP_CODE2 - Acesso com domínio OK"
else
    echo "   ⚠️  HTTP $HTTP_CODE2 - Acesso com domínio"
fi

echo "   Teste 3: API através do proxy..."
API_CODE=$(curl -s -o /dev/null -w "%{http_code}" -H "Host: $DOMAIN" http://127.0.0.1/api/ 2>/dev/null)
if [ "$API_CODE" = "200" ] || [ "$API_CODE" = "404" ]; then
    echo "   ✅ HTTP $API_CODE - Proxy da API OK"
else
    echo "   ⚠️  HTTP $API_CODE - Proxy da API"
fi
echo ""

# Verificar status
echo "📋 Status final:"
echo "   Nginx: $(systemctl is-active nginx)"
echo "   API:   $(systemctl is-active singleone-api)"
echo ""

echo "=========================================="
echo "✅ CORREÇÃO CONCLUÍDA"
echo "=========================================="
echo ""
echo "🌐 Teste acessando:"
echo "   - Por IP: http://$SERVER_IP"
echo "   - Por domínio: http://$DOMAIN"
echo ""
echo "📋 Se ainda não funcionar:"
echo "   1. Verifique DNS: nslookup $DOMAIN"
echo "   2. Verifique logs: sudo tail -f /var/log/nginx/error.log"
echo "   3. Execute diagnóstico: sudo bash /opt/SingleOne/SingleOne_Backend/scripts/diagnosticar_dns_e_nginx.sh"
echo ""

