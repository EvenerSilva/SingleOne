#!/bin/bash

# Script para subir Frontend, Backend e Banco de Dados

echo "=========================================="
echo "🚀 SUBINDO SISTEMA COMPLETO"
echo "=========================================="
echo ""

# 1. Iniciar PostgreSQL
echo "📋 [1/4] Iniciando PostgreSQL..."
if systemctl is-active --quiet postgresql; then
    echo "   ✅ PostgreSQL já está rodando"
else
    systemctl start postgresql
    sleep 3
    if systemctl is-active --quiet postgresql; then
        echo "   ✅ PostgreSQL iniciado"
    else
        echo "   ❌ Erro ao iniciar PostgreSQL!"
        exit 1
    fi
fi
echo ""

# 2. Verificar conexão com banco
echo "📋 [2/4] Verificando banco de dados..."
if sudo -u postgres psql -d singleone -c "SELECT 1;" > /dev/null 2>&1; then
    echo "   ✅ Banco de dados OK"
else
    echo "   ⚠️  Banco 'singleone' pode não existir"
    echo "   Execute: sudo -u postgres psql -c 'CREATE DATABASE singleone;'"
fi
echo ""

# 3. Iniciar API (Backend)
echo "📋 [3/4] Iniciando API (Backend)..."
if systemctl is-active --quiet singleone-api; then
    echo "   ✅ API já está rodando"
else
    systemctl start singleone-api
    sleep 5
    if systemctl is-active --quiet singleone-api; then
        echo "   ✅ API iniciada"
    else
        echo "   ❌ Erro ao iniciar API!"
        echo "   Logs: journalctl -u singleone-api -n 20"
        exit 1
    fi
fi

# Verificar se API está respondendo
sleep 2
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1:5000/api/ 2>/dev/null || echo "000")
if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "404" ]; then
    echo "   ✅ API respondendo (HTTP $HTTP_CODE)"
else
    echo "   ⚠️  API pode não estar respondendo (HTTP $HTTP_CODE)"
fi
echo ""

# 4. Compilar e configurar Frontend
echo "📋 [4/4] Verificando Frontend..."

# Verificar se precisa compilar
if [ ! -f "/opt/SingleOne/SingleOne_Frontend/dist/SingleOne/index.html" ]; then
    echo "   📦 Frontend não compilado, compilando agora..."
    cd /opt/SingleOne/SingleOne_Frontend
    npm run build-prod
    if [ $? -eq 0 ]; then
        echo "   ✅ Frontend compilado com sucesso"
    else
        echo "   ❌ Erro ao compilar frontend!"
        exit 1
    fi
else
    echo "   ✅ Frontend já está compilado"
fi

# Garantir configuração do Nginx
echo "   🔧 Garantindo configuração do Nginx..."
NGINX_CONFIG="/etc/nginx/sites-available/singleone"
NGINX_ENABLED="/etc/nginx/sites-enabled/singleone"

if [ ! -f "$NGINX_CONFIG" ] || ! grep -q "demo.singleone.com.br" "$NGINX_CONFIG" 2>/dev/null; then
    cat > "$NGINX_CONFIG" << 'NGINX_EOF'
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
NGINX_EOF
    echo "   ✅ Configuração do Nginx criada/atualizada"
fi

# Garantir link simbólico
if [ ! -L "$NGINX_ENABLED" ]; then
    ln -sf "$NGINX_CONFIG" "$NGINX_ENABLED"
    echo "   ✅ Link simbólico criado"
fi

# Testar e iniciar Nginx
if nginx -t > /dev/null 2>&1; then
    if systemctl is-active --quiet nginx; then
        systemctl reload nginx
        echo "   ✅ Nginx recarregado"
    else
        systemctl start nginx
        echo "   ✅ Nginx iniciado"
    fi
else
    echo "   ❌ Erro na configuração do Nginx!"
    nginx -t
    exit 1
fi
echo ""

# Resumo final
echo "=========================================="
echo "✅ SISTEMA SUBIDO COM SUCESSO"
echo "=========================================="
echo ""
echo "📊 Status dos serviços:"
echo "   PostgreSQL: $(systemctl is-active postgresql)"
echo "   API:        $(systemctl is-active singleone-api)"
echo "   Nginx:      $(systemctl is-active nginx)"
echo ""
echo "🌐 Acesse:"
SERVER_IP=$(hostname -I | awk '{print $1}')
echo "   - Por IP: http://$SERVER_IP"
echo "   - Por domínio: http://demo.singleone.com.br"
echo ""
echo "📋 Comandos úteis:"
echo "   - Ver logs da API: journalctl -u singleone-api -f"
echo "   - Ver logs do Nginx: tail -f /var/log/nginx/error.log"
echo "   - Parar tudo: sudo systemctl stop postgresql singleone-api nginx"
echo ""

