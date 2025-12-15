#!/usr/bin/env bash

###############################################################################
# Script para configurar SSL/HTTPS com Let's Encrypt
#
# Uso: sudo bash deploy/linux/configurar_ssl_letsencrypt.sh
#
# Pré-requisitos:
#   - Domínio apontando para o IP do servidor (DNS configurado)
#   - Porta 80 acessível de fora (para validação do Let's Encrypt)
###############################################################################

if [[ "$EUID" -ne 0 ]]; then
  echo "❌ Este script precisa ser executado como root (use: sudo $0)"
  exit 1
fi

# Solicitar domínio se não fornecido
if [[ -z "${SITE_DOMAIN:-}" ]]; then
  echo "======================================================="
  echo " 🔒 Configuração SSL/HTTPS com Let's Encrypt"
  echo "======================================================="
  echo
  read -p "Digite o domínio (ex.: fitbank.singleone.com.br): " SITE_DOMAIN
  if [[ -z "${SITE_DOMAIN}" ]]; then
    echo "❌ Domínio não fornecido. Saindo."
    exit 1
  fi
fi

echo "======================================================="
echo " 🔒 Configurando SSL para: ${SITE_DOMAIN}"
echo "======================================================="
echo

# Verificar se o domínio está apontando para este servidor
echo ">>> [1/5] Verificando DNS..."
SERVER_IP=$(curl -s ifconfig.me || curl -s icanhazip.com || echo "")
DOMAIN_IP=$(dig +short "${SITE_DOMAIN}" | tail -n 1)

if [[ -z "${DOMAIN_IP}" ]]; then
  echo "   ⚠️  Não foi possível resolver o DNS de ${SITE_DOMAIN}"
  echo "   Certifique-se de que o domínio está apontando para este servidor"
  read -p "   Continuar mesmo assim? (s/N): " CONTINUAR
  if [[ ! "${CONTINUAR}" =~ ^[Ss]$ ]]; then
    exit 1
  fi
else
  echo "   Domínio ${SITE_DOMAIN} aponta para: ${DOMAIN_IP}"
  if [[ -n "${SERVER_IP}" ]] && [[ "${DOMAIN_IP}" != "${SERVER_IP}" ]]; then
    echo "   ⚠️  AVISO: O IP do domínio (${DOMAIN_IP}) não corresponde ao IP do servidor (${SERVER_IP})"
    echo "   O Let's Encrypt pode falhar na validação"
    read -p "   Continuar mesmo assim? (s/N): " CONTINUAR
    if [[ ! "${CONTINUAR}" =~ ^[Ss]$ ]]; then
      exit 1
    fi
  else
    echo "   ✅ DNS configurado corretamente"
  fi
fi
echo

# Instalar Certbot
echo ">>> [2/5] Instalando Certbot (Let's Encrypt)..."
if ! command -v certbot >/dev/null 2>&1; then
  apt update
  apt install -y certbot python3-certbot-nginx
else
  echo "   ✅ Certbot já está instalado"
fi
echo

# Verificar se o Nginx está configurado
NGINX_CONF="/etc/nginx/sites-available/singleone"
if [[ ! -f "${NGINX_CONF}" ]]; then
  echo "❌ Arquivo de configuração Nginx não encontrado: ${NGINX_CONF}"
  echo "   Execute primeiro o script de instalação: install_singleone_full.sh"
  exit 1
fi

# Fazer backup da configuração atual
echo ">>> [3/5] Fazendo backup da configuração Nginx..."
cp "${NGINX_CONF}" "${NGINX_CONF}.backup.$(date +%Y%m%d_%H%M%S)"
echo "   ✅ Backup criado"
echo

# Atualizar configuração Nginx para incluir o domínio no server_name
echo ">>> [4/5] Atualizando configuração Nginx..."
# Verificar se o domínio já está no server_name
if ! grep -q "server_name.*${SITE_DOMAIN}" "${NGINX_CONF}"; then
  # Adicionar domínio ao server_name (manter IP também se existir)
  if grep -q "server_name.*173.249.37.16\|server_name.*_" "${NGINX_CONF}"; then
    sed -i "s/server_name.*;/server_name ${SITE_DOMAIN} 173.249.37.16 _;/" "${NGINX_CONF}"
  else
    sed -i "s/server_name.*;/server_name ${SITE_DOMAIN} _;/" "${NGINX_CONF}"
  fi
  echo "   ✅ Domínio adicionado ao server_name"
else
  echo "   ✅ Domínio já está no server_name"
fi

# Testar configuração
if nginx -t 2>&1 | grep -q "successful"; then
  echo "   ✅ Sintaxe do Nginx está OK"
  systemctl reload nginx
else
  echo "   ❌ Erro na sintaxe do Nginx:"
  nginx -t
  exit 1
fi
echo

# Obter certificado SSL
echo ">>> [5/5] Obtendo certificado SSL do Let's Encrypt..."
echo "   Isso pode levar alguns minutos..."
echo

# Usar certbot com nginx plugin (mais fácil)
certbot --nginx -d "${SITE_DOMAIN}" --non-interactive --agree-tos --email "admin@${SITE_DOMAIN}" --redirect

if [[ $? -eq 0 ]]; then
  echo
  echo ">>> [6/6] Atualizando SITE_URL no serviço da API..."
  # Atualizar SITE_URL no serviço systemd
  if [[ -f /etc/systemd/system/singleone-api.service ]]; then
    # Fazer backup
    cp /etc/systemd/system/singleone-api.service /etc/systemd/system/singleone-api.service.backup.$(date +%Y%m%d_%H%M%S)
    # Atualizar SITE_URL
    sed -i "s|Environment=SITE_URL=.*|Environment=SITE_URL=https://${SITE_DOMAIN}|" /etc/systemd/system/singleone-api.service
    systemctl daemon-reload
    systemctl restart singleone-api
    echo "   ✅ SITE_URL atualizado para https://${SITE_DOMAIN}"
  else
    echo "   ⚠️  Serviço singleone-api não encontrado"
  fi
  echo
  
  echo "======================================================="
  echo " ✅ SSL configurado com sucesso!"
  echo "======================================================="
  echo "🌐 Acesse:"
  echo "   https://${SITE_DOMAIN}"
  echo ""
  echo "📋 Informações:"
  echo "   - Certificado válido por 90 dias"
  echo "   - Renovação automática configurada"
  echo "   - Redirecionamento HTTP -> HTTPS ativo"
  echo "   - SITE_URL da API atualizado para HTTPS"
  echo ""
  echo "🔄 Para renovar manualmente:"
  echo "   certbot renew"
  echo ""
  echo "📝 Verificar renovação automática:"
  echo "   systemctl status certbot.timer"
  echo "======================================================="
else
  echo
  echo "❌ Erro ao obter certificado SSL"
  echo "   Verifique:"
  echo "   1. DNS está apontando para este servidor"
  echo "   2. Porta 80 está acessível de fora"
  echo "   3. Firewall não está bloqueando"
  echo ""
  echo "   Para tentar novamente:"
  echo "   certbot --nginx -d ${SITE_DOMAIN}"
  exit 1
fi

