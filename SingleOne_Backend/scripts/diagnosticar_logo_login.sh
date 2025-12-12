#!/bin/bash

echo "=========================================="
echo "🔍 DIAGNÓSTICO: Logo no Login"
echo "=========================================="
echo ""

# 1. Verificar API sem autenticação
echo "📋 [1/5] Testando endpoint BuscarLogoCliente (sem auth)..."
echo ""
RESPONSE=$(curl -s -w "\nHTTP_CODE:%{http_code}" https://demo.singleone.com.br/api/configuracoes/buscarlogocliente)
HTTP_CODE=$(echo "$RESPONSE" | grep "HTTP_CODE" | cut -d':' -f2)
BODY=$(echo "$RESPONSE" | sed '/HTTP_CODE/d')

echo "Status HTTP: $HTTP_CODE"
echo "Resposta:"
echo "$BODY" | python3 -m json.tool 2>/dev/null || echo "$BODY"
echo ""

if [ "$HTTP_CODE" != "200" ]; then
    echo "❌ ERRO: API retornou código $HTTP_CODE (esperado 200)"
    echo "   A rota deve ter [AllowAnonymous] para funcionar no login"
    echo ""
fi

# 2. Verificar se o arquivo da logo existe
echo "📋 [2/5] Verificando arquivo físico da logo..."
LOGO_FILE=$(echo "$BODY" | grep -o 'cliente_[0-9_]*\.png' | head -1)
if [ -n "$LOGO_FILE" ]; then
    echo "Arquivo da logo na resposta: $LOGO_FILE"
    LOGO_PATH="/opt/singleone-api-publish/wwwroot/logos/$LOGO_FILE"
    if [ -f "$LOGO_PATH" ]; then
        echo "✅ Arquivo físico encontrado: $LOGO_PATH"
        ls -lh "$LOGO_PATH"
    else
        echo "❌ Arquivo físico NÃO encontrado: $LOGO_PATH"
        echo "   Listando logos disponíveis:"
        ls -lh /opt/singleone-api-publish/wwwroot/logos/cliente_* 2>/dev/null || echo "   Nenhuma logo encontrada"
    fi
else
    echo "⚠️  Nenhum arquivo de logo na resposta da API"
fi
echo ""

# 3. Testar acesso direto à logo
echo "📋 [3/5] Testando acesso HTTP à logo..."
if [ -n "$LOGO_FILE" ]; then
    LOGO_URL="https://demo.singleone.com.br/api/logos/$LOGO_FILE"
    echo "URL: $LOGO_URL"
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$LOGO_URL")
    echo "Status HTTP: $HTTP_CODE"
    if [ "$HTTP_CODE" = "200" ]; then
        echo "✅ Logo acessível via HTTP"
    else
        echo "❌ Logo NÃO acessível via HTTP (código $HTTP_CODE)"
    fi
else
    echo "⚠️  Sem URL de logo para testar"
fi
echo ""

# 4. Verificar logs da API
echo "📋 [4/5] Últimos logs da API (BuscarLogoCliente)..."
journalctl -u singleone-api -n 50 --no-pager | grep -i "BUSCAR-LOGO" | tail -10
echo ""

# 5. Verificar CORS e headers
echo "📋 [5/5] Verificando headers CORS..."
curl -s -I -H "Origin: https://demo.singleone.com.br" https://demo.singleone.com.br/api/configuracoes/buscarlogocliente | grep -i "access-control\|content-type"
echo ""

echo "=========================================="
echo "📊 RESUMO E RECOMENDAÇÕES"
echo "=========================================="

if [ "$HTTP_CODE" = "200" ] && [ -n "$LOGO_FILE" ] && [ -f "/opt/singleone-api-publish/wwwroot/logos/$LOGO_FILE" ]; then
    echo "✅ API e arquivo físico OK"
    echo ""
    echo "🔧 O problema pode estar no frontend (login.component):"
    echo "   1. Abra o navegador em https://demo.singleone.com.br"
    echo "   2. Abra DevTools (F12) e vá para Console"
    echo "   3. Procure por logs [LOGIN] e veja se há erros"
    echo "   4. Vá para Network e veja se a requisição para /api/configuracoes/buscarlogocliente foi feita"
    echo "   5. Se não aparecer no Network, o problema é no componente Angular"
    echo ""
    echo "🔧 Teste manual no console do navegador:"
    echo "   localStorage.removeItem('cliente_logo_url');"
    echo "   localStorage.removeItem('cliente_logo_timestamp');"
    echo "   location.reload();"
else
    echo "❌ Problema identificado no backend"
    echo ""
    echo "🔧 Ações necessárias:"
    if [ "$HTTP_CODE" != "200" ]; then
        echo "   - Verificar se BuscarLogoCliente tem [AllowAnonymous]"
    fi
    if [ -z "$LOGO_FILE" ]; then
        echo "   - API não está retornando logo na resposta"
    fi
    if [ -n "$LOGO_FILE" ] && [ ! -f "/opt/singleone-api-publish/wwwroot/logos/$LOGO_FILE" ]; then
        echo "   - Fazer upload da logo do cliente"
    fi
fi
echo ""

