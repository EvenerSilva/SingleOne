#!/bin/bash

echo "🚀 Configurando ambiente de desenvolvimento SingleOne..."

# Verificar se Docker está instalado
if ! command -v docker &> /dev/null; then
    echo "❌ Docker não está instalado. Por favor, instale o Docker primeiro."
    exit 1
fi

# Verificar se Docker Compose está instalado
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose não está instalado. Por favor, instale o Docker Compose primeiro."
    exit 1
fi

echo "✅ Docker e Docker Compose encontrados"

# Parar containers existentes
echo "🛑 Parando containers existentes..."
docker-compose down

# Construir e iniciar containers
echo "🔨 Construindo e iniciando containers..."
docker-compose up --build -d

# Aguardar PostgreSQL estar pronto
echo "⏳ Aguardando PostgreSQL estar pronto..."
sleep 30

# Verificar se os containers estão rodando
echo "🔍 Verificando status dos containers..."
docker-compose ps

echo ""
echo "🎉 Ambiente de desenvolvimento configurado!"
echo ""
echo "📋 URLs de acesso:"
echo "   🌐 Frontend: http://localhost"
echo "   🔧 Backend API: http://localhost:5000"
echo "   📚 Swagger: http://localhost:5000/swagger"
echo "   🗄️  PostgreSQL: localhost:5432"
echo ""
echo "📝 Comandos úteis:"
echo "   - Ver logs: docker-compose logs -f"
echo "   - Parar: docker-compose down"
echo "   - Reiniciar: docker-compose restart"
echo ""
echo "🔧 Para desenvolvimento local sem Docker:"
echo "   1. Instale .NET 6.0 SDK"
echo "   2. Instale Node.js 10+"
echo "   3. Configure PostgreSQL localmente"
echo "   4. Execute os scripts de setup manual" 