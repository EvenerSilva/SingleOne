#!/bin/bash

# 🔧 Script para diagnosticar e acessar container nginx

echo "🔍 Diagnósticos do Container Nginx..."

# 1. Verificar se container está rodando
echo "1. Verificando containers nginx:"
docker ps | grep nginx

if [ $? -ne 0 ]; then
    echo "❌ Nenhum container nginx encontrado!"
    echo "Containers disponíveis:"
    docker ps --format "table {{.Names}}\t{{.Image}}\t{{.Status}}"
    exit 1
fi

# 2. Identificar nome exato do container
NGINX_CONTAINER=$(docker ps --format "{{.Names}}" | grep nginx | head -1)
echo "✅ Container nginx encontrado: $NGINX_CONTAINER"

# 3. Verificar qual shell está disponível
echo "2. Testando shells disponíveis..."
echo "Testando /bin/sh:"
if docker exec $NGINX_CONTAINER /bin/sh -c "echo 'OK: /bin/sh funciona'" 2>/dev/null; then
    echo "✅ /bin/sh disponível"
    SHELL_PATH="/bin/sh"
else
    echo "❌ /bin/sh não disponível"
fi

echo "Testando sh:"
if docker exec $NGINX_CONTAINER sh -c "echo 'OK: sh funciona'" 2>/dev/null; then
    echo "✅ sh disponível"
    SHELL_PATH="sh"
else
    echo "❌ sh não disponível"
fi

echo "Testando /bin/ash:"
if docker exec $NGINX_CONTAINER /bin/ash -c "echo 'OK: /bin/ash funciona'" 2>/dev/null; then
    echo "✅ /bin/ash disponível"
    SHELL_PATH="/bin/ash"
else
    echo "❌ /bin/ash não disponível"
fi

# 4. Verificar estrutura do container
echo "3. Verificando estrutura do container:"
docker exec $NGINX_CONTAINER $SHELL_PATH -c "ls -la /etc/nginx/"

# 5. Verificar arquivos de configuração existentes
echo "4. Arquivos de configuração nginx:"
docker exec $NGINX_CONTAINER $SHELL_PATH -c "find /etc/nginx -name '*.conf' -type f"

# 6. Mostrar comando correto para acessar
echo ""
echo "🎯 COMANDOS CORRETOS PARA ACESSAR:"
echo "docker exec -it $NGINX_CONTAINER $SHELL_PATH"
echo ""
echo "Ou via Portainer:"
echo "1. Acesse: http://84.247.128.180:9000"
echo "2. Containers > $NGINX_CONTAINER > Console"
echo "3. Execute: $SHELL_PATH"












