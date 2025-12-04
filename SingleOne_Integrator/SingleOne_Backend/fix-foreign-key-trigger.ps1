# Script para corrigir problema de chave estrangeira no trigger
Write-Host "Corrigindo problema de chave estrangeira no trigger..." -ForegroundColor Yellow

# Solicitar senha do banco
$password = Read-Host "Digite a senha do PostgreSQL" -AsSecureString
$plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))

try {
    # Executar comando SQL direto
    $env:PGPASSWORD = $plainPassword
    
    Write-Host "Verificando usuarios existentes..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "SELECT COUNT(*) as total_usuarios FROM usuarios;"
    
    Write-Host "Verificando se existe usuario com ID 0..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "SELECT id FROM usuarios WHERE id = 0;"
    
    Write-Host "Criando usuario sistema se nao existir..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "INSERT INTO usuarios (id, nome, email, senha, ativo, created_at) VALUES (0, 'Sistema', 'sistema@singleone.com.br', 'sistema', true, CURRENT_TIMESTAMP) ON CONFLICT (id) DO NOTHING;"
    
    Write-Host "Atualizando trigger para usar usuario sistema..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "
    CREATE OR REPLACE FUNCTION criar_historico_sinalizacao()
    RETURNS TRIGGER AS \$\$
    BEGIN
        -- Inserir no histórico quando uma sinalização é criada
        IF TG_OP = 'INSERT' THEN
            INSERT INTO historico_investigacoes (sinalizacao_id, usuario_id, acao, descricao)
            VALUES (NEW.id, COALESCE(NULLIF(NEW.vigilante_id, 0), 0), 'criada', 'Sinalização de suspeita criada');
        END IF;
        
        -- Inserir no histórico quando status muda
        IF TG_OP = 'UPDATE' AND OLD.status != NEW.status THEN
            INSERT INTO historico_investigacoes (sinalizacao_id, usuario_id, acao, descricao, dados_antes, dados_depois)
            VALUES (NEW.id, COALESCE(NULLIF(NEW.investigador_id, 0), NULLIF(NEW.vigilante_id, 0), 0), 
                    'status_alterado', 
                    'Status alterado de ' || OLD.status || ' para ' || NEW.status,
                    json_build_object('status', OLD.status),
                    json_build_object('status', NEW.status));
        END IF;
        
        RETURN NEW;
    END;
    \$\$ language 'plpgsql';"
    
    Write-Host "Verificando se trigger foi atualizado..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "SELECT routine_name FROM information_schema.routines WHERE routine_name = 'criar_historico_sinalizacao';"
    
    Write-Host "Correcao aplicada com sucesso!" -ForegroundColor Green
    
} catch {
    Write-Host "Erro ao executar correcao: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}
