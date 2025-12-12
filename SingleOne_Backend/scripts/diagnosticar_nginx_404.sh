#!/bin/bash

# Script de diagnóstico completo para problema de 404 no Nginx

echo "=========================================="
echo "🔍 DIAGNÓSTICO COMPLETO - NGINX 404"
echo "=========================================="
echo ""

NGINX_CONFIG="/etc/nginx/sites-available/singleone"
TEST_FILE="cliente_1_20250815151721.png"
TEST_URL="http://127.0.0.1/api/logos/$TEST_FILE"

# 1. Verificar se o backend está respondendo
echo "📋 [1/8] Verificando se o backend está respondendo..."
BACKEND_URL="http://127.0.0.1:5000/api/logos/$TEST_FILE"
BACKEND_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "$BACKEND_URL" 2>/dev/null)
echo "   URL do backend: $BACKEND_URL"
echo "   Resposta: $BACKEND_RESPONSE"
if [ "$BACKEND_RESPONSE" = "200" ]; then
    echo "   ✅ Backend está respondendo corretamente"
else
    echo "   ❌ Backend NÃO está respondendo (código: $BACKEND_RESPONSE)"
    echo "   Verificando se o serviço está rodando..."
    if systemctl is-active --quiet singleone-api; then
        echo "   ✅ Serviço está ativo"
    else
        echo "   ❌ Serviço NÃO está ativo!"
    fi
fi
echo ""

# 2. Verificar configuração do Nginx
echo "📋 [2/8] Verificando configuração do Nginx..."
if [ -f "$NGINX_CONFIG" ]; then
    echo "   ✅ Arquivo de configuração existe: $NGINX_CONFIG"
    
    # Verificar se /api/ está configurado
    if grep -q "location /api/" "$NGINX_CONFIG"; then
        echo "   ✅ Regra location /api/ encontrada"
        echo "   Conteúdo da regra:"
        grep -A 10 "location /api/" "$NGINX_CONFIG" | head -12
    else
        echo "   ❌ Regra location /api/ NÃO encontrada!"
    fi
    
    # Verificar ordem das location blocks
    echo ""
    echo "   📋 Ordem das location blocks:"
    grep -n "^[[:space:]]*location" "$NGINX_CONFIG" | head -10
else
    echo "   ❌ Arquivo de configuração NÃO existe!"
fi
echo ""

# 3. Verificar se há outros arquivos de configuração
echo "📋 [3/8] Verificando outros arquivos de configuração do Nginx..."
echo "   Arquivos em sites-enabled:"
ls -la /etc/nginx/sites-enabled/ 2>/dev/null | grep -v "^total"
echo ""
echo "   Arquivos em sites-available:"
ls -la /etc/nginx/sites-available/ 2>/dev/null | grep -v "^total"
echo ""

# 4. Verificar se há configuração default interferindo
echo "📋 [4/8] Verificando configuração default do Nginx..."
if [ -f "/etc/nginx/sites-enabled/default" ]; then
    echo "   ⚠️  Arquivo default existe e pode estar interferindo"
    if grep -q "location /api/" "/etc/nginx/sites-enabled/default"; then
        echo "   ⚠️  Arquivo default também tem regra /api/!"
        echo "   Conteúdo:"
        grep -A 5 "location /api/" "/etc/nginx/sites-enabled/default"
    fi
else
    echo "   ✅ Nenhum arquivo default encontrado"
fi
echo ""

# 5. Verificar sintaxe do Nginx
echo "📋 [5/8] Verificando sintaxe do Nginx..."
NGINX_TEST=$(nginx -t 2>&1)
if echo "$NGINX_TEST" | grep -q "syntax is ok"; then
    echo "   ✅ Sintaxe está correta"
else
    echo "   ❌ Erro na sintaxe!"
    echo "$NGINX_TEST"
fi
echo ""

# 6. Verificar status do Nginx
echo "📋 [6/8] Verificando status do Nginx..."
if systemctl is-active --quiet nginx; then
    echo "   ✅ Nginx está rodando"
    systemctl status nginx --no-pager | head -5
else
    echo "   ❌ Nginx NÃO está rodando!"
