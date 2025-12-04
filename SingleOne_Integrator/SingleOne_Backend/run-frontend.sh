#!/bin/bash

echo "🌐 Iniciando frontend SingleOne..."

# Verificar se estamos no diretório correto
if [ ! -f "SingleOne_Frontend/package.json" ]; then
    echo "❌ Execute este script na raiz do projeto SingleOne"
    exit 1
fi

# Navegar para o diretório do frontend
cd SingleOne_Frontend

# Verificar se Node.js está instalado
if ! command -v node &> /dev/null; then
    echo "❌ Node.js não está instalado"
    exit 1
fi

# Verificar se npm está instalado
if ! command -v npm &> /dev/null; then
    echo "❌ npm não está instalado"
    exit 1
fi

# Verificar se node_modules existe
if [ ! -d "node_modules" ]; then
    echo "📦 Instalando dependências..."
    npm install
fi

# Verificar se o backend está rodando
echo "🔧 Verificando backend..."
if ! curl -s http://localhost:5000/api/ > /dev/null; then
    echo "⚠️  Backend não está rodando em http://localhost:5000"
    echo "   Execute o backend primeiro: ./run-backend.sh"
    echo ""
    echo "   Ou continue sem backend (algumas funcionalidades podem não funcionar)"
    read -p "Continuar? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
else
    echo "✅ Backend está rodando"
fi

# Executar aplicação
echo "🚀 Iniciando frontend..."
echo "   🌐 Frontend: http://localhost:4200"
echo "   🔧 Backend: http://localhost:5000"
echo ""
echo "Pressione Ctrl+C para parar"
echo ""

npm start 