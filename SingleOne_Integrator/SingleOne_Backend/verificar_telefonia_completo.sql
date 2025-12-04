-- Script completo para verificar e corrigir estrutura da telefonia
BEGIN;

-- 1. Verificar se a tabela telefonialinhas existe e sua estrutura
\echo '=== ESTRUTURA DA TABELA telefonialinhas ==='
\d telefonialinhas

-- 2. Verificar se a tabela requisicoesitens tem a coluna linhatelefonica
\echo '=== ESTRUTURA DA TABELA requisicoesitens ==='
\d requisicoesitens

-- 3. Verificar constraints existentes
\echo '=== CONSTRAINTS EXISTENTES ==='
SELECT 
    tc.constraint_name, 
    tc.table_name, 
    kcu.column_name, 
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name 
FROM 
    information_schema.table_constraints AS tc 
    JOIN information_schema.key_column_usage AS kcu
      ON tc.constraint_name = kcu.constraint_name
      AND tc.table_schema = kcu.table_schema
    JOIN information_schema.constraint_column_usage AS ccu
      ON ccu.constraint_name = tc.constraint_name
      AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY' 
    AND tc.table_name='requisicoesitens';

-- 4. Verificar se há dados na tabela telefonialinhas
\echo '=== DADOS NA TABELA telefonialinhas ==='
SELECT COUNT(*) as total_linhas FROM telefonialinhas;
SELECT id, numero, emuso FROM telefonialinhas LIMIT 5;

-- 5. Verificar se há dados na tabela requisicoesitens com linhas telefônicas
\echo '=== DADOS DE REQUISIÇÕES COM LINHAS TELEFÔNICAS ==='
SELECT COUNT(*) as total_requisicoes_com_linha FROM requisicoesitens WHERE linhatelefonica IS NOT NULL;

COMMIT;
