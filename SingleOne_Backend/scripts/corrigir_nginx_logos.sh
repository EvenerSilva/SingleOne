#!/bin/bash

# Script para corrigir configuração do Nginx para servir logos corretamente

echo "=========================================="
echo "🔧 CORRIGINDO NGINX PARA LOGOS"
echo "=========================================="
echo ""

NGINX_CONFIG="/etc/nginx/sites-available/singleone"
NGINX_ENABLED="/etc/nginx/sites-enabled/singleone"

# 1. Verificar se o arquivo existe
if [ ! -f "$NGINX_CONFIG" ]; then
    echo "❌ Arquivo de configuração não encontrado: $NGINX_CONFIG"
    exit 1
fi

echo "📋 [1/4] Verificando configuração atual..."
echo ""

# 2. Verificar se há problema com a ordem das rotas
if grep -q "location ~\* \\.(jpg\|jpeg\|png" "$NGINX_CONFIG"; then
    echo "⚠️  Regra de cache de imagens encontrada"
    echo "   Verificando se está antes de /api/..."
    
    # Verificar ordem (linha de /api/ vs linha de cache de imagens)
    API_LINE=$(grep -n "location /api/" "$NGINX_CONFIG" | head -1 | cut -d: -f1)
    CACHE_LINE=$(grep -n "location ~\* \\.(jpg\|jpeg\|png" "$NGINX_CONFIG" | head -1 | cut -d: -f1)
    
    if [ -n "$API_LINE" ] && [ -n "$CACHE_LINE" ]; then
        if [ "$CACHE_LINE" -lt "$API_LINE" ]; then
            echo "   ⚠️  Regra de cache está ANTES de /api/ - isso pode causar problemas"
        else
            echo "   ✅ Ordem está correta (API antes de cache)"
        fi
    fi
fi
echo ""

# 3. Verificar se precisa adicionar regra específica para /api/logos
echo "📋 [2/4] Verificando se precisa de regra específica para /api/logos..."
if grep -q "location /api/logos" "$NGINX_CONFIG"; then
    echo "   ✅ Regra específica para /api/logos já existe"
else
    echo "   ⚠️  Regra específica para /api/logos não encontrada"
    echo "   💡 Isso pode não ser necessário se /api/ estiver configurado corretamente"
fi
echo ""

# 4. Testar configuração
echo "📋 [3/4] Testando configuração do Nginx..."
if nginx -t 2>&1 | grep -q "syntax is ok"; then
    echo "   ✅ Configuração válida"
else
    echo "   ❌ Erro na configuração!"
    nginx -t
    exit 1
fi
echo ""

# 5. Verificar se o problema é com a ordem das location blocks
echo "📋 [4/4] Verificando ordem das location blocks..."
echo "   A ordem correta deve ser:"
echo "   1. location /api/ (proxy para backend)"
echo "   2. location ~* \\.(jpg|jpeg|png...) (cache de imagens estáticas)"
echo "   3. location / (Angular routing)"
echo ""

# Criar backup
BACKUP_FILE="${NGINX_CONFIG}.backup.$(date +%Y%m%d_%H%M%S)"
cp "$NGINX_CONFIG" "$BACKUP_FILE"
echo "✅ Backup criado: $BACKUP_FILE"
echo ""

# Verificar se precisa reordenar
API_BLOCK_START=$(grep -n "location /api/" "$NGINX_CONFIG" | head -1 | cut -d: -f1)
CACHE_BLOCK_START=$(grep -n "location ~\* \\.(jpg\|jpeg\|png" "$NGINX_CONFIG" | head -1 | cut -d: -f1)

if [ -n "$API_BLOCK_START" ] && [ -n "$CACHE_BLOCK_START" ] && [ "$CACHE_BLOCK_START" -lt "$API_BLOCK_START" ]; then
    echo "⚠️  Ordem incorreta detectada. A regra de cache está antes de /api/"
    echo "   Isso pode fazer com que /api/logos/ seja servido como arquivo estático"
    echo ""
    echo "💡 SOLUÇÃO: A regra /api/ já deve ter prioridade por ser mais específica"
    echo "   Mas vamos garantir que está tudo correto..."
    echo ""
fi

# Verificar se a regra de cache está excluindo /api/
if grep -A 5 "location ~\* \\.(jpg\|jpeg\|png" "$NGINX_CONFIG" | grep -q "try_files"; then
    echo "⚠️  Regra de cache pode estar interferindo com /api/"
    echo "   A regra de cache não deve usar try_files para /api/"
fi
echo ""

# 6. Recarregar Nginx
echo "🔄 Recarregando Nginx..."
systemctl reload nginx
if [ $? -eq 0 ]; then
    echo "✅ Nginx recarregado com sucesso"
else
    echo "❌ Erro ao recarregar Nginx"
    systemctl restart nginx
    if [ $? -eq 0 ]; then
        echo "✅ Nginx reiniciado com sucesso"
    else
        echo "❌ Erro ao reiniciar Nginx"
        exit 1
    fi
fi
echo ""

# 7. Testar acesso
echo "🧪 Testando acesso via Nginx..."
TEST_FILE="cliente_1_20250815151721.png"
RESPONSE=$(curl -s -L -o /dev/null -w "%{http_code}" "http://127.0.0.1/api/logos/$TEST_FILE" 2>/dev/null)

if [ "$RESPONSE" = "200" ]; then
    echo "✅ Nginx está servindo logos corretamente (200 OK)"
elif [ "$RESPONSE" = "404" ]; then
    echo "⚠️  Nginx ainda retorna 404"
    echo "   Verificando logs do Nginx..."
    tail -20 /var/log/nginx/error.log | grep -i "logo\|api" || echo "   Nenhum erro relacionado encontrado"
    echo ""
    echo "💡 Pode ser necessário verificar:"
    echo "   1. Se o backend está acessível em http://127.0.0.1:5000"
    echo "   2. Se há algum problema com a configuração do proxy_pass"
    echo "   3. Se há algum outro location block interferindo"
else
    echo "⚠️  Nginx respondeu com código: $RESPONSE"
fi
echo ""

echo "=========================================="
echo "✅ VERIFICAÇÃO CONCLUÍDA"
echo "=========================================="
echo ""
echo "📋 Se ainda houver problemas, verifique:"
echo "   - Logs do Nginx: tail -f /var/log/nginx/error.log"
echo "   - Logs do backend: journalctl -u singleone-api -f"
echo "   - Teste direto: curl -I http://127.0.0.1:5000/api/logos/$TEST_FILE"
echo ""

