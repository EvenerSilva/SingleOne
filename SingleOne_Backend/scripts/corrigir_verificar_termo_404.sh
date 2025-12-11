#!/bin/bash

# Script completo para corrigir o erro 404 em /verificar-termo
# Este script:
# 1. Atualiza o código do Git
# 2. Rebuilda o frontend
# 3. Corrige a configuração do Nginx
# 4. Recarrega o Nginx

echo "=========================================="
echo "🔧 CORRIGINDO ERRO 404 EM /verificar-termo"
echo "=========================================="
echo ""

# 1. Atualizar código
echo "📥 Atualizando código do Git..."
cd /opt/SingleOne
git stash
git pull origin main
if [ $? -ne 0 ]; then
    echo "❌ Erro ao fazer pull do Git!"
    exit 1
fi
echo "✅ Código atualizado!"
echo ""

# 2. Rebuildar frontend
echo "🔨 Rebuildando frontend..."
cd /opt/SingleOne/SingleOne_Frontend
npm install
if [ $? -ne 0 ]; then
    echo "❌ Erro ao instalar dependências!"
    exit 1
fi

npm run build-prod
if [ $? -ne 0 ]; then
    echo "❌ Erro ao buildar frontend!"
    exit 1
fi
echo "✅ Frontend rebuildado!"
echo ""

# 3. Corrigir Nginx
echo "🔧 Corrigindo configuração do Nginx..."
chmod +x /opt/SingleOne/SingleOne_Backend/scripts/corrigir_nginx_rotas_publicas.sh
bash /opt/SingleOne/SingleOne_Backend/scripts/corrigir_nginx_rotas_publicas.sh
if [ $? -ne 0 ]; then
    echo "❌ Erro ao corrigir Nginx!"
    exit 1
fi
echo "✅ Nginx corrigido!"
echo ""

# 4. Verificar se o index.html existe
echo "📋 Verificando arquivos do frontend..."
if [ ! -f "/opt/SingleOne/SingleOne_Frontend/dist/SingleOne/index.html" ]; then
    echo "❌ index.html não encontrado em /opt/SingleOne/SingleOne_Frontend/dist/SingleOne/"
    echo "⚠️  O build pode ter falhado ou o diretório está incorreto!"
    exit 1
fi
echo "✅ index.html encontrado!"
echo ""

# 5. Verificar permissões
echo "🔐 Verificando permissões..."
chmod -R 755 /opt/SingleOne/SingleOne_Frontend/dist/SingleOne
echo "✅ Permissões ajustadas!"
echo ""

echo "=========================================="
echo "✅ CORREÇÃO CONCLUÍDA!"
echo "=========================================="
echo ""
echo "🧪 Teste acessando:"
echo "   http://84.247.128.180/verificar-termo/974357ad-0b41-4bfa-a143-154288325fda"
echo ""
echo "📋 Se ainda der erro 404, verifique:"
echo "   1. Logs do Nginx: sudo journalctl -u nginx -n 50"
echo "   2. Configuração do Nginx: sudo nginx -t"
echo "   3. Arquivos do frontend: ls -la /opt/SingleOne/SingleOne_Frontend/dist/SingleOne/"
echo ""

