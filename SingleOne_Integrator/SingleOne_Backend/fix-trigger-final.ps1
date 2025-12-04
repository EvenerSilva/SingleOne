# Script para corrigir trigger de histórico
Write-Host "Corrigindo trigger de histórico..." -ForegroundColor Yellow

# Solicitar senha do banco
$password = Read-Host "Digite a senha do PostgreSQL" -AsSecureString
$plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))

try {
    # Executar comando SQL direto
    $env:PGPASSWORD = $plainPassword
    
    Write-Host "Criando usuario sistema..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "INSERT INTO usuarios (id, nome, email, senha, ativo) VALUES (0, 'Sistema', 'sistema@singleone.com.br', 'sistema', true) ON CONFLICT (id) DO NOTHING;"
    
    Write-Host "Atualizando trigger..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "CREATE OR REPLACE FUNCTION criar_historico_sinalizacao() RETURNS TRIGGER AS \$\$ BEGIN IF TG_OP = 'INSERT' THEN INSERT INTO historico_investigacoes (sinalizacao_id, usuario_id, acao, descricao) VALUES (NEW.id, COALESCE(NULLIF(NEW.vigilante_id, 0), 0), 'criada', 'Sinalização de suspeita criada'); END IF; IF TG_OP = 'UPDATE' AND OLD.status != NEW.status THEN INSERT INTO historico_investigacoes (sinalizacao_id, usuario_id, acao, descricao, dados_antes, dados_depois) VALUES (NEW.id, COALESCE(NULLIF(NEW.investigador_id, 0), NULLIF(NEW.vigilante_id, 0), 0), 'status_alterado', 'Status alterado de ' || OLD.status || ' para ' || NEW.status, json_build_object('status', OLD.status), json_build_object('status', NEW.status)); END IF; RETURN NEW; END; \$\$ language 'plpgsql';"
    
    Write-Host "Verificando usuario sistema..." -ForegroundColor Cyan
    psql -h localhost -U postgres -d singleone -c "SELECT id, nome FROM usuarios WHERE id = 0;"
    
    Write-Host "Correcao aplicada com sucesso!" -ForegroundColor Green
    
} catch {
    Write-Host "Erro ao executar correcao: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}
