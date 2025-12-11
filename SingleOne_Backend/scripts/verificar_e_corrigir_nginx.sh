#!/bin/bash
# ========================================
# Script para VERIFICAR e CORRIGIR NGINX
# Garante que rotas Angular (incluindo /termos/) funcionem
# Execute: sudo bash verificar_e_corrigir_nginx.sh
# ========================================

echo "=========================================="
echo "🔍 VERIFICAÇÃO E CORREÇÃO DO NGINX"
echo "=========================================="
echo ""

# 1. Verificar se Nginx está instalado
if ! command -v nginx &> /dev/null; then
    echo "❌ Nginx não está instalado!"
    echo "📦 Instalando Nginx..."
    apt update && apt install -y nginx
fi

echo "✅ Nginx instalado"
echo ""

# 2. Verificar diretório do frontend
FRONTEND_DIR="/opt/SingleOne/SingleOne_Frontend/dist/SingleOne"
echo "📋 Verificando diretório do frontend..."
if [ ! -d "$FRONTEND_DIR" ]; then
    echo "⚠️  Diretório do frontend não encontrado: $FRONTEND_DIR"
    echo "📦 Você precisa fazer build do frontend primeiro!"
    echo ""
    echo "Execute:"
    echo "  cd /opt/SingleOne/SingleOne_Frontend"
    echo "  npm install"
    echo "  npm run build-prod"
    echo ""
    read -p "Deseja continuar mesmo assim? (s/n): " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Ss]$ ]]; then
        exit 1
    fi
else
    echo "✅ Diretório do frontend encontrado: $FRONTEND_DIR"
    if [ ! -f "$FRONTEND_DIR/index.html" ]; then
        echo "⚠️  index.html não encontrado no diretório!"
    else
        echo "✅ index.html encontrado"
    fi
fi
echo ""

# 3. Verificar configuração atual do Nginx
NGINX_CONFIG="/etc/nginx/sites-available/singleone"
echo "📋 Verificando configuração do Nginx..."

if [ ! -f "$NGINX_CONFIG" ]; then
    echo "⚠️  Arquivo de configuração não encontrado: $NGINX_CONFIG"
    echo "📝 Criando configuração padrão..."
    
    # Criar configuração básica
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

    # Proxy para API do Backend
    location /api/ {
        proxy_pass http://127.0.0.1:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 300s;
        proxy_connect_timeout 75s;
    }

    # Angular routing - TODAS as rotas (incluindo /termos/) devem retornar index.html
    location / {
        try_files $uri $uri/ /index.html;
        add_header Content-Type "text/html; charset=utf-8";
    }

    # Cache para assets estáticos
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # Não fazer cache do index.html
    location = /index.html {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Content-Type "text/html; charset=utf-8";
        expires 0;
    }

    # Error pages - redirecionar 404 para index.html (Angular routing)
    error_page 404 /index.html;
}
EOF
    echo "✅ Configuração criada"
else
    echo "✅ Arquivo de configuração encontrado"
    
    # Verificar se tem try_files para Angular routing
    if ! grep -q "try_files.*index.html" "$NGINX_CONFIG"; then
        echo "⚠️  Configuração não tem try_files para Angular routing!"
        echo "📝 Adicionando configuração..."
        
        # Fazer backup
        cp "$NGINX_CONFIG" "${NGINX_CONFIG}.backup.$(date +%Y%m%d_%H%M%S)"
        
        # Adicionar try_files se não existir
        if grep -q "location / {" "$NGINX_CONFIG"; then
            # Substituir location / existente
            sed -i 's|location / {.*|location / {\n        try_files $uri $uri/ /index.html;\n        add_header Content-Type "text/html; charset=utf-8";\n    }|' "$NGINX_CONFIG"
        else
            # Adicionar location / antes do fechamento do server
            sed -i '/^}$/i\    location / {\n        try_files $uri $uri/ /index.html;\n        add_header Content-Type "text/html; charset=utf-8";\n    }' "$NGINX_CONFIG"
        fi
        
        echo "✅ Configuração atualizada"
    else
        echo "✅ Configuração já tem try_files para Angular routing"
    fi
fi
echo ""

# 4. Verificar link simbólico
NGINX_ENABLED="/etc/nginx/sites-enabled/singleone"
echo "📋 Verificando link simbólico..."
if [ ! -L "$NGINX_ENABLED" ]; then
    echo "⚠️  Link simbólico não encontrado, criando..."
    ln -s "$NGINX_CONFIG" "$NGINX_ENABLED"
    echo "✅ Link simbólico criado"
else
    echo "✅ Link simbólico existe"
fi
echo ""

# 5. Verificar sintaxe do Nginx
echo "🔍 Verificando sintaxe do Nginx..."
if nginx -t 2>&1 | grep -q "syntax is ok"; then
    echo "✅ Sintaxe do Nginx está correta"
else
    echo "❌ ERRO na sintaxe do Nginx!"
    nginx -t
    exit 1
fi
echo ""

# 6. Recarregar Nginx
echo "🔄 Recarregando Nginx..."
systemctl reload nginx
if [ $? -eq 0 ]; then
    echo "✅ Nginx recarregado com sucesso"
else
    echo "❌ Erro ao recarregar Nginx!"
    systemctl status nginx --no-pager -l | head -20
    exit 1
fi
echo ""

# 7. Verificar status
echo "📋 Status do Nginx:"
systemctl status nginx --no-pager -l | head -10
echo ""

# 8. Mostrar configuração aplicada
echo "=========================================="
echo "📋 Configuração de roteamento Angular:"
echo "=========================================="
grep -A 3 "location / {" "$NGINX_CONFIG" | head -5
echo ""

echo "=========================================="
echo "✅ VERIFICAÇÃO CONCLUÍDA!"
echo "=========================================="
echo ""
echo "📋 Teste acessando:"
echo "   http://84.247.128.180/termos/teste/false"
echo ""
echo "📋 Se ainda der 404, verifique:"
echo "   1. Frontend está buildado: ls -la $FRONTEND_DIR"
echo "   2. index.html existe: ls -la $FRONTEND_DIR/index.html"
echo "   3. Logs do Nginx: tail -f /var/log/nginx/error.log"
echo ""

