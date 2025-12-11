#!/bin/bash
# ========================================
# Script para configurar SITE_URL no serviço systemd
# Execute: sudo bash configurar_site_url.sh
# ========================================

SERVICE_FILE="/etc/systemd/system/singleone-api.service"
BACKUP_FILE="/etc/systemd/system/singleone-api.service.backup"

echo "=========================================="
echo "Configurar SITE_URL para SingleOne API"
echo "=========================================="
echo ""

# Verificar se o arquivo existe
if [ ! -f "$SERVICE_FILE" ]; then
    echo "❌ Arquivo de serviço não encontrado: $SERVICE_FILE"
    exit 1
fi

# Fazer backup
echo "📋 Fazendo backup do arquivo de serviço..."
cp "$SERVICE_FILE" "$BACKUP_FILE"
echo "✅ Backup criado: $BACKUP_FILE"
echo ""

# Obter IP do servidor
SERVER_IP=$(hostname -I | awk '{print $1}')
echo "🔍 IP do servidor detectado: $SERVER_IP"
echo ""

# Perguntar ao usuário qual URL usar
echo "Escolha a URL do site:"
echo "1) Usar IP do servidor: http://$SERVER_IP"
echo "2) Usar domínio personalizado"
echo "3) Manter configuração atual"
read -p "Opção (1/2/3): " opcao

case $opcao in
    1)
        SITE_URL="http://$SERVER_IP"
        ;;
    2)
        read -p "Digite o domínio completo (ex: https://seudominio.com): " SITE_URL
        ;;
    3)
        echo "✅ Mantendo configuração atual"
        exit 0
        ;;
    *)
        echo "❌ Opção inválida"
        exit 1
        ;;
esac

echo ""
echo "📝 Configurando SITE_URL=$SITE_URL"
echo ""

# Verificar se já existe SITE_URL no arquivo
if grep -q "Environment=SITE_URL" "$SERVICE_FILE"; then
    # Atualizar linha existente
    sed -i "s|Environment=SITE_URL=.*|Environment=SITE_URL=$SITE_URL|" "$SERVICE_FILE"
    echo "✅ SITE_URL atualizado no arquivo de serviço"
else
    # Adicionar nova linha após ASPNETCORE_ENVIRONMENT
    sed -i "/Environment=ASPNETCORE_ENVIRONMENT=Production/a Environment=SITE_URL=$SITE_URL" "$SERVICE_FILE"
    echo "✅ SITE_URL adicionado ao arquivo de serviço"
fi

# Remover placeholder se existir
sed -i "s|Environment=SITE_URL=http://SEU_IP_AQUI|Environment=SITE_URL=$SITE_URL|" "$SERVICE_FILE"

echo ""
echo "🔄 Recarregando systemd..."
systemctl daemon-reload

echo ""
echo "🔄 Reiniciando serviço..."
systemctl restart singleone-api

echo ""
echo "✅ Configuração aplicada!"
echo ""
echo "📋 Verificar status:"
echo "   systemctl status singleone-api"
echo ""
echo "📋 Verificar logs:"
echo "   journalctl -u singleone-api -n 50 --no-pager"
echo ""
echo "📋 Verificar URL configurada nos logs:"
echo "   journalctl -u singleone-api | grep SITE_URL"

