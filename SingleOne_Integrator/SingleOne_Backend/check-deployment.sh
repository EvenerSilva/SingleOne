#!/bin/bash
# ============================================
# Script de Diagnóstico - SingleOne Deploy
# ============================================

echo "🔍 Verificando Deploy do SingleOne..."
echo "=========================================="
echo ""

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Função para verificar serviço
check_service() {
    local service=$1
    local port=$2
    
    echo -n "Verificando $service (porta $port)... "
    
    if docker ps | grep -q "$service"; then
        echo -e "${GREEN}✓ Container rodando${NC}"
        
        # Verificar se a porta está respondendo
        if nc -z localhost $port 2>/dev/null; then
            echo -e "  ${GREEN}✓ Porta $port acessível${NC}"
        else
            echo -e "  ${RED}✗ Porta $port NÃO acessível${NC}"
        fi
        
        # Verificar saúde do container
        local status=$(docker inspect --format='{{.State.Health.Status}}' $service 2>/dev/null)
        if [ ! -z "$status" ]; then
            if [ "$status" = "healthy" ]; then
                echo -e "  ${GREEN}✓ Health check OK${NC}"
            else
                echo -e "  ${YELLOW}⚠ Health: $status${NC}"
            fi
        fi
    else
        echo -e "${RED}✗ Container NÃO está rodando${NC}"
        return 1
    fi
    echo ""
}

# Verificar Docker
echo "1. Verificando Docker..."
if command -v docker &> /dev/null; then
    echo -e "${GREEN}✓ Docker instalado${NC}"
    docker --version
else
    echo -e "${RED}✗ Docker NÃO encontrado${NC}"
    exit 1
fi
echo ""

# Verificar Docker Compose
echo "2. Verificando Docker Compose..."
if command -v docker-compose &> /dev/null; then
    echo -e "${GREEN}✓ Docker Compose instalado${NC}"
    docker-compose --version
else
    echo -e "${RED}✗ Docker Compose NÃO encontrado${NC}"
fi
echo ""

# Verificar containers
echo "3. Verificando Containers..."
check_service "singleone-postgres" 5432
check_service "singleone-backend" 5000
check_service "singleone-frontend" 3000

# Verificar uso de recursos
echo "4. Uso de Recursos..."
docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}" | grep singleone
echo ""

# Verificar portas abertas
echo "5. Portas em uso..."
echo "Porta 5432 (PostgreSQL):"
netstat -tulpn 2>/dev/null | grep 5432 || ss -tulpn 2>/dev/null | grep 5432 || echo "  Não consegui verificar (requer root)"
echo "Porta 5000 (Backend):"
netstat -tulpn 2>/dev/null | grep 5000 || ss -tulpn 2>/dev/null | grep 5000 || echo "  Não consegui verificar (requer root)"
echo "Porta 3000 (Frontend):"
netstat -tulpn 2>/dev/null | grep 3000 || ss -tulpn 2>/dev/null | grep 3000 || echo "  Não consegui verificar (requer root)"
echo ""

# Testar endpoints
echo "6. Testando Endpoints..."

echo -n "Backend API (Swagger): "
if curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/swagger | grep -q "200\|301\|302"; then
    echo -e "${GREEN}✓ Respondendo${NC}"
else
    echo -e "${RED}✗ Não respondendo${NC}"
fi

echo -n "Frontend: "
if curl -s -o /dev/null -w "%{http_code}" http://localhost:3000 | grep -q "200\|301\|302"; then
    echo -e "${GREEN}✓ Respondendo${NC}"
else
    echo -e "${RED}✗ Não respondendo${NC}"
fi
echo ""

# Verificar logs recentes
echo "7. Últimas linhas dos logs..."
echo ""
echo "--- Backend (últimas 5 linhas) ---"
docker logs singleone-backend --tail 5 2>&1
echo ""
echo "--- Frontend (últimas 5 linhas) ---"
docker logs singleone-frontend --tail 5 2>&1
echo ""

# Verificar rede Docker
echo "8. Rede Docker..."
docker network inspect singleone-network --format '{{range .Containers}}{{.Name}}: {{.IPv4Address}} {{end}}' 2>/dev/null || echo "Rede não encontrada"
echo ""

# Resumo
echo "=========================================="
echo "📊 Resumo"
echo "=========================================="

# Contar containers rodando
running=$(docker ps | grep singleone | wc -l)
total=3

echo "Containers rodando: $running/$total"

if [ $running -eq $total ]; then
    echo -e "${GREEN}✓ Todos os containers estão rodando!${NC}"
else
    echo -e "${RED}⚠ Alguns containers NÃO estão rodando${NC}"
    echo ""
    echo "Para ver containers parados:"
    echo "  docker ps -a | grep singleone"
    echo ""
    echo "Para ver logs completos:"
    echo "  docker-compose logs"
fi

echo ""
echo "=========================================="
echo "Para mais detalhes, execute:"
echo "  docker-compose logs -f [backend|frontend|postgres]"
echo "=========================================="














