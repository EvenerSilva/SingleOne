# ============================================
# Script de Criação de Release
# Projeto: SingleOne
# ============================================

param(
    [Parameter(Mandatory=$false)]
    [string]$versao = ""
)

# Cores para output
$Green = 'Green'
$Yellow = 'Yellow'
$Red = 'Red'
$Cyan = 'Cyan'
$Magenta = 'Magenta'

function Write-ColorOutput {
    param([string]$Message, [string]$Color)
    Write-Host $Message -ForegroundColor $Color
}

# Banner
Write-ColorOutput "`n🎯 SingleOne - Release Creator`n" $Magenta

# 1. Verificar Git
if (-not (Test-Path ".git")) {
    Write-ColorOutput "❌ ERRO: Este diretório não é um repositório Git!" $Red
    exit 1
}

# 2. Pedir versão se não foi fornecida
if ([string]::IsNullOrWhiteSpace($versao)) {
    Write-ColorOutput "📦 Digite a versão da release (ex: v2.6.0):" $Yellow
    $versao = Read-Host "   Versão"
    
    if ([string]::IsNullOrWhiteSpace($versao)) {
        Write-ColorOutput "❌ Versão não pode estar vazia!" $Red
        exit 1
    }
}

# Adicionar 'v' se não tiver
if (-not $versao.StartsWith("v")) {
    $versao = "v$versao"
}

Write-ColorOutput "`n🚀 Criando release: $versao`n" $Cyan

# 3. Verificar se tag já existe
$tagExists = git tag -l $versao
if ($tagExists) {
    Write-ColorOutput "❌ ERRO: Tag $versao já existe!" $Red
    Write-ColorOutput "Tags existentes:" $Yellow
    git tag
    exit 1
}

# 4. Confirmar
Write-ColorOutput "⚠️  Este script irá:" $Yellow
Write-ColorOutput "   1. Criar branch release/$versao" $Cyan
Write-ColorOutput "   2. Fazer merge para main" $Cyan
Write-ColorOutput "   3. Criar tag $versao" $Cyan
Write-ColorOutput "   4. Fazer push" $Cyan
Write-ColorOutput "   5. Merge de volta para develop`n" $Cyan

$confirmacao = Read-Host "Continuar? (S/N)"
if ($confirmacao -ne 'S' -and $confirmacao -ne 's') {
    Write-ColorOutput "❌ Operação cancelada!" $Red
    exit 0
}

try {
    # 5. Atualizar develop
    Write-ColorOutput "`n📥 Atualizando branch develop..." $Yellow
    git checkout develop
    if ($LASTEXITCODE -ne 0) { throw "Erro ao fazer checkout para develop" }
    
    git pull origin develop
    if ($LASTEXITCODE -ne 0) { throw "Erro ao fazer pull de develop" }

    # 6. Criar branch de release
    Write-ColorOutput "🌿 Criando branch release/$versao..." $Yellow
    git checkout -b "release/$versao"
    if ($LASTEXITCODE -ne 0) { throw "Erro ao criar branch de release" }

    # 7. Atualizar version.txt (se existir)
    $versionFile = "SingleOne_Frontend\src\assets\version.txt"
    if (Test-Path $versionFile) {
        Write-ColorOutput "📝 Atualizando version.txt..." $Yellow
        $versao | Out-File -FilePath $versionFile -Encoding UTF8 -NoNewline
        git add $versionFile
    }

    # 8. Commitar mudanças de versão
    Write-ColorOutput "💾 Commitando mudanças de versão..." $Yellow
    git commit -m "chore: prepare release $versao" --allow-empty
    if ($LASTEXITCODE -ne 0) { throw "Erro ao commitar" }

    # 9. Merge para main
    Write-ColorOutput "🔀 Fazendo merge para main..." $Yellow
    git checkout main
    if ($LASTEXITCODE -ne 0) { throw "Erro ao fazer checkout para main" }
    
    git pull origin main
    if ($LASTEXITCODE -ne 0) { throw "Erro ao fazer pull de main" }
    
    git merge "release/$versao" --no-ff -m "chore: release $versao"
    if ($LASTEXITCODE -ne 0) { throw "Erro ao fazer merge" }

    # 10. Criar tag
    Write-ColorOutput "🏷️  Criando tag $versao..." $Yellow
    git tag -a $versao -m "Release $versao"
    if ($LASTEXITCODE -ne 0) { throw "Erro ao criar tag" }

    # 11. Push main e tag
    Write-ColorOutput "🚀 Enviando main e tag..." $Yellow
    git push origin main
    if ($LASTEXITCODE -ne 0) { throw "Erro ao fazer push de main" }
    
    git push origin $versao
    if ($LASTEXITCODE -ne 0) { throw "Erro ao fazer push da tag" }

    # 12. Merge de volta para develop
    Write-ColorOutput "🔀 Fazendo merge de volta para develop..." $Yellow
    git checkout develop
    if ($LASTEXITCODE -ne 0) { throw "Erro ao fazer checkout para develop" }
    
    git merge "release/$versao" --no-ff -m "chore: merge release $versao back to develop"
    if ($LASTEXITCODE -ne 0) { throw "Erro ao fazer merge para develop" }
    
    git push origin develop
    if ($LASTEXITCODE -ne 0) { throw "Erro ao fazer push de develop" }

    # 13. Deletar branch de release
    Write-ColorOutput "🗑️  Deletando branch de release..." $Yellow
    git branch -d "release/$versao"

    # 14. Sucesso!
    Write-ColorOutput "`n✅ RELEASE CRIADA COM SUCESSO!`n" $Green
    Write-ColorOutput "   📦 Versão: $versao" $Cyan
    Write-ColorOutput "   🏷️  Tag criada: $versao" $Cyan
    Write-ColorOutput "   🌿 Branch main atualizada" $Cyan
    Write-ColorOutput "   🌿 Branch develop atualizada`n" $Cyan

    Write-ColorOutput "🚀 PRÓXIMOS PASSOS:" $Magenta
    Write-ColorOutput "   1. Conecte no servidor Contabo via SSH" $Yellow
    Write-ColorOutput "   2. Execute: cd /var/www/singleone" $Yellow
    Write-ColorOutput "   3. Execute: git pull origin main" $Yellow
    Write-ColorOutput "   4. Execute: ./deploy-contabo.sh`n" $Yellow

    # Mostrar tags
    Write-ColorOutput "📋 Tags disponíveis:" $Cyan
    git tag | Sort-Object -Descending | Select-Object -First 5

    Write-ColorOutput "`n🎉 Release concluída!`n" $Green

} catch {
    Write-ColorOutput "`n❌ ERRO: $_" $Red
    Write-ColorOutput "`n🔙 Revertendo mudanças..." $Yellow
    
    # Tentar voltar para develop
    git checkout develop 2>$null
    
    # Tentar deletar branch de release se foi criada
    git branch -D "release/$versao" 2>$null
    
    Write-ColorOutput "⚠️  Verifique o estado do repositório com: git status" $Yellow
    exit 1
}

