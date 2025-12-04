-- Script para adicionar suporte ao status 'negada' nas contestações
-- Banco: PostgreSQL
-- Tabela: patrimonio_contestoes
-- Data: 2025-01-02

-- 1. Verificar estrutura atual da tabela
SELECT column_name, data_type, is_nullable, column_default 
FROM information_schema.columns 
WHERE table_name = 'patrimonio_contestoes' 
ORDER BY ordinal_position;

-- 2. Verificar valores únicos atuais na coluna status
SELECT DISTINCT status, COUNT(*) as quantidade
FROM patrimonio_contestoes 
GROUP BY status
ORDER BY quantidade DESC;

-- 3. Verificar se já existem registros com status 'negada'
SELECT COUNT(*) as total_negadas
FROM patrimonio_contestoes 
WHERE LOWER(TRIM(status)) = 'negada';

-- 4. (Opcional) Atualizar registros existentes que deveriam ser 'negada' 
--    Baseado na observacao_resolucao que contenha palavras-chave
-- UPDATE patrimonio_contestoes 
-- SET status = 'negada'
-- WHERE LOWER(TRIM(status)) = 'cancelada' 
--   AND LOWER(TRIM(observacao_resolucao)) LIKE '%negado%'
--   AND LOWER(TRIM(observacao_resolucao)) LIKE '%equipe técnica%';

-- 5. Verificar se a coluna status aceita o novo valor
--    (PostgreSQL aceita qualquer string, então não precisa de alteração de schema)

-- 6. Teste de inserção do novo status
-- INSERT INTO patrimonio_contestoes (
--     colaborador_id, equipamento_id, motivo, descricao, status, 
--     data_contestacao, created_at, updated_at
-- ) VALUES (
--     1, 1, 'Teste', 'Teste de status negada', 'negada',
--     NOW(), NOW(), NOW()
-- );

-- 7. Limpar teste (descomente se executou o teste)
-- DELETE FROM patrimonio_contestoes WHERE motivo = 'Teste' AND status = 'negada';

-- 8. Verificar estatísticas finais
SELECT 
    status,
    COUNT(*) as total,
    ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) as percentual
FROM patrimonio_contestoes 
GROUP BY status
ORDER BY total DESC;

-- 9. Verificar se o sistema está funcionando corretamente
--    Consulta para verificar contestações que podem ser negadas (pendentes)
SELECT 
    id,
    colaborador_id,
    equipamento_id,
    status,
    data_contestacao,
    motivo
FROM patrimonio_contestoes 
WHERE LOWER(TRIM(status)) IN ('pendente', 'aberta', 'em análise')
ORDER BY data_contestacao DESC
LIMIT 10;

COMMENT ON COLUMN patrimonio_contestoes.status IS 'Status da contestação: pendente, em análise, resolvida, cancelada, negada, pendente colaborador';
