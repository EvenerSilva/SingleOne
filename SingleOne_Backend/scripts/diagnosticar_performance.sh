#!/bin/bash

# ==========================================
# 🔍 DIAGNÓSTICO DE PERFORMANCE
# ==========================================
# Verifica: Latência, Gzip, Cache, API, DB
# ==========================================

echo "=========================================="
echo "🔍 DIAGNÓSTICO DE PERFORMANCE"
echo "=========================================="
echo ""

DOMAIN="demo.singleone.com.br"
NGINX_CONFIG="/etc/nginx/sites-available/singleone"

# ==========================================
# 1. VERIFICAR CONFIGURAÇÃO DO NGINX
# ==========================================
echo "📋 [1/7] Verificando configuração do Nginx..."
echo ""

if [ ! -f "$NGINX_CONFIG" ]; then
    echo "❌ Arquivo de configuração não encontrado: $NGINX_CONFIG"
    exit 1
fi

# Verificar Gzip
if grep -q "gzip on;" "$NGINX_CONFIG"; then
    echo "   ✅ Gzip está habilitado"
    GZIP_LEVEL=$(grep "gzip_comp_level" "$NGINX_CONFIG" | awk '{print $2}' | tr -d ';' || echo "não configurado")
    echo "      Nível de compressão: $GZIP_LEVEL"
else
    echo "   ❌ Gzip NÃO está habilitado (CRÍTICO para performance!)"
fi

# Verificar Cache
if grep -q "expires 1y;" "$NGINX_CONFIG"; then
    echo "   ✅ Cache de assets está habilitado"
else
    echo "   ⚠️  Cache de assets NÃO está configurado"
fi

# Verificar Timeouts do Proxy
PROXY_TIMEOUT=$(grep "proxy_read_timeout" "$NGINX_CONFIG" | head -1 | awk '{print $2}' | tr -d ';' || echo "não configurado")
echo "   Proxy read timeout: $PROXY_TIMEOUT"
echo ""

# ==========================================
# 2. TESTAR LATÊNCIA DE REDE
# ==========================================
echo "📋 [2/7] Testando latência de rede..."
echo ""

# Ping
PING_TIME=$(ping -c 3 8.8.8.8 2>/dev/null | grep "avg" | awk -F'/' '{print $5}' || echo "N/A")
echo "   Latência para 8.8.8.8: ${PING_TIME}ms"

# DNS
DNS_TIME=$(time (dig +short $DOMAIN > /dev/null 2>&1) 2>&1 | grep real | awk '{print $2}' || echo "N/A")
echo "   Tempo de resolução DNS: $DNS_TIME"
echo ""

# ==========================================
# 3. TESTAR COMPRESSÃO GZIP
# ==========================================
echo "📋 [3/7] Testando compressão Gzip..."
echo ""

# Testar se o servidor aceita gzip
GZIP_HEADER=$(curl -s -H "Accept-Encoding: gzip" -I "https://$DOMAIN" 2>/dev/null | grep -i "content-encoding" || echo "")
if [ -n "$GZIP_HEADER" ]; then
    echo "   ✅ Servidor aceita compressão Gzip"
else
    echo "   ⚠️  Servidor pode não estar comprimindo (teste manual necessário)"
fi

# Testar tamanho de um arquivo JS (se existir)
JS_FILE=$(curl -s "https://$DOMAIN" 2>/dev/null | grep -oP 'src="[^"]*\.js[^"]*"' | head -1 | cut -d'"' -f2 | sed 's|^/||' || echo "")
if [ -n "$JS_FILE" ] && [ "$JS_FILE" != "" ]; then
    SIZE_NO_GZIP=$(curl -s -H "Accept-Encoding: identity" "https://$DOMAIN/$JS_FILE" 2>/dev/null | wc -c)
    SIZE_GZIP=$(curl -s -H "Accept-Encoding: gzip" "https://$DOMAIN/$JS_FILE" 2>/dev/null | wc -c)
    if [ "$SIZE_NO_GZIP" -gt 0 ] && [ "$SIZE_GZIP" -gt 0 ]; then
        REDUCTION=$((100 - (SIZE_GZIP * 100 / SIZE_NO_GZIP)))
        echo "   Tamanho sem compressão: ${SIZE_NO_GZIP} bytes"
        echo "   Tamanho com compressão: ${SIZE_GZIP} bytes"
        echo "   Redução: ${REDUCTION}%"
    fi
fi
echo ""

# ==========================================
# 4. TESTAR TEMPO DE RESPOSTA DA API
# ==========================================
echo "📋 [4/7] Testando tempo de resposta da API..."
echo ""

# Testar endpoint de login (sem auth, deve retornar erro rápido)
API_START=$(date +%s%N)
API_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "https://$DOMAIN/api/configuracoes/buscarlogocliente" 2>/dev/null)
API_END=$(date +%s%N)
API_TIME=$(( (API_END - API_START) / 1000000 ))
echo "   Tempo de resposta da API: ${API_TIME}ms (status: $API_RESPONSE)"

# Testar endpoint do dashboard (se acessível)
DASHBOARD_START=$(date +%s%N)
DASHBOARD_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "https://$DOMAIN/api/relatorio/dashboardweb" 2>/dev/null)
DASHBOARD_END=$(date +%s%N)
DASHBOARD_TIME=$(( (DASHBOARD_END - DASHBOARD_START) / 1000000 ))
echo "   Tempo de resposta do dashboard: ${DASHBOARD_TIME}ms (status: $DASHBOARD_RESPONSE)"
echo ""

