#!/usr/bin/env bash

###############################################################################
# Script para corrigir git e configurar SSL em um comando
#
# Uso: sudo bash deploy/linux/fix_git_e_configurar_ssl.sh
###############################################################################

if [[ "$EUID" -ne 0 ]]; then
  echo "❌ Este script precisa ser executado como root (use: sudo $0)"
  exit 1
fi

SITE_DOMAIN="${SITE_DOMAIN:-fitbank.singleone.com.br}"

echo "======================================================="
echo " 🔧 Corrigindo Git e configurando SSL"
echo "======================================================="
echo

cd /opt/SingleOne

# Descartar mudanças locais que estão bloqueando
echo ">>> [1/3] Descartando mudanças locais..."
git checkout -- SingleOne_Backend/deploy/linux/verificar_e_completar_banco.sh 2>/dev/null || true
git reset --hard HEAD 2>/dev/null || true
echo "   ✅ Mudanças locais descartadas"
echo

# Fazer pull
echo ">>> [2/3] Atualizando repositório..."
git pull origin main
echo "   ✅ Repositório atualizado"
echo

# Executar script de SSL
echo ">>> [3/3] Configurando SSL..."
cd SingleOne_Backend
chmod +x deploy/linux/configurar_ssl_letsencrypt.sh
SITE_DOMAIN="${SITE_DOMAIN}" bash deploy/linux/configurar_ssl_letsencrypt.sh

