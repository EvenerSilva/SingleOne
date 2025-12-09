#!/bin/bash
# =====================================================
# SCRIPT PARA ATUALIZAR OS SCRIPTS DE BANCO
# =====================================================
# Este script faz stash das mudanças locais e atualiza do repositório

cd /opt/SingleOne

echo "🔄 Atualizando scripts de banco..."
echo ""

# Fazer stash das mudanças locais
echo "📦 Fazendo stash das mudanças locais..."
git stash push -m "Stash antes de atualizar scripts de banco" recriar_banco_contabo.sh verificar_banco_contabo.sh 2>/dev/null || true

# Fazer pull
echo "⬇️  Fazendo pull do repositório..."
git pull origin main

# Restaurar permissões
chmod +x recriar_banco_contabo.sh verificar_banco_contabo.sh 2>/dev/null || true

echo ""
echo "✅ Scripts atualizados com sucesso!"
echo ""
echo "Agora você pode executar:"
echo "  ./verificar_banco_contabo.sh"
echo "  ./recriar_banco_contabo.sh"

