#!/bin/bash

# Script para verificar se as correções de IP e geolocalização estão funcionando

echo "=========================================="
echo "🔍 VERIFICANDO CORREÇÕES DE TERMO"
echo "=========================================="
echo ""

# 1. Verificar se a API está rodando
echo "📋 [1/4] Verificando API..."
if systemctl is-active --quiet singleone-api; then
    echo "   ✅ API está rodando"
    API_PID=$(systemctl show -p MainPID --value singleone-api)
    echo "   PID: $API_PID"
else
    echo "   ❌ API NÃO está rodando!"
    exit 1
fi
echo ""

# 2. Verificar logs recentes para erros
echo "📋 [2/4] Verificando logs recentes..."
echo "   Últimas 30 linhas de logs:"
journalctl -u singleone-api -n 30 --no-pager | tail -20
echo ""

# 3. Verificar se há erros relacionados a geolocalização
echo "📋 [3/4] Verificando erros de geolocalização..."
ERROR_COUNT=$(journalctl -u singleone-api -n 100 --no-pager | grep -i "ERRO_GEOLOCALIZAÇÃO\|geolocalização\|fk_geolocalizacao" | wc -l)
if [ "$ERROR_COUNT" -gt 0 ]; then
    echo "   ⚠️  Encontrados $ERROR_COUNT erros relacionados a geolocalização:"
    journalctl -u singleone-api -n 100 --no-pager | grep -i "ERRO_GEOLOCALIZAÇÃO\|geolocalização\|fk_geolocalizacao" | tail -5
else
    echo "   ✅ Nenhum erro de geolocalização encontrado"
fi
echo ""

# 4. Verificar se há logs de captura de IP
echo "📋 [4/4] Verificando captura de IP..."
IP_LOGS=$(journalctl -u singleone-api -n 100 --no-pager | grep -i "IP_SERVICE\|IP capturado\|CONTROLLER.*IP" | wc -l)
if [ "$IP_LOGS" -gt 0 ]; then
    echo "   ✅ Encontrados $IP_LOGS logs de captura de IP"
    echo "   Últimos logs de IP:"
    journalctl -u singleone-api -n 100 --no-pager | grep -i "IP_SERVICE\|IP capturado\|CONTROLLER.*IP" | tail -3
else
    echo "   ⚠️  Nenhum log de captura de IP encontrado (pode ser normal se não houver requisições recentes)"
fi
echo ""

# 5. Verificar se há registros de geolocalização no banco
echo "📋 Verificando registros no banco de dados..."
RECORDS=$(sudo -u postgres psql -d singleone -t -c "SELECT COUNT(*) FROM geolocalizacao_assinatura WHERE timestamp_captura > NOW() - INTERVAL '1 hour';" 2>/dev/null | tr -d ' ')
if [ ! -z "$RECORDS" ] && [ "$RECORDS" != "0" ]; then
    echo "   ✅ Encontrados $RECORDS registros de geolocalização na última hora"
    
    # Mostrar último registro
    echo "   Último registro:"
    sudo -u postgres psql -d singleone -c "SELECT colaborador_nome, ip_address, city, country, timestamp_captura FROM geolocalizacao_assinatura ORDER BY timestamp_captura DESC LIMIT 1;" 2>/dev/null | tail -3
else
    echo "   ℹ️  Nenhum registro de geolocalização na última hora (normal se não houver assinaturas recentes)"
fi
echo ""

echo "=========================================="
echo "✅ VERIFICAÇÃO CONCLUÍDA"
echo "=========================================="
echo ""
echo "💡 Para testar as correções:"
echo "   1. Acesse: https://demo.singleone.com.br/termos/[HASH]/[BYOD]"
echo "   2. Assine um termo"
echo "   3. Verifique os logs: journalctl -u singleone-api -f"
echo "   4. Verifique no banco: sudo -u postgres psql -d singleone -c \"SELECT * FROM geolocalizacao_assinatura ORDER BY timestamp_captura DESC LIMIT 1;\""
echo ""

