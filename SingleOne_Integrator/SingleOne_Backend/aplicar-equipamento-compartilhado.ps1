# ============================================================================
# Script: Aplicar Equipamento Compartilhado
# Descrição: Executa os scripts SQL para criar a estrutura de equipamentos compartilhados
# Data: 03/10/2025
# ============================================================================

param(
    [switch]$TestarApenas,
    [switch]$ComTestes,
    [string]$PostgresHost = "localhost",
    [string]$PostgresPort = "5432",
    [string]$PostgresUser = "postgres",
    [string]$DatabaseName = "singleone"
)

Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host " Aplicação de Equipamento Compartilhado" -ForegroundColor Cyan
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host ""

# Função para exibir mensagem de sucesso
function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

# Função para exibir mensagem de erro
function Write-Error-Message {
    param([string]$Message)
    Write-Host "[ERRO] $Message" -ForegroundColor Red
}

# Função para exibir mensagem de aviso
function Write-Warning-Message {
    param([string]$Message)
    Write-Host "[AVISO] $Message" -ForegroundColor Yellow
}

# Função para exibir mensagem de informação
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

# Verificar se o PostgreSQL está acessível
Write-Info "Verificando conexão com PostgreSQL..."
$testConnection = psql -h $PostgresHost -p $PostgresPort -U $PostgresUser -d $DatabaseName -c "SELECT 1;" 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Error-Message "Não foi possível conectar ao PostgreSQL"
    Write-Host ""
    Write-Host "Certifique-se de que:"
    Write-Host "  1. O PostgreSQL está rodando"
    Write-Host "  2. As credenciais estão corretas"
    Write-Host "  3. O banco de dados '$DatabaseName' existe"
    Write-Host ""
    Write-Host "Você pode especificar parâmetros customizados:"
    Write-Host "  .\aplicar-equipamento-compartilhado.ps1 -PostgresHost 'localhost' -PostgresPort '5432' -PostgresUser 'postgres' -DatabaseName 'singleone'"
    exit 1
}

Write-Success "Conexão com PostgreSQL estabelecida"
Write-Host ""

# Se for apenas teste, executar script de testes
if ($TestarApenas) {
    Write-Info "Modo TESTE: Executando apenas script de testes..."
    Write-Host ""
    
    $scriptTeste = ".\testar-equipamento-compartilhado.sql"
    
    if (-not (Test-Path $scriptTeste)) {
        Write-Error-Message "Arquivo de teste não encontrado: $scriptTeste"
        exit 1
    }
    
    psql -h $PostgresHost -p $PostgresPort -U $PostgresUser -d $DatabaseName -f $scriptTeste
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Success "Testes executados com sucesso!"
    } else {
        Write-Host ""
        Write-Error-Message "Erro ao executar testes"
        exit 1
    }
    
    exit 0
}

# Executar script de criação
Write-Info "Aplicando script de criação da estrutura..."
Write-Host ""

$scriptCriacao = ".\criar-equipamento-compartilhado.sql"

if (-not (Test-Path $scriptCriacao)) {
    Write-Error-Message "Arquivo de criação não encontrado: $scriptCriacao"
    exit 1
}

# Confirmar antes de executar
Write-Warning-Message "ATENÇÃO: Este script irá modificar o banco de dados!"
Write-Host ""
Write-Host "Será executado:"
Write-Host "  - Adição de coluna 'compartilhado' na tabela equipamentos"
Write-Host "  - Criação da tabela equipamento_usuarios_compartilhados"
Write-Host "  - Criação de índices, views e funções"
Write-Host ""
$confirmacao = Read-Host "Deseja continuar? (S/N)"

if ($confirmacao -ne "S" -and $confirmacao -ne "s") {
    Write-Info "Operação cancelada pelo usuário"
    exit 0
}

Write-Host ""
Write-Info "Executando script de criação..."

psql -h $PostgresHost -p $PostgresPort -U $PostgresUser -d $DatabaseName -f $scriptCriacao

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Error-Message "Erro ao executar script de criação"
    exit 1
}

Write-Host ""
Write-Success "Script de criação executado com sucesso!"

# Se solicitado, executar testes
if ($ComTestes) {
    Write-Host ""
    Write-Info "Executando testes..."
    Write-Host ""
    
    $scriptTeste = ".\testar-equipamento-compartilhado.sql"
    
    if (Test-Path $scriptTeste) {
        psql -h $PostgresHost -p $PostgresPort -U $PostgresUser -d $DatabaseName -f $scriptTeste
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Success "Testes executados com sucesso!"
        } else {
            Write-Host ""
            Write-Warning-Message "Houve erros nos testes, mas a estrutura foi criada"
        }
    } else {
        Write-Warning-Message "Arquivo de teste não encontrado: $scriptTeste"
    }
}

# Resumo final
Write-Host ""
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host " Resumo da Aplicação" -ForegroundColor Cyan
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Success "Estrutura de equipamento compartilhado criada com sucesso!"
Write-Host ""
Write-Host "Estruturas criadas:"
Write-Host "  ✓ Coluna 'compartilhado' na tabela equipamentos"
Write-Host "  ✓ Tabela equipamento_usuarios_compartilhados"
Write-Host "  ✓ Índices de performance"
Write-Host "  ✓ Views: vw_equipamentos_compartilhados"
Write-Host "  ✓ Views: vw_equipamentos_usuarios_compartilhados"
Write-Host "  ✓ Função: fn_adicionar_usuario_compartilhado"
Write-Host "  ✓ Função: fn_remover_usuario_compartilhado"
Write-Host "  ✓ Trigger: trg_validar_equipamento_compartilhado"
Write-Host ""
Write-Info "Próximos passos:"
Write-Host "  1. Implementar modelos C# (ver PROPOSTA_EQUIPAMENTO_COMPARTILHADO.md)"
Write-Host "  2. Criar endpoints de API"
Write-Host "  3. Desenvolver interface frontend"
Write-Host "  4. Atualizar relatórios"
Write-Host ""
Write-Info "Para executar apenas os testes:"
Write-Host "  .\aplicar-equipamento-compartilhado.ps1 -TestarApenas"
Write-Host ""
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host ""

