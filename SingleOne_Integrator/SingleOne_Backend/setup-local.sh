#!/bin/bash

echo "🚀 Configurando ambiente de desenvolvimento local SingleOne..."

# Verificar se .NET 6.0 está instalado
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET 6.0 SDK não está instalado. Por favor, instale o .NET 6.0 SDK primeiro."
    exit 1
fi

# Verificar se Node.js está instalado
if ! command -v node &> /dev/null; then
    echo "❌ Node.js não está instalado. Por favor, instale o Node.js primeiro."
    exit 1
fi

# Verificar se npm está instalado
if ! command -v npm &> /dev/null; then
    echo "❌ npm não está instalado. Por favor, instale o npm primeiro."
    exit 1
fi

echo "✅ .NET 6.0 SDK e Node.js encontrados"

# Configurar backend
echo "🔧 Configurando backend..."
cd SingleOne_Backend/SingleOneAPI

# Restaurar dependências
echo "📦 Restaurando dependências do backend..."
dotnet restore

# Verificar se PostgreSQL está rodando
echo "🗄️  Verificando conexão com PostgreSQL..."
if ! pg_isready -h localhost -p 5432 -U postgres &> /dev/null; then
    echo "⚠️  PostgreSQL não está rodando ou não está acessível."
    echo "   Por favor, certifique-se de que o PostgreSQL está instalado e rodando."
    echo "   Você pode usar: sudo systemctl start postgresql"
    exit 1
fi

echo "✅ PostgreSQL está acessível"

# Executar migrations (se existirem)
echo "🔄 Executando migrations..."
dotnet ef database update

# Configurar frontend
echo "🔧 Configurando frontend..."
cd ../../SingleOne_Frontend

# Instalar dependências
echo "📦 Instalando dependências do frontend..."
npm install

echo ""
echo "🎉 Configuração local concluída!"
echo ""
echo "📋 Para executar o sistema:"
echo ""
echo "🔧 Backend:"
echo "   cd SingleOne_Backend/SingleOneAPI"
echo "   dotnet run"
echo ""
echo "🌐 Frontend:"
echo "   cd SingleOne_Frontend"
echo "   npm start"
echo ""
echo "📋 URLs de acesso:"
echo "   🌐 Frontend: http://localhost:4200"
echo "   🔧 Backend API: http://localhost:5000"
echo "   📚 Swagger: http://localhost:5000/swagger"
echo ""
echo "⚠️  Certifique-se de que o PostgreSQL está rodando antes de executar o backend!" 