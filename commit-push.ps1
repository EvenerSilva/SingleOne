# ============================================
# Script de Commit e Push Rápido
# Projeto: SingleOne
# ============================================

param(
    [Parameter(Mandatory=$false)]
    [string]$mensagem = ""
)

# Cores para output
$Green = 'Green'
$Yellow = 'Yellow'
$Red = 'Red'
$Cyan = 'Cyan'

function Write-ColorOutput {
    param([string]$Message, [string]$Color)
    Write-Host $Message -ForegroundColor $Color
}

# Banner
Write-ColorOutput "`n🚀 SingleOne - Commit & Push Helper`n" $Cyan

# 1. Verificar se estamos em um repositório Git
if (-not (Test-Path ".git")) {
    Write-ColorOutput "❌ ERRO: Este diretório não é um repositório Git!" $Red
    Write-ColorOutput "Execute primeiro: git init" $Yellow
    exit 1
}

# 2. Verificar branch atual
$branch = git rev-parse --abbrev-ref HEAD 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "❌ ERRO: Não foi possível detectar a branch!" $Red
    exit 1
}

Write-ColorOutput "📍 Branch atual: $branch" $Cyan

# 3. Verificar se há mudanças
$status = git status --porcelain
if ([string]::IsNullOrWhiteSpace($status)) {
    Write-ColorOutput "✅ Nenhuma mudança para commitar!" $Green
    exit 0
}

# 4. Mostrar mudanças
Write-ColorOutput "`n📝 Arquivos modificados:" $Yellow
git status --short

# 5. Pedir mensagem de commit se não foi fornecida
if ([string]::IsNullOrWhiteSpace($mensagem)) {
    Write-ColorOutput "`n💬 Digite a mensagem do commit:" $Yellow
    Write-ColorOutput "   (Exemplo: 'feat: adiciona nova funcionalidade')" $Cyan
    $mensagem = Read-Host "   Mensagem"
    
    if ([string]::IsNullOrWhiteSpace($mensagem)) {
        Write-ColorOutput "❌ Mensagem de commit não pode estar vazia!" $Red
        exit 1
    }
}

# 6. Confirmar ação
Write-ColorOutput "`n⚠️  Você está prestes a:" $Yellow
Write-ColorOutput "   • Adicionar TODOS os arquivos modificados" $Cyan
Write-ColorOutput "   • Commitar com: '$mensagem'" $Cyan
Write-ColorOutput "   • Push para: origin/$branch`n" $Cyan

$confirmacao = Read-Host "Continuar? (S/N)"
if ($confirmacao -ne 'S' -and $confirmacao -ne 's') {
    Write-ColorOutput "❌ Operação cancelada!" $Red
    exit 0
}

# 7. Adicionar arquivos
Write-ColorOutput "`n📦 Adicionando arquivos..." $Yellow
git add .
if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "❌ Erro ao adicionar arquivos!" $Red
    exit 1
}

# 8. Commitar
Write-ColorOutput "💾 Criando commit..." $Yellow
git commit -m $mensagem
if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "❌ Erro ao criar commit!" $Red
    exit 1
}

# 9. Push
Write-ColorOutput "🚀 Enviando para origin/$branch..." $Yellow
git push origin $branch
if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "❌ Erro ao fazer push!" $Red
    Write-ColorOutput "Tente: git push --set-upstream origin $branch" $Yellow
    exit 1
}

# 10. Sucesso!
Write-ColorOutput "`n✅ SUCESSO!" $Green
Write-ColorOutput "   • Commit criado: $mensagem" $Cyan
Write-ColorOutput "   • Push feito para: origin/$branch`n" $Cyan

# Mostrar último commit
Write-ColorOutput "📋 Último commit:" $Yellow
git log -1 --oneline

Write-ColorOutput "`n🎉 Tudo pronto!`n" $Green

