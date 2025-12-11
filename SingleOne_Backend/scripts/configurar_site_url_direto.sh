#!/bin/bash
# ========================================
# Script DIRETO para configurar SITE_URL
# Execute: sudo bash configurar_site_url_direto.sh SEU_IP_OU_DOMINIO
# Exemplo: sudo bash configurar_site_url_direto.sh 185.123.45.67
# ========================================

if [ -z "$1" ]; then
    echo "❌ Erro: Forneça o IP ou domínio como argumento"
    echo "Uso: sudo bash configurar_site_url_direto.sh SEU_IP_OU_DOMINIO"
    echo "Exemplo: sudo bash configurar_site_url_direto.sh 185.123.45.67"
    exit 1
fi

SITE_URL="$1"
SERVICE_FILE="/etc/systemd/system/singleone-api.service"

# Se não começar com http, adicionar
if [[ ! "$SITE_URL" =~ ^https?:// ]]; then
    SITE_URL="http://$SITE_URL"
fi

echo "=========================================="
echo "Configurando SITE_URL=$SITE_URL"
echo "=========================================="
echo ""

# Verificar se o arquivo existe
if [ ! -f "$SERVICE_FILE" ]; then
    echo "❌ Arquivo de serviço não encontrado: $SERVICE_FILE"
    exit 1
fi

# Fazer backup
echo "📋 Fazendo backup..."
cp "$SERVICE_FILE" "${SERVICE_FILE}.backup.$(date +%Y%m%d_%H%M%S)"

# Remover linha antiga se existir
sed -i '/Environment=SITE_URL=/d' "$SERVICE_FILE"

# Adicionar nova linha após ASPNETCORE_ENVIRONMENT
sed -i "/Environment=ASPNETCORE_ENVIRONMENT=Production/a Environment=SITE_URL=$SITE_URL" "$SERVICE_FILE"

echo "✅ SITE_URL configurado: $SITE_URL"
echo ""
echo "🔄 Recarregando systemd..."
systemctl daemon-reload

echo "🔄 Reiniciando serviço..."
systemctl restart singleone-api

echo ""
echo "✅ Configuração aplicada!"
echo ""
echo "📋 Verificar nos logs:"
echo "   journalctl -u singleone-api -n 100 | grep OBTER_URL"
echo ""
echo "📋 Verificar se está funcionando:"
echo "   journalctl -u singleone-api -n 50 --no-pager | grep 'SiteUrl usado'"

