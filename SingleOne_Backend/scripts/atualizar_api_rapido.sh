#!/bin/bash

# Script rápido para atualizar apenas a API (sem rebuild completo)

echo "=========================================="
echo "🔄 ATUALIZANDO API"
echo "=========================================="
echo ""

# 1. Ir para o diretório do projeto
cd /opt/SingleOne/SingleOne_Backend/SingleOneAPI

# 2. Fazer pull das mudanças
echo "📥 [1/4] Atualizando código do repositório..."
git pull
if [ $? -ne 0 ]; then
    echo "❌ Erro ao fazer pull do repositório!"
    exit 1
fi
echo "✅ Código atualizado"
echo ""

# 3. Compilar o projeto
echo "🔨 [2/4] Compilando projeto..."
dotnet build -c Release
if [ $? -ne 0 ]; then
    echo "❌ Erro na compilação!"
    exit 1
fi
echo "✅ Compilação concluída"
echo ""

# 4. Publicar para o diretório de produção
echo "📦 [3/4] Publicando para produção..."
dotnet publish -c Release -o /opt/singleone-api-publish
if [ $? -ne 0 ]; then
    echo "❌ Erro na publicação!"
    exit 1
fi
echo "✅ Publicação concluída"
echo ""

# 5. Reiniciar o serviço
echo "🔄 [4/4] Reiniciando serviço da API..."
systemctl restart singleone-api
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

