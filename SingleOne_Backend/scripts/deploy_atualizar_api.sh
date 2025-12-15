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

# 3. Preservar logos antes de limpar diretório de publish
echo "🧹 Preparando limpeza do diretório de publish (preservando logos)..."
LOGOS_DIR="/opt/singleone-api-publish/wwwroot/logos"
LOGOS_TMP="/tmp/logos_before_deploy_$(date +%s)"

if [ -d "$LOGOS_DIR" ] && [ "$(ls -A "$LOGOS_DIR" 2>/dev/null)" ]; then
    echo "   ✅ Logos encontrados em $LOGOS_DIR, fazendo cópia temporária..."
    mkdir -p "$LOGOS_TMP"
    cp -a "$LOGOS_DIR" "$LOGOS_TMP/" || echo "   ⚠️  Não foi possível copiar logos (continuando mesmo assim)"
else
    echo "   ℹ️  Nenhuma logo encontrada em $LOGOS_DIR (nada para preservar)"
fi

# 4. Limpar diretório de publish
echo "🧹 Limpando diretório de publish..."
rm -rf /opt/singleone-api-publish/*

# 5. Publicar API
echo "📦 Publicando API..."
cd /opt/SingleOne/SingleOne_Backend/SingleOneAPI
dotnet publish -c Release -o /opt/singleone-api-publish

# 6. Restaurar logos após publish (se existirem)
if [ -d "$LOGOS_TMP/logos" ]; then
    echo "   ✅ Restaurando logos preservados..."
    mkdir -p /opt/singleone-api-publish/wwwroot
    cp -a "$LOGOS_TMP/logos" /opt/singleone-api-publish/wwwroot/ || echo "   ⚠️  Não foi possível restaurar logos automaticamente"
    rm -rf "$LOGOS_TMP"
else
    echo "   ℹ️  Nenhuma logo preservada para restaurar"
fi

# 7. Detectar IP do servidor
echo "🔍 Detectando IP do servidor..."
SERVER_IP=$(hostname -I | awk '{print $1}')
if [ -z "$SERVER_IP" ]; then
    echo "⚠️  Não foi possível detectar IP automaticamente"
    read -p "Digite o IP ou domínio do servidor: " SERVER_IP
fi

SITE_URL="http://${SERVER_IP}"
echo "✅ IP detectado: ${SERVER_IP}"
echo "✅ URL configurada: ${SITE_URL}"

# 8. Configurar SITE_URL no systemd
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

# 9. Recarregar systemd
echo "🔄 Recarregando systemd..."
systemctl daemon-reload

# 10. Iniciar API
echo "▶️  Iniciando serviço..."
systemctl start singleone-api

# 11. Aguardar alguns segundos
sleep 3

# 12. Verificar status
echo ""
echo "📋 Status do serviço:"
systemctl status singleone-api --no-pager -l | head -20

# 13. Mostrar logs recentes com URL
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

