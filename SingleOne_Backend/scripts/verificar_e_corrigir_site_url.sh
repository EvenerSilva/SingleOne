#!/bin/bash
# ========================================
# Script para VERIFICAR e CORRIGIR SITE_URL
# Execute: sudo bash verificar_e_corrigir_site_url.sh
# ========================================

SERVICE_FILE="/etc/systemd/system/singleone-api.service"

echo "=========================================="
echo "🔍 VERIFICAÇÃO E CORREÇÃO DE SITE_URL"
echo "=========================================="
echo ""

# 1. Verificar arquivo de serviço
echo "📋 Verificando arquivo de serviço..."
if [ ! -f "$SERVICE_FILE" ]; then
    echo "❌ Arquivo de serviço não encontrado: $SERVICE_FILE"
    exit 1
fi

echo "✅ Arquivo encontrado: $SERVICE_FILE"
echo ""

# 2. Mostrar linhas SITE_URL atuais
echo "📋 Linhas SITE_URL atuais no arquivo:"
grep -n "SITE_URL" "$SERVICE_FILE" || echo "   (nenhuma linha encontrada)"
echo ""

# 3. Verificar variável no systemd
echo "📋 Variável SITE_URL no systemd (processo atual):"
systemctl show singleone-api | grep SITE_URL || echo "   (variável não encontrada)"
echo ""

# 4. Detectar IP
echo "🔍 Detectando IP do servidor..."
SERVER_IP=$(hostname -I | awk '{print $1}')
if [ -z "$SERVER_IP" ]; then
    echo "⚠️  Não foi possível detectar IP automaticamente"
    read -p "Digite o IP ou domínio do servidor: " SERVER_IP
fi

SITE_URL="http://${SERVER_IP}"
echo "✅ IP detectado: ${SERVER_IP}"
echo "✅ URL a configurar: ${SITE_URL}"
echo ""

# 5. Perguntar se deseja corrigir
read -p "Deseja corrigir o arquivo de serviço? (s/n): " -n 1 -r
echo ""
if [[ ! $REPLY =~ ^[Ss]$ ]]; then
    echo "❌ Operação cancelada."
    exit 0
fi

# 6. Fazer backup
echo "💾 Fazendo backup..."
cp "$SERVICE_FILE" "${SERVICE_FILE}.backup.$(date +%Y%m%d_%H%M%S)"
echo "✅ Backup criado"
echo ""

# 7. Remover TODAS as linhas SITE_URL
echo "🧹 Removendo linhas antigas de SITE_URL..."
sed -i '/Environment=SITE_URL=/d' "$SERVICE_FILE"
echo "✅ Linhas antigas removidas"
echo ""

# 8. Adicionar nova linha
echo "➕ Adicionando nova linha SITE_URL..."
sed -i "/Environment=ASPNETCORE_ENVIRONMENT=Production/a Environment=SITE_URL=$SITE_URL" "$SERVICE_FILE"
echo "✅ Nova linha adicionada"
echo ""

# 9. Verificar resultado
echo "📋 Verificando resultado..."
if grep -q "Environment=SITE_URL=$SITE_URL" "$SERVICE_FILE"; then
    echo "✅ SITE_URL configurado corretamente!"
    echo ""
    echo "📋 Linha no arquivo:"
    grep "SITE_URL" "$SERVICE_FILE"
    echo ""
else
    echo "❌ ERRO: SITE_URL não foi configurado corretamente!"
    exit 1
fi

# 10. Recarregar systemd
echo "🔄 Recarregando systemd..."
systemctl daemon-reload
echo "✅ Systemd recarregado"
echo ""

# 11. Reiniciar serviço
echo "🔄 Reiniciando serviço..."
systemctl restart singleone-api
sleep 3
echo "✅ Serviço reiniciado"
echo ""

# 12. Verificar variável novamente
echo "📋 Verificando variável após reinício:"
systemctl show singleone-api | grep SITE_URL || echo "   (variável não encontrada)"
echo ""

# 13. Mostrar logs
echo "=========================================="
echo "📋 Logs de STARTUP (últimas 20 linhas):"
echo "=========================================="
journalctl -u singleone-api -n 20 --no-pager | grep -E "STARTUP|SITE_URL|OBTER_URL" || echo "Nenhum log relevante encontrado"
echo ""

echo "=========================================="
echo "✅ VERIFICAÇÃO CONCLUÍDA!"
echo "=========================================="
echo ""
echo "📋 Para ver todos os logs:"
echo "   journalctl -u singleone-api -f"
echo ""

