#!/bin/bash
# ========================================
# Script de DEPLOY COMPLETO da API
# Atualiza código, publica e configura SITE_URL automaticamente
# Execute: sudo bash deploy_atualizar_api.sh
# ========================================

set -e  # Parar em caso de erro

echo "=========================================="
echo "🚀 DEPLOY COMPLETO - SingleOne API"
echo "=========================================="
echo ""

# 1. Parar a API
echo "⏹️  Parando serviço..."
systemctl stop singleone-api || true
sleep 2

# 2. Atualizar código do Git
echo "📥 Atualizando código do Git..."
cd /opt/SingleOne
git pull origin main

# 3. Limpar diretório de publish
echo "🧹 Limpando diretório de publish..."
rm -rf /opt/singleone-api-publish/*

# 4. Publicar API
echo "📦 Publicando API..."
cd /opt/SingleOne/SingleOne_Backend/SingleOneAPI
dotnet publish -c Release -o /opt/singleone-api-publish

# 5. Detectar IP do servidor
echo "🔍 Detectando IP do servidor..."
SERVER_IP=$(hostname -I | awk '{print $1}')
if [ -z "$SERVER_IP" ]; then
    echo "⚠️  Não foi possível detectar IP automaticamente"
    read -p "Digite o IP ou domínio do servidor: " SERVER_IP
fi

SITE_URL="http://${SERVER_IP}"
echo "✅ IP detectado: ${SERVER_IP}"
echo "✅ URL configurada: ${SITE_URL}"

# 6. Configurar SITE_URL no systemd
echo "⚙️  Configurando SITE_URL no systemd..."
SERVICE_FILE="/etc/systemd/system/singleone-api.service"

# Fazer backup
cp "$SERVICE_FILE" "${SERVICE_FILE}.backup.$(date +%Y%m%d_%H%M%S)"

# Remover TODAS as linhas antigas de SITE_URL (incluindo placeholders)
sed -i '/Environment=SITE_URL=/d' "$SERVICE_FILE"

# Adicionar nova linha após ASPNETCORE_ENVIRONMENT
sed -i "/Environment=ASPNETCORE_ENVIRONMENT=Production/a Environment=SITE_URL=$SITE_URL" "$SERVICE_FILE"

echo "✅ SITE_URL configurado: $SITE_URL"

# Verificar se foi aplicado corretamente
if grep -q "Environment=SITE_URL=$SITE_URL" "$SERVICE_FILE"; then
    echo "✅ Verificação: SITE_URL encontrado no arquivo de serviço"
else
    echo "⚠️  AVISO: SITE_URL não foi encontrado no arquivo após configuração!"
    echo "📋 Conteúdo do arquivo:"
    grep "SITE_URL" "$SERVICE_FILE" || echo "   (nenhuma linha SITE_URL encontrada)"
fi

# 7. Recarregar systemd
echo "🔄 Recarregando systemd..."
systemctl daemon-reload

# 8. Iniciar API
echo "▶️  Iniciando serviço..."
systemctl start singleone-api

# 9. Aguardar alguns segundos
sleep 3

# 10. Verificar status
echo ""
echo "📋 Status do serviço:"
systemctl status singleone-api --no-pager -l | head -20

# 11. Mostrar logs recentes com URL
echo ""
echo "=========================================="
echo "📋 Logs de detecção de URL:"
echo "=========================================="
journalctl -u singleone-api -n 50 --no-pager | grep -E "OBTER_URL|SiteUrl usado|SITE_URL" || echo "Nenhum log de URL encontrado ainda. Aguarde alguns segundos e execute: journalctl -u singleone-api -n 100 | grep OBTER_URL"

echo ""
echo "=========================================="
echo "✅ DEPLOY CONCLUÍDO!"
echo "=========================================="
echo ""
echo "📋 Para ver todos os logs:"
echo "   journalctl -u singleone-api -f"
echo ""
echo "📋 Para verificar URL detectada:"
echo "   journalctl -u singleone-api -n 100 | grep OBTER_URL"
echo ""

