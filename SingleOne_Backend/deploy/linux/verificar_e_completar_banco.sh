#!/usr/bin/env bash

###############################################################################
# Script para verificar e completar tabelas/views faltantes no banco
#
# Uso: sudo bash deploy/linux/verificar_e_completar_banco.sh
###############################################################################

if [[ "$EUID" -ne 0 ]]; then
  echo "❌ Este script precisa ser executado como root (use: sudo $0)"
  exit 1
fi

DB_NAME="${DB_NAME:-singleone}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-Admin@2025}"

REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
if [[ "$(basename "${REPO_DIR}")" == "SingleOne_Backend" ]]; then
  REPO_DIR="$(dirname "${REPO_DIR}")"
fi

echo "======================================================="
echo " 🔍 Verificando e completando banco de dados"
echo "======================================================="
echo "Banco: ${DB_NAME}"
echo "Diretório: ${REPO_DIR}"
echo

# Contar tabelas e views existentes
TABELAS_EXISTENTES=$(PGPASSWORD="${DB_PASSWORD}" psql -h 127.0.0.1 -U "${DB_USER}" -d "${DB_NAME}" -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';" | xargs)
VIEWS_EXISTENTES=$(PGPASSWORD="${DB_PASSWORD}" psql -h 127.0.0.1 -U "${DB_USER}" -d "${DB_NAME}" -t -c "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public';" | xargs)

echo ">>> Status atual:"
echo "   Tabelas: ${TABELAS_EXISTENTES}"
echo "   Views: ${VIEWS_EXISTENTES}"
echo

# Verificar se os arquivos SQL existem
SQL_TABELAS="${REPO_DIR}/01. Criar Tabelas.sql"
SQL_VIEWS="${REPO_DIR}/02. Criar Views.sql"
SQL_TEMPLATES="${REPO_DIR}/03. Importar_templates.sql"

if [[ ! -f "${SQL_TABELAS}" ]]; then
  echo "❌ Arquivo ${SQL_TABELAS} não encontrado"
  exit 1
fi

if [[ ! -f "${SQL_VIEWS}" ]]; then
  echo "❌ Arquivo ${SQL_VIEWS} não encontrado"
  exit 1
fi

echo ">>> Reexecutando scripts SQL (com tratamento de erros)..."
echo

# Reexecutar criação de tabelas (IF NOT EXISTS garante que não duplica)
echo "   [1/3] Executando 01. Criar Tabelas.sql..."
PGPASSWORD="${DB_PASSWORD}" psql -h 127.0.0.1 -U "${DB_USER}" -d "${DB_NAME}" -f "${SQL_TABELAS}" 2>&1 | grep -v "already exists" | grep -v "NOTICE" | tail -n 20
echo "   ✅ Tabelas processadas"
echo

# Reexecutar criação de views (com tratamento de erros)
echo "   [2/3] Executando 02. Criar Views.sql..."
echo "   ⚠️  Nota: Alguns erros são esperados (views que dependem de tabelas opcionais)"
# Executar e capturar apenas erros críticos, ignorando erros de views que dependem de tabelas não existentes
PGPASSWORD="${DB_PASSWORD}" psql -h 127.0.0.1 -U "${DB_USER}" -d "${DB_NAME}" -f "${SQL_VIEWS}" 2>&1 | \
  grep -v "does not exist" | \
  grep -v "NOTICE.*does not exist, skipping" | \
  grep -E "(ERROR|CREATE VIEW|CREATE OR REPLACE VIEW|WARNING)" | \
  head -n 50
echo "   ✅ Views processadas (alguns erros podem ser ignorados)"
echo

# Reexecutar templates se existir
if [[ -f "${SQL_TEMPLATES}" ]]; then
  echo "   [3/3] Executando 03. Importar_templates.sql..."
  PGPASSWORD="${DB_PASSWORD}" psql -h 127.0.0.1 -U "${DB_USER}" -d "${DB_NAME}" -f "${SQL_TEMPLATES}" 2>&1 | tail -n 10
  echo "   ✅ Templates processados"
else
  echo "   [3/3] Arquivo 03. Importar_templates.sql não encontrado (pulando)"
fi
echo

# Contar novamente
TABELAS_FINAIS=$(PGPASSWORD="${DB_PASSWORD}" psql -h 127.0.0.1 -U "${DB_USER}" -d "${DB_NAME}" -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';" | xargs)
VIEWS_FINAIS=$(PGPASSWORD="${DB_PASSWORD}" psql -h 127.0.0.1 -U "${DB_USER}" -d "${DB_NAME}" -t -c "SELECT COUNT(*) FROM information_schema.views WHERE table_schema = 'public';" | xargs)

echo "======================================================="
echo " ✅ Verificação concluída"
echo "======================================================="
echo "Antes:"
echo "   Tabelas: ${TABELAS_EXISTENTES}"
echo "   Views: ${VIEWS_EXISTENTES}"
echo ""
echo "Depois:"
echo "   Tabelas: ${TABELAS_FINAIS}"
echo "   Views: ${VIEWS_FINAIS}"
echo ""
echo "Diferença:"
echo "   Tabelas: +$((TABELAS_FINAIS - TABELAS_EXISTENTES))"
echo "   Views: +$((VIEWS_FINAIS - VIEWS_EXISTENTES))"
echo "======================================================="
echo
echo "📋 Para ver todas as tabelas:"
echo "   sudo -u postgres psql -d ${DB_NAME} -c \"\\dt\""
echo ""
echo "📋 Para ver todas as views:"
echo "   sudo -u postgres psql -d ${DB_NAME} -c \"\\dv\""
echo "======================================================="

