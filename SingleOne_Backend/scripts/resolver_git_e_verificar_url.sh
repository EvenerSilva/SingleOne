#!/bin/bash
# ========================================
# Script para RESOLVER CONFLITOS GIT e VERIFICAR SITE_URL
# Execute: sudo bash resolver_git_e_verificar_url.sh
# ========================================

echo "=========================================="
echo "🔧 RESOLVENDO CONFLITOS GIT"
echo "=========================================="
echo ""

cd /opt/SingleOne

# 1. Verificar status
echo "📋 Verificando status do Git..."
git status --short
echo ""

# 2. Fazer backup das mudanças locais
echo "💾 Fazendo backup das mudanças locais..."
git stash push -m "Backup antes de pull $(date +%Y%m%d_%H%M%S)"
echo "✅ Mudanças locais salvas em stash"
echo ""

# 3. Fazer pull
echo "📥 Fazendo pull do repositório..."
git pull origin main
echo "✅ Pull concluído"
echo ""

# 4. Dar permissão de execução aos scripts
echo "🔐 Dando permissão de execução aos scripts..."
chmod +x /opt/SingleOne/SingleOne_Backend/scripts/*.sh
echo "✅ Permissões configuradas"
echo ""

# 5. Executar script de verificação
echo "=========================================="
echo "🔍 EXECUTANDO VERIFICAÇÃO DE SITE_URL"
echo "=========================================="
echo ""

bash /opt/SingleOne/SingleOne_Backend/scripts/verificar_e_corrigir_site_url.sh

