#!/usr/bin/env bash

###############################################################################
# Script para verificar logs de login da API
#
# Uso: sudo bash deploy/linux/verificar_logs_login.sh
###############################################################################

echo "======================================================="
echo " 🔍 Verificando logs de login da API"
echo "======================================================="
echo
echo ">>> Últimos logs da API (últimas 50 linhas):"
echo
journalctl -u singleone-api -n 50 --no-pager | grep -E "(LOGIN|Login|ERRO|ERROR|401|Unauthorized|twoFactor|2FA)" -i
echo
echo "======================================================="
echo " 📋 Para ver logs em tempo real:"
echo "   journalctl -u singleone-api -f"
echo "======================================================="

