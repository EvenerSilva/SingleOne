#!/bin/bash

# Script para diagnosticar problemas com DNS e Nginx

echo "=========================================="
echo "🔍 DIAGNÓSTICO DNS E NGINX"
echo "=========================================="
echo ""

DOMAIN="demo.singleone.com.br"
SERVER_IP=$(hostname -I | awk '{print $1}')

# 1. Verificar DNS
echo "📋 [1/6] Verificando DNS..."
echo "   Domínio: $DOMAIN"
echo "   IP do servidor: $SERVER_IP"
echo ""

DNS_IP=$(nslookup $DOMAIN 2>/dev/null | grep -A 1 "Name:" | grep "Address:" | tail -1 | awk '{print $2}')
if [ ! -z "$DNS_IP" ]; then
    echo "   IP do DNS: $DNS_IP"
    if [ "$SERVER_IP" = "$DNS_IP" ]; then
        echo "   ✅ DNS apontando corretamente"
    else
        echo "   ❌ DNS NÃO está apontando para o IP do servidor!"
        echo "   Atualize o DNS para apontar para: $SERVER_IP"
    fi
else
    echo "   ⚠️  Não foi possível resolver DNS"
fi
echo ""

# 2. Verificar configuração do Nginx
echo "📋 [2/6] Verificando configuração do Nginx..."
NGINX_CONFIG="/etc/nginx/sites-available/singleone"

if [ -f "$NGINX_CONFIG" ]; then
    echo "   ✅ Arquivo de configuração encontrado"
    echo ""
    echo "   📄 server_name configurado:"
    grep "server_name" "$NGINX_CONFIG" | head -1
    echo ""
    
    # Verificar se o domínio está no server_name
    if grep -q "$DOMAIN" "$NGINX_CONFIG"; then
        echo "   ✅ Domínio $DOMAIN está no server_name"
    else
        echo "   ❌ Domínio $DOMAIN NÃO está no server_name!"
    fi
    
    # Verificar se o IP está no server_name
    if grep -q "$SERVER_IP" "$NGINX_CONFIG"; then
        echo "   ✅ IP $SERVER_IP está no server_name"
    else
        echo "   ⚠️  IP $SERVER_IP NÃO está no server_name"
    fi
else
    echo "   ❌ Arquivo de configuração não encontrado!"
fi
echo ""

# 3. Verificar status do Nginx
echo "📋 [3/6] Verificando status do Nginx..."
if systemctl is-active --quiet nginx; then
    echo "   ✅ Nginx está rodando"
    systemctl status nginx --no-pager | head -5
else
    echo "   ❌ Nginx NÃO está rodando!"
    echo "   Execute: sudo systemctl start nginx"
fi
echo ""

# 4. Testar acesso local com diferentes headers
echo "📋 [4/6] Testando acesso local..."
echo "   Teste 1: Acesso por IP (127.0.0.1)..."
HTTP_CODE1=$(curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1/ 2>/dev/null)
echo "   HTTP $HTTP_CODE1"
echo ""

echo "   Teste 2: Acesso com header Host=$DOMAIN..."
HTTP_CODE2=$(curl -s -o /dev/null -w "%{http_code}" -H "Host: $DOMAIN" http://127.0.0.1/ 2>/dev/null)
echo "   HTTP $HTTP_CODE2"
echo ""

echo "   Teste 3: Acesso com header Host=$SERVER_IP..."
HTTP_CODE3=$(curl -s -o /dev/null -w "%{http_code}" -H "Host: $SERVER_IP" http://127.0.0.1/ 2>/dev/null)
echo "   HTTP $HTTP_CODE3"
echo ""

# 5. Verificar logs do Nginx
echo "📋 [5/6] Verificando logs do Nginx..."
if [ -f /var/log/nginx/access.log ]; then
    echo "   Últimas 5 requisições:"
    tail -5 /var/log/nginx/access.log 2>/dev/null | awk '{print "   " $0}'
else
    echo "   ⚠️  Arquivo de log não encontrado"
fi

if [ -f /var/log/nginx/error.log ]; then
    ERROR_COUNT=$(tail -20 /var/log/nginx/error.log 2>/dev/null | grep -i error | wc -l)
    if [ "$ERROR_COUNT" -gt 0 ]; then
        echo "   ⚠️  Erros encontrados nos logs:"
        tail -5 /var/log/nginx/error.log 2>/dev/null | grep -i error | head -3 | awk '{print "   " $0}'
    else
        echo "   ✅ Nenhum erro recente nos logs"
    fi
fi
echo ""

# 6. Verificar API
echo "📋 [6/6] Verificando API..."
if systemctl is-active --quiet singleone-api; then
    echo "   ✅ API está rodando"
    
    # Testar API localmente
    API_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1:5000/api/ 2>/dev/null)
    echo "   Resposta local: HTTP $API_CODE"
    
    # Testar API através do Nginx
    API_PROXY_CODE=$(curl -s -o /dev/null -w "%{http_code}" -H "Host: $DOMAIN" http://127.0.0.1/api/ 2>/dev/null)
    echo "   Resposta via proxy: HTTP $API_PROXY_CODE"
else
    echo "   ❌ API NÃO está rodando!"
    echo "   Execute: sudo systemctl start singleone-api"
fi
echo ""

# 7. Verificar portas
echo "📋 Verificando portas..."
echo "   Porta 80 (Nginx):"
if ss -tunlp | grep -q ":80"; then
    ss -tunlp | grep ":80" | head -1 | awk '{print "   ✅ " $0}'
else
    echo "   ❌ Porta 80 não está em uso!"
fi

echo "   Porta 5000 (API):"
if ss -tunlp | grep -q ":5000"; then
    ss -tunlp | grep ":5000" | head -1 | awk '{print "   ✅ " $0}'
else
    echo "   ❌ Porta 5000 não está em uso!"
fi
echo ""

# Resumo e recomendações
echo "=========================================="
echo "📊 RESUMO E RECOMENDAÇÕES"
echo "=========================================="
echo ""

if [ "$DNS_IP" != "$SERVER_IP" ] && [ ! -z "$DNS_IP" ]; then
    echo "❌ PROBLEMA: DNS não está apontando para o IP correto"
    echo "   Atualize o DNS para: $SERVER_IP"
    echo ""
fi

if [ "$HTTP_CODE2" != "200" ]; then
    echo "⚠️  PROBLEMA: Nginx não está respondendo corretamente para o domínio"
    echo "   Verifique a configuração do server_name no Nginx"
    echo ""
fi

echo "🔧 Comandos para corrigir:"
echo "   1. Verificar/corrigir Nginx:"
echo "      sudo bash /opt/SingleOne/SingleOne_Backend/scripts/verificar_e_corrigir_nginx_demo.sh"
echo ""
echo "   2. Reiniciar Nginx:"
echo "      sudo systemctl restart nginx"
echo ""
echo "   3. Verificar logs em tempo real:"
echo "      sudo tail -f /var/log/nginx/error.log"
echo ""

