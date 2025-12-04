# Script para reverter todas as mudanças de nullable de volta para int
Write-Host "🔄 Revertendo mudanças para estrutura original..." -ForegroundColor Yellow

$filePath = "SingleOneAPI\Negocios\RequisicoesNegocio.cs"

# Ler o arquivo
$content = Get-Content $filePath -Raw

# Reversões necessárias
$reversoes = @(
    @{
        Old = 'ri\.Equipamento\.Value'
        New = 'ri.Equipamento'
    },
    @{
        Old = 'item\.Equipamento\.Value'
        New = 'item.Equipamento'
    },
    @{
        Old = 'ri\.Equipamento \?\? 0'
        New = 'ri.Equipamento'
    },
    @{
        Old = 'item\.Equipamento \?\? 0'
        New = 'item.Equipamento'
    },
    @{
        Old = 'ri\.Equipamento\.HasValue && ri\.Equipamento > 0'
        New = 'ri.Equipamento > 0'
    },
    @{
        Old = 'item\.Equipamento\.HasValue && item\.Equipamento > 0'
        New = 'item.Equipamento > 0'
    },
    @{
        Old = 'ri\.Equipamento\.HasValue && ri\.Equipamento > 0 && _equipamentoRepository\.ObterPorId\(ri\.Equipamento\.Value\)'
        New = 'ri.Equipamento > 0 && _equipamentoRepository.ObterPorId(ri.Equipamento)'
    },
    @{
        Old = 'item\.Linhatelefonica == null && item\.Equipamento\.HasValue\) \? item\.Equipamento\.Value : 0'
        New = 'item.Linhatelefonica == null) ? item.Equipamento : 0'
    }
)

# Aplicar reversões
foreach ($reversao in $reversoes) {
    $content = $content -replace $reversao.Old, $reversao.New
}

# Salvar arquivo revertido
Set-Content $filePath $content -Encoding UTF8

Write-Host "✅ Reversões aplicadas com sucesso!" -ForegroundColor Green
Write-Host "📝 Arquivo salvo: $filePath" -ForegroundColor Cyan

# Tentar compilar para verificar
Write-Host "🔨 Testando compilação..." -ForegroundColor Yellow
dotnet build SingleOneAPI --verbosity quiet

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Compilação bem-sucedida!" -ForegroundColor Green
} else {
    Write-Host "❌ Ainda há erros de compilação. Verifique manualmente." -ForegroundColor Red
}
