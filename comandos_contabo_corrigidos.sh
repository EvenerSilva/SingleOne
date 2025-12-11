#!/bin/bash

# Script para encontrar e atualizar o projeto SingleOne no Contabo

echo "🔍 Procurando o projeto SingleOne..."
echo ""

# Tentar encontrar o diretório do projeto
PROJECT_PATH=""

# Opção 1: Buscar por arquivo .sln
PROJECT_PATH=$(find /home -name "SingleOne*.sln" 2>/dev/null | head -1 | xargs dirname 2>/dev/null)

# Opção 2: Se não encontrou, buscar por .csproj
if [ -z "$PROJECT_PATH" ]; then
    PROJECT_PATH=$(find /home -name "SingleOneAPI.csproj" 2>/dev/null | head -1 | xargs dirname 2>/dev/null)
fi

# Opção 3: Se ainda não encontrou, buscar diretório SingleOne_Backend
if [ -z "$PROJECT_PATH" ]; then
    PROJECT_PATH=$(find /home -type d -name "SingleOne_Backend" 2>/dev/null | head -1)
fi

# Opção 4: Se ainda não encontrou, buscar diretório SingleOne
if [ -z "$PROJECT_PATH" ]; then
    PROJECT_PATH=$(find /home -type d -name "SingleOne" 2>/dev/null | head -1)
fi

# Opção 5: Verificar diretório atual
if [ -z "$PROJECT_PATH" ]; then
    if [ -f "SingleOneAPI.csproj" ] || [ -f "SingleOne*.sln" ]; then
        PROJECT_PATH=$(pwd)
    fi
fi

if [ -z "$PROJECT_PATH" ]; then
    echo "❌ Não foi possível encontrar o projeto automaticamente."
    echo ""
    echo "Por favor, execute manualmente:"
    echo "1. find /home -name 'SingleOne*.sln' -o -name 'SingleOneAPI.csproj'"
    echo "2. cd /caminho/encontrado"
    echo "3. dotnet build"
    exit 1
fi

echo "✅ Projeto encontrado em: $PROJECT_PATH"
echo ""

# Navegar até o diretório
cd "$PROJECT_PATH" || exit 1

# Verificar se está no diretório correto
if [ ! -f "SingleOneAPI.csproj" ] && [ ! -f "*.sln" ]; then
    # Tentar entrar em SingleOne_Backend se existir
    if [ -d "SingleOne_Backend" ]; then
        cd "SingleOne_Backend" || exit 1
    fi
fi

echo "📂 Diretório atual: $(pwd)"
echo ""

# Verificar se tem arquivo .csproj ou .sln
if [ -f "SingleOneAPI.csproj" ]; then
    echo "✅ Arquivo SingleOneAPI.csproj encontrado!"
    echo ""
    echo "🔨 Recompilando o projeto..."
    dotnet build
    BUILD_STATUS=$?
    
    if [ $BUILD_STATUS -eq 0 ]; then
        echo ""
        echo "✅ Build concluído com sucesso!"
        echo ""
        echo "🔄 Próximos passos:"
        echo "1. Reiniciar o serviço:"
        echo "   sudo systemctl restart singleone-api"
        echo "   # ou"
        echo "   sudo systemctl restart singleone"
        echo ""
        echo "2. Verificar status:"
        echo "   systemctl status singleone-api"
        echo ""
        echo "3. Ver logs:"
        echo "   journalctl -u singleone-api -f"
    else
        echo ""
        echo "❌ Erro na compilação. Verifique os erros acima."
    fi
elif [ -f "*.sln" ]; then
    echo "✅ Arquivo .sln encontrado!"
    echo ""
    echo "🔨 Recompilando a solução..."
    dotnet build
else
    echo "❌ Arquivo .csproj ou .sln não encontrado no diretório atual."
    echo ""
    echo "Conteúdo do diretório:"
    ls -la
    exit 1
fi

