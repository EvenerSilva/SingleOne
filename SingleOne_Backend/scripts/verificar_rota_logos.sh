#!/bin/bash

# Script para verificar se a rota de logos está funcionando

echo "=========================================="
echo "🔍 VERIFICANDO ROTA DE LOGOS"
echo "=========================================="
echo ""

# 1. Verificar se o backend está rodando
echo "📋 [1/5] Verificando se o backend está rodando..."
if systemctl is-active --quiet singleone-api; then
    echo "✅ Backend está rodando"
else
    echo "❌ Backend NÃO está rodando!"
    echo "   Execute: sudo systemctl start singleone-api"
    exit 1
fi
echo ""

# 2. Verificar se a porta 5000 está escutando
echo "📋 [2/5] Verificando se a porta 5000 está escutando..."
# Tentar usar 'ss' primeiro (mais moderno), depois 'netstat', depois verificar via systemctl
if command -v ss >/dev/null 2>&1; then
    if ss -tuln | grep -q ":5000"; then
        echo "✅ Porta 5000 está escutando (verificado via ss)"
    else
        echo "⚠️  Porta 5000 não encontrada via ss"
        # Verificar via systemctl se o serviço está ativo
        if systemctl is-active --quiet singleone-api; then
            echo "   ℹ️  Mas o serviço está ativo, pode estar iniciando..."
        else
            echo "❌ Porta 5000 NÃO está escutando e serviço não está ativo!"
            exit 1
        fi
    fi
elif command -v netstat >/dev/null 2>&1; then
    if netstat -tuln | grep -q ":5000"; then
        echo "✅ Porta 5000 está escutando (verificado via netstat)"
    else
        echo "⚠️  Porta 5000 não encontrada via netstat"
        if systemctl is-active --quiet singleone-api; then
            echo "   ℹ️  Mas o serviço está ativo, pode estar iniciando..."
        else
            echo "❌ Porta 5000 NÃO está escutando e serviço não está ativo!"
            exit 1
        fi
    fi
else
    # Se nem ss nem netstat estão disponíveis, verificar via systemctl e curl
    echo "⚠️  ss e netstat não disponíveis, verificando via systemctl e curl..."
    if systemctl is-active --quiet singleone-api; then
        echo "   ✅ Serviço está ativo"
        # Tentar fazer uma requisição de teste
        if curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1:5000/api/values >/dev/null 2>&1; then
            echo "✅ Backend responde na porta 5000 (verificado via curl)"
        else
            echo "⚠️  Backend não responde na porta 5000, mas serviço está ativo"
        fi
    else
        echo "❌ Serviço não está ativo!"
        exit 1
    fi
fi
echo ""

# 3. Testar acesso direto ao backend (sem Nginx)
echo "📋 [3/5] Testando acesso direto ao backend..."
RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1:5000/api/logos/cliente_1_20251211221041.png 2>/dev/null)
if [ "$RESPONSE" = "200" ]; then
    echo "✅ Backend respondeu com 200 OK"
elif [ "$RESPONSE" = "404" ]; then
    echo "⚠️  Backend respondeu com 404 - arquivo não encontrado"
    echo "   Verificando se o arquivo existe..."
    if [ -f "/opt/singleone-api-publish/wwwroot/logos/cliente_1_20251211221041.png" ]; then
        echo "   ✅ Arquivo existe em: /opt/singleone-api-publish/wwwroot/logos/cliente_1_20251211221041.png"
        echo "   ⚠️  Problema pode ser na rota do controller"
    else
        echo "   ❌ Arquivo NÃO existe!"
        echo "   📁 Verificando diretório de logos..."
        if [ -d "/opt/singleone-api-publish/wwwroot/logos" ]; then
            echo "   ✅ Diretório existe"
            echo "   📋 Arquivos no diretório:"
            ls -la /opt/singleone-api-publish/wwwroot/logos/ | head -10
        else
            echo "   ❌ Diretório NÃO existe!"
        fi
    fi
else
    echo "❌ Backend respondeu com código: $RESPONSE"
fi
echo ""

# 4. Testar acesso via Nginx
echo "📋 [4/5] Testando acesso via Nginx..."
NGINX_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1/api/logos/cliente_1_20251211221041.png 2>/dev/null)
if [ "$NGINX_RESPONSE" = "200" ]; then
    echo "✅ Nginx respondeu com 200 OK"
elif [ "$NGINX_RESPONSE" = "404" ]; then
    echo "⚠️  Nginx respondeu com 404"
    echo "   Verificando configuração do Nginx..."
    if grep -q "location /api/" /etc/nginx/sites-available/singleone; then
        echo "   ✅ Configuração /api/ encontrada no Nginx"
    else
        echo "   ❌ Configuração /api/ NÃO encontrada no Nginx!"
    fi
else
    echo "❌ Nginx respondeu com código: $NGINX_RESPONSE"
fi
echo ""

# 5. Verificar logs do backend
echo "📋 [5/5] Últimas linhas dos logs do backend relacionados a logos:"
journalctl -u singleone-api -n 50 --no-pager | grep -i "logo\|GET-LOGO" | tail -10
echo ""

echo "=========================================="
echo "✅ VERIFICAÇÃO CONCLUÍDA"
echo "=========================================="
echo ""
echo "📋 Comandos úteis:"
echo "   - Ver logs do backend: journalctl -u singleone-api -f"
echo "   - Testar URL diretamente: curl -I http://127.0.0.1:5000/api/logos/cliente_1_20251211221041.png"
echo "   - Verificar arquivo: ls -la /opt/singleone-api-publish/wwwroot/logos/"
echo ""

