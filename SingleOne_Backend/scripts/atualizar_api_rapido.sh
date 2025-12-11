#!/bin/bash

# Script rápido para atualizar apenas a API (sem rebuild completo)

echo "=========================================="
echo "🔄 ATUALIZANDO API"
echo "=========================================="
echo ""

# 1. Parar a API antes de publicar
echo "⏹️  [1/5] Parando serviço da API..."
systemctl stop singleone-api
sleep 2
echo "✅ Serviço parado"
echo ""

# 2. Ir para o diretório do projeto
cd /opt/SingleOne/SingleOne_Backend/SingleOneAPI

# 3. Fazer pull das mudanças
echo "📥 [2/5] Atualizando código do repositório..."
git pull
if [ $? -ne 0 ]; then
    echo "❌ Erro ao fazer pull do repositório!"
    systemctl start singleone-api
    exit 1
fi
echo "✅ Código atualizado"
echo ""

# 4. Compilar o projeto
echo "🔨 [3/5] Compilando projeto..."
dotnet build -c Release
if [ $? -ne 0 ]; then
    echo "❌ Erro na compilação!"
    systemctl start singleone-api
    exit 1
fi
echo "✅ Compilação concluída"
echo ""

# 5. Publicar para o diretório de produção
echo "📦 [4/5] Publicando para produção..."
# Limpar arquivos que podem estar em uso
rm -f /opt/singleone-api-publish/*.pdb
rm -f /opt/singleone-api-publish/*.dll
sleep 1
dotnet publish -c Release -o /opt/singleone-api-publish
if [ $? -ne 0 ]; then
    echo "❌ Erro na publicação!"
    systemctl start singleone-api
    exit 1
fi
echo "✅ Publicação concluída"
echo ""

# 6. Reiniciar o serviço
echo "🔄 [5/5] Reiniciando serviço da API..."
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

