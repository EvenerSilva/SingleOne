#!/bin/bash

# Script para verificar e corrigir configuração do Nginx para demo.singleone.com.br

echo "=========================================="
echo "🔧 VERIFICANDO E CORRIGINDO NGINX"
echo "=========================================="
echo ""

NGINX_CONFIG="/etc/nginx/sites-available/singleone"
NGINX_ENABLED="/etc/nginx/sites-enabled/singleone"

# 1. Verificar configuração atual
echo "📋 Verificando configuração atual do Nginx..."
if [ -f "$NGINX_CONFIG" ]; then
    echo "✅ Arquivo de configuração encontrado: $NGINX_CONFIG"
    echo ""
    echo "📄 Conteúdo atual:"
    cat "$NGINX_CONFIG"
    echo ""
else
    echo "⚠️  Arquivo de configuração não encontrado!"
fi

# 2. Verificar se está habilitado
echo "📋 Verificando se está habilitado..."
if [ -L "$NGINX_ENABLED" ]; then
    echo "✅ Link simbólico existe: $NGINX_ENABLED"
    ls -la "$NGINX_ENABLED"
else
    echo "⚠️  Link simbólico não existe!"
fi
echo ""

# 3. Verificar DNS
echo "📋 Verificando resolução DNS..."
if nslookup demo.singleone.com.br > /dev/null 2>&1; then
    echo "✅ DNS resolvendo corretamente:"
    nslookup demo.singleone.com.br | grep -A 2 "Name:"
else
    echo "⚠️  DNS pode não estar resolvendo corretamente"
fi
echo ""

# 4. Verificar se o IP do servidor corresponde ao DNS
echo "📋 Verificando IP do servidor..."
SERVER_IP=$(hostname -I | awk '{print $1}')
echo "   IP do servidor: $SERVER_IP"

DNS_IP=$(nslookup demo.singleone.com.br 2>/dev/null | grep -A 1 "Name:" | grep "Address:" | tail -1 | awk '{print $2}')
if [ ! -z "$DNS_IP" ]; then
    echo "   IP do DNS: $DNS_IP"
    if [ "$SERVER_IP" = "$DNS_IP" ]; then
        echo "✅ IP do servidor corresponde ao DNS"
    else
        echo "⚠️  IP do servidor NÃO corresponde ao DNS!"
        echo "   Você precisa atualizar o DNS para apontar para: $SERVER_IP"
    fi
else
    echo "⚠️  Não foi possível obter IP do DNS"
fi
echo ""

# 5. Criar/atualizar configuração do Nginx
echo "📝 Criando/atualizando configuração do Nginx..."
cat > "$NGINX_CONFIG" << 'EOF'
server {
    listen 80 default_server;
    listen [::]:80 default_server;

    server_name demo.singleone.com.br _;

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
EOF

echo "✅ Configuração criada/atualizada"
echo ""

# 6. Garantir que está habilitado
echo "🔗 Garantindo que está habilitado..."
if [ ! -L "$NGINX_ENABLED" ]; then
    ln -sf "$NGINX_CONFIG" "$NGINX_ENABLED"
    echo "✅ Link simbólico criado"
else
    echo "✅ Link simbólico já existe"
fi
echo ""

# 7. Testar configuração
echo "🧪 Testando configuração do Nginx..."
if nginx -t; then
    echo "✅ Configuração válida!"
else
    echo "❌ Erro na configuração!"
    exit 1
fi
echo ""

# 8. Recarregar Nginx
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

# 9. Verificar se está escutando
echo "📋 Verificando se está escutando..."
if ss -tunlp | grep -q ":80"; then
    echo "✅ Nginx está escutando na porta 80"
    ss -tunlp | grep ":80" | head -1
else
    echo "❌ Nginx NÃO está escutando na porta 80!"
fi
echo ""

# 10. Testar acesso local
echo "🧪 Testando acesso local..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -H "Host: demo.singleone.com.br" http://127.0.0.1/ 2>/dev/null)
if [ "$HTTP_CODE" = "200" ]; then
    echo "✅ Acesso local funcionando (HTTP $HTTP_CODE)"
else
    echo "⚠️  Acesso local retornou HTTP $HTTP_CODE"
fi
echo ""

# 11. Verificar arquivos do frontend
echo "📋 Verificando arquivos do frontend..."
if [ -f "/opt/SingleOne/SingleOne_Frontend/dist/SingleOne/index.html" ]; then
    echo "✅ index.html encontrado"
    ls -lh /opt/SingleOne/SingleOne_Frontend/dist/SingleOne/index.html
else
    echo "❌ index.html NÃO encontrado!"
    echo "   Execute: cd /opt/SingleOne/SingleOne_Frontend && npm run build-prod"
fi
echo ""

echo "=========================================="
echo "✅ VERIFICAÇÃO CONCLUÍDA"
echo "=========================================="
echo ""
echo "📋 Próximos passos:"
echo "   1. Verifique se o DNS está apontando para: $SERVER_IP"
echo "   2. Teste acessando: http://demo.singleone.com.br"
echo "   3. Se usar HTTPS, configure o certificado SSL"
echo ""
echo "🧪 Teste local:"
echo "   curl -H 'Host: demo.singleone.com.br' http://127.0.0.1/"
echo ""

