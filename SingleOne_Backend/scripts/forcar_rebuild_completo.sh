#!/bin/bash

echo "🔄 Forçando rebuild completo da API..."

cd /opt/SingleOne/SingleOne_Backend/SingleOneAPI

# 1. Clean completo
echo "🧹 Limpando build anterior..."
dotnet clean

# 2. Rebuild
echo "🔨 Recompilando..."
dotnet build

# 3. Reiniciar serviço
echo "🔄 Reiniciando serviço..."
sudo systemctl restart singleone-api

# 4. Aguardar inicialização
echo "⏳ Aguardando inicialização (5 segundos)..."
sleep 5

# 5. Verificar se iniciou
echo ""
echo "✅ Status do serviço:"
sudo systemctl status singleone-api --no-pager | head -10

echo ""
echo "📋 Últimas linhas do log:"
journalctl -u singleone-api -n 15 --no-pager | grep -E "Started|Application started|CONTESTACOES"

echo ""
echo "✅ Rebuild completo finalizado!"
echo "Agora teste enviando um inventário forçado e verifique os logs com:"
echo "journalctl -u singleone-api -f | grep CONTESTACOES"

