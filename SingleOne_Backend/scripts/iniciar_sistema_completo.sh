#!/bin/bash

# Script para iniciar o sistema SingleOne após reinicialização do servidor

echo "=========================================="
echo "🚀 INICIANDO SISTEMA SINGLEONE"
echo "=========================================="
echo ""

# 1. Verificar e iniciar PostgreSQL
echo "📋 Verificando PostgreSQL..."
if systemctl is-active --quiet postgresql; then
    echo "✅ PostgreSQL já está rodando"
else
    echo "🔄 Iniciando PostgreSQL..."
    systemctl start postgresql
    sleep 3
    if systemctl is-active --quiet postgresql; then
        echo "✅ PostgreSQL iniciado com sucesso"
    else
        echo "❌ Erro ao iniciar PostgreSQL!"
        systemctl status postgresql --no-pager -l | head -10
        exit 1
    fi
fi
echo ""

# 2. Verificar conexão com o banco
echo "📋 Testando conexão com o banco de dados..."
if sudo -u postgres psql -d singleone -c "SELECT 1;" > /dev/null 2>&1; then
    echo "✅ Conexão com banco de dados OK"
else
    echo "❌ Erro ao conectar com banco de dados!"
    echo "   Verifique se o banco 'singleone' existe e está acessível"
    exit 1
fi
echo ""

# 3. Verificar e iniciar API
echo "📋 Verificando API..."
if systemctl is-active --quiet singleone-api; then
    echo "✅ API já está rodando"
else
    echo "🔄 Iniciando API..."
    systemctl start singleone-api
    sleep 5
    
    if systemctl is-active --quiet singleone-api; then
        echo "✅ API iniciada com sucesso"
    else
        echo "❌ Erro ao iniciar API!"
        echo "📋 Logs da API:"
        journalctl -u singleone-api -n 30 --no-pager
        exit 1
    fi
fi
echo ""

# 4. Verificar se a API está respondendo
echo "📋 Testando resposta da API..."
sleep 2
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1:5000/api/health 2>/dev/null || echo "000")

if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "404" ]; then
    echo "✅ API está respondendo (HTTP $HTTP_CODE)"
else
    echo "⚠️  API pode não estar respondendo corretamente (HTTP $HTTP_CODE)"
    echo "📋 Verificando logs..."
    journalctl -u singleone-api -n 20 --no-pager
fi
echo ""

# 5. Garantir configuração do Nginx antes de iniciar
echo "📋 Garantindo configuração do Nginx..."
NGINX_CONFIG="/etc/nginx/sites-available/singleone"
NGINX_ENABLED="/etc/nginx/sites-enabled/singleone"

# Criar/atualizar configuração do Nginx
if [ ! -f "$NGINX_CONFIG" ] || ! grep -q "demo.singleone.com.br" "$NGINX_CONFIG" 2>/dev/null; then
    echo "📝 Criando/atualizando configuração do Nginx..."
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
    echo "✅ Configuração do Nginx criada/atualizada"
else
    echo "✅ Configuração do Nginx já existe e está correta"
fi

# Garantir link simbólico
if [ ! -L "$NGINX_ENABLED" ]; then
    echo "🔗 Criando link simbólico..."
    ln -sf "$NGINX_CONFIG" "$NGINX_ENABLED"
    echo "✅ Link simbólico criado"
else
    echo "✅ Link simbólico já existe"
fi

# Testar configuração
echo "🧪 Testando configuração do Nginx..."
if nginx -t > /dev/null 2>&1; then
    echo "✅ Configuração do Nginx válida"
else
    echo "❌ Erro na configuração do Nginx:"
    nginx -t
    exit 1
fi
echo ""

# 6. Verificar e iniciar Nginx
echo "📋 Verificando Nginx..."
if systemctl is-active --quiet nginx; then
    echo "✅ Nginx já está rodando"
    echo "🔄 Recarregando configuração..."
    systemctl reload nginx
