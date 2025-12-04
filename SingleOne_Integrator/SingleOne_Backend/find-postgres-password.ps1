Write-Host "Encontrando senha do PostgreSQL..." -ForegroundColor Green
Write-Host ""

$senhas = @("password", "", "postgres", "admin", "123456", "root", "1234", "12345")

foreach ($senha in $senhas) {
    Write-Host "Testando senha: '$senha'" -ForegroundColor Yellow
    $env:PGPASSWORD = $senha
    $resultado = psql -h localhost -U postgres -c "SELECT 1;" 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "SUCESSO! Senha encontrada: '$senha'" -ForegroundColor Green
        Write-Host ""
        Write-Host "Configurando variavel de ambiente..." -ForegroundColor Yellow
        $env:DB_PASSWORD = $senha
        Write-Host "DB_PASSWORD = $senha" -ForegroundColor White
        Write-Host ""
        Write-Host "Testando conexao com banco singleone..." -ForegroundColor Yellow
        
        # Verificar se banco singleone existe
        $dbExiste = psql -h localhost -U postgres -c "SELECT 1 FROM pg_database WHERE datname='singleone';" 2>$null
        if ($dbExiste -like "*1*") {
            Write-Host "Banco singleone existe!" -ForegroundColor Green
        } else {
            Write-Host "Criando banco singleone..." -ForegroundColor Yellow
            psql -h localhost -U postgres -c "CREATE DATABASE singleone;" 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Banco singleone criado!" -ForegroundColor Green
            }
        }
        
        Write-Host ""
        Write-Host "PRONTO! Agora execute: .\run-backend.ps1" -ForegroundColor Green
        Write-Host "Mas antes, precisa atualizar a senha no script..." -ForegroundColor Yellow
        exit 0
    }
}

Write-Host "Nenhuma senha funcionou." -ForegroundColor Red
Write-Host "Verifique a instalacao do PostgreSQL ou redefina a senha." -ForegroundColor Yellow

