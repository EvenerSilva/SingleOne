-- Script para permitir NULL na coluna equipamento da tabela requisicoesitens
-- Necessário para suportar requisições de linhas telefônicas

BEGIN;

-- 1. Verificar quantos registros existem com equipamento preenchido
SELECT COUNT(*) as total_com_equipamento 
FROM requisicoesitens 
WHERE equipamento IS NOT NULL;

-- 2. Verificar quantos registros existem com linhatelefonica preenchida
SELECT COUNT(*) as total_com_linha 
FROM requisicoesitens 
WHERE linhatelefonica IS NOT NULL;

-- 3. Remover a constraint NOT NULL da coluna equipamento
ALTER TABLE requisicoesitens 
ALTER COLUMN equipamento DROP NOT NULL;

-- 4. Verificar a estrutura atualizada
\d requisicoesitens

-- 5. Confirmar que a mudança foi aplicada
SELECT column_name, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'requisicoesitens' 
AND column_name = 'equipamento';

COMMIT;

-- Mensagem de sucesso
SELECT 'Coluna equipamento agora permite NULL para suportar requisições de linhas telefônicas' as resultado;

