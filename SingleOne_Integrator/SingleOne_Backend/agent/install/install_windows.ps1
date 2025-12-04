# SingleOne Agent - Instalador Windows
# Execute como Administrador: .\install_windows.ps1

$ErrorActionPreference = "Stop"

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "SingleOne Agent - Instalador Windows" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se está rodando como administrador
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$isAdmin = $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERRO: Este script deve ser executado como Administrador!" -ForegroundColor Red
    Write-Host "Clique com botão direito no PowerShell e selecione 'Executar como Administrador'" -ForegroundColor Yellow
    exit 1
}

# Verificar se Python está instalado
Write-Host "Verificando Python..." -ForegroundColor Yellow
try {
    $pythonVersion = python --version 2>&1
    Write-Host "✓ Python encontrado: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Python não encontrado!" -ForegroundColor Red
    Write-Host "Por favor, instale Python 3.8 ou superior de https://www.python.org/downloads/" -ForegroundColor Yellow
    exit 1
}

# Diretório de instalação
$installDir = "C:\Program Files\SingleOne\Agent"
Write-Host ""
Write-Host "Instalando em: $installDir" -ForegroundColor Yellow

# Criar diretório
if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
    Write-Host "✓ Diretório criado" -ForegroundColor Green
} else {
    Write-Host "✓ Diretório já existe" -ForegroundColor Green
}

# Copiar arquivos
Write-Host ""
Write-Host "Copiando arquivos..." -ForegroundColor Yellow
$currentDir = $PSScriptRoot | Split-Path
Copy-Item -Path "$currentDir\*" -Destination $installDir -Recurse -Force -Exclude @("install", "*.md", ".git*")
Write-Host "✓ Arquivos copiados" -ForegroundColor Green

# Criar diretório de logs
$logsDir = "$installDir\logs"
if (-not (Test-Path $logsDir)) {
    New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
}

# Instalar dependências Python
Write-Host ""
Write-Host "Instalando dependências Python..." -ForegroundColor Yellow
Set-Location $installDir
python -m pip install --upgrade pip --quiet
python -m pip install -r requirements.txt --quiet
Write-Host "✓ Dependências instaladas" -ForegroundColor Green

# Verificar se arquivo de configuração existe
$configFile = "$installDir\config\agent.yaml"
if (-not (Test-Path $configFile)) {
    Write-Host ""
    Write-Host "ATENÇÃO: Arquivo de configuração não encontrado!" -ForegroundColor Yellow
    Write-Host "Copiando arquivo de exemplo..." -ForegroundColor Yellow
    Copy-Item -Path "$installDir\config\agent.example.yaml" -Destination $configFile
    Write-Host "✓ Arquivo de configuração criado: $configFile" -ForegroundColor Green
    Write-Host ""
    Write-Host "IMPORTANTE: Edite o arquivo de configuração antes de iniciar o agente!" -ForegroundColor Red
    Write-Host "Configure a URL do servidor e a API Key em: $configFile" -ForegroundColor Yellow
}

# Criar tarefa agendada do Windows
Write-Host ""
Write-Host "Configurando tarefa agendada..." -ForegroundColor Yellow

$taskName = "SingleOne Agent"
$taskExists = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue

if ($taskExists) {
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
}

$action = New-ScheduledTaskAction -Execute "python" -Argument "$installDir\main.py --daemon" -WorkingDirectory $installDir
$trigger = New-ScheduledTaskTrigger -AtStartup
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -RestartCount 3 -RestartInterval (New-TimeSpan -Minutes 5)

Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Description "SingleOne Agent - Agente de Inventário de Ativos" | Out-Null

Write-Host "✓ Tarefa agendada criada" -ForegroundColor Green

# Testar conexão
Write-Host ""
Write-Host "Testando conexão com o servidor..." -ForegroundColor Yellow
$testResult = python "$installDir\main.py" --test
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Conexão bem-sucedida!" -ForegroundColor Green
} else {
    Write-Host "✗ Falha na conexão" -ForegroundColor Red
    Write-Host "Verifique a configuração em: $configFile" -ForegroundColor Yellow
}

# Resumo
Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Instalação Concluída!" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Próximos passos:" -ForegroundColor Yellow
Write-Host "1. Edite o arquivo de configuração: $configFile" -ForegroundColor White
Write-Host "2. Inicie o serviço: Start-ScheduledTask -TaskName '$taskName'" -ForegroundColor White
Write-Host "3. Verifique os logs: $logsDir\agent.log" -ForegroundColor White
Write-Host ""
Write-Host "Comandos úteis:" -ForegroundColor Yellow
Write-Host "  Iniciar:  Start-ScheduledTask -TaskName '$taskName'" -ForegroundColor White
Write-Host "  Parar:    Stop-ScheduledTask -TaskName '$taskName'" -ForegroundColor White
Write-Host "  Status:   Get-ScheduledTask -TaskName '$taskName'" -ForegroundColor White
Write-Host "  Teste:    python '$installDir\main.py' --test" -ForegroundColor White
Write-Host ""

