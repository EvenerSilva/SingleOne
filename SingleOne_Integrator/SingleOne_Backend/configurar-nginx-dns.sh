#!/bin/bash

# 🌐 Script para configurar Nginx Reverse Proxy com DNS

echo "🔧 Configurando Nginx para DNS..."

# Identificar container nginx
NGINX_CONTAINER="nginx-nginx-1"

# Verificar se container existe
if ! docker ps | grep -q $NGINX_CONTAINER; then
    echo "❌ Container $NGINX_CONTAINER não encontrado!"
    echo "Containers disponíveis:"
    docker ps --format "table {{.Names}}\t{{.Image}}"
    exit 1
fi

echo "✅ Container $NGINX_CONTAINER encontrado"

# Identificar containers da aplicação
echo "🔍 Identificando containers da aplicação..."
docker ps --format "table {{.Names}}\t{{.Ports}}"

# Perguntar pelo domínio principal
read -p "🌐 Digite seu domínio principal (ex: meusite.com): " DOMAIN_PRINCIPAL

if [ -z "$DOMAIN_PRINCIPAL" ]; then
    echo "❌ Domínio não pode ser vazio!"
    exit 1
fi

# Criar configuração nginx
CONFIG_FILE="/tmp/nginx-dns.conf"

cat > $CONFIG_FILE << EOF
# Configuração DNS para $DOMAIN_PRINCIPAL

# Frontend Principal
server {
    listen 80;
    server_name $DOMAIN_PRINCIPAL www.$DOMAIN_PRINCIPAL;
    
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
    server_name api.$DOMAIN_PRINCIPAL;
    
    location / {
        proxy_pass http://singleone-backend:5000;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}

# Demo/Público
server {
    listen 80;
    server_name demo.$DOMAIN_PRINCIPAL;
    
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
    server_name admin.$DOMAIN_PRINCIPAL;
    
    location / {
        proxy_pass http://host.docker.internal:9000;
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

# Servidor padrão
server {
    listen 80 default_server;
    server_name _;
    return 301 http://$DOMAIN_PRINCIPAL\$request_uri;
}
EOF

echo "📝 Configuração criada em $CONFIG_FILE"
echo ""
echo "📋 Configurações DNS necessárias:"
echo "Tipo    Nome                    Valor"
echo "A       $DOMAIN_PRINCIPAL       84.247.128.180"
echo "A       www.$DOMAIN_PRINCIPAL   84.247.128.180"
echo "A       api.$DOMAIN_PRINCIPAL   84.247.128.180"
echo "A       demo.$DOMAIN_PRINCIPAL  84.247.128.180"
echo "A       admin.$DOMAIN_PRINCIPAL 84.247.128.180"
echo ""

# Copiar configuração para o container nginx
echo "🚀 Aplicando configuração no nginx..."
docker cp $CONFIG_FILE $NGINX_CONTAINER:/etc/nginx/conf.d/dns.conf

# Testar configuração
echo "🧪 Testando configuração nginx..."
if docker exec $NGINX_CONTAINER nginx -t; then
    echo "✅ Configuração válida!"
    
    # Reload nginx
    echo "🔄 Recarregando nginx..."
    docker exec $NGINX_CONTAINER nginx -s reload
    
    echo "🎉 Nginx configurado com sucesso!"
    echo ""
    echo "🌐 URLs disponíveis:"
    echo "   Principal:  http://$DOMAIN_PRINCIPAL"
    echo "   API:        http://api.$DOMAIN_PRINCIPAL"
    echo "   Demo:       http://demo.$DOMAIN_PRINCIPAL"
    echo "   Admin:      http://admin.$DOMAIN_PRINCIPAL"
    echo ""
    echo "⚠️  Lembre-se de configurar os DNS records no seu provedor!"
    
else
    echo "❌ Erro na configuração nginx!"
    echo "Verifique os logs:"
    docker exec $NGINX_CONTAINER nginx -t
fi

# Limpar arquivo temporário
rm $CONFIG_FILE












