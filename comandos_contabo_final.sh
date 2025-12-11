#!/bin/bash

# Comandos para atualizar o SingleOne no Contabo
# Caminho: /opt/SingleOne

echo "📂 Navegando para /opt/SingleOne..."
cd /opt/SingleOne || exit 1

echo "📂 Diretório atual: $(pwd)"
echo ""

# Verificar se está no diretório correto
if [ -d "SingleOne_Backend" ]; then
    echo "✅ Diretório SingleOne_Backend encontrado!"
    cd SingleOne_Backend || exit 1
    echo "📂 Agora em: $(pwd)"
    echo ""
fi

# Verificar se tem arquivo .csproj
if [ -f "SingleOneAPI.csproj" ]; then
    echo "✅ Arquivo SingleOneAPI.csproj encontrado!"
    echo ""
    echo "🔨 Recompilando o projeto..."
    dotnet build
    
    if [ $? -eq 0 ]; then
        echo ""
        echo "✅ Build concluído com sucesso!"
        echo ""
        echo "🔄 Próximos passos para reiniciar o serviço:"
        echo ""
        echo "# Opção 1 - Systemd:"
        echo "sudo systemctl restart singleone-api"
        echo "# ou"
        echo "sudo systemctl restart singleone"
        echo ""
        echo "# Opção 2 - Verificar qual serviço está rodando:"
        echo "systemctl list-units | grep -i singleone"
        echo ""
        echo "# Opção 3 - Verificar processo manual:"
        echo "ps aux | grep dotnet | grep SingleOneAPI"
        echo ""
        echo "# Opção 4 - Ver logs:"
        echo "journalctl -u singleone-api -f"
    else
        echo ""
        echo "❌ Erro na compilação. Verifique os erros acima."
    fi
else
    echo "❌ Arquivo SingleOneAPI.csproj não encontrado."
    echo ""
    echo "Conteúdo do diretório atual:"
    ls -la
    echo ""
    echo "Tentando encontrar o arquivo .csproj..."
    find . -name "*.csproj" -type f 2>/dev/null
fi

