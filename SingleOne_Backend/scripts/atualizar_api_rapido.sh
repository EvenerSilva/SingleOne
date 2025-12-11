#!/bin/bash

# Script rápido para atualizar apenas a API (sem rebuild completo)

echo "=========================================="
echo "🔄 ATUALIZANDO API"
echo "=========================================="
echo ""

# 0. Resolver conflitos Git se houver
echo "📋 [0/6] Resolvendo conflitos Git..."
cd /opt/SingleOne
if [ -n "$(git status --porcelain)" ]; then
    echo "   ⚠️  Mudanças locais detectadas, fazendo stash..."
    git stash push -m "Stash automático antes de atualizar API - $(date +%Y%m%d_%H%M%S)"
    echo "   ✅ Mudanças locais salvas em stash"
fi
echo ""

# 1. Parar a API antes de publicar
echo "⏹️  [1/6] Parando serviço da API..."
systemctl stop singleone-api
sleep 3

# Matar processos que possam estar usando os arquivos
echo "   🔍 Verificando processos usando arquivos..."
lsof +D /opt/singleone-api-publish 2>/dev/null | grep -v COMMAND | awk '{print $2}' | sort -u | while read pid; do
    if [ ! -z "$pid" ] && [ "$pid" != "$$" ]; then
        echo "   ⚠️  Matando processo $pid que está usando arquivos..."
        kill -9 $pid 2>/dev/null || true
    fi
done
sleep 2
echo "✅ Serviço parado e processos limpos"
echo ""

# 2. Ir para o diretório do projeto e atualizar código
cd /opt/SingleOne/SingleOne_Backend/SingleOneAPI

echo "📥 [2/6] Atualizando código do repositório..."
cd /opt/SingleOne
git pull
if [ $? -ne 0 ]; then
    echo "❌ Erro ao fazer pull do repositório!"
    systemctl start singleone-api
    exit 1
fi
echo "✅ Código atualizado"
echo ""

# 3. Compilar o projeto
cd /opt/SingleOne/SingleOne_Backend/SingleOneAPI
echo "🔨 [3/6] Compilando projeto..."
dotnet build -c Release
if [ $? -ne 0 ]; then
    echo "❌ Erro na compilação!"
    systemctl start singleone-api
    exit 1
fi
echo "✅ Compilação concluída"
echo ""

# 4. Limpar completamente o diretório de publicação
echo "🧹 [4/6] Limpando diretório de publicação..."
# Mover para um backup temporário em vez de deletar
BACKUP_DIR="/opt/singleone-api-publish-backup-$(date +%Y%m%d_%H%M%S)"
if [ -d "/opt/singleone-api-publish" ]; then
    mv /opt/singleone-api-publish "$BACKUP_DIR" 2>/dev/null || {
        # Se mover falhar, tentar deletar arquivo por arquivo
        echo "   ⚠️  Não foi possível mover diretório, limpando arquivos..."
        find /opt/singleone-api-publish -type f -name "*.pdb" -delete 2>/dev/null || true
        find /opt/singleone-api-publish -type f -name "*.dll" -delete 2>/dev/null || true
        find /opt/singleone-api-publish -type f -name "*.exe" -delete 2>/dev/null || true
        sleep 2
    }
fi

# Recriar diretório se não existir
mkdir -p /opt/singleone-api-publish
sleep 1
echo "✅ Diretório limpo"
echo ""

# 5. Publicar para o diretório de produção
echo "📦 [5/6] Publicando para produção..."
dotnet publish -c Release -o /opt/singleone-api-publish
if [ $? -ne 0 ]; then
    echo "❌ Erro na publicação!"
    # Restaurar backup se houver
    if [ -d "$BACKUP_DIR" ]; then
        rm -rf /opt/singleone-api-publish
        mv "$BACKUP_DIR" /opt/singleone-api-publish
    fi
    systemctl start singleone-api
    exit 1
fi
echo "✅ Publicação concluída"
echo ""

# Limpar backup antigo se publicação foi bem-sucedida
if [ -d "$BACKUP_DIR" ]; then
    rm -rf "$BACKUP_DIR"
fi

# 6. Reiniciar o serviço
echo "🔄 [6/6] Reiniciando serviço da API..."
systemctl start singleone-api
sleep 3

if systemctl is-active --quiet singleone-api; then
    echo "✅ API reiniciada com sucesso"
else
    echo "❌ Erro ao reiniciar API!"
    echo "📋 Logs:"
    journalctl -u singleone-api -n 20 --no-pager
    exit 1
fi
echo ""

# 6. Verificar status
echo "📋 Status final:"
systemctl status singleone-api --no-pager | head -10
echo ""

echo "=========================================="
echo "✅ ATUALIZAÇÃO CONCLUÍDA"
echo "=========================================="
echo ""
echo "📋 Para ver logs em tempo real:"
echo "   journalctl -u singleone-api -f"
echo ""

