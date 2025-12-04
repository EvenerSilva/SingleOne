# Script para atualizar todas as verificações de Equipamento no RequisicoesNegocio.cs
Write-Host "🔄 Atualizando verificações de Equipamento..." -ForegroundColor Yellow

$filePath = "SingleOneAPI\Negocios\RequisicoesNegocio.cs"

# Ler o arquivo
$content = Get-Content $filePath -Raw

# Atualizações necessárias
$atualizacoes = @(
    @{
        Old = 'ri\.Equipamento > 0'
        New = 'ri.Equipamento.HasValue && ri.Equipamento > 0'
    },
    @{
        Old = 'item\.Equipamento > 0'
        New = 'item.Equipamento.HasValue && item.Equipamento > 0'
    },
    @{
        Old = 'ri\.Equipamento\.Value'
        New = 'ri.Equipamento.Value'
    },
    @{
        Old = 'item\.Equipamento\.Value'
        New = 'item.Equipamento.Value'
    },
    @{
        Old = 'ri\.Equipamento \?\? 0'
        New = 'ri.Equipamento ?? 0'
    },
    @{
        Old = 'item\.Equipamento \?\? 0'
        New = 'item.Equipamento ?? 0'
    }
)

# Aplicar atualizações
foreach ($atualizacao in $atualizacoes) {
    $content = $content -replace $atualizacao.Old, $atualizacao.New
}

# Salvar arquivo atualizado
Set-Content $filePath $content -Encoding UTF8

Write-Host "✅ Atualizações aplicadas com sucesso!" -ForegroundColor Green
Write-Host "📝 Arquivo salvo: $filePath" -ForegroundColor Cyan

# Tentar compilar para verificar
Write-Host "🔨 Testando compilação..." -ForegroundColor Yellow
dotnet build SingleOneAPI --verbosity quiet

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Compilação bem-sucedida!" -ForegroundColor Green
} else {
    Write-Host "❌ Ainda há erros de compilação. Verifique manualmente." -ForegroundColor Red
}
