#!/bin/bash

# 🎯 Configuração Nginx para seu ambiente específico
# Containers identificados:
# - singleone-frontend:3000
# - singleone-backend:5000  
# - portainer:9000
# - nginx-nginx-1:8080

echo "🔧 Configurando Nginx para seu ambiente..."

# Pedir domínio
read -p "🌐 Digite seu domínio (ex: meusite.com): " DOMAIN

if [ -z "$DOMAIN" ]; then
    echo "❌ Domínio não pode ser vazio!"
    exit 1
fi

echo "📝 Configurando para domínio: $DOMAIN"

# Criar configuração corrigida
cat > /tmp/nginx-dns-corrected.conf << EOF
# Configuração DNS para $DOMAIN
# nginx-nginx-1 está na porta 8080 externamente, mas 80 internamente

# Frontend Principal
server {
    listen 80;
    server_name $DOMAIN www.$DOMAIN;
    
    location / {
        proxy_pass http://singleone-frontend:80;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        
        # Para Angular SPA
        try_files \$uri \$uri/ /index.html;
    }
}

# API Backend
server {
    listen 80;
    server_name api.$DOMAIN;
    
    location / {
        proxy_pass http://singleone-backend:5000;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}

# Demo/Público (opcional)
server {
    listen 80;
    server_name demo.$DOMAIN;
    
    location / {
        proxy_pass http://singleone-frontend:80;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
    }
}

# Portainer Admin
server {
    listen 80;
    server_name admin.$DOMAIN;
    
    location / {
        # Nota: pode precisar testar diferentes formas de acessar o portainer
        proxy_pass http://portainer:9000;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        
        # WebSocket support para Portainer
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_read_timeout 86400;
    }
}

# Fallback - redirecionar tudo para domínio principal
server {
    listen 80 default_server;
    server_name _;
    
    location / {
        return 301 http://$DOMAIN\$request_uri;
    }
}
EOF

echo "📋 Configuração criada. Agora aplicando..."

# Testar se nginx container está acessível
if ! docker exec nginx-nginx-1 nginx -v > /dev/null 2>&1; then
    echo "❌ Erro: Não consegue acessar o container nginx-nginx-1"
    echo "Teste primeiro: docker exec nginx-nginx-1 nginx -v"
    exit 1
fi

# Fazer backup da configuração atual
echo "💾 Fazendo backup da configuração atual..."
docker exec nginx-nginx-1 sh -c "cp /etc/nginx/conf.d/default.conf /etc/nginx/conf.d/default.conf.backup.$(date +%Y%m%d-%H%M%S) 2>/dev/null || true"

# Copiar nova configuração
echo "📤 Copiando nova configuração..."
docker cp /tmp/nginx-dns-corrected.conf nginx-nginx-1:/etc/nginx/conf.d/dns.conf

# Testar configuração
echo "🧪 Testando configuração nginx..."
if docker exec nginx-nginx-1 nginx -t; then
    echo "✅ Configuração válida!"
    
    # Recarregar nginx
    echo "🔄 Recarregando nginx..."
    docker exec nginx-nginx-1 nginx -s reload
    
    echo "🎉 Nginx configurado com sucesso!"
    echo ""
    echo "🌐 URLs que devem funcionar:"
    echo "   Principal:  http://$DOMAIN"
    echo "   API:        http://api.$DOMAIN"
    echo "   Demo:       http://demo.$DOMAIN"  
    echo "   Admin:      http://admin.$DOMAIN"
    echo ""
    echo "📋 Configure estes DNS records no seu provedor:"
    echo "   A     $DOMAIN           84.247.128.180"
    echo "   A     www.$DOMAIN       84.247.128.180"
    echo "   A     api.$DOMAIN       84.247.128.180"
    echo "   A     demo.$DOMAIN      84.247.128.180"
    echo "   A     admin.$DOMAIN     84.247.128.180"
    echo ""
    echo "⚠️  Acesse via porta 8080 até configurar DNS:"
    echo "   http://84.247.128.180:8080 (nginx)"
    
else
    echo "❌ Erro na configuração nginx!"
    echo "Logs de erro:"
    docker exec nginx-nginx-1 nginx -t
    echo ""
    echo "🔧 Restaurando backup..."
    docker exec nginx-nginx-1 sh -c "rm /etc/nginx/conf.d/dns.conf"
fi

# Limpar arquivo temporário
rm /tmp/nginx-dns-corrected.conf

echo ""
echo "🔍 Para debugar, execute:"
echo "   docker exec nginx-nginx-1 nginx -T | grep -A 10 'server_name'"












