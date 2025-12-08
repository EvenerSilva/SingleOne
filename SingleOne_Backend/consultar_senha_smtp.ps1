# Script PowerShell para consultar a senha SMTP do banco de dados
# Execute: .\consultar_senha_smtp.ps1

param(
    [string]$DbHost = "localhost",
    [string]$Port = "5432",
    [string]$Database = "singleone",
    [string]$User = "postgres",
    [string]$Password = "postgres"
)

Write-Host "=== Consultando Senha SMTP ===" -ForegroundColor Cyan
Write-Host ""

$query = @"
SELECT 
    id,
    cliente,
    smtp_enabled,
    smtp_host,
    smtp_port,
    smtp_login,
    smtp_password,
    smtp_email_from,
    smtp_enable_ssl
FROM parametros
WHERE smtp_enabled = true
ORDER BY cliente;
"@

try {
    # Verificar se psql está disponível
    $psqlPath = Get-Command psql -ErrorAction SilentlyContinue
    if (-not $psqlPath) {
        Write-Host "❌ psql não encontrado. Instale o PostgreSQL ou ajuste o PATH." -ForegroundColor Red
        Write-Host "   Download: https://www.postgresql.org/download/" -ForegroundColor Yellow
        exit 1
    }

    # Executar consulta
    $env:PGPASSWORD = $Password
    $result = & psql -h $DbHost -p $Port -d $Database -U $User -c $query 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host $result
        Write-Host ""
        Write-Host "✅ Consulta executada com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "❌ Erro ao executar consulta:" -ForegroundColor Red
        Write-Host $result
    }
} catch {
    Write-Host "❌ Erro: $_" -ForegroundColor Red
} finally {
    $env:PGPASSWORD = $null
}

