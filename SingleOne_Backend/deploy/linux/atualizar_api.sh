#!/usr/bin/env bash

###############################################################################
# Script para atualizar a API SingleOne (recompilar e reiniciar)
#
# Uso: sudo bash deploy/linux/atualizar_api.sh
###############################################################################

if [[ "$EUID" -ne 0 ]]; then
  echo "❌ Este script precisa ser executado como root (use: sudo $0)"
  exit 1
fi

REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
if [[ "$(basename "${REPO_DIR}")" == "SingleOne_Backend" ]]; then
  REPO_DIR="$(dirname "${REPO_DIR}")"
fi

API_DIR="${REPO_DIR}/SingleOne_Backend/SingleOneAPI"
PUBLISH_DIR="/opt/singleone-api-publish"

if [[ ! -d "${API_DIR}" ]]; then
  API_DIR="${REPO_DIR}/SingleOneAPI"
  if [[ ! -d "${API_DIR}" ]]; then
    echo "❌ Diretório SingleOneAPI não encontrado"
    exit 1
  fi
fi

echo "======================================================="
echo " 🔄 Atualizando API SingleOne"
echo "======================================================="
echo "Diretório da API: ${API_DIR}"
echo "Diretório de publicação: ${PUBLISH_DIR}"
echo

# 1. Parar o serviço
echo ">>> [1/4] Parando serviço singleone-api..."
systemctl stop singleone-api
sleep 2
echo "   ✅ Serviço parado"
echo

# 2. Atualizar código (git pull)
echo ">>> [2/4] Atualizando código do repositório..."
cd "${REPO_DIR}"
git pull origin main
echo "   ✅ Código atualizado"
echo

# 3. Publicar API
echo ">>> [3/4] Compilando e publicando API..."
cd "${API_DIR}"
dotnet publish -c Release -o "${PUBLISH_DIR}"
if [[ $? -eq 0 ]]; then
  echo "   ✅ API publicada com sucesso"
else
  echo "   ❌ Erro ao publicar API"
  systemctl start singleone-api
  exit 1
fi
echo

# 4. Reiniciar serviço
echo ">>> [4/4] Reiniciando serviço singleone-api..."
systemctl start singleone-api
sleep 2

if systemctl is-active --quiet singleone-api; then
  echo "   ✅ Serviço reiniciado com sucesso"
  systemctl status singleone-api --no-pager -l | head -n 10
else
  echo "   ❌ Erro ao reiniciar serviço"
  echo "   Verifique os logs: journalctl -u singleone-api -n 50"
  exit 1
fi

echo
echo "======================================================="
echo " ✅ Atualização concluída!"
echo "======================================================="
echo "Para verificar logs:"
echo "   journalctl -u singleone-api -f"
echo "======================================================="

