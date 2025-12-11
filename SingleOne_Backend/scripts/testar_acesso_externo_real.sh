#!/bin/bash

# Script para testar acesso externo real ao site

echo "=========================================="
echo "🌐 TESTANDO ACESSO EXTERNO REAL"
echo "=========================================="
echo ""

DOMAIN="demo.singleone.com.br"
SERVER_IP=$(hostname -I | awk '{print $1}')

echo "📋 Informações:"
echo "   Domínio: $DOMAIN"
echo "   IP do servidor: $SERVER_IP"
echo ""

# 1. Verificar DNS de diferentes servidores
echo "📋 [1/5] Verificando DNS de diferentes servidores..."
echo "   Google DNS (8.8.8.8):"
DNS_GOOGLE=$(dig @8.8.8.8 +short $DOMAIN 2>/dev/null | head -1)
if [ ! -z "$DNS_GOOGLE" ]; then
    echo "   ✅ $DNS_GOOGLE"
    if [ "$DNS_GOOGLE" = "$SERVER_IP" ]; then
        echo "   ✅ DNS do Google está correto"
    else
        echo "   ❌ DNS do Google NÃO está correto! Esperado: $SERVER_IP"
    fi
else
    echo "   ⚠️  Não foi possível resolver via Google DNS"
fi

echo "   Cloudflare DNS (1.1.1.1):"
DNS_CLOUDFLARE=$(dig @1.1.1.1 +short $DOMAIN 2>/dev/null | head -1)
if [ ! -z "$DNS_CLOUDFLARE" ]; then
    echo "   ✅ $DNS_CLOUDFLARE"
    if [ "$DNS_CLOUDFLARE" = "$SERVER_IP" ]; then
        echo "   ✅ DNS do Cloudflare está correto"
    else
        echo "   ❌ DNS do Cloudflare NÃO está correto! Esperado: $SERVER_IP"
    fi
else
    echo "   ⚠️  Não foi possível resolver via Cloudflare DNS"
fi
echo ""

# 2. Testar acesso externo por IP
echo "📋 [2/5] Testando acesso externo por IP..."
echo "   Testando: http://$SERVER_IP"
HTTP_CODE_IP=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 5 --max-time 10 http://$SERVER_IP/ 2>/dev/null)
if [ "$HTTP_CODE_IP" = "200" ]; then
    echo "   ✅ HTTP $HTTP_CODE_IP - Acesso por IP funciona"
else
    echo "   ⚠️  HTTP $HTTP_CODE_IP - Acesso por IP"
fi
echo ""

# 3. Testar acesso externo por domínio
echo "📋 [3/5] Testando acesso externo por domínio..."
echo "   Testando: http://$DOMAIN"
HTTP_CODE_DOMAIN=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 5 --max-time 10 http://$DOMAIN/ 2>/dev/null)
if [ "$HTTP_CODE_DOMAIN" = "200" ]; then
    echo "   ✅ HTTP $HTTP_CODE_DOMAIN - Acesso por domínio funciona"
else
    echo "   ⚠️  HTTP $HTTP_CODE_DOMAIN - Acesso por domínio"
    if [ "$HTTP_CODE_DOMAIN" = "000" ]; then
        echo "   ❌ Não foi possível conectar (timeout ou DNS não resolve)"
    fi
fi
echo ""

# 4. Verificar headers HTTP
echo "📋 [4/5] Verificando headers HTTP..."
echo "   Headers ao acessar por IP:"
curl -I --connect-timeout 5 --max-time 10 http://$SERVER_IP/ 2>/dev/null | head -5
echo ""

echo "   Headers ao acessar por domínio:"
curl -I --connect-timeout 5 --max-time 10 http://$DOMAIN/ 2>/dev/null | head -5
echo ""

# 5. Verificar se há diferença no Nginx ao receber requisições
echo "📋 [5/5] Verificando configuração do Nginx para diferentes hosts..."
NGINX_CONFIG="/etc/nginx/sites-available/singleone"

if [ -f "$NGINX_CONFIG" ]; then
    echo "   server_name configurado:"
    grep "server_name" "$NGINX_CONFIG" | head -1
    echo ""
    
    # Verificar se há múltiplos blocos server
    SERVER_BLOCKS=$(grep -c "^server {" "$NGINX_CONFIG" 2>/dev/null || echo "0")
    if [ "$SERVER_BLOCKS" -gt 1 ]; then
        echo "   ⚠️  Múltiplos blocos server encontrados ($SERVER_BLOCKS)"
        echo "   Isso pode causar conflitos!"
    else
        echo "   ✅ Apenas um bloco server (OK)"
    fi
fi
echo ""

# 6. Verificar logs de acesso recentes
echo "📋 Verificando logs de acesso recentes..."
if [ -f /var/log/nginx/access.log ]; then
    echo "   Últimas 10 requisições (últimos 5 minutos):"
    tail -100 /var/log/nginx/access.log 2>/dev/null | grep "$(date +%d/%b/%Y:%H)" | tail -10 | awk '{print "   " $1 " - " $7 " - " $9}'
else
    echo "   ⚠️  Arquivo de log não encontrado"
fi
echo ""

# Resumo
echo "=========================================="
echo "📊 RESUMO"
echo "=========================================="
echo ""
echo "Acesso por IP:     HTTP $HTTP_CODE_IP"
echo "Acesso por domínio: HTTP $HTTP_CODE_DOMAIN"
echo ""

if [ "$HTTP_CODE_IP" = "200" ] && [ "$HTTP_CODE_DOMAIN" != "200" ]; then
    echo "❌ PROBLEMA IDENTIFICADO:"
    echo "   - Acesso por IP funciona ✅"
    echo "   - Acesso por domínio NÃO funciona ❌"
    echo ""
    echo "🔧 Possíveis causas:"
    echo "   1. DNS não está propagado em todos os servidores"
    echo "   2. Cache DNS no cliente"
    echo "   3. Firewall ou regras de rede bloqueando requisições com Host header específico"
    echo "   4. Problema de propagação DNS (pode levar até 48h)"
    echo ""
    echo "💡 Soluções:"
    echo "   1. Aguardar propagação DNS (pode levar algumas horas)"
    echo "   2. Limpar cache DNS no cliente:"
    echo "      Windows: ipconfig /flushdns"
    echo "      Linux: sudo systemd-resolve --flush-caches"
    echo "   3. Testar de outro dispositivo/rede"
    echo "   4. Verificar configuração DNS no provedor de domínio"
elif [ "$HTTP_CODE_IP" = "200" ] && [ "$HTTP_CODE_DOMAIN" = "200" ]; then
    echo "✅ TUDO FUNCIONANDO!"
    echo "   Tanto IP quanto domínio estão acessíveis"
    echo ""
    echo "💡 Se você ainda não consegue acessar pelo domínio:"
    echo "   - Limpe o cache DNS do seu navegador"
    echo "   - Tente em modo anônimo/privado"
    echo "   - Teste de outro dispositivo/rede"
fi
echo ""