else
    echo "🔄 Iniciando Nginx..."
    systemctl start nginx
    sleep 2
    
    if systemctl is-active --quiet nginx; then
        echo "✅ Nginx iniciado com sucesso"
    else
        echo "❌ Erro ao iniciar Nginx!"
        systemctl status nginx --no-pager -l | head -10
        exit 1
    fi
fi
echo ""

# 7. Verificar se os arquivos do frontend existem
echo "📋 Verificando arquivos do frontend..."
if [ -f "/opt/SingleOne/SingleOne_Frontend/dist/SingleOne/index.html" ]; then
    echo "✅ Arquivos do frontend encontrados"
else
    echo "⚠️  Arquivos do frontend não encontrados!"
    echo "   Execute: cd /opt/SingleOne/SingleOne_Frontend && npm run build-prod"
fi
echo ""

# 8. Verificar porta da API
echo "📋 Verificando se a porta 5000 está em uso..."
if ss -tunlp | grep -q ":5000"; then
    echo "✅ Porta 5000 está em uso (API rodando)"
    ss -tunlp | grep ":5000"
else
    echo "❌ Porta 5000 NÃO está em uso!"
    echo "   A API pode não estar escutando corretamente"
fi
echo ""

# 9. Verificar porta do Nginx
echo "📋 Verificando se a porta 80 está em uso..."
if ss -tunlp | grep -q ":80"; then
    echo "✅ Porta 80 está em uso (Nginx rodando)"
    ss -tunlp | grep ":80"
else
    echo "❌ Porta 80 NÃO está em uso!"
    echo "   O Nginx pode não estar escutando corretamente"
fi
echo ""

# 10. Resumo final
echo "=========================================="
echo "📊 RESUMO DO SISTEMA"
echo "=========================================="
echo ""
echo "PostgreSQL: $(systemctl is-active postgresql)"
echo "API:        $(systemctl is-active singleone-api)"
echo "Nginx:      $(systemctl is-active nginx)"
echo ""

# 11. Teste final
echo "🧪 Testando acesso externo..."
EXTERNAL_IP=$(hostname -I | awk '{print $1}')
echo "   IP do servidor: $EXTERNAL_IP"
echo "   Teste acessando: http://$EXTERNAL_IP"
echo "   ou: https://demo.singleone.com.br"
echo ""

# 12. Verificar serviços habilitados para iniciar automaticamente
echo "📋 Verificando serviços habilitados para iniciar automaticamente..."
if systemctl is-enabled postgresql > /dev/null 2>&1; then
    echo "✅ PostgreSQL habilitado para iniciar automaticamente"
else
    echo "⚠️  PostgreSQL NÃO está habilitado para iniciar automaticamente"
    echo "   Execute: sudo systemctl enable postgresql"
fi

if systemctl is-enabled singleone-api > /dev/null 2>&1; then
    echo "✅ API habilitada para iniciar automaticamente"
else
    echo "⚠️  API NÃO está habilitada para iniciar automaticamente"
    echo "   Execute: sudo systemctl enable singleone-api"
fi

if systemctl is-enabled nginx > /dev/null 2>&1; then
    echo "✅ Nginx habilitado para iniciar automaticamente"
else
    echo "⚠️  Nginx NÃO está habilitado para iniciar automaticamente"
    echo "   Execute: sudo systemctl enable nginx"
fi
echo ""

echo "=========================================="
echo "✅ INICIALIZAÇÃO CONCLUÍDA"
echo "=========================================="
echo ""
echo "📋 Se algum serviço não estiver funcionando:"
echo "   1. Verifique os logs: journalctl -u NOME_DO_SERVICO -n 50"
echo "   2. Verifique o status: systemctl status NOME_DO_SERVICO"
echo "   3. Tente reiniciar: systemctl restart NOME_DO_SERVICO"
echo ""


