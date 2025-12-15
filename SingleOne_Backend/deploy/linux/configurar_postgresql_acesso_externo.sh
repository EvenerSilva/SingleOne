#!/usr/bin/env bash

###############################################################################
# Script para configurar PostgreSQL para aceitar conexões externas
#
# Uso: sudo bash deploy/linux/configurar_postgresql_acesso_externo.sh
#
# ATENÇÃO: Isso permite conexões externas. Use apenas em ambientes seguros
#          ou configure firewall adequadamente.
###############################################################################

if [[ "$EUID" -ne 0 ]]; then
  echo "❌ Este script precisa ser executado como root (use: sudo $0)"
  exit 1
fi

echo "======================================================="
echo " 🔧 Configurando PostgreSQL para acesso externo"
echo "======================================================="
echo

# Encontrar versão do PostgreSQL
PG_VERSION=$(sudo -u postgres psql -t -c "SHOW server_version_num;" | xargs)
PG_MAJOR_VERSION=$(echo "${PG_VERSION}" | cut -c1-2)

if [[ -z "${PG_VERSION}" ]]; then
  echo "❌ Não foi possível detectar versão do PostgreSQL"
  exit 1
fi

echo ">>> Versão detectada: ${PG_MAJOR_VERSION}"
echo

# Encontrar arquivo postgresql.conf
PG_CONF="/etc/postgresql/${PG_MAJOR_VERSION}/main/postgresql.conf"
if [[ ! -f "${PG_CONF}" ]]; then
  # Tentar caminho alternativo
  PG_CONF=$(find /etc -name "postgresql.conf" 2>/dev/null | head -n 1)
  if [[ -z "${PG_CONF}" ]]; then
    echo "❌ Arquivo postgresql.conf não encontrado"
    exit 1
  fi
fi

echo ">>> Arquivo de configuração: ${PG_CONF}"

# Encontrar arquivo pg_hba.conf
PG_HBA="/etc/postgresql/${PG_MAJOR_VERSION}/main/pg_hba.conf"
if [[ ! -f "${PG_HBA}" ]]; then
  PG_HBA=$(find /etc -name "pg_hba.conf" 2>/dev/null | head -n 1)
  if [[ -z "${PG_HBA}" ]]; then
    echo "❌ Arquivo pg_hba.conf não encontrado"
    exit 1
  fi
fi

echo ">>> Arquivo pg_hba.conf: ${PG_HBA}"
echo

# 1. Configurar postgresql.conf para escutar em todas as interfaces
echo ">>> [1/3] Configurando postgresql.conf..."

# Verificar se já está configurado
if grep -q "^listen_addresses" "${PG_CONF}"; then
  echo "   Atualizando listen_addresses..."
  sed -i "s/^listen_addresses.*/listen_addresses = '*'/" "${PG_CONF}"
else
  echo "   Adicionando listen_addresses..."
  echo "listen_addresses = '*'" >> "${PG_CONF}"
fi

echo "   ✅ listen_addresses configurado para '*'"
echo

# 2. Configurar pg_hba.conf para permitir conexões externas
echo ">>> [2/3] Configurando pg_hba.conf..."

# Verificar se já existe regra para IPv4
if grep -q "^host.*all.*all.*0.0.0.0/0" "${PG_HBA}"; then
  echo "   ⚠️  Regra para 0.0.0.0/0 já existe"
else
  echo "   Adicionando regra para conexões externas..."
  # Adicionar no final do arquivo
  echo "" >> "${PG_HBA}"
  echo "# Permitir conexões externas (adicionado automaticamente)" >> "${PG_HBA}"
  echo "host    all             all             0.0.0.0/0               md5" >> "${PG_HBA}"
  echo "   ✅ Regra adicionada"
fi

echo

# 3. Reiniciar PostgreSQL
echo ">>> [3/3] Reiniciando PostgreSQL..."
systemctl restart postgresql
sleep 2

if systemctl is-active --quiet postgresql; then
  echo "   ✅ PostgreSQL reiniciado com sucesso"
else
  echo "   ❌ Erro ao reiniciar PostgreSQL"
  echo "   Verifique os logs: journalctl -u postgresql -n 20"
  exit 1
fi

echo
echo "======================================================="
echo " ✅ Configuração concluída!"
echo "======================================================="
echo
echo "📋 Próximos passos:"
echo "   1. Verifique se a porta 5432 está aberta no firewall:"
echo "      ufw allow 5432/tcp"
echo "      # OU no firewall do provedor (Contabo, etc.)"
echo ""
echo "   2. Teste a conexão do PGAdmin:"
echo "      Host: 173.249.37.16"
echo "      Port: 5432"
echo "      Database: singleone"
echo "      User: postgres"
echo "      Password: (a senha que você configurou)"
echo ""
echo "   3. Verificar se está escutando:"
echo "      ss -tlnp | grep 5432"
echo ""
echo "⚠️  SEGURANÇA:"
echo "   - Certifique-se de que o firewall está configurado"
echo "   - Use senhas fortes"
echo "   - Considere usar VPN ou IP whitelist no pg_hba.conf"
echo "======================================================="