# ==========================================
# 5. VERIFICAR STATUS DOS SERVIÇOS
# ==========================================
echo "📋 [5/7] Verificando status dos serviços..."
echo ""

# Nginx
if systemctl is-active --quiet nginx; then
    echo "   ✅ Nginx está rodando"
    NGINX_MEM=$(ps aux | grep nginx | grep -v grep | awk '{sum+=$6} END {print sum/1024 " MB"}' || echo "N/A")
    echo "      Uso de memória: $NGINX_MEM"
else
    echo "   ❌ Nginx NÃO está rodando!"
fi

# API .NET
if systemctl is-active --quiet singleone-api; then
    echo "   ✅ API .NET está rodando"
    API_MEM=$(ps aux | grep "SingleOneAPI" | grep -v grep | awk '{sum+=$6} END {print sum/1024 " MB"}' || echo "N/A")
    echo "      Uso de memória: $API_MEM"
    
    # Verificar últimos erros
    ERROR_COUNT=$(journalctl -u singleone-api --since "5 minutes ago" --no-pager | grep -i "error\|exception" | wc -l)
    if [ "$ERROR_COUNT" -gt 0 ]; then
        echo "      ⚠️  $ERROR_COUNT erros nos últimos 5 minutos"
    else
        echo "      ✅ Sem erros recentes"
    fi
else
    echo "   ❌ API .NET NÃO está rodando!"
fi

# PostgreSQL
if systemctl is-active --quiet postgresql; then
    echo "   ✅ PostgreSQL está rodando"
else
    echo "   ⚠️  PostgreSQL pode não estar rodando (ou não é systemd)"
fi
echo ""

# ==========================================
# 6. VERIFICAR RECURSOS DO SERVIDOR
# ==========================================
echo "📋 [6/7] Verificando recursos do servidor..."
echo ""

# CPU
CPU_USAGE=$(top -bn1 | grep "Cpu(s)" | sed "s/.*, *\([0-9.]*\)%* id.*/\1/" | awk '{print 100 - $1}' || echo "N/A")
echo "   Uso de CPU: ${CPU_USAGE}%"

# Memória
MEM_TOTAL=$(free -m | awk 'NR==2{printf "%.1f", $2}')
MEM_USED=$(free -m | awk 'NR==2{printf "%.1f", $3}')
MEM_PERCENT=$(free | awk 'NR==2{printf "%.1f", $3/$2*100}')
echo "   Memória: ${MEM_USED}MB / ${MEM_TOTAL}MB (${MEM_PERCENT}%)"

# Disco
DISK_USAGE=$(df -h / | awk 'NR==2 {print $5}' | sed 's/%//')
DISK_AVAIL=$(df -h / | awk 'NR==2 {print $4}')
echo "   Disco: ${DISK_USAGE}% usado (${DISK_AVAIL} disponível)"
echo ""

# ==========================================
# 7. VERIFICAR QUERIES LENTAS NO BANCO
# ==========================================
echo "📋 [7/7] Verificando queries lentas no banco..."
echo ""

# Verificar se há queries ativas demoradas
SLOW_QUERIES=$(sudo -u postgres psql -d singleone -t -c "SELECT count(*) FROM pg_stat_activity WHERE state = 'active' AND now() - query_start > interval '5 seconds';" 2>/dev/null | tr -d ' ' || echo "0")
if [ "$SLOW_QUERIES" -gt 0 ]; then
    echo "   ⚠️  $SLOW_QUERIES queries ativas há mais de 5 segundos"
else
    echo "   ✅ Sem queries lentas detectadas"
fi

# Verificar conexões
CONNECTIONS=$(sudo -u postgres psql -d singleone -t -c "SELECT count(*) FROM pg_stat_activity;" 2>/dev/null | tr -d ' ' || echo "N/A")
echo "   Conexões ativas: $CONNECTIONS"
echo ""

# ==========================================
# RESUMO E RECOMENDAÇÕES
# ==========================================
echo "=========================================="
echo "📊 RESUMO E RECOMENDAÇÕES"
echo "=========================================="
echo ""

# Verificar problemas críticos
PROBLEMAS=0

if ! grep -q "gzip on;" "$NGINX_CONFIG"; then
    echo "❌ CRÍTICO: Gzip não está habilitado"
    echo "   → Execute: sudo bash /opt/SingleOne/SingleOne_Backend/scripts/otimizar_nginx_performance.sh"
    PROBLEMAS=$((PROBLEMAS + 1))
fi

if [ "$API_TIME" -gt 2000 ]; then
    echo "⚠️  API está lenta (${API_TIME}ms > 2000ms)"
    echo "   → Verifique logs: journalctl -u singleone-api -f"
    PROBLEMAS=$((PROBLEMAS + 1))
fi

if [ "$MEM_PERCENT" -gt 90 ]; then
    echo "⚠️  Memória quase esgotada (${MEM_PERCENT}%)"
    PROBLEMAS=$((PROBLEMAS + 1))
fi

if [ "$DISK_USAGE" -gt 90 ]; then
    echo "⚠️  Disco quase cheio (${DISK_USAGE}%)"
    PROBLEMAS=$((PROBLEMAS + 1))
fi

if [ "$PROBLEMAS" -eq 0 ]; then
    echo "✅ Nenhum problema crítico detectado!"
    echo ""
    echo "💡 Dicas para melhorar ainda mais:"
    echo "   1. Execute o script de otimização do Nginx"
    echo "   2. Verifique se há índices faltando no banco"
    echo "   3. Considere usar CDN para assets estáticos"
else
    echo ""
    echo "🔧 $PROBLEMAS problema(s) encontrado(s) - veja acima"
fi

echo ""
echo "=========================================="