fi
echo ""

# 7. Testar acesso via Nginx
echo "📋 [7/8] Testando acesso via Nginx..."
echo "   URL de teste: $TEST_URL"
NGINX_RESPONSE=$(curl -s -L -o /dev/null -w "%{http_code}" "$TEST_URL" 2>/dev/null)
echo "   Resposta: $NGINX_RESPONSE"

if [ "$NGINX_RESPONSE" = "200" ]; then
    echo "   ✅ Nginx está servindo corretamente"
elif [ "$NGINX_RESPONSE" = "404" ]; then
    echo "   ❌ Nginx retorna 404"
    echo ""
    echo "   📋 Verificando headers da resposta..."
    curl -I "$TEST_URL" 2>/dev/null | head -10
    echo ""
    echo "   📋 Verificando se há redirecionamento..."
    curl -v "$TEST_URL" 2>&1 | grep -i "location\|301\|302" | head -5
else
    echo "   ⚠️  Resposta inesperada: $NGINX_RESPONSE"
fi
echo ""

# 8. Verificar logs do Nginx
echo "📋 [8/8] Verificando logs do Nginx..."
echo "   Últimas linhas do error.log relacionadas a /api/ ou logos:"
tail -50 /var/log/nginx/error.log 2>/dev/null | grep -i "api\|logo\|404" | tail -10 || echo "   Nenhum erro relacionado encontrado"
echo ""
echo "   Últimas linhas do access.log relacionadas a /api/logos:"
tail -50 /var/log/nginx/access.log 2>/dev/null | grep "/api/logos" | tail -5 || echo "   Nenhum acesso relacionado encontrado"
echo ""

# 9. Verificar se há problema com a ordem de processamento
echo "📋 [9/9] Verificando ordem de processamento das location blocks..."
if [ -f "$NGINX_CONFIG" ]; then
    echo "   Ordem completa das location blocks:"
    grep -n "^[[:space:]]*location" "$NGINX_CONFIG"
    echo ""
    
    # Verificar se /api/ vem antes de cache de imagens
    API_LINE=$(grep -n "^[[:space:]]*location /api/" "$NGINX_CONFIG" | head -1 | cut -d: -f1)
    CACHE_LINE=$(grep -n "^[[:space:]]*location ~\* \\.(jpg\|jpeg\|png" "$NGINX_CONFIG" | head -1 | cut -d: -f1)
    
    if [ -n "$API_LINE" ] && [ -n "$CACHE_LINE" ]; then
        if [ "$API_LINE" -lt "$CACHE_LINE" ]; then
            echo "   ✅ /api/ vem ANTES de cache de imagens (correto)"
        else
            echo "   ❌ /api/ vem DEPOIS de cache de imagens (ERRADO!)"
            echo "   Isso pode causar o problema de 404"
        fi
    fi
fi
echo ""

echo "=========================================="
echo "✅ DIAGNÓSTICO CONCLUÍDO"
echo "=========================================="
echo ""
echo "📋 Próximos passos baseados no diagnóstico:"
echo ""

if [ "$BACKEND_RESPONSE" != "200" ]; then
    echo "   1. ❌ Backend não está respondendo - corrija isso primeiro"
    echo "      sudo systemctl restart singleone-api"
    echo ""
fi

if [ "$NGINX_RESPONSE" = "404" ] && [ "$BACKEND_RESPONSE" = "200" ]; then
    echo "   2. ⚠️  Backend OK mas Nginx retorna 404"
    echo "      Execute: sudo bash /opt/SingleOne/SingleOne_Backend/scripts/corrigir_nginx_404_logos.sh"
    echo ""
    echo "   3. Se ainda não funcionar, verifique:"
    echo "      - Se há arquivo default interferindo: sudo rm /etc/nginx/sites-enabled/default"
    echo "      - Se a configuração foi recarregada: sudo nginx -s reload"
    echo "      - Logs em tempo real: sudo tail -f /var/log/nginx/error.log"
    echo ""
fi

echo "   4. Teste manual:"
echo "      curl -v http://127.0.0.1/api/logos/$TEST_FILE"
echo ""

