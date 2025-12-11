#!/bin/bash

# Script para corrigir a logo do cliente usando o arquivo mais recente disponível

echo "=========================================="
echo "🔧 CORRIGINDO LOGO DO CLIENTE"
echo "=========================================="
echo ""

# 1. Verificar arquivos disponíveis
echo "📋 [1/4] Verificando arquivos de logo disponíveis..."
LOGOS_DIR="/opt/singleone-api-publish/wwwroot/logos"
if [ ! -d "$LOGOS_DIR" ]; then
    echo "❌ Diretório de logos não existe: $LOGOS_DIR"
    exit 1
fi

# Encontrar o arquivo mais recente do cliente 1
LATEST_FILE=$(ls -t "$LOGOS_DIR"/cliente_1_*.png 2>/dev/null | head -1)

if [ -z "$LATEST_FILE" ]; then
    echo "❌ Nenhum arquivo de logo encontrado para cliente 1"
    exit 1
fi

LATEST_FILENAME=$(basename "$LATEST_FILE")
echo "✅ Arquivo mais recente encontrado: $LATEST_FILENAME"
echo ""

# 2. Verificar qual arquivo está no banco
echo "📋 [2/4] Verificando arquivo registrado no banco de dados..."
CURRENT_LOGO=$(sudo -u postgres psql -d singleone -t -c "SELECT logo FROM clientes WHERE id = 1;" 2>/dev/null | xargs)

if [ -z "$CURRENT_LOGO" ]; then
    echo "⚠️  Nenhuma logo registrada no banco para cliente 1"
else
    echo "📋 Logo atual no banco: $CURRENT_LOGO"
    
    # Verificar se o arquivo do banco existe
    if [ -f "$LOGOS_DIR/$CURRENT_LOGO" ]; then
        echo "✅ Arquivo do banco existe: $CURRENT_LOGO"
        echo "✅ Tudo OK, não precisa atualizar"
        exit 0
    else
        echo "⚠️  Arquivo do banco NÃO existe: $CURRENT_LOGO"
    fi
fi
echo ""

# 3. Atualizar banco de dados
echo "📋 [3/4] Atualizando banco de dados com arquivo mais recente..."
UPDATE_SQL="UPDATE clientes SET logo = '$LATEST_FILENAME' WHERE id = 1;"
sudo -u postgres psql -d singleone -c "$UPDATE_SQL" 2>/dev/null

if [ $? -eq 0 ]; then
    echo "✅ Banco de dados atualizado com sucesso"
else
    echo "❌ Erro ao atualizar banco de dados"
    exit 1
fi
echo ""

# 4. Verificar atualização
echo "📋 [4/4] Verificando atualização..."
NEW_LOGO=$(sudo -u postgres psql -d singleone -t -c "SELECT logo FROM clientes WHERE id = 1;" 2>/dev/null | xargs)
echo "📋 Nova logo no banco: $NEW_LOGO"

if [ "$NEW_LOGO" = "$LATEST_FILENAME" ]; then
    echo "✅ Logo atualizada com sucesso!"
else
    echo "⚠️  Logo no banco não corresponde ao arquivo esperado"
fi
echo ""

echo "=========================================="
echo "✅ CORREÇÃO CONCLUÍDA"
echo "=========================================="
echo ""
echo "📋 Próximos passos:"
echo "   1. Teste a URL: curl -I http://127.0.0.1:5000/api/logos/$NEW_LOGO"
echo "   2. Verifique no frontend se a logo aparece"
echo ""

