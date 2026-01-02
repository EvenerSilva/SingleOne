#!/usr/bin/env bash

###############################################################################
# Script para verificar se as views necessárias para KPIs existem
#
# Uso: sudo bash deploy/linux/verificar_kpis_dashboard.sh
###############################################################################

DB_NAME="${DB_NAME:-singleone}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-Admin@2025}"

echo "======================================================="
echo " 🔍 Verificando views necessárias para KPIs"
echo "======================================================="
echo

# Views necessárias para os KPIs
VIEWS_REQUERIDAS=(
    "vwequipamentosdetalhes"
    "vwequipamentosstatus"
    "vwequipamentoscomcolaboradoresdesligados"
    "vwdevolucaoprogramadum"
    "vw_nao_conformidade_elegibilidade"
    "requisicoesvm"
    "requisicaoequipamentosvm"
    "colaboradoresvm"
    "equipamentovm"
)

echo ">>> Verificando views no banco..."
echo

VIEWS_EXISTENTES=0
VIEWS_FALTANDO=()

for view in "${VIEWS_REQUERIDAS[@]}"; do
    COUNT=$(PGPASSWORD="${DB_PASSWORD}" psql -h 127.0.0.1 -U "${DB_USER}" -d "${DB_NAME}" -t -c "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public' AND table_name = '${view}';" | xargs)
    
    if [[ "${COUNT}" == "1" ]]; then
        echo "   ✅ ${view}"
        ((VIEWS_EXISTENTES++))
    else
        echo "   ❌ ${view} (NÃO EXISTE)"
        VIEWS_FALTANDO+=("${view}")
    fi
done

echo
echo "======================================================="
echo " 📊 Resumo"
echo "======================================================="
echo "Views existentes: ${VIEWS_EXISTENTES}/${#VIEWS_REQUERIDAS[@]}"
echo

if [[ ${#VIEWS_FALTANDO[@]} -gt 0 ]]; then
    echo "⚠️  Views faltando:"
    for view in "${VIEWS_FALTANDO[@]}"; do
        echo "   - ${view}"
    done
    echo
    echo "💡 Execute o script para completar o banco:"
    echo "   sudo bash deploy/linux/verificar_e_completar_banco.sh"
else
    echo "✅ Todas as views necessárias existem!"
fi

echo
echo ">>> Verificando logs da API para erros de KPIs..."
echo
journalctl -u singleone-api -n 100 --no-pager | grep -E "(DASHBOARD|KPI|ERROR|42703|does not exist)" -i | tail -n 20

echo
echo "======================================================="

