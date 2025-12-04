#!/bin/bash
# ============================================
# Script de Deploy Automatizado para Contabo
# Projeto: SingleOne
# ============================================

set -e  # Parar em caso de erro

# Cores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Funções helper
log_info() {
    echo -e "${CYAN}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

log_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Banner
echo -e "${CYAN}"
echo "╔════════════════════════════════════════╗"
echo "║   SingleOne - Deploy para Contabo    ║"
echo "╚════════════════════════════════════════╝"
echo -e "${NC}"

# Verificar se está no diretório correto
if [ ! -f "docker-compose.yml" ]; then
    log_error "Erro: docker-compose.yml não encontrado!"
    log_error "Execute este script no diretório raiz do projeto"
    exit 1
fi

# Obter branch atual
BRANCH=$(git rev-parse --abbrev-ref HEAD)
log_info "Branch atual: $BRANCH"

# Obter commit atual antes do pull
COMMIT_BEFORE=$(git rev-parse HEAD)

# 1. Backup do banco (opcional mas recomendado)
log_info "Fazendo backup do banco de dados..."
if [ -f "./backup-database.sh" ]; then
    ./backup-database.sh || log_warning "Backup falhou, continuando..."
else
    log_warning "Script de backup não encontrado, pulando..."
fi

# 2. Atualizar código
log_info "Atualizando código do repositório..."
git fetch origin
git pull origin $BRANCH

# Obter commit atual depois do pull
COMMIT_AFTER=$(git rev-parse HEAD)

# Verificar se houve mudanças
if [ "$COMMIT_BEFORE" == "$COMMIT_AFTER" ]; then
    log_success "Nenhuma atualização disponível. Sistema já está atualizado!"
    exit 0
fi

log_success "Código atualizado!"
log_info "Commit anterior: ${COMMIT_BEFORE:0:7}"
log_info "Commit atual: ${COMMIT_AFTER:0:7}"

# Verificar quais arquivos mudaram
CHANGED_FILES=$(git diff-tree --no-commit-id --name-only -r HEAD)
BACKEND_CHANGED=false
FRONTEND_CHANGED=false
DB_CHANGED=false

if echo "$CHANGED_FILES" | grep -q "SingleOne_Backend"; then
    BACKEND_CHANGED=true
    log_info "Detectadas mudanças no BACKEND"
fi

if echo "$CHANGED_FILES" | grep -q "SingleOne_Frontend"; then
    FRONTEND_CHANGED=true
    log_info "Detectadas mudanças no FRONTEND"
fi

if echo "$CHANGED_FILES" | grep -q ".sql"; then
    DB_CHANGED=true
    log_warning "Detectadas mudanças no BANCO DE DADOS"
fi

# 3. Atualizar Backend (se houver mudanças)
if [ "$BACKEND_CHANGED" = true ]; then
    log_info "Atualizando Backend (.NET)..."
    
    cd SingleOne_Backend/SingleOneAPI || exit 1
    
    log_info "Restaurando pacotes NuGet..."
    dotnet restore
    
    log_info "Compilando em modo Release..."
    dotnet build --configuration Release --no-restore
    
    log_info "Reiniciando serviço do backend..."
    sudo systemctl restart singleone-backend || log_error "Falha ao reiniciar backend!"
    
    log_success "Backend atualizado!"
    cd ../..
else
    log_info "Nenhuma mudança no backend, pulando..."
fi

# 4. Atualizar Frontend (se houver mudanças)
if [ "$FRONTEND_CHANGED" = true ]; then
    log_info "Atualizando Frontend (Angular)..."
    
    cd SingleOne_Frontend || exit 1
    
    log_info "Instalando dependências..."
    npm install --production
    
    log_info "Compilando em modo produção..."
    npm run build --prod
    
    log_info "Reiniciando Nginx..."
    sudo systemctl restart nginx || log_error "Falha ao reiniciar nginx!"
    
    log_success "Frontend atualizado!"
    cd ..
else
    log_info "Nenhuma mudança no frontend, pulando..."
fi

# 5. Aplicar migrações de banco (se houver mudanças)
if [ "$DB_CHANGED" = true ]; then
    log_warning "Mudanças no banco de dados detectadas!"
    log_warning "ATENÇÃO: Scripts SQL devem ser executados MANUALMENTE"
    log_warning "Verifique os arquivos SQL modificados e execute-os com cuidado!"
    
    echo ""
    echo "Arquivos SQL modificados:"
    echo "$CHANGED_FILES" | grep ".sql"
    echo ""
fi

# 6. Verificar status dos serviços
log_info "Verificando status dos serviços..."

if [ "$BACKEND_CHANGED" = true ]; then
    if sudo systemctl is-active --quiet singleone-backend; then
        log_success "Backend está rodando"
    else
        log_error "Backend NÃO está rodando!"
        log_info "Logs do backend:"
        sudo journalctl -u singleone-backend -n 20 --no-pager
        exit 1
    fi
fi

if [ "$FRONTEND_CHANGED" = true ]; then
    if sudo systemctl is-active --quiet nginx; then
        log_success "Nginx está rodando"
    else
        log_error "Nginx NÃO está rodando!"
        log_info "Logs do nginx:"
        sudo journalctl -u nginx -n 20 --no-pager
        exit 1
    fi
fi

# 7. Limpar cache (opcional)
log_info "Limpando cache..."
if [ -d "SingleOne_Backend/SingleOneAPI/obj" ]; then
    rm -rf SingleOne_Backend/SingleOneAPI/obj
fi
if [ -d "SingleOne_Frontend/.angular" ]; then
    rm -rf SingleOne_Frontend/.angular
fi

# 8. Sucesso!
echo ""
echo -e "${GREEN}"
echo "╔════════════════════════════════════════╗"
echo "║     ✅ DEPLOY CONCLUÍDO COM SUCESSO!  ║"
echo "╚════════════════════════════════════════╝"
echo -e "${NC}"

log_info "Resumo:"
if [ "$BACKEND_CHANGED" = true ]; then
    echo "  • Backend atualizado e reiniciado"
fi
if [ "$FRONTEND_CHANGED" = true ]; then
    echo "  • Frontend atualizado e compilado"
fi
if [ "$DB_CHANGED" = true ]; then
    echo "  ⚠️  Scripts SQL precisam ser executados manualmente"
fi

log_info "Commit atual: ${COMMIT_AFTER:0:7}"
log_info "Branch: $BRANCH"

echo ""
log_success "Sistema SingleOne atualizado com sucesso!"
echo ""

# 9. Mostrar últimos commits
log_info "Últimas mudanças:"
git log --oneline -5

echo ""
log_info "Para visualizar logs:"
echo "  • Backend: sudo journalctl -u singleone-backend -f"
echo "  • Nginx: sudo journalctl -u nginx -f"
echo ""

