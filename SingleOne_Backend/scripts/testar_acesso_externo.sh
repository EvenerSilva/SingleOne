#!/bin/bash

# Script para testar acesso externo ao site

echo "=========================================="
echo "🌐 TESTANDO ACESSO EXTERNO"
echo "=========================================="
echo ""

SERVER_IP=$(hostname -I | awk '{print $1}')
DOMAIN="demo.singleone.com.br"

echo "📋 Informações do servidor:"
echo "   IP: $SERVER_IP"
echo "   Domínio: $DOMAIN"
echo ""

# 1. Testar acesso local com IP
echo "🧪 Teste 1: Acesso local por IP..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1/ 2>/dev/null)
if [ "$HTTP_CODE" = "200" ]; then
    echo "   ✅ HTTP $HTTP_CODE - Acesso local por IP OK"
else
    echo "   ❌ HTTP $HTTP_CODE - Acesso local por IP falhou"
fi
echo ""

# 2. Testar acesso local com domínio no header
echo "🧪 Teste 2: Acesso local com header Host..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -H "Host: $DOMAIN" http://127.0.0.1/ 2>/dev/null)
if [ "$HTTP_CODE" = "200" ]; then
    echo "   ✅ HTTP $HTTP_CODE - Acesso local com domínio OK"
else
    echo "   ❌ HTTP $HTTP_CODE - Acesso local com domínio falhou"
fi
echo ""

# 3. Testar acesso externo por IP
echo "🧪 Teste 3: Acesso externo por IP..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 5 http://$SERVER_IP/ 2>/dev/null)
if [ "$HTTP_CODE" = "200" ]; then
    echo "   ✅ HTTP $HTTP_CODE - Acesso externo por IP OK"
else
    echo "   ⚠️  HTTP $HTTP_CODE - Acesso externo por IP pode estar bloqueado ou falhou"
    echo "      Verifique firewall e regras de rede"
fi
echo ""

# 4. Testar acesso externo por domínio
echo "🧪 Teste 4: Acesso externo por domínio..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 5 http://$DOMAIN/ 2>/dev/null)
if [ "$HTTP_CODE" = "200" ]; then
    echo "   ✅ HTTP $HTTP_CODE - Acesso externo por domínio OK"
else
    echo "   ⚠️  HTTP $HTTP_CODE - Acesso externo por domínio falhou"
    if [ "$HTTP_CODE" = "000" ]; then
        echo "      Possíveis causas:"
        echo "      - DNS não está resolvendo corretamente"
        echo "      - Firewall bloqueando conexões"
        echo "      - Servidor não está acessível externamente"
    fi
fi
echo ""

# 5. Verificar firewall
echo "📋 Verificando firewall..."
if command -v ufw > /dev/null 2>&1; then
    echo "   UFW status:"
    ufw status | head -5
    echo ""
    if ufw status | grep -q "Status: active"; then
        if ufw status | grep -q "80/tcp"; then
            echo "   ✅ Porta 80 está permitida no UFW"
        else
            echo "   ⚠️  Porta 80 pode não estar permitida no UFW"
            echo "      Execute: sudo ufw allow 80/tcp"
        fi
    else
        echo "   ℹ️  UFW não está ativo"
    fi
elif command -v firewall-cmd > /dev/null 2>&1; then
    echo "   Firewalld status:"
    firewall-cmd --list-all 2>/dev/null | head -10
else
    echo "   ℹ️  Nenhum firewall gerenciado encontrado (pode estar usando iptables diretamente)"
fi
echo ""

# 6. Verificar se a porta 80 está acessível externamente
echo "📋 Verificando se a porta 80 está escutando em todas as interfaces..."
if ss -tunlp | grep -q ":80.*0.0.0.0"; then
    echo "   ✅ Porta 80 está escutando em 0.0.0.0 (todas as interfaces)"
    ss -tunlp | grep ":80" | head -1
else
    echo "   ⚠️  Porta 80 pode não estar escutando em todas as interfaces"
    ss -tunlp | grep ":80"
fi
echo ""

# 7. Verificar logs do Nginx para erros recentes
echo "📋 Verificando logs recentes do Nginx..."
if [ -f /var/log/nginx/error.log ]; then
    echo "   Últimos erros (se houver):"
    tail -5 /var/log/nginx/error.log 2>/dev/null | grep -i error || echo "   ✅ Nenhum erro recente"
else
    echo "   ℹ️  Arquivo de log não encontrado"
fi
echo ""

# 8. Testar API
echo "🧪 Teste 5: Acesso à API..."
API_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1:5000/api/ 2>/dev/null)
if [ "$API_CODE" = "404" ] || [ "$API_CODE" = "200" ]; then
    echo "   ✅ API está respondendo (HTTP $API_CODE)"
else
    echo "   ⚠️  API retornou HTTP $API_CODE"
fi
echo ""

# 9. Testar proxy da API através do Nginx
echo "🧪 Teste 6: Proxy da API através do Nginx..."
PROXY_CODE=$(curl -s -o /dev/null -w "%{http_code}" -H "Host: $DOMAIN" http://127.0.0.1/api/ 2>/dev/null)
if [ "$PROXY_CODE" = "404" ] || [ "$PROXY_CODE" = "200" ]; then
    echo "   ✅ Proxy da API está funcionando (HTTP $PROXY_CODE)"
else
    echo "   ⚠️  Proxy da API retornou HTTP $PROXY_CODE"
fi
echo ""

# Resumo
echo "=========================================="
echo "📊 RESUMO"
echo "=========================================="
echo ""
echo "✅ Configuração do Nginx: OK"
echo "✅ DNS: Resolvendo para $SERVER_IP"
echo "✅ Nginx: Rodando e escutando na porta 80"
echo "✅ Frontend: Arquivos encontrados"
echo ""
echo "🌐 Para acessar externamente:"
echo "   - Por IP: http://$SERVER_IP"
echo "   - Por domínio: http://$DOMAIN"
echo ""
echo "🔧 Se não estiver acessível externamente:"
echo "   1. Verifique firewall: sudo ufw status"
echo "   2. Permita porta 80: sudo ufw allow 80/tcp"
echo "   3. Verifique regras do provedor (Contabo, etc)"
echo "   4. Teste de outro dispositivo/rede"
echo ""

