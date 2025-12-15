#!/usr/bin/env bash

###############################################################################
# Script de Diagnóstico - Verificar acesso ao SingleOne
#
# Uso: sudo bash deploy/linux/diagnosticar_acesso.sh
###############################################################################

echo "======================================================="
echo " 🔍 Diagnóstico de Acesso - SingleOne"
echo "======================================================="
echo

# 1. Verificar se Nginx está rodando
echo ">>> [1/6] Verificando Nginx..."
if systemctl is-active --quiet nginx; then
    echo "   ✅ Nginx está rodando"
    systemctl status nginx --no-pager -l | head -n 5
else
    echo "   ❌ Nginx NÃO está rodando!"
    echo "   💡 Execute: systemctl start nginx"
fi
echo

# 2. Verificar se API está rodando
echo ">>> [2/6] Verificando API SingleOne..."
if systemctl is-active --quiet singleone-api; then
    echo "   ✅ API está rodando"
    systemctl status singleone-api --no-pager -l | head -n 5
else
    echo "   ❌ API NÃO está rodando!"
    echo "   💡 Execute: systemctl start singleone-api"
    echo "   📋 Logs: journalctl -u singleone-api -n 20"
fi
echo

# 3. Verificar portas abertas
echo ">>> [3/6] Verificando portas abertas..."
if command -v netstat >/dev/null 2>&1; then
    echo "   Porta 80 (HTTP):"
    netstat -tlnp | grep ":80 " || echo "      ⚠️  Porta 80 não está escutando"
    echo "   Porta 5000 (API):"
    netstat -tlnp | grep ":5000 " || echo "      ⚠️  Porta 5000 não está escutando"
elif command -v ss >/dev/null 2>&1; then
    echo "   Porta 80 (HTTP):"
    ss -tlnp | grep ":80 " || echo "      ⚠️  Porta 80 não está escutando"
    echo "   Porta 5000 (API):"
    ss -tlnp | grep ":5000 " || echo "      ⚠️  Porta 5000 não está escutando"
fi
echo

# 4. Verificar firewall (ufw)
echo ">>> [4/6] Verificando firewall (ufw)..."
if command -v ufw >/dev/null 2>&1; then
    if ufw status | grep -q "Status: active"; then
        echo "   ⚠️  Firewall está ATIVO"
        ufw status | grep -E "(80|443|5000)" || echo "      ⚠️  Portas 80/443/5000 podem estar bloqueadas"
        echo "   💡 Para liberar: ufw allow 80/tcp && ufw allow 443/tcp"
    else
        echo "   ✅ Firewall não está ativo (ou não instalado)"
    fi
else
    echo "   ℹ️  ufw não instalado (verifique iptables manualmente)"
fi
echo

# 5. Verificar configuração Nginx
echo ">>> [5/6] Verificando configuração Nginx..."
if [[ -f /etc/nginx/sites-available/singleone ]]; then
    echo "   ✅ Arquivo de configuração existe"
    if nginx -t 2>&1 | grep -q "successful"; then
        echo "   ✅ Sintaxe do Nginx está OK"
    else
        echo "   ❌ Erro na sintaxe do Nginx:"
        nginx -t
    fi
else
    echo "   ❌ Arquivo de configuração não encontrado!"
fi
echo

# 6. Testar acesso local
echo ">>> [6/6] Testando acesso local..."
echo "   Testando API (localhost:5000):"
if curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/swagger 2>/dev/null | grep -q "200\|301\|302"; then
    echo "      ✅ API responde localmente"
else
    echo "      ❌ API não responde localmente"
fi

echo "   Testando Nginx (localhost:80):"
if curl -s -o /dev/null -w "%{http_code}" http://localhost 2>/dev/null | grep -q "200\|301\|302"; then
    echo "      ✅ Nginx responde localmente"
else
    echo "      ❌ Nginx não responde localmente"
fi
echo

# 7. Verificar IPs da interface
echo ">>> [7/6] IPs da interface de rede..."
ip addr show | grep -E "inet " | grep -v "127.0.0.1" || ifconfig | grep -E "inet " | grep -v "127.0.0.1"
echo

echo "======================================================="
echo " 📋 Resumo e Próximos Passos"
echo "======================================================="
echo "Se algum serviço não estiver rodando:"
echo "  systemctl start nginx"
echo "  systemctl start singleone-api"
echo ""
echo "Se o firewall estiver bloqueando:"
echo "  ufw allow 80/tcp"
echo "  ufw allow 443/tcp"
echo ""
echo "Para ver logs:"
echo "  journalctl -u nginx -f"
echo "  journalctl -u singleone-api -f"
echo "======================================================="

